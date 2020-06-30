using System;
using System.Collections.Generic;
using System.Text;

namespace InfraStructure.Connectors.Models
{
    public static class AuthorizationClaimsTypes
    {
        /// <summary>
        /// "x_tags"; token claim that contains client application tags
        /// </summary>
        public static readonly string Tags = "x_tags";

        /// <summary>
        /// "x_data_auth_type"; Claim type for data authorization type
        /// </summary>
        public static readonly string DataAuthorizationType = "x_data_auth_type";

        /// <summary>
        /// "x_org_id"; Claim type for organisation's identifier
        /// </summary>
        public const string DataAuthorizationOrgId = "x_org_id";

        /// <summary>
        /// "x_app_name"; Custom data authorization claim type for application's name
        /// </summary>
        public static readonly string AppName = "x_app_name";

    }
}
