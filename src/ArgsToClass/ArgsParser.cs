using System;
using System.Collections.Generic;
using System.Linq;

namespace ArgsToClass
{
    public class ArgsParser
    {
        internal static IReadOnlyList<ArgToken> ParseToArgTokens(string[] args) =>
            args.Select(ArgToken.Create)
                .ToArray();


        internal static IReadOnlyList<(TokenBase, SchemaBase[])> ParseToTokenSchemaPairs(CommandSchema schema, string[] args)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            var list = new List<(TokenBase, SchemaBase[])>();

            OptionSchema prevOption = null;
            foreach (var arg in args)
            {
                if (prevOption != null)
                {
                    var optionToken = OptionToken.Create(prevOption, arg);
                    list.Add((optionToken, new SchemaBase[] {prevOption}));
                    prevOption = null;
                    continue;
                }

                var argToken = ArgToken.Create(arg);

                if (argToken.IsOptionFormat)
                {
                    var options = SelectOptionSchema(schema, argToken);

                    if (options.Length == 1)
                    {
                        var option = options[0];
                        if (option.IsSwitch)
                        {
                            var optionToken = OptionToken.Create(option, argToken.Switch != "-" ? "true" : "false");
                            list.Add((optionToken, new SchemaBase[] {option}));
                        }
                        else if (argToken.Value.HasValue)
                        {
                            var optionToken = OptionToken.Create(option, argToken.Value.Value);
                            list.Add((optionToken, new SchemaBase[] {option}));
                        }
                        else
                        {
                            prevOption = option;
                        }
                        continue;
                    }

                    var name = argToken.Name.Value.ToLower();
                    options = schema.Options
                        .Where(x => x.IsSwitch)
                        .Where(x => x.ShortName.HasValue)
                        .Where(x => 0 <= name.IndexOf(char.ToLower(x.ShortName.Value)))
                        .ToArray();
                    foreach (var optionSchema in options)
                    {
                        var optionToken = OptionToken.Create(optionSchema, "true");
                        list.Add((optionToken, new SchemaBase[] {optionSchema}));
                    }
                }
                else
                {
                    var command = schema.Commands.FirstOrDefault(x => string.Equals(x.Name, arg, StringComparison.OrdinalIgnoreCase));
                    if (command != null)
                    {
                        list.Add((CommandToken.Create(command), new SchemaBase[] {command}));
                        schema = command;
                        continue;
                    }
                }

                var extraToken = new ExtraToken(arg);
                list.Add((extraToken, schema.Extras.Cast<SchemaBase>().ToArray()));
            }

            return list;
        }

        internal static OptionSchema[] SelectOptionSchema(CommandSchema schema, ArgToken argToken)
        {
            var name = argToken.Name.Value.ToLower();

            var option = schema.Options.FirstOrDefault(x =>
                ((argToken.Prefix == "--" || argToken.Prefix == "-" || argToken.Prefix == "/") && string.Equals(x.LongName, name, StringComparison.OrdinalIgnoreCase))
                || (x.ShortName.HasValue && (argToken.Prefix == "-" || argToken.Prefix == "/") && string.Equals(x.ShortName.Value.ToString(), name, StringComparison.OrdinalIgnoreCase))
                );

            if (option != null)
                return new[] { option };

            if ((argToken.Prefix == "-" || argToken.Prefix == "/") == false)
                return new OptionSchema[0];

            return schema.Options
                .Where(x => x.IsSwitch)
                .Where(x => x.ShortName.HasValue)
                .Where(x => 0 <= name.IndexOf(char.ToLower(x.ShortName.Value)))
                .ToArray();
        }
    }

    public class ArgsParser<TOption> : ArgsParser
        where TOption : class, new()
    {
        public ArgsParser()
        {
            var schemaParser = new SchemaParser<TOption>();

            _commandSchema = schemaParser.Parse();
        }


        private readonly CommandSchema _commandSchema;


        public IArgsData<TOption> Parse(string[] args)
        {
            var rootSchema = _commandSchema;

            var tokenSchemaPairs = ParseToTokenSchemaPairs(rootSchema, args);

            var option = CreateOption(rootSchema, tokenSchemaPairs);

            return option;
        }

        private IArgsData<TOption> CreateOption(CommandSchema rootSchema, IReadOnlyList<(TokenBase, SchemaBase[])> tokenSchemaPairs)
        {
            var option = ActivateOption();
            var hasExpressionTextHashSet = new HashSet<string>();
            string commandExpressionName = ".";
            var extra = new List<string>();

            object commandCursor = option;
            var commandSchemaCursor = rootSchema;
            var commands = new List<object> {commandCursor};
            // Key : commands index
            var extraSchemataDic = new Dictionary<int, ExtraSchema[]>();
            var extraDic = new Dictionary<int, List<string>>();

            foreach (var (token, schemata) in tokenSchemaPairs)
            {
                if (token is OptionToken optionToken && 0 <schemata.Length && schemata[0] is OptionSchema optionSchema)
                {
                    var value = ValueParser.Parse(optionSchema.PropertyInfo.PropertyType, optionToken.Value);

                    optionSchema.PropertyInfo.SetValue(commandCursor, value);

                    hasExpressionTextHashSet.Add(commandExpressionName + "." + optionSchema.PropertyInfo.Name);
                }
                else if (token is CommandToken commandToken && 0 < schemata.Length && schemata[0] is SubCommandSchema commandSchema)
                {
                    var command = Activator.CreateInstance(commandSchema.PropertyInfo.PropertyType);

                    commandSchema.PropertyInfo.SetValue(commandCursor, command);
                    commandCursor = command;
                    commands.Add(commandCursor);
                    commandExpressionName += "." + commandSchema.PropertyInfo.Name;

                    hasExpressionTextHashSet.Add(commandExpressionName);

                    commandSchemaCursor = commandSchema;
                }
                else if (token is ExtraToken extraToken && 0 < schemata.Length && schemata[0] is ExtraSchema)
                {
                    extra.Add(extraToken.Value);

                    if (extraSchemataDic.ContainsKey(commands.Count - 1) == false)
                        extraSchemataDic[commands.Count - 1] = schemata.OfType<ExtraSchema>().ToArray();
                    

                    if (extraDic.TryGetValue(commands.Count - 1, out var extraList) == false)
                    {
                        extraDic[commands.Count - 1] = extraList = new List<string>();
                    }
                    extraList.Add(extraToken.Value);
                }
            }

            foreach (var (index, extraList) in extraDic.Select(x => (index: x.Key, extraList: x.Value)))
            {
                var command = commands[index];
                foreach (var extraSchema in extraSchemataDic[index])
                {
                    var extras = extraList.ToList();
                    extraSchema.PropertyInfo.SetValue(command, extras);
                }
            }

            return new ArgsData<TOption>(option, hasExpressionTextHashSet, extra, commandSchemaCursor, commandCursor, rootSchema);
        }

        private static TOption ActivateOption()
        {
            return Activator.CreateInstance<TOption>();
        }

        public string GetHelpText(object option)
        {
            throw new NotImplementedException();
        }
    }
}
