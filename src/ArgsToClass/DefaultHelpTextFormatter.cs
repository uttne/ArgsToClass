using System.Linq;
using System.Text;

namespace ArgsToClass
{
    public class DefaultHelpTextFormatter : IHelpTextFormatter
    {
        public string Format(RootSchema rootSchema)
        {
            var sb = new StringBuilder();

            if (rootSchema.Options.Count != 0)
            {
                sb.AppendLine("options:");

                var optionTexts = rootSchema.Options.Select(GetOptionText).ToArray();
                var optionTextMaxLength = optionTexts.Max(x => x.optionText.Length);
                var optionFormat = $" {{0,-{optionTextMaxLength + 2}}}{{1}}";
                foreach (var (optionText, description) in optionTexts)
                {
                    sb.AppendLine(string.Format(optionFormat, optionText, description));
                }
            }

            if (rootSchema.Commands.Count != 0)
            {
                sb.AppendLine("command:");

                var commandTextMaxLength = rootSchema.Commands.Max(x => x.Name.Length);
                var commandFormat = $" {{0,-{commandTextMaxLength + 2}}}{{1}}";
                foreach (var command in rootSchema.Commands)
                {
                    sb.AppendLine(string.Format(commandFormat, command.Name, command.Description));
                }
            }
            

            return sb.ToString();
        }

        public string Format(CommandSchema commandSchema)
        {
            var sb = new StringBuilder();

            if (commandSchema.Options.Count != 0)
            {
                sb.AppendLine("options:");

                var optionTexts = commandSchema.Options.Select(GetOptionText).ToArray();
                var optionTextMaxLength = optionTexts.Max(x => x.optionText.Length);
                var optionFormat = $" {{0,-{optionTextMaxLength + 2}}}{{1}}";
                foreach (var (optionText, description) in optionTexts)
                {
                    sb.AppendLine(string.Format(optionFormat, optionText, description));
                }
            }

            if (commandSchema.Commands.Count != 0)
            {
                sb.AppendLine("command:");

                var commandTextMaxLength = commandSchema.Commands.Max(x => x.Name.Length);
                var commandFormat = $" {{0,-{commandTextMaxLength + 2}}}{{1}}";
                foreach (var command in commandSchema.Commands)
                {
                    sb.AppendLine(string.Format(commandFormat, command.Name, command.Description));
                }
            }

            return sb.ToString();
        }

        public static (string optionText, string description) GetOptionText(OptionSchema optionSchema)
        {
            return (
                optionSchema.ShortName.HasValue
                    ? $"-{optionSchema.ShortName.Value}|--{optionSchema.LongName}"
                    : $"--{optionSchema.LongName}", optionSchema.Description);
        }
    }
}