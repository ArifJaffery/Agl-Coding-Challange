using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domains.Pets;
using PetsProcessApi.Models;

namespace PetsProcessApi.Orchestrator
{
    public interface IPetProcessOrchestrator
    {
        Task<List<PetGenderSummary>> ProcessData();
    }
}
