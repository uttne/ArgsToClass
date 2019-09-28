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
    }
}
