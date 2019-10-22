using ArgsToClass.Attributes;
using Xunit;
using Xunit.Abstractions;

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

            [Command]
            [Description(@"Program running command.
More description.")]
            public Run Run { get; set; }

            [Command]
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
            var text = formatter.Format(rootSchema);
            _outputHelper.WriteLine(text);
        }
    }
}
