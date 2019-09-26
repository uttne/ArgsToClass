using System.Text.RegularExpressions;

namespace ArgsAnalyzer
{
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
}