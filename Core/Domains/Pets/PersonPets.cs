using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Domains.Pets
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class PersonPets: Person
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "Pets")]
        public List<Pet> Pets { get; set; }
    }
}
