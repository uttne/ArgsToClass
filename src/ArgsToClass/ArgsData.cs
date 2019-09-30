using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ArgsToClass
{
    internal class ArgsData<TOption> : IArgsData<TOption>
        where TOption :class,new()
    {
        private readonly HashSet<string> _hasExpressionTextHashSet;
        private readonly SchemaBase _commandSchema;
        private readonly object _command;

        public ArgsData(TOption option, HashSet<string> hasExpressionTextHashSet, IReadOnlyList<string> extra,SchemaBase commandSchema,object command)
        {
            Option = option ?? throw new ArgumentNullException(nameof(option));

            _hasExpressionTextHashSet = hasExpressionTextHashSet ?? throw new ArgumentNullException(nameof(hasExpressionTextHashSet));
            _commandSchema = commandSchema;
            _command = command;

            Extra = extra ?? new string[0];
        }

        public TOption Option { get; }
        public IReadOnlyList<string> Extra { get; }

        public static string ExpressionToString<T>(Expression<Func<TOption, T>> propExpression)
        {
            if (propExpression.Body.NodeType != ExpressionType.MemberAccess)
                return "";

            var memberExpression = (MemberExpression)propExpression.Body;

            var stringBuilder = new StringBuilder();

            {
                var name = memberExpression.Member.Name;
                stringBuilder.Append(".");
                stringBuilder.Append(name);
            }

            while (memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = (MemberExpression)memberExpression.Expression;
                var name = memberExpression.Member.Name;
                
                stringBuilder.Insert(0, name);
                stringBuilder.Insert(0, ".");
            }

            return stringBuilder.ToString();
        }

        public bool Has<T>(Expression<Func<TOption, T>> propExpression)
        {
            var expressionText = ExpressionToString(propExpression);

            return _hasExpressionTextHashSet.Contains(expressionText);
        }

        public (SchemaBase schema, object command) GetCommand()
        {
            return (_commandSchema, _command);
        }
    }
}