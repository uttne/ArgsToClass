namespace ArgsToClass.Attributes
{
    public class CommandAttribute: SchemaAttribute
    {
        public string Name { get; }
        public string Description { get; }

        public CommandAttribute(string name = null,string description = null)
        {
            Name = name;
            Description = description;
        }
    }
}
