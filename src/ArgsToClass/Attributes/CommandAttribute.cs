namespace ArgsToClass.Attributes
{
    public class CommandAttribute: SchemaAttribute
    {
        public string Name { get; }

        public CommandAttribute(string name = null)
        {
            Name = name;
        }
    }
}
