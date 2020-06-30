using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetsProcessApi.Models
{
    public class PetGenderSummary
    {
        public string Gender { get; set; }
        public List<string> Name { get; set; }
    }
}
