using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ArgsAnalyzer
{
    public interface IArgsData<TOption>
    {
        TOption Option { get; }

        bool Has<T>(Expression<Func<TOption, T>> propExpression);
    }

    partial class ArgsParser<TOption>
    {
        private class ArgsData: IArgsData<TOption>
        {
            public ArgsData(TOption option)
            {

            }

            public TOption Option { get; }
            public bool Has<T>(Expression<Func<TOption, T>> propExpression)
            {
                throw new NotImplementedException();
            }
        }
    }

}
