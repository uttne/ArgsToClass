using System.Linq;
using ArgsToClass.Attributes;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace ArgsToClass.Tests
{
    public class HelpTextGeneratorTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public class Option
        {
            [Option(description:"Show help.",shortName:'h')]
            public bool Help { get; set; }
            [Option(description:"Target path.")]
            public string Path { get; set; }

            [Command(description:"Run command.")]
            public Run Run { get; set; }
        }

        public class Run
        {
            [Option(description:"Show help.",shortName:'h')]
            public bool Help { get; set; }
            [Option(description:"Output dir path.")]
            public string Dest { get; set; }
        }

        public HelpTextGeneratorTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void GetHelpText_success()
        {
            var helpText = new HelpTextGenerator<Option>();

            var argsDataMock = new Mock<IArgsData<Option>>();
            var rootSchema = new SchemaParser<Option>().Parse();

            SchemaBase schema = rootSchema;
            var option = new Option();
            object command = option;
            argsDataMock.Setup(x => x.GetCommand())
                .Returns(()=>(schema, command));

            var argsData = argsDataMock.Object;
            

            {
                var actual = helpText.GetHelpText(argsData);

                _testOutputHelper.WriteLine(actual);
            }

            _testOutputHelper.WriteLine("---------------------------------");

            {
                command = option.Run;
                schema = rootSchema.Commands.First();
                var actual = helpText.GetHelpText(argsData);

                _testOutputHelper.WriteLine(actual);
            }
        }
    }
}
