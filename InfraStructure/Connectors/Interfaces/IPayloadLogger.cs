using System.Threading.Tasks;

namespace InfraStructure.Connectors.Interfaces
{
    public interface IPayloadLogger
    {
        Task<string> LogPayloadAsync(string correlationId, string payload, string extension = "txt");
    }
}
