using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PetsProcessApi.Models;
using PetsProcessApi.Orchestrator;

namespace PetsProcessApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PeoplePetsController : ControllerBase
    {
        readonly IPetProcessOrchestrator _orchestrator;

        public PeoplePetsController(IPetProcessOrchestrator orchestrator)
        {
            _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator)); ;
        }

        [HttpGet]
        public async Task<IEnumerable<PetGenderSummary>> Get()
        {
            return await _orchestrator.ProcessData();
        }
    }
}
