using Core.Domains.Pets;
using InfraStructure.Connectors.Interfaces;
using PetsProcessApi.Constants;
using PetsProcessApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PetsProcessApi.Orchestrator
{
    /// <summary>
    /// 
    /// </summary>
    public class PetProcessOrchestrator : IPetProcessOrchestrator
    {
        /// <summary>
        /// 
        /// </summary>
        readonly IPetProcessApiConnector _petProcessApiConnector;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="petProcessApiConnector"></param>
        public PetProcessOrchestrator(IPetProcessApiConnector petProcessApiConnector)
        {
            _petProcessApiConnector = petProcessApiConnector;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<PetGenderSummary>> ProcessData()
        {
            // Call Pet Process API connctor to get all personal pets
            var personPets = await _petProcessApiConnector.GetAllPersonPets<List<PersonPets>>(CancellationToken.None);
            var petOwnerGenderSummary = new List<PetGenderSummary>();
            // Creatin Summary for each owner gender
            personPets.ForEach(pPet =>
            {
                var searchOwnerGender = petOwnerGenderSummary.FirstOrDefault(p => p.Gender == pPet.Gender);
                if (searchOwnerGender == null)
                {
                    searchOwnerGender = new PetGenderSummary() { Gender = pPet.Gender, Name = new List<string>() };
                    petOwnerGenderSummary.Add(searchOwnerGender);
                }

                pPet.Pets?.ForEach(pet => {
                    if (pet.PetType == Constant.CatType) {
                        searchOwnerGender.Name.Add(pet.Name);
                    }
                });
            });
            // Sort Cat names
            petOwnerGenderSummary?.ForEach(og => og.Name.Sort());
            return petOwnerGenderSummary;
        }
    }
}
