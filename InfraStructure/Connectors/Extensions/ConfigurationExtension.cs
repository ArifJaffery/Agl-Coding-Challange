using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfraStructure.Connectors.Extensions
{
    public static class ConfigurationExtension
    {
        /// <summary>
        /// Gets a typed config object from a configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static T GetTypedSection<T>(this IConfiguration configuration, string sectionName) where T : class, new()
        {
            if (sectionName == null)
            {
                sectionName = typeof(T).Name;
            }
            var options = new T();
            configuration.GetSection(sectionName).Bind(options);
            return options;
        }

        /// <summary>
        /// Gets a typed config object from a configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static T GetTypedSection<T>(this IConfiguration configuration) where T : class, new()
        {
            return configuration.GetTypedSection<T>(null);
        }



    }
}
