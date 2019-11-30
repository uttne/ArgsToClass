using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ArgsToClass
{
    /// <summary>
    /// Help text generator.
    /// </summary>
    /// <typeparam name="TMainCommand"></typeparam>
    public class HelpTextGenerator<TMainCommand>
    where TMainCommand:class,new()
    {
        private readonly IHelpTextFormatter _helpTextFormatter;
        private CommandSchema _commandSchema;

        public HelpTextGenerator(IHelpTextFormatter helpTextFormatter = null)
        {
            _helpTextFormatter = helpTextFormatter ?? new DefaultHelpTextFormatter();
        }

        /// <summary>
        /// Get help text for root option.
        /// </summary>
        /// <returns>Help text</returns>
        public string GetHelpText()
        {
            if (_commandSchema == null)
            {
                var schemaParser = new SchemaParser<TMainCommand>();
                _commandSchema = schemaParser.Parse();
            }
            
            var schema = _commandSchema;

            switch (schema)
            {
                case SubCommandSchema commandSchema:
                    return _helpTextFormatter.Format(commandSchema);
                case CommandSchema rootSchema:
                    return _helpTextFormatter.Format(rootSchema);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get help text for root option.
        /// </summary>
        /// <param name="argsData">Parse result</param>
        /// <returns>Help text</returns>
        public string GetHelpText(IArgsData<TMainCommand> argsData)
        {
            var schema = argsData.GetSchema();

            switch (schema)
            {
                case SubCommandSchema commandSchema:
                    return _helpTextFormatter.Format(commandSchema);
                case CommandSchema rootSchema:
                    return _helpTextFormatter.Format(rootSchema);
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
        public string GetHelpText<TCommand>(IArgsData<TMainCommand> argsData, Expression<Func<TMainCommand, TCommand>> expression)
        {
            var schema = argsData.GetSchema(expression);

            switch (schema)
            {
                case SubCommandSchema commandSchema:
                    return _helpTextFormatter.Format(commandSchema);
                case CommandSchema rootSchema:
                    return _helpTextFormatter.Format(rootSchema);
                default:
                    return null;
            }
        }


        public string GetDescription<T>(Expression<Func<TMainCommand, T>> expression)
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

            SchemaBase schema = _commandSchema;
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
                case SubCommandSchema commandSchema:
                    return commandSchema.Description;
                case OptionSchema optionSchema:
                    return optionSchema.Description;
                default:
                    return null;
            }
        }
    }
}