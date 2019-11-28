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
    class SubCommandSchemaBuilder : IEnumerable<SchemaBase>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public IList<SubCommandSchema> Commands { get; set; }

        public IList<OptionSchema> Options { get; set; }
        public Type Type { get; set; }
        public SubCommandSchema Build()
        {
            return new SubCommandSchema(Name, Description, Type ?? PropertyInfo?.PropertyType, PropertyInfo, Commands?.ToArray(), Options?.ToArray());
        }

        public string OneLineDescription { get; set; }

        public void Add(SchemaBase schema)
        {
            if (schema is SubCommandSchema command)
            {
                if(Commands == null)
                    Commands = new List<SubCommandSchema>();
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
                .Concat(Commands ?? new List<SubCommandSchema>())
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

        public string OneLineDescription { get; set; }
    }


    public class SchemaBaseTest : SchemaBase
    {
        public class Option
        {

        }

        [Fact]
        public void GetContainAllCommands_test()
        {
            var command = new SubCommandSchemaBuilder()
            {
                Name = "0",
                Commands = new List<SubCommandSchema>() { 
                    new SubCommandSchemaBuilder()
                    {
                        Name = "1",
                        Commands = new List<SubCommandSchema>()
                        {
                            new SubCommandSchemaBuilder()
                            {
                                Name = "2",
                                Commands = new List<SubCommandSchema>()
                                {
                                    new SubCommandSchemaBuilder()
                                    {
                                        Name = "3",
                                        Commands = new List<SubCommandSchema>()
                                    }.Build(),
                                    new SubCommandSchemaBuilder()
                                    {
                                        Name = "4",
                                    }.Build(),
                                    null
                                }
                            }.Build(),
                            new SubCommandSchemaBuilder()
                            {
                                Name = "5",
                            }.Build(),
                            null
                        }
                    }.Build(),
                    new SubCommandSchemaBuilder()
                    {
                        Name = "6",
                        Commands = new List<SubCommandSchema>()
                    }.Build(),
                    new SubCommandSchemaBuilder()
                    {
                        Name = "7",
                    }.Build(),
                    null
                    ,
                    }
            }.Build();
            var actual = CommandSchema.GetContainAllCommands(command);

            var expected = new List<SubCommandSchema>()
            {
                command.Commands[0],
                command.Commands[0].Commands[0],
                command.Commands[0].Commands[0].Commands[0],
                command.Commands[0].Commands[0].Commands[1],
                command.Commands[0].Commands[1],
                command.Commands[1],
                command.Commands[2],
            };

            var commandHashSet = new HashSet<SubCommandSchema>(expected);

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
