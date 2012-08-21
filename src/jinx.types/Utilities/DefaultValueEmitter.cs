namespace jinx.types.Utilities
{
    public static class DefaultValueEmitter
    {
        public static string FromType(string typeName)
        {
            string initValue = "\"\"";

            switch (typeName)
            {
                case "bool":
                    initValue = "false";
                    break;
            }

            return initValue;
        }
    }
}