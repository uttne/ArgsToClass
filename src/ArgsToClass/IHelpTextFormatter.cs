namespace ArgsToClass
{
    public interface IHelpTextFormatter
    {
        string Format(CommandSchema commandSchema);
        string Format(SubCommandSchema subCommandSchema);
    }
}