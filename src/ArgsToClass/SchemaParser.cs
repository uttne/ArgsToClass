﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ArgsToClass.Attributes;

namespace ArgsToClass
{
    internal static class SchemaParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Note that it is a recursive function via <see cref="GetSubCommandSchema"/>.
        /// </remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IReadOnlyList<SubCommandSchema> GetSubCommandSchemata(Type type) =>
            type.GetProperties()
                .Where(prop => prop.CanWrite)
                .Select(GetSchemaAttribute)
                .Where(x => x.schema is SubCommandAttribute)
                .Select(x => x.propInfo.PropertyType.IsClass ? x : throw new InvalidOperationException($"'{x.propInfo.PropertyType.FullName}' is not class."))
                .Select(GetSubCommandSchema)
                .ToArray();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IReadOnlyList<OptionSchema> GetOptionSchemata(Type type) =>
            type.GetProperties()
                .Where(prop => prop.CanWrite)
                .Select(GetSchemaAttribute)
                .Where(x =>
                    x.schema is OptionAttribute
                    || (
                        x.schema is null
                        && (
                            string.Equals(x.propInfo.Name, nameof(ICommand.Extra), StringComparison.OrdinalIgnoreCase)
                            && typeof(List<string>).IsAssignableFrom(x.propInfo.PropertyType)
                            ) == false
                        )
                    )
                .Select(GetOptionSchema)
                .ToArray();

        public static IReadOnlyList<ExtraSchema> GetExtraSchemata(Type type) =>
            type.GetProperties()
                .Where(prop => prop.CanWrite)
                .Select(GetSchemaAttribute)
                .Where(x => x.schema is ExtraAttribute
                            || (x.schema is null
                                && (
                                    string.Equals(x.propInfo.Name, nameof(ICommand.Extra), StringComparison.OrdinalIgnoreCase)
                                    && typeof(List<string>).IsAssignableFrom(x.propInfo.PropertyType)
                                )
                            )
                )
                .Select(GetExtraSchema)
                .ToArray();

        public static SubCommandSchema GetSubCommandSchema(
            (SchemaAttribute schemaAtt, DescriptionAttribute description, PropertyInfo propInfo) set) =>
            SubCommandSchema.Create(set.schemaAtt as SubCommandAttribute, set.description, set.propInfo);

        public static OptionSchema GetOptionSchema(
            (SchemaAttribute schemaAtt, DescriptionAttribute description, PropertyInfo propInfo) set) =>
            OptionSchema.Create(set.schemaAtt as OptionAttribute, set.description, set.propInfo);

        public static ExtraSchema GetExtraSchema(
            (SchemaAttribute schemaAtt, DescriptionAttribute description, PropertyInfo propInfo) set) =>
            ExtraSchema.Create(set.schemaAtt as ExtraAttribute, set.description, set.propInfo);

        public static (SchemaAttribute schema, DescriptionAttribute description, PropertyInfo propInfo) GetSchemaAttribute(PropertyInfo propInfo) =>
        (
            Attribute.GetCustomAttributes(propInfo)
                .OfType<SchemaAttribute>()
                .OrderBy(att =>
                    att is OptionIgnoreAttribute ? 0 :
                    att is SubCommandAttribute ? 1 :
                    att is OptionAttribute ? 2 :
                    att is ExtraAttribute ? 3 :
                    int.MaxValue)
                .FirstOrDefault(),
            Attribute.GetCustomAttributes(propInfo)
                .OfType<DescriptionAttribute>()
                .FirstOrDefault()
            ,
            propInfo
        );

    }

    internal class SchemaParser<T>
    {
        public SchemaBase GetSchema<TResult>(CommandSchema commandSchema, Expression<Func<T, TResult>> expression)
        {
            throw new NotImplementedException();
        }

        public (CommandSchema root, CommandSchemaTree tree) Parse() 
        {
            var root = CommandSchema.Create(typeof(T));

            var dic = new Dictionary<CommandSchema, IEnumerable<CommandSchema>>();
            var queue = new Queue<CommandSchema>();

            queue.Enqueue(root);

            while(queue.Count != 0)
            {
                var command = queue.Dequeue();
                var subCommands = SchemaParser
                    .GetSubCommandSchemata(command.Type)
                    .Where(x=>dic.ContainsKey(x) == false)
                    .Cast<CommandSchema>()                    
                    .ToArray();

                dic[command] = subCommands;

                foreach(var subCommand in subCommands)
                {
                    queue.Enqueue(subCommand);
                }
            }

            var tree = CommandSchemaTree.Create(dic.Select(x => (x.Key, x.Value)));

            return (root, tree);
        }
    }
}