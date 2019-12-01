using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ArgsToClass.Attributes;

namespace ArgsToClass
{
    public abstract class SchemaBase
    {
        private readonly int _commandsHashCode;
        private readonly int _optionsHashCode;

        protected SchemaBase()
            :this(new SubCommandSchema[0], new OptionSchema[0])
        {
        }

        protected SchemaBase(IReadOnlyList<SubCommandSchema> commands, IReadOnlyList<OptionSchema> options)
        {
            Commands = commands ?? new SubCommandSchema[0];
            Options = options ?? new OptionSchema[0];

            _commandsHashCode =
                Commands.Aggregate(0, (code, schema) => unchecked((code * 397) ^ (schema?.GetHashCode() ?? 0)));
            _optionsHashCode =
                Options.Aggregate(0, (code, schema) => unchecked((code * 397) ^ (schema?.GetHashCode() ?? 0)));
        }

        public IReadOnlyList<SubCommandSchema> Commands { get; }
        public IReadOnlyList<OptionSchema> Options { get; }

        public IReadOnlyList<SubCommandSchema> GetAllCommands() =>
            Commands.Concat(Commands.SelectMany(GetContainAllCommands)).ToArray();

        protected static IReadOnlyList<SubCommandSchema> GetContainAllCommands(SubCommandSchema subCommand) =>
            subCommand == null
                ? new SubCommandSchema[0]
                : subCommand.Commands
                    .Where(x => x is null == false)
                    .Concat(subCommand.Commands.SelectMany(GetContainAllCommands))
                    .ToArray();

        protected IReadOnlyList<OptionSchema> GetAllOptions() =>
            Options.Concat(Commands.SelectMany(GetContainAllOptions)).ToArray();

        public static IReadOnlyList<OptionSchema> GetContainAllOptions(SubCommandSchema subCommand) =>
            subCommand == null
                ? new OptionSchema[0]
                : subCommand.Options.Concat(subCommand.Commands.SelectMany(GetContainAllOptions)).ToArray();

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

    public class CommandSchema : SchemaBase
    {
        public string Description { get; }
        public Type Type { get; }

        // Todo 実装する
        public IReadOnlyList<ExtraSchema> Extras { get; }

        public CommandSchema(string description,Type type,IReadOnlyList<SubCommandSchema> commands, IReadOnlyList<OptionSchema> options) 
            : base(commands, options)
        {
            Description = description;
            Type = type;
        }

        public static CommandSchema Create(DescriptionAttribute descriptionAttribute, Type type, IReadOnlyList<SubCommandSchema> commands, IReadOnlyList<OptionSchema> options)
        {
            var description = descriptionAttribute?.Description;
            return new CommandSchema(description, type, commands, options);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CommandSchema);
        }

        public bool Equals(CommandSchema other)
        {
            return other != null
                   && base.Equals(other)
                   && string.Equals(Description, other.Description)
                   && EqualityComparer<Type>.Default.Equals(Type, other.Type);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                return hashCode;
            }
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
            : base(new SubCommandSchema[0], new OptionSchema[0])
        {
            ShortName = shortName;
            LongName = longName;
            Description = description;
            PropertyInfo = propertyInfo;
            IsSwitch = propertyInfo?.PropertyType == typeof(bool);
        }

        public static OptionSchema Create(OptionAttribute optionAttribute,DescriptionAttribute descriptionAttribute,PropertyInfo propertyInfo)
        {
            var shortName = optionAttribute != null && optionAttribute.ShortName != '\0' ? ImmVal.Value(optionAttribute.ShortName) : default;
            var longName = optionAttribute?.LongName ?? ConvertOptionName(propertyInfo.Name);
            var description = descriptionAttribute?.Description;
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

    public sealed class SubCommandSchema : CommandSchema
    {
        public string Name { get; }
        public PropertyInfo PropertyInfo { get; }

        public SubCommandSchema(string name, string description,Type type,PropertyInfo propertyInfo,IReadOnlyList < SubCommandSchema> commands, IReadOnlyList<OptionSchema> options)
            : base(description, type, commands, options)
        {
            Name = name;
            PropertyInfo = propertyInfo;
        }

        public static SubCommandSchema Create(SubCommandAttribute subCommandAttribute,DescriptionAttribute descriptionAttribute,PropertyInfo propertyInfo , IReadOnlyList<SubCommandSchema> commands, IReadOnlyList<OptionSchema> options)
        {
            var name = subCommandAttribute?.Name ?? ConvertOptionName(propertyInfo.Name);
            var description = descriptionAttribute?.Description;
            var type = propertyInfo.PropertyType;
            return new SubCommandSchema(name, description, type, propertyInfo, commands, options);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SubCommandSchema);
        }

        public bool Equals(SubCommandSchema other)
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

    public sealed class ExtraSchema:SchemaBase
    {
        public string Description { get; }
        public PropertyInfo PropertyInfo { get; }

        public ExtraSchema(string description, PropertyInfo propertyInfo)
        :base(null,null)
        {
            Description = description;
            PropertyInfo = propertyInfo;
        }

        public static ExtraSchema Create(ExtraAttribute extraAttribute, DescriptionAttribute descriptionAttribute, PropertyInfo propertyInfo)
        {
            var description = descriptionAttribute?.Description;
            return new ExtraSchema(description,propertyInfo);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as ExtraSchema);
        }

        private bool Equals(ExtraSchema other)
        {
            return other != null
                   && base.Equals(other)
                   && Description == other.Description
                   && EqualityComparer<PropertyInfo>.Default.Equals(PropertyInfo, other.PropertyInfo);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PropertyInfo != null ? PropertyInfo.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
