using BankMore.Core.Auth;
using BankMore.Transfer.API.Infrastructure;
using BankMore.Transfer.API.Infrastructure.Repositories;
using BankMore.Transfer.API.Infrastructure.Services;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = new JwtSettings();
builder.Services.AddSingleton(jwtSettings);

builder.Services.AddScoped<DbInitializer>();
builder.Services.AddScoped<ITransferRepository, TransferRepository>();

// Polly Retry Policy
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

builder.Services.AddHttpClient<IAccountService, AccountService>(client =>
{
    var accountApiUrl = builder.Configuration.GetValue<string>("AccountApiUrl");
    client.BaseAddress = new Uri(accountApiUrl ?? "http://localhost:5000"); 
})
.AddPolicyHandler(retryPolicy);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

builder.Services.AddKafka(kafka => 
{
    var brokers = builder.Configuration.GetSection("Kafka:Brokers").Get<string[]>();
    if (brokers == null || brokers.Length == 0)
    {
        var brokerString = builder.Configuration.GetValue<string>("Kafka:Brokers");
        if (!string.IsNullOrEmpty(brokerString))
        {
            brokers = brokerString.Split(',');
        }
    }
    
    // Fallback default for local dev if config missing
    if (brokers == null || brokers.Length == 0) brokers = new[] { "localhost:9092" };

    kafka.AddCluster(cluster => cluster
        .WithBrokers(brokers)
        .AddProducer("transferencia-producer", producer => producer
            .DefaultTopic("transferencias-realizadas")
            .AddMiddlewares(m => m.AddSerializer<KafkaFlow.Serializer.NewtonsoftJsonSerializer>())
        )
    );
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    initializer.Initialize();
}

var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
