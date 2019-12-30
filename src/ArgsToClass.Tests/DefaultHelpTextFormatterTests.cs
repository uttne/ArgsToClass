using ArgsToClass.Attributes;
using Xunit;
using Xunit.Abstractions;
using System.Linq;

namespace ArgsToClass.Tests
{
    public class DefaultHelpTextFormatterTests
    {
        [Description(@"This class is test.
Help text show!")]
        public class Option
        {
            [Description("Show help text.")]
            public bool Help { get; set; }

            [Description(@"Login user name.
Please full name.")]
            [Option(shortName: 'n')]
            public string Name { get; set; }

            [Description("Login user password.")]
            [Option(shortName:'p')]
            public string Password { get; set; }

            [SubCommand]
            [Description(@"Program running command.
More description.")]
            public Run Run { get; set; }

            [SubCommand]
            [Description("Program debug command.")]
            public Debug Debug { get; set; }
        }

        public class Run
        {
            [Description("Show help text.")]
            public bool Help { get; set; }

            [Description("Run property.")]
            public string Property { get; set; }
        }

        public class Debug
        {
            [Description("Show help text.")]
            public bool Help { get; set; }

            [Description("Debug property.")]
            public string Property { get; set; }
        }

        private readonly ITestOutputHelper _outputHelper;

        public DefaultHelpTextFormatterTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void FormatWithRootTest()
        {
            var formatter = new DefaultHelpTextFormatter();
            var rootSchema = new SchemaParser<Option>().Parse();
            var text = formatter.Format(rootSchema.root, rootSchema.tree[rootSchema.root].OfType<SubCommandSchema>().ToArray());
            _outputHelper.WriteLine(text);
        }
    }
}
