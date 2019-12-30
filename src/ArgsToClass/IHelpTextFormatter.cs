namespace ArgsToClass
{
    public interface IHelpTextFormatter
    {
        string Format(CommandSchema commandSchema, SubCommandSchema[] subCommandSchemata);
        string Format(SubCommandSchema subCommandSchema, SubCommandSchema[] subCommandSchemata);
    }
}