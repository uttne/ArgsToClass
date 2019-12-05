using System.Collections.Generic;

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

        /// <summary>
        /// Extra command line arguments.
        /// </summary>
        List<string> Extra { get; set; }
    }
}