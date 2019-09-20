using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ArgsAnalyzer.Attributes;
using ArgsAnalyzer.Exceptions;

namespace ArgsAnalyzer
{
    public readonly struct ImmVal<T>
    {
        public readonly T Value;
        public readonly bool HasValue;

        public ImmVal(T value,bool hasValue)
        {
            Value = value;
            HasValue = hasValue;
        }

        public static implicit operator T (ImmVal<T> x)
        {
            return x.Value;
        }

        public static implicit operator ImmVal<T>(T x)
        {
            return ImmVal.Value(x);
        }

        public override bool Equals(object obj)
        {
            if (obj is ImmVal<T> val)
                return Equals(val);
            return false;
        }

        public bool Equals(ImmVal<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value) && HasValue == other.HasValue;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(Value) * 397) ^ HasValue.GetHashCode();
            }
        }
    }

    public static class ImmVal
    {
        public static ImmVal<T> Value<T>(T value)
        {
            return new ImmVal<T>(value, true);
        }
    }


    public class ArgToken
    {
        public ArgToken(string arg, bool isOptionFormat,
            ImmVal<string> prefix = default,
            ImmVal<string> name = default,
            ImmVal<string> @switch = default,
            ImmVal<string> value = default)
        {
            Prefix = prefix;
            Name = name;
            Switch = @switch;
            Value = value;
            Arg = arg;
            IsOptionFormat = isOptionFormat;
        }

        public string Arg { get; }
        public bool IsOptionFormat { get; }
        public ImmVal<string> Prefix { get; }
        public ImmVal<string> Name { get; }
        public ImmVal<string> Value { get; }
        public ImmVal<string> Switch { get; }

        private static readonly Regex OptionRegex = new Regex(@"^(-{1,2}|/)([a-zA-Z0-9\-]+?)($|([+])$|(-)$|=(.*)$)", RegexOptions.Compiled);

        public static ArgToken Create(string arg)
        {
            var match = OptionRegex.Match(arg);

            if (match.Success == false)
                return new ArgToken(arg, false);

            var prefix = match.Groups[1].Success ? ImmVal.Value(match.Groups[1].Value) : default;
            var name = match.Groups[2].Success ? ImmVal.Value(match.Groups[2].Value) : default;
            var @switch =
                match.Groups[4].Success ? ImmVal.Value(match.Groups[4].Value) :
                match.Groups[5].Success ? ImmVal.Value(match.Groups[5].Value) : default;
            var value = match.Groups[6].Success ? ImmVal.Value(match.Groups[6].Value) : default;

            return new ArgToken(arg, true, prefix, name, @switch, value);
        }
    }


    public class ArgsParser
    {

        public static IReadOnlyList<TokenBase> ParseToTokens(SchemaBase schema, string[] args)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            var tokens = new List<TokenBase>();

            bool nextIsValue = false;
            foreach (var arg in args)
            {
                if (nextIsValue)
                {
                    var valueToken = new ValueToken(arg);
                    tokens.Add(valueToken);
                    nextIsValue = false;
                    continue;
                }

                var argToken = ArgToken.Create(arg);

                if (argToken.IsOptionFormat)
                {
                    var name = argToken.Name.Value.ToLower();
                    var option = schema.Options.FirstOrDefault(x =>
                        string.Equals(x.LongName, name, StringComparison.OrdinalIgnoreCase));
                    if (option != null)
                    {
                        var optionToken = OptionToken.Create(option);
                        tokens.Add(optionToken);

                        if (option.IsSwitch)
                        {
                            var switchToken = new SwitchToken(argToken.Switch != "-");
                            tokens.Add(switchToken);
                        }
                        else if (argToken.Value.HasValue)
                        {
                            var valueToken = new ValueToken(argToken.Value);
                            tokens.Add(valueToken);
                        }
                        else
                        {
                            nextIsValue = true;
                        }
                        continue;
                    }

                    if (argToken.Prefix == "-" || argToken.Prefix == "/")
                    {
                        var options = schema.Options
                            .Where(x => x.IsSwitch)
                            .Where(x => x.ShortName.HasValue)
                            .Where(x => 0 <= name.IndexOf(char.ToLower(x.ShortName.Value)))
                            .ToArray();
                        foreach (var optionSchema in options)
                        {
                            var optionToken = OptionToken.Create(optionSchema);
                            tokens.Add(optionToken);
                            var switchToken = new SwitchToken(true);
                            tokens.Add(switchToken);
                        }
                    }
                }
                else
                {
                    var command = schema.Commands.FirstOrDefault(x => string.Equals(x.Name, arg, StringComparison.OrdinalIgnoreCase));
                    if (command != null)
                    {
                        tokens.Add(CommandToken.Create(command));
                        schema = command;
                        continue;
                    }
                }

                var extraToken = new ExtraToken(arg);
                tokens.Add(extraToken);
            }

            return tokens;
        }
    }

    public partial class ArgsParser<TOption>: ArgsParser
        where TOption : class, new()
    {
        private readonly Dictionary<string, Option> _optionDic = new Dictionary<string, Option>();
        private readonly Dictionary<string, Option> _shortOptionDic = new Dictionary<string, Option>();
        private readonly Dictionary<string, Command> _commandDic = new Dictionary<string, Command>();
        private readonly Option _defaultOption;


        public ArgsParser()
        {
            var schemaParser = new SchemaParser<TOption>();
            
            _rootSchema = schemaParser.Parse();


            var propertyInfos = typeof(TOption).GetProperties();

            var options = propertyInfos
                .Where(x => x.CanWrite)
                .Select(propertyInfo => new { propertyInfo, attributs = Attribute.GetCustomAttributes(propertyInfo) })
                .Select(x => new
                {
                    x.propertyInfo,
                    option = x.attributs.OfType<OptionAttribute>().FirstOrDefault(),
                    ignore = x.attributs.OfType<OptionIgnoreAttribute>().FirstOrDefault(),
                    command = x.attributs.OfType<CommandAttribute>().FirstOrDefault(),
                })
                .Where(x => x.ignore == null)
                .Select(x =>
                {
                    if (x.option != null && x.command != null)
                        throw new ArgsAnalysisException(x.propertyInfo);

                    var option = x.option != null ? Option.Create(x.propertyInfo, x.option) : null;
                    var parameters = x.command != null ? Command.Create(x.propertyInfo, x.command) : null;

                    return new { option, parameters };
                })
                .ToArray();

            // Eliminate duplicate options.
            foreach (var option in options.Where(x => x.option != null).Select(x => x.option))
            {
                if (option.IsDefault)
                    _defaultOption = _defaultOption == null
                        ? option
                        : throw new ArgsAnalysisException(option.PropertyInfo);
                _optionDic.Add(option.OptionHashKey, option);
                _shortOptionDic.Add(option.ShortOptionHashKey, option);
            }

            foreach (var command in options.Where(x => x.parameters != null).Select(x => x.parameters))
            {
                _commandDic.Add(command.CommandHashKey, command);
            }
        }

        private static bool IsOption(string arg, out string optionText)
        {
            optionText = null;

            var regex = new Regex("^--(.*)$", RegexOptions.Compiled);

            var match = regex.Match(arg);

            if (match.Success)
                optionText = match.Groups[1].Value.ToLower();

            return optionText != null;
        }

        private static bool IsShortOption(string arg, out string[] optionTexts)
        {
            optionTexts = null;

            var regex = new Regex("^-(.*)$", RegexOptions.Compiled);

            var match = regex.Match(arg);

            if (match.Success)
                optionTexts = match.Groups[1].Value.Select(x => x.ToString()).ToArray();

            return optionTexts != null && optionTexts.Length != 0;
        }





        private readonly RootSchema _rootSchema;




        public IReadOnlyList<TokenBase> ParseToTokens(string[] args)
        {
            SchemaBase schema = _rootSchema;
            var tokens = new List<TokenBase>();

            bool nextIsValue = false;
            foreach (var arg in args)
            {
                if (nextIsValue)
                {
                    var valueToken = new ValueToken(arg);
                    tokens.Add(valueToken);
                    nextIsValue = false;
                    continue;
                }

                var argToken = ArgToken.Create(arg);

                if (argToken.IsOptionFormat)
                {
                    var name = argToken.Name.Value.ToLower();
                    var option = schema.Options.FirstOrDefault(x =>
                        string.Equals(x.LongName, name, StringComparison.OrdinalIgnoreCase));
                    if (option != null)
                    {
                        var optionToken = OptionToken.Create(option);
                        tokens.Add(optionToken);

                        if (option.IsSwitch)
                        {
                            var switchToken = new SwitchToken(argToken.Switch != "-");
                            tokens.Add(switchToken);
                        }
                        else if (argToken.Value.HasValue)
                        {
                            var valueToken = new ValueToken(argToken.Value);
                            tokens.Add(valueToken);
                        }
                        else
                        {
                            nextIsValue = true;
                        }
                        continue;
                    }

                    if (argToken.Prefix == "-" || argToken.Prefix == "/")
                    {
                        var options = schema.Options
                            .Where(x => x.IsSwitch)
                            .Where(x => x.ShortName.HasValue)
                            .Where(x => 0 <= name.IndexOf(char.ToLower(x.ShortName.Value)))
                            .ToArray();
                        foreach (var optionSchema in options)
                        {
                            var optionToken = OptionToken.Create(optionSchema);
                            tokens.Add(optionToken);
                            var switchToken = new SwitchToken(true);
                            tokens.Add(switchToken);
                        }
                    }
                }
                else
                {
                    var command = schema.Commands.FirstOrDefault(x => string.Equals(x.Name, arg, StringComparison.OrdinalIgnoreCase));
                    if (command != null)
                    {
                        tokens.Add(CommandToken.Create(command));
                        schema = command;
                        continue;
                    }
                }

                var extraToken = new ExtraToken(arg);
                tokens.Add(extraToken);
            }

            return tokens;
        }

        public IArgsData<TOption> Parse(string[] args)
        {
            
            

            var ret = new TOption();
            var propertyInfoSet = new HashSet<PropertyInfo>();

            var defaultOption = _defaultOption;

            for (var i = 0; i < args.Length; i++)
            {
                string GetValueText()
                {
                    var index = i + 1;
                    if (0 <= index && index < args.Length)
                        return args[index];
                    return null;
                }

                var s = args[i];

                if (IsOption(s, out var optionText))
                {
                    if (_optionDic.TryGetValue(optionText, out var option) == false)
                    {
                        throw new ArgumentException($"Option '{s}' can not be specified.");
                    }

                    propertyInfoSet.Add(option.PropertyInfo);

                    var valueText = GetValueText();
                    option.Set(ref ret, valueText);

                    if (option.IsRequiredValue == false)
                        i++;
                }
                else if (IsShortOption(s, out var optionTexts))
                {
                    foreach (var text in optionTexts)
                    {
                        if (_shortOptionDic.TryGetValue(text, out var option) == false)
                        {
                            throw new ArgumentException($"Option '{text}' can not be specified.");
                        }

                        propertyInfoSet.Add(option.PropertyInfo);

                        var valueText = GetValueText();
                        option.Set(ref ret, valueText);

                        if (option.IsRequiredValue == false)
                            i++;
                    }
                }
                else
                {
                    if (defaultOption == null)
                        continue;

                    defaultOption.Set(ref ret, s);
                    propertyInfoSet.Add(defaultOption.PropertyInfo);

                    defaultOption = null;
                }
            }

            throw new NotImplementedException();
        }

        public string GetHelpText(object option)
        {
            throw new NotImplementedException();
        }
    }
}
