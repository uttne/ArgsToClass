using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Xunit;

namespace ArgsAnalyzer.Tests
{
    class CommandSchemaBuilder
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public IReadOnlyList<CommandSchema> Commands { get; set; }
        public IReadOnlyList<OptionSchema> Options { get; set; }
        public CommandSchema Build()
        {
            return new CommandSchema(Name, Description, PropertyInfo, Commands, Options);
        }
    }

    class OptionSchemaBuilder
    {
        public char ShortName { get; set; }
        public string LongName { get; set; }
        public string Description { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public OptionSchema Build()
        {
            return new OptionSchema(ShortName,LongName,Description,PropertyInfo);
        }
    }


    public class SchemaBaseTest : SchemaBase
    {
        public class Option
        {

        }

        [Fact]
        public void GetContainAllCommands_test()
        {
            var command = new CommandSchemaBuilder()
            {
                Commands = new[]
                {
                    new CommandSchemaBuilder()
                    {
                        Commands = new []
                        {
                            new CommandSchemaBuilder()
                            {
                                Commands = new[]
                                {
                                    new CommandSchemaBuilder()
                                    {
                                        Commands = new CommandSchema[0]
                                    }.Build(), 
                                    new CommandSchemaBuilder().Build(), 
                                    null
                                }
                            }.Build(),
                            new CommandSchemaBuilder().Build(),
                            null
                        }
                    }.Build(),
                    new CommandSchemaBuilder()
                    {
                        Commands = new CommandSchema[0]
                    }.Build(),
                    new CommandSchemaBuilder().Build(),
                    null
                },
            }.Build();
            var actual = RootSchema.GetContainAllCommands(command);

            var expected = new List<CommandSchema>()
            {
                command.Commands[0],
                command.Commands[0].Commands[0],
                command.Commands[0].Commands[0].Commands[0],
                command.Commands[0].Commands[0].Commands[1],
                command.Commands[0].Commands[1],
                command.Commands[1],
                command.Commands[2],
            };

            var commandHashSet = new HashSet<CommandSchema>(expected);

            foreach (var commandSchema in actual)
            {
                Assert.True(commandHashSet.Remove(commandSchema));
            }

            Assert.Empty(commandHashSet);
        }

        [Theory]
        [InlineData("option","option")]
        [InlineData("TestOption", "test-option")]
        [InlineData("testOption", "test-option")]
        [InlineData("Testoption", "testoption")]
        [InlineData("testoption", "testoption")]
        [InlineData("test-option", "test-option")]
        public void ConvertOptionNameTest(string src,string expected)
        {
            var actual = ConvertOptionName(src);
            Assert.Equal(expected, actual);
        }
    }
}
