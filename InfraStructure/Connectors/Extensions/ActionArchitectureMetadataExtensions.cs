using InfraStructure.Connectors.ConcreteClasses;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfraStructure.Connectors.Extensions
{
    public static class ActionArchitectureMetadataExtensions
    {
        /// <summary>
        /// Extracts and returns first instance of ActionArchitectureMetadata type in the HttpContext Items
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns>First instance of ActionArchitectureMetadata type in the HttpContext Items</returns>
        public static ArchitectureMetadata GetActionArchitectureMetadata(this HttpContext httpContext)
        {
            var actionArchitectureMetadata = httpContext?.Items?.FirstOrDefault(x => string.Equals(x.Key.ToString(), nameof(ArchitectureMetadata), StringComparison.OrdinalIgnoreCase));
            return actionArchitectureMetadata?.Value as ArchitectureMetadata;
        }
    }
}
