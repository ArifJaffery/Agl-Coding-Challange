using System.Threading.Tasks;

namespace InfraStructure.Connectors.Interfaces
{
    public interface IHealthCheckable
    {
        Task IsAlive();

    }
}
