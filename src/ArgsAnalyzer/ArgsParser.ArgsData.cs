using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ArgsAnalyzer
{
    public interface IArgsData<TOption>
        where TOption : class, new()
    {
        TOption Option { get; }

        bool Has<T>(Expression<Func<TOption, T>> propExpression);
    }

    partial class ArgsParser<TOption>
    {
        private class ArgsData: IArgsData<TOption>
        {
            private readonly HashSet<string> _hasExpressionTextHashSet;

            public ArgsData(TOption option,HashSet<string> hasExpressionTextHashSet)
            {
                Option = option ?? throw new ArgumentNullException(nameof(option));

                _hasExpressionTextHashSet = hasExpressionTextHashSet ?? throw new ArgumentNullException(nameof(hasExpressionTextHashSet));
            }

            public TOption Option { get; }

            public static string ExpressionToString<T>(Expression<Func<TOption, T>> propExpression)
            {
                if (propExpression.Body.NodeType != ExpressionType.MemberAccess)
                    return "";

                var memberExpression = (MemberExpression)propExpression.Body;

                var stringBuilder = new StringBuilder();

                {
                    var name = memberExpression.Member.Name;
                    stringBuilder.Append(name);
                }

                while (memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
                {
                    memberExpression = (MemberExpression)memberExpression.Expression;
                    var name = memberExpression.Member.Name;

                    stringBuilder.Insert(0, ".");
                    stringBuilder.Insert(0, name);
                }

                return stringBuilder.ToString();
            }

            public bool Has<T>(Expression<Func<TOption, T>> propExpression)
            {
                var expressionText = ExpressionToString(propExpression);

                return _hasExpressionTextHashSet.Contains(expressionText);
            }
        }
    }

}
