using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Xunit;

namespace ArgsToClass.Tests
{
    class CommandSchemaBuilder : IEnumerable<SchemaBase>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public List<CommandSchema> Commands { get; set; }

        public List<OptionSchema> Options { get; set; }
        public CommandSchema Build()
        {
            return new CommandSchema(Name, Description, PropertyInfo, Commands, Options);
        }

        public void Add(SchemaBase schema)
        {
            if (schema is CommandSchema command)
            {
                if(Commands == null)
                    Commands = new List<CommandSchema>();
                Commands.Add(command);
            }
            else if (schema is OptionSchema option)
            {
                if(Options == null)
                    Options = new List<OptionSchema>();
                Options.Add(option);
            }
        }

        public IEnumerator<SchemaBase> GetEnumerator()
        {
            return (Options ?? new List<OptionSchema>())
                .OfType<SchemaBase>()
                .Concat(Commands ?? new List<CommandSchema>())
                .ToList()
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
            return new OptionSchema(ShortName, LongName, Description, PropertyInfo);
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
                Name = "0",
                Commands = new List<CommandSchema>() { 
                    new CommandSchemaBuilder()
                    {
                        Name = "1",
                        Commands = new List<CommandSchema>()
                        {
                            new CommandSchemaBuilder()
                            {
                                Name = "2",
                                Commands = new List<CommandSchema>()
                                {
                                    new CommandSchemaBuilder()
                                    {
                                        Name = "3",
                                        Commands = new List<CommandSchema>()
                                    }.Build(),
                                    new CommandSchemaBuilder()
                                    {
                                        Name = "4",
                                    }.Build(),
                                    null
                                }
                            }.Build(),
                            new CommandSchemaBuilder()
                            {
                                Name = "5",
                            }.Build(),
                            null
                        }
                    }.Build(),
                    new CommandSchemaBuilder()
                    {
                        Name = "6",
                        Commands = new List<CommandSchema>()
                    }.Build(),
                    new CommandSchemaBuilder()
                    {
                        Name = "7",
                    }.Build(),
                    null
                    ,
                    }
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
        [InlineData("option", "option")]
        [InlineData("TestOption", "test-option")]
        [InlineData("testOption", "test-option")]
        [InlineData("Testoption", "testoption")]
        [InlineData("testoption", "testoption")]
        [InlineData("test-option", "test-option")]
        public void ConvertOptionNameTest(string src, string expected)
        {
            var actual = ConvertOptionName(src);
            Assert.Equal(expected, actual);
        }
    }
}
