﻿namespace ArgsToClass
{
    internal abstract class TokenBase
    {
        public override bool Equals(object obj)
        {
            return Equals(obj as TokenBase);
        }

        protected bool Equals(TokenBase other)
        {
            return other != null;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

    internal sealed class OptionToken : TokenBase
    {
        public char ShortName { get; }
        public string LongName { get; }
        public string Description { get; }
        public bool IsSwitch { get; }
        public string Value { get; }
        public OptionToken(char shortName,string longName,string description,bool isSwitch,string value)
        {
            ShortName = shortName;
            LongName = longName;
            Description = description;
            IsSwitch = isSwitch;
            Value = value;
        }
        

        public static OptionToken Create(OptionSchema schema,string value)
        {
            return new OptionToken(schema.ShortName, schema.LongName, schema.Description, schema.IsSwitch, value);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as OptionToken);
        }

        private bool Equals(OptionToken other)
        {
            return other != null && base.Equals(other)
                && ShortName == other.ShortName 
                && string.Equals(LongName, other.LongName) 
                && string.Equals(Description, other.Description) 
                && IsSwitch == other.IsSwitch;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ShortName.GetHashCode();
                hashCode = (hashCode * 397) ^ (LongName != null ? LongName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsSwitch.GetHashCode();
                return hashCode;
            }
        }
    }

    internal sealed class CommandToken : TokenBase
    {
        public string Name { get; }
        public string Description { get; }

        public CommandToken(string name,string description)
        {
            Name = name;
            Description = description;
        }

        public static CommandToken Create(SubCommandSchema schema)
        {
            return new CommandToken(schema.Name, schema.Description);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CommandToken);
        }

        private bool Equals(CommandToken other)
        {
            return other != null && base.Equals(other)
                       && string.Equals(Name, other.Name) 
                       && string.Equals(Description, other.Description);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    internal sealed class ExtraToken : TokenBase
    {
        public string Value { get; }

        public ExtraToken(string value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ExtraToken);
        }

        private bool Equals(ExtraToken other)
        {
            return other != null
                   && base.Equals(other) 
                   && string.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }
    }
}
