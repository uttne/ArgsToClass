using System;
using System.Reflection;
using ArgsAnalyzer.Attributes;

namespace ArgsAnalyzer
{
    partial class ArgsParser<TOption>
    {
        private class Command
        {
            private Command(PropertyInfo propertyInfo, string name)
            {
                if (propertyInfo == null)
                    throw new ArgumentNullException();
                if (propertyInfo.CanWrite == false)
                    throw new ArgumentException();

                PropertyInfo = propertyInfo;

                Name = string.IsNullOrWhiteSpace(name)
                    ? Option.ConvertOptionName(propertyInfo.Name)
                    : name.ToLower();

                CommandHashKey = Name.ToLower();
            }

            public string CommandHashKey { get; }
            public string Name { get; }
            public PropertyInfo PropertyInfo { get; }
            public static Command Create(PropertyInfo propertyInfo, CommandAttribute command)
            {
                throw new NotImplementedException();
            }
        }
    }
}
