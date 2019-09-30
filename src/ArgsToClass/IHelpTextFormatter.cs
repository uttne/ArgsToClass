namespace ArgsToClass
{
    public interface IHelpTextFormatter
    {
        string Format(RootSchema rootSchema);
        string Format(CommandSchema commandSchema);
    }
}