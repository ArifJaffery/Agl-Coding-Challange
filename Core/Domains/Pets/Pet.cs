using System.Runtime.Serialization;

namespace Core.Domains.Pets
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class Pet { 
    /// <summary>
    /// 
    /// </summary>
    [DataMember(Name="name")]
        public string Name { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [DataMember(Name ="type")]
        public PetTypes PetType { get; set; }
    }
}
