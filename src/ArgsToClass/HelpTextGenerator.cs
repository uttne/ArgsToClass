using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ArgsToClass
{
    /// <summary>
    /// Help text generator.
    /// </summary>
    /// <typeparam name="TOption"></typeparam>
    public class HelpTextGenerator<TOption>
    where TOption:class,new()
    {
        private readonly IHelpTextFormatter _helpTextFormatter;
        private readonly RootSchema _rootSchema;

        public HelpTextGenerator(IHelpTextFormatter helpTextFormatter = null)
        {
            _helpTextFormatter = helpTextFormatter ?? new DefaultHelpTextFormatter();
            var schemaParser = new SchemaParser<TOption>();

            _rootSchema = schemaParser.Parse();
        }

        /// <summary>
        /// Get help text for root option.
        /// </summary>
        /// <param name="argsData">Parse result</param>
        /// <returns>Help text</returns>
        public string GetHelpText(IArgsData<TOption> argsData)
        {
            var schema = argsData.GetSchema();

            switch (schema)
            {
                case RootSchema rootSchema:
                    return _helpTextFormatter.Format(rootSchema);
                case CommandSchema commandSchema:
                    return _helpTextFormatter.Format(commandSchema);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get command help text.
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="argsData">Parse result</param>
        /// <param name="expression">Specifying command</param>
        /// <returns>Help text</returns>
        public string GetHelpText<TCommand>(IArgsData<TOption> argsData, Expression<Func<TOption, TCommand>> expression)
        {
            var schema = argsData.GetSchema(expression);

            switch (schema)
            {
                case RootSchema rootSchema:
                    return _helpTextFormatter.Format(rootSchema);
                case CommandSchema commandSchema:
                    return _helpTextFormatter.Format(commandSchema);
                default:
                    return null;
            }
        }


        public string GetDescription<T>(Expression<Func<TOption, T>> expression)
        {
            if (expression.Body.NodeType != ExpressionType.MemberAccess)
                return null;

            var memberExpression = (MemberExpression) expression.Body;
            var list = new List<string>();

            {
                var name = memberExpression.Member.Name;
                list.Add(name);
            }

            while (memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = (MemberExpression)memberExpression.Expression;
                var name = memberExpression.Member.Name;

                list.Insert(0, name);
            }

            SchemaBase schema = _rootSchema;
            foreach (var name in list)
            {
                var command = schema.Commands.FirstOrDefault(x => x.PropertyInfo.Name == name);
                if (command != null)
                {
                    schema = command;
                    continue;
                }

                var option = schema.Options.FirstOrDefault(x => x.PropertyInfo.Name == name);

                if (option == null)
                    return null;

                schema = option;
            }

            switch (schema)
            {
                case CommandSchema commandSchema:
                    return commandSchema.Description;
                case OptionSchema optionSchema:
                    return optionSchema.Description;
                default:
                    return null;
            }
        }
    }
}