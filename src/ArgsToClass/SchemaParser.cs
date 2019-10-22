using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ArgsToClass.Attributes;

namespace ArgsToClass
{
    public class SchemaParser<T>
    {
        public SchemaBase GetSchema<TResult>(RootSchema rootSchema ,Expression<Func<T,TResult>> expression)
        {
            throw new NotImplementedException();
        }

        public RootSchema Parse() =>
            new RootSchema(GetCommandSchemata(typeof(T)), GetOptionSchemata(typeof(T)));

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Note that it is a recursive function via <see cref="GetCommandSchema"/>.
        /// Todo Command が循環参照になっている場合スタックオーバーフローになると思うので適切な例外を発生するように修正する
        /// </remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IReadOnlyList<CommandSchema> GetCommandSchemata(Type type) =>
            type.GetProperties()
                .Where(prop => prop.CanWrite)
                .Select(GetSchemaAttribute)
                .Where(x => x.schema is CommandAttribute)
                .Select(x=>x.propInfo.PropertyType.IsClass ? x : throw new InvalidOperationException($"'{x.propInfo.PropertyType.FullName}' is not class."))
                .Select(GetCommandSchema)
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
                .Where(x => x.schema is OptionAttribute || x.schema is null)
                .Select(GetOptionSchema)
                .ToArray();

        public static CommandSchema GetCommandSchema(
            (SchemaAttribute schemaAtt, DescriptionAttribute description, PropertyInfo propInfo) set) =>
            CommandSchema.Create(
                set.schemaAtt as CommandAttribute, set.description, set.propInfo,
                GetCommandSchemata(set.propInfo.PropertyType),
                GetOptionSchemata(set.propInfo.PropertyType)
            );

        public static OptionSchema GetOptionSchema(
            (SchemaAttribute schemaAtt, DescriptionAttribute description, PropertyInfo propInfo) set) =>
            OptionSchema.Create(set.schemaAtt as OptionAttribute, set.description, set.propInfo);

        public static (SchemaAttribute schema, DescriptionAttribute description, PropertyInfo propInfo) GetSchemaAttribute(PropertyInfo propInfo) =>
        (
            Attribute.GetCustomAttributes(propInfo)
                .OfType<SchemaAttribute>()
                .OrderBy(att =>
                    att is OptionIgnoreAttribute ? 0 :
                    att is CommandAttribute ? 1 :
                    att is OptionAttribute ? 2 : int.MaxValue)
                .FirstOrDefault(),
            Attribute.GetCustomAttributes(propInfo)
                .OfType<DescriptionAttribute>()
                .FirstOrDefault()
            ,
            propInfo
        );

    }
}