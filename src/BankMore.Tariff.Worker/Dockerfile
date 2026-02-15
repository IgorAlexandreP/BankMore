FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/BankMore.Tariff.Worker/BankMore.Tariff.Worker.csproj", "src/BankMore.Tariff.Worker/"]
COPY ["src/BankMore.Core/BankMore.Core.csproj", "src/BankMore.Core/"]
RUN dotnet restore "src/BankMore.Tariff.Worker/BankMore.Tariff.Worker.csproj"
COPY . .
WORKDIR "/src/src/BankMore.Tariff.Worker"
RUN dotnet build "BankMore.Tariff.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BankMore.Tariff.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankMore.Tariff.Worker.dll"]
