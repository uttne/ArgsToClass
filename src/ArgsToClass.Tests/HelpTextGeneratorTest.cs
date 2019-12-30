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
            [Option(shortName:'h')]
            public bool Help { get; set; }
            [Option()]
            public string Path { get; set; }

            [SubCommand()]
            public Run Run { get; set; }
        }

        public class Run
        {
            [Option(shortName:'h')]
            public bool Help { get; set; }
            [Option()]
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

            var schema = rootSchema.root;
            var option = new Option();
            object command = option;
            argsDataMock.Setup(x => x.GetRootSchema())
                .Returns(()=>schema);

            var argsData = argsDataMock.Object;
            

            {
                var actual = helpText.GetHelpText(argsData);

                _testOutputHelper.WriteLine(actual);
            }

            _testOutputHelper.WriteLine("---------------------------------");

            {
                command = option.Run;
                schema = rootSchema.tree[rootSchema.root].First();
                var actual = helpText.GetHelpText(argsData);

                _testOutputHelper.WriteLine(actual);
            }
        }

        [Fact]
        public void GetHelpText_success_2()
        {
            var helpText = new HelpTextGenerator<Option>();


            var argsData = new ArgsParser<Option>().Parse(new string[0]);
            var text = helpText.GetHelpText(argsData, x => x.Run);

            Assert.NotNull(text);
            _testOutputHelper.WriteLine(text);
        }
    }
}
