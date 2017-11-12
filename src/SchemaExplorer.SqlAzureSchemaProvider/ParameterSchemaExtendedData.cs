namespace SchemaExplorer
{
    internal class ParameterSchemaExtendedData
    {
        public ParameterSchemaExtendedData(string name, string defaultValue, string comment)
        {
            Name = name;
            DefaultValue = defaultValue;
            Comment = comment;
        }

        public string Name { get; set; }

        public string DefaultValue { get; set; }

        public string Comment { get; set; }
    }
}
