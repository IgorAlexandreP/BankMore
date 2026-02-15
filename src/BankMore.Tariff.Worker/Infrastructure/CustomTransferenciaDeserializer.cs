using KafkaFlow;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using BankMore.Tariff.Worker.Domain;
using BankMore.Tariff.Worker.Consumers;

namespace BankMore.Tariff.Worker.Infrastructure;

public class CustomTransferenciaDeserializer : IDeserializer
{
    public async Task<object> DeserializeAsync(Stream input, Type type, ISerializerContext context)
    {
        try 
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var obj = await JsonSerializer.DeserializeAsync<TransferenciaRealizadaEvent>(input, options);
            if (obj == null) throw new InvalidOperationException("Deserialized object is null");
            return obj;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao deserializar: {ex.Message}");
            throw;
        }
    }
}
