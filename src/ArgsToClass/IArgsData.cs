using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ArgsToClass
{
    public interface IArgsData<TMainCommand>
        where TMainCommand : class, new()
    {
        /// <summary>
        /// Get the main command.
        /// </summary>
        TMainCommand MainCommand { get; }

        /// <summary>
        /// Get the class corresponding to the selected sub command.
        /// If there is no specified sub command, get <seealso cref="MainCommand"/>.
        /// </summary>
        object Command { get; }

        /// <summary>
        /// Extra command line arguments.
        /// </summary>
        IReadOnlyList<string> Extra { get; }

        bool Has<T>(Expression<Func<TMainCommand, T>> propExpression);
        
        /// <summary>
        /// Get option class schema data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propExpression">Specify the schema property to be acquired.
        /// if null, root schema is returned.</param>
        /// <returns>Returns null if schema does not exist.</returns>
        SchemaBase GetSchema<T>(Expression<Func<TMainCommand, T>> propExpression = null);

        /// <summary>
        /// Get root option class schema data.
        /// </summary>
        /// <returns>schema</returns>
        SchemaBase GetSchema();
    }
}
