using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
namespace Core.Domains.Pets
{
    /// <summary>
    /// 
    /// </summary>
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum PetTypes
    {
        [EnumMember(Value = "Cat")]
        Cat  = 1,
        [EnumMember(Value="Dog")]
        Dog =2,
        [EnumMember(Value = "Fish")]
        Fish =3
    }
}
