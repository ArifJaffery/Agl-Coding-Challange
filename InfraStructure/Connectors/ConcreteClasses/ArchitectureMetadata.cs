using System;
using System.Collections.Generic;
using System.Text;

namespace InfraStructure.Connectors.ConcreteClasses
{
    /// <summary>
    /// Metadata that describes the context for individual operation/action/message in the overall architecture
    /// </summary>
    public class ArchitectureMetadata
    {
        public ArchitectureMetadata(string orgUnit = "",
            string domain = "",
            string service = "",
            string serviceInterface = "",
            string entityType = "",
            string target = "",
            string component = "")
        {
            OrgUnit = orgUnit;
            Domain = domain;
            Service = service;
            ServiceInterface = serviceInterface;
            EntityType = entityType;
            Target = target;
            Component = component;
        }

        /// <summary>
        /// Logical name for the service in the architecture. Source: API Action Code Annotation
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// Logical name for the interface in the architecture. Source: API Action Code Annotation
        /// </summary>
        public string ServiceInterface { get; set; }

        /// <summary>
        /// Org unit that owns this component. Source: API Action Code Annotation
        /// </summary>
        public string OrgUnit { get; set; }

        /// <summary>
        /// Logical name for the domain in the architecture. Source: API Action Code Annotation
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Target system of the HTTP request (Web API). Source: API Action Code Annotation
        /// </summary>
        public string Target { get; set; } = string.Empty;

        /// <summary>
        /// Entity type for resource that is the target of the HTTP request. Source: API Action Code Annotation
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Logical name for the component in the architecture. Source: API Action Code Annotation
        /// </summary>
        public string Component { get; set; } = string.Empty;


        private string[] ToArray()
        {
            var array = new string[7];
            array[0] = this.OrgUnit;
            array[1] = this.Domain;
            array[2] = this.Service;
            array[3] = this.ServiceInterface;
            array[4] = this.EntityType;
            array[5] = this.Target;
            array[6] = this.Component;
            return array;
        }
        private void FromArray(string[] array)
        {
            this.OrgUnit = array[0];
            this.Domain = array[1];
            this.Service = array[2];
            this.ServiceInterface = array[3];
            this.EntityType = array[4];
            this.Target = array[5];
            this.Component = array[6];
        }

        public override string ToString()
        {
            return string.Join("/", ToArray());
        }

        public static ArchitectureMetadata FromString(string str)
        {
            var metadata = new ArchitectureMetadata();
            var array = metadata.ToArray();

            if (!String.IsNullOrEmpty(str))
            {
                var parts = str.Split('/');
                for (var i = 0; i < Math.Min(parts.Length, array.Length); i++)
                {
                    array[i] = parts[i];
                }
                metadata.FromArray(array);
            }

            return metadata;
        }

        public static ArchitectureMetadata FromObjectDictionary(IDictionary<string, object> dict)
        {
            var metadata = new ArchitectureMetadata();
            metadata.Component = GetObjectDictionaryValue(dict, nameof(ArchitectureMetadata.Component));
            metadata.Domain = GetObjectDictionaryValue(dict, nameof(ArchitectureMetadata.Domain));
            metadata.EntityType = GetObjectDictionaryValue(dict, nameof(ArchitectureMetadata.EntityType));
            metadata.OrgUnit = GetObjectDictionaryValue(dict, nameof(ArchitectureMetadata.OrgUnit));
            metadata.Service = GetObjectDictionaryValue(dict, nameof(ArchitectureMetadata.Service));
            metadata.ServiceInterface = GetObjectDictionaryValue(dict, nameof(ArchitectureMetadata.ServiceInterface));
            metadata.Target = GetObjectDictionaryValue(dict, nameof(ArchitectureMetadata.Target));

            return metadata;
        }

        private static string GetObjectDictionaryValue(IDictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key))
            {
                var value = dict[key];
                if (value == null)
                {
                    return null;
                }
                return value.ToString();
            }
            return null;
        }
    }
}
