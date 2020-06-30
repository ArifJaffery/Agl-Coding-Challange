using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InfraStructure.Connectors.Interfaces
{
    public interface IPetProcessApiConnector : IHealthCheckable
    {
        Task<T> GetAllPersonPets<T>(CancellationToken cancellationToken);
    }
}
