using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ArgsToClass
{
    public interface IArgsData<TOption>
        where TOption : class, new()
    {
        TOption Option { get; }

        IReadOnlyList<string> Extra { get; }

        bool Has<T>(Expression<Func<TOption, T>> propExpression);
        
        /// <summary>
        /// Get option class schema data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propExpression">Specify the schema property to be acquired.
        /// if null, root schema is returned.</param>
        /// <returns>Returns null if schema does not exist.</returns>
        SchemaBase GetSchema<T>(Expression<Func<TOption, T>> propExpression = null);

        /// <summary>
        /// Get root option class schema data.
        /// </summary>
        /// <returns>schema</returns>
        SchemaBase GetSchema();
    }
}
