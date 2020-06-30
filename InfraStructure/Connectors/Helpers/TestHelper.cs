using System;

namespace InfraStructure.Connectors.Helpers
{
    public static class TestHelper
    {
        public const string UNIT_TEST_CONTEXT = "UNIT_TEST_CONTEXT";
        public const string TEST_OAUTH2_TOKEN = "TEST_OAUTH2_TOKEN";

        public static void SetUnitTestContext(bool value)
        {
            Environment.SetEnvironmentVariable(UNIT_TEST_CONTEXT, "true");
        }

        public static bool IsUnitTestContext()
        {
            return Environment.GetEnvironmentVariable(UNIT_TEST_CONTEXT) == "true";
        }

    }
}
