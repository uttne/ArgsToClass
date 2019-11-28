using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly SchemaBase _rootSchema;

        public ArgsData(TOption option, HashSet<string> hasExpressionTextHashSet, IReadOnlyList<string> extra,
            SchemaBase commandSchema, object command, SchemaBase rootSchema)
        {
            MainCommand = option ?? throw new ArgumentNullException(nameof(option));

            _hasExpressionTextHashSet = hasExpressionTextHashSet ?? throw new ArgumentNullException(nameof(hasExpressionTextHashSet));
            _commandSchema = commandSchema;
            _command = command;
            _rootSchema = rootSchema;

            Extra = extra ?? new string[0];
        }

        public TOption MainCommand { get; }
        public object Command { get{throw new NotImplementedException();} }
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
        
        public SchemaBase GetSchema<T>(Expression<Func<TOption, T>> propExpression = null)
        {
            if (propExpression == null)
                return _rootSchema;

            var expressionText = ExpressionToString(propExpression);

            var propertyNames = expressionText.Split('.').Skip(1).ToArray();


            SchemaBase GetSchemaBase(SchemaBase schemaBase, string propertyName)
            {
                if (schemaBase == null)
                    return null;

                foreach (var commandSchema in schemaBase.Commands)
                {
                    if (commandSchema.PropertyInfo.Name == propertyName)
                    {
                        return commandSchema;
                    }
                }

                foreach (var optionSchema in schemaBase.Options)
                {
                    if (optionSchema.PropertyInfo.Name == propertyName)
                    {
                        return optionSchema;
                    }
                }

                return null;
            }
            
            var schema = _rootSchema;
            
            foreach (var propertyName in propertyNames)
            {
                schema = GetSchemaBase(schema, propertyName);
                if(schema == null)
                    break;
            }

            return schema;
        }

        public SchemaBase GetSchema()
        {
            return _rootSchema;
        }
    }
}