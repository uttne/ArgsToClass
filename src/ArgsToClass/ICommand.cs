namespace ArgsToClass
{
    /// <summary>
    /// Interface for defining execution function of main command or sub command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Command execution.
        /// </summary>
        void Run();
    }
}