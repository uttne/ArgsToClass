using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ArgsAnalyzer.Attributes;
using ArgsAnalyzer.Exceptions;

namespace ArgsAnalyzer
{
    partial class ArgsParser<TOption>
    {
        private class Option
        {
            private Option(PropertyInfo propertyInfo, string longOptionName, char shortOptionName)
            {
                if (propertyInfo == null)
                    throw new ArgumentNullException();
                if (propertyInfo.CanWrite == false)
                    throw new ArgumentException();

                PropertyInfo = propertyInfo;
                OptionName = string.IsNullOrWhiteSpace(longOptionName)
                    ? ConvertOptionName(propertyInfo.Name)
                    : longOptionName;

                ShortOptionName = shortOptionName == (char) 0 ? null : shortOptionName.ToString();
                
                OptionHashKey = OptionName.ToLower();
                ShortOptionHashKey = ShortOptionName?.ToLower();

                IsRequiredValue = propertyInfo.PropertyType == typeof(bool);
            }

            public string Description { get; private set; }

            public bool IsDefault { get; private set; }

            public bool IsRequiredValue { get; }

            public string OptionHashKey { get; }

            public string OptionName { get; }

            public PropertyInfo PropertyInfo { get; }

            public string ShortOptionHashKey { get; }

            public string ShortOptionName { get; }

            public static string ConvertOptionName(string propertyName)
            {
                var sb = new StringBuilder();

                foreach (var c in propertyName)
                {
                    if ('A' <= c && c <= 'Z')
                    {
                        sb.Append('-');
                        sb.Append(c - 'A' + 'a');
                        continue;
                    }

                    sb.Append(c);
                }

                return sb.ToString();
            }

            public static Option Create(PropertyInfo propertyInfo, OptionAttribute optionAttribute)
            {
                var regex = new Regex(@"^((([a-zA-Z])[|]([a-zA-Z\-]+))|([a-zA-Z])|([a-zA-Z\-]+))$", RegexOptions.Compiled);


                var optionName = optionAttribute.LongName;

                var shortOptionName = optionAttribute.ShortName;

                var description = optionAttribute.Description;
                return new Option(propertyInfo, optionName, shortOptionName)
                {
                    Description = description,
                    IsDefault = optionAttribute.IsDefault,
                };
            }
            public void Set(ref TOption target, string valueText)
            {
                var propertyType = PropertyInfo.PropertyType;

                try
                {
                    var value = Convert(propertyType, valueText);
                    PropertyInfo.SetValue(target, value);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgsAnalysisException(PropertyInfo, ex.Message, ex);
                }
            }
            private object Convert(Type type, string valueText)
            {
                object value;

                var parseMethodInfo = type.GetMethod(
                    "Parse",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new[] { typeof(string) },
                    null
                    );

                if (type == typeof(string))
                {
                    value = valueText ?? throw new ArgumentException($"Value is not found.");
                }
                else if (type == typeof(bool))
                {
                    value = true;
                }
                else if (type.IsEnum)
                {
                    try
                    {
                        value = Enum.Parse(type, valueText ?? throw new ArgumentException($"Value is not found."));
                    }
                    catch (ArgumentException ex)
                    {
                        throw new ArgumentException($"'{valueText}' is not a suitable value.", ex);
                    }
                    catch (OverflowException ex)
                    {
                        throw new ArgumentException($"'{valueText}' is outside the range.", ex);
                    }
                }
                else if (parseMethodInfo != null)
                {
                    value = parseMethodInfo.Invoke(null, new object[] { valueText });
                }
                else
                {
                    var constructorInfo = type.GetConstructor(new[] { typeof(string) });

                    if (constructorInfo == null)
                    {
                        throw new InvalidOperationException($"'{type.FullName}' has no constructor with an argument of type string.");
                    }

                    value = constructorInfo.Invoke(new object[] { valueText });
                }

                return value;
            }
        }
    }
}
