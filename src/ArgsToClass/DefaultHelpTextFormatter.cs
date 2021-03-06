﻿using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ArgsToClass
{
    public class DefaultHelpTextFormatter : IHelpTextFormatter
    {
        public string Format(CommandSchema commandSchema, SubCommandSchema[] subCommandSchemata)
        {
            var sb = new StringBuilder();

            sb.AppendLine(commandSchema.Description);
            
            if (commandSchema.Options.Count != 0)
            {
                sb.AppendLine();
                sb.AppendLine("options:");

                var optionTexts = commandSchema.Options.Select(GetOptionText).ToArray();
                var optionTextMaxLength = optionTexts.Max(x => x.optionText.Length);
                var optionFormat = $" {{0,-{optionTextMaxLength + 2}}}{{1}}";
                foreach (var (optionText, description) in optionTexts)
                {
                    var descriptionLines = description.Replace("\r", "").Split('\n');
                    foreach (var (line, index) in descriptionLines.Select((line, index) => (line, index)))
                    {
                        sb.AppendLine(index == 0
                            ? string.Format(optionFormat, optionText, line)
                            : string.Format(optionFormat, "", line));
                    }
                }
            }

            if (subCommandSchemata != null && subCommandSchemata.Length != 0)
            {
                sb.AppendLine();
                sb.AppendLine("command:");

                var commandTextMaxLength = subCommandSchemata.Max(x => x.Name.Length);
                var commandFormat = $" {{0,-{commandTextMaxLength + 2}}}{{1}}";
                foreach (var (name, description) in subCommandSchemata.Select(x => (x.Name, x.Description ?? "")))
                {
                    var descriptionLines = description.Replace("\r", "").Split('\n');
                    foreach (var (line, index) in descriptionLines.Select((line, index) => (line, index)))
                    {
                        sb.AppendLine(index == 0
                            ? string.Format(commandFormat, name, line)
                            : string.Format(commandFormat, "", line));
                    }
                }
            }
            

            return sb.ToString();
        }

        public string Format(SubCommandSchema subCommandSchema, SubCommandSchema[] subCommandSchemata)
        {
            var sb = new StringBuilder();

            if (subCommandSchema.Options.Count != 0)
            {
                sb.AppendLine("options:");

                var optionTexts = subCommandSchema.Options.Select(GetOptionText).ToArray();
                var optionTextMaxLength = optionTexts.Max(x => x.optionText.Length);
                var optionFormat = $" {{0,-{optionTextMaxLength + 2}}}{{1}}";
                foreach (var (optionText, description) in optionTexts)
                {
                    sb.AppendLine(string.Format(optionFormat, optionText, description));
                }
            }

            if (subCommandSchemata != null && subCommandSchemata.Length != 0)
            {
                sb.AppendLine("command:");

                var commandTextMaxLength = subCommandSchemata.Max(x => x.Name.Length);
                var commandFormat = $" {{0,-{commandTextMaxLength + 2}}}{{1}}";
                foreach (var command in subCommandSchemata)
                {
                    sb.AppendLine(string.Format(commandFormat, command.Name, command.Description));
                }
            }

            return sb.ToString();
        }

        public static (string optionText, string description) GetOptionText(OptionSchema optionSchema)
        {
            return (
                (optionSchema.ShortName.HasValue
                    ? $"-{optionSchema.ShortName.Value}|--{optionSchema.LongName}"
                    : $"--{optionSchema.LongName}")
                + (optionSchema.IsSwitch ? "" : $" <{optionSchema.LongName.ToUpperInvariant()}>"),
                optionSchema.Description ?? "");
        }
    }
}