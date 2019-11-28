namespace ArgsToClass.Attributes
{
    public class SubCommandAttribute: SchemaAttribute
    {
        public string Name { get; }

        public SubCommandAttribute(string name = null)
        {
            Name = name;
        }
    }
}
