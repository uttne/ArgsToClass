using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ArgsAnalyzer.Attributes;
using ArgsAnalyzer.Exceptions;
using static System.String;

namespace ArgsAnalyzer
{
    public partial class ArgsParser<TOption>
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


        public IEnumerable<TokenBase> ParseToTokens(SchemaBase schema, string[] args)
        {
            throw new NotImplementedException();
        }

        public IArgsData<TOption> Parse(string[] args)
        {
            Regex optionRegex = new Regex(@"^(-{1,2}|/)([a-zA-Z0-9\-]+)($|=(.*)$)");

            SchemaBase schema = _rootSchema;
            var tokens = new List<TokenBase>();
            var extra = new List<string>();
            foreach (var arg in args)
            {
                var optionMatch = optionRegex.Match(arg);

                if (optionMatch.Success)
                {
                    var optionPrefix = optionMatch.Groups[1].Value;
                    var optionName = optionMatch.Groups[2].Value;


                    var option = schema.Options.FirstOrDefault(x =>
                        string.Equals(x.LongName, arg, StringComparison.OrdinalIgnoreCase));
                    if (option == null && optionPrefix != "--")
                        option = null;

                }
                else
                {
                    var prevToken = tokens.LastOrDefault();
                    if(prevToken is OptionToken prevOptionToken)
                    {
                        if (prevOptionToken.IsSwitch == false)
                        {
                            tokens.Add(new ValueToken(arg));
                            continue;
                        }
                    }

                    var command = schema.Commands.FirstOrDefault(x => string.Equals(x.Name, arg, StringComparison.OrdinalIgnoreCase));
                    if (command != null)
                    {
                        tokens.Add(CommandToken.Create(command));
                        continue;
                    }
                }


            }
            

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
