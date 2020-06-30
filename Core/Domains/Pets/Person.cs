using System.Runtime.Serialization;

namespace Core.Domains.Pets
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class Person
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name ="name")]
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name ="gender")]
        public string Gender { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name ="age")]
        public int Age { get; set; }
    }
}
