﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ArgsAnalyzer.Attributes;

namespace ArgsAnalyzer
{
    #region Schema

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
        private static IReadOnlyList<CommandSchema> GetCommandSchemata(Type type) =>
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

        public static CommandSchema GetCommandSchema((SchemaAttribute schemaAtt, PropertyInfo propInfo) set) =>
            CommandSchema.Create(
                set.schemaAtt as CommandAttribute, set.propInfo,
                GetCommandSchemata(set.propInfo.PropertyType),
                GetOptionSchemata(set.propInfo.PropertyType)
                );

        public static OptionSchema GetOptionSchema((SchemaAttribute schemaAtt, PropertyInfo propInfo) set) =>
            OptionSchema.Create(set.schemaAtt as OptionAttribute,set.propInfo);

        public static (SchemaAttribute schema, PropertyInfo propInfo) GetSchemaAttribute(PropertyInfo propInfo) =>
            (
                Attribute.GetCustomAttributes(propInfo)
                    .OfType<SchemaAttribute>()
                    .OrderBy(att =>
                        att is OptionIgnoreAttribute ? 0 :
                        att is CommandAttribute ? 1 :
                        att is OptionAttribute ? 2 : int.MaxValue)
                    .FirstOrDefault(),
                propInfo
            );

    }

    public abstract class SchemaBase
    {
        private readonly int _commandsHashCode;
        private readonly int _optionsHashCode;

        protected SchemaBase()
            :this(new CommandSchema[0], new OptionSchema[0])
        {
        }

        protected SchemaBase(IReadOnlyList<CommandSchema> commands, IReadOnlyList<OptionSchema> options)
        {
            Commands = commands ?? new CommandSchema[0];
            Options = options ?? new OptionSchema[0];

            _commandsHashCode =
                Commands.Aggregate(0, (code, schema) => unchecked((code * 397) ^ (schema?.GetHashCode() ?? 0)));
            _optionsHashCode =
                Options.Aggregate(0, (code, schema) => unchecked((code * 397) ^ (schema?.GetHashCode() ?? 0)));
        }

        public IReadOnlyList<CommandSchema> Commands { get; }
        public IReadOnlyList<OptionSchema> Options { get; }

        public IReadOnlyList<CommandSchema> GetAllCommands() =>
            Commands.Concat(Commands.SelectMany(GetContainAllCommands)).ToArray();

        protected static IReadOnlyList<CommandSchema> GetContainAllCommands(CommandSchema command) =>
            command == null
                ? new CommandSchema[0]
                : command.Commands
                    .Where(x => x is null == false)
                    .Concat(command.Commands.SelectMany(GetContainAllCommands))
                    .ToArray();

        protected IReadOnlyList<OptionSchema> GetAllOptions() =>
            Options.Concat(Commands.SelectMany(GetContainAllOptions)).ToArray();

        public static IReadOnlyList<OptionSchema> GetContainAllOptions(CommandSchema command) =>
            command == null
                ? new OptionSchema[0]
                : command.Options.Concat(command.Commands.SelectMany(GetContainAllOptions)).ToArray();

        public static string ConvertOptionName(string propertyName) =>
            propertyName.Aggregate("",
                    (str, c) => 
                        str.Length == 0 
                            ? ('A' <= c && c <= 'Z' ? str+(char)(c - 'A' + 'a') : str+c)
                            : ('A' <= c && c <= 'Z' ? str+"-"+(char)(c - 'A' + 'a') : str+c)
                    );

        public override bool Equals(object obj)
        {
            return Equals(obj as SchemaBase);
        }

        public bool Equals(SchemaBase other)
        {

            return other != null 
                   && Commands.SequenceEqual(other.Commands)
                   && Options.SequenceEqual(other.Options);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_commandsHashCode * 397) ^ _optionsHashCode;
            }
        }

        public static bool operator ==(SchemaBase x, SchemaBase y)
        {
            return x?.Equals(y) ?? y is null;
        }
        public static bool operator !=(SchemaBase x, SchemaBase y)
        {
            return !(x == y);
        }
    }

    public sealed class RootSchema : SchemaBase
    {
        public RootSchema(IReadOnlyList<CommandSchema> commands, IReadOnlyList<OptionSchema> options) 
            : base(commands, options)
        {
        }
    }

    public sealed class OptionSchema : SchemaBase
    {
        public ImmVal<char> ShortName { get; }
        public string LongName { get; }
        public string Description { get; }
        public PropertyInfo PropertyInfo { get; }
        public bool IsSwitch { get; }

        public OptionSchema(ImmVal<char> shortName,string longName,string description, PropertyInfo propertyInfo)
            : base(new CommandSchema[0], new OptionSchema[0])
        {
            ShortName = shortName;
            LongName = longName;
            Description = description;
            PropertyInfo = propertyInfo;
            IsSwitch = propertyInfo?.PropertyType == typeof(bool);
        }

        public static OptionSchema Create(OptionAttribute optionAttribute,PropertyInfo propertyInfo)
        {
            var shortName = optionAttribute != null ? ImmVal.Value(optionAttribute.ShortName) : default;
            var longName = optionAttribute?.LongName ?? ConvertOptionName(propertyInfo.Name);
            var description = optionAttribute?.Description;
            return new OptionSchema(shortName, longName, description, propertyInfo);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as OptionSchema);
        }

        public bool Equals(OptionSchema other)
        {
            return other != null 
                   && base.Equals(other) 
                   && ShortName == other.ShortName 
                   && string.Equals(LongName, other.LongName) 
                   && string.Equals(Description, other.Description) 
                   && EqualityComparer<PropertyInfo>.Default.Equals(PropertyInfo, other.PropertyInfo) 
                   && IsSwitch == other.IsSwitch;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ ShortName.GetHashCode();
                hashCode = (hashCode * 397) ^ (LongName != null ? LongName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PropertyInfo != null ? PropertyInfo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsSwitch.GetHashCode();
                return hashCode;
            }
        }
    }

    public sealed class CommandSchema : SchemaBase
    {
        public string Name { get; }
        public string Description { get; }
        public PropertyInfo PropertyInfo { get; }

        public CommandSchema(string name, string description,PropertyInfo propertyInfo,IReadOnlyList < CommandSchema> commands, IReadOnlyList<OptionSchema> options)
            : base(commands, options)
        {
            Name = name;
            Description = description;
            PropertyInfo = propertyInfo;
        }

        public static CommandSchema Create(CommandAttribute commandAttribute,PropertyInfo propertyInfo , IReadOnlyList<CommandSchema> commands, IReadOnlyList<OptionSchema> options)
        {
            var name = commandAttribute?.Name ?? ConvertOptionName(propertyInfo.Name);
            var description = commandAttribute?.Description;
            return new CommandSchema(name, description, propertyInfo, commands, options);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CommandSchema);
        }

        public bool Equals(CommandSchema other)
        {
            return other != null 
                   && base.Equals(other)
                   && string.Equals(Name, other.Name)
                   && string.Equals(Description, other.Description)
                   && EqualityComparer<PropertyInfo>.Default.Equals(PropertyInfo, other.PropertyInfo);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PropertyInfo != null ? PropertyInfo.GetHashCode() : 0);
                return hashCode;
            }
        }
    }


    #endregion
}