using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ArgsToClass.Tests
{
    public class CommandSchemaTest
    {
        [Fact]
        public void EqualsTest()
        {
            var commands = new SubCommandSchema[2];

            for (int i = 0; i < commands.Length; ++i)
            {
                commands[i] = new SubCommandSchemaBuilder()
                {
                    Commands = new List<SubCommandSchema>()
                    {
                        new SubCommandSchemaBuilder()
                        {
                            Name = "command1",
                            Description = "description1",
                            PropertyInfo = typeof(SubCommandSchema).GetProperty(nameof(SubCommandSchema.PropertyInfo)),
                            Commands = new List<SubCommandSchema>()
                            {
                                new SubCommandSchemaBuilder().Build(),
                                null,
                            },
                            Options = new List<OptionSchema>()
                            {
                                new OptionSchemaBuilder().Build(),
                                null,
                            }
                        }.Build(),
                        new SubCommandSchemaBuilder()
                        {
                            Name = "command2",
                            Description = "description2",
                            PropertyInfo = typeof(SubCommandSchema).GetProperty(nameof(SubCommandSchema.PropertyInfo)),
                            Commands = new List<SubCommandSchema>()
                            {
                                new SubCommandSchemaBuilder().Build(),
                                null,
                            },
                            Options = new List<OptionSchema>()
                            {
                                new OptionSchemaBuilder().Build(),
                                null,
                            }
                        }.Build(),
                        new SubCommandSchemaBuilder().Build(),
                        null,
                    },
                    Options = new List<OptionSchema>()
                    {
                        new OptionSchemaBuilder()
                        {
                            ShortName = 'a',
                            LongName = "aaaa",
                            Description = "bbbb",
                            PropertyInfo = typeof(OptionSchema).GetProperty(nameof(OptionSchema.PropertyInfo)),
                        }.Build(),
                        new OptionSchemaBuilder()
                        {
                            ShortName = 'x',
                            LongName = "yyyy",
                            Description = "zzzz",
                            PropertyInfo = typeof(OptionSchema).GetProperty(nameof(OptionSchema.PropertyInfo)),
                        }.Build(),
                        new OptionSchemaBuilder().Build(),
                        null
                    }
                }.Build();
            }

            Assert.True(commands[0] == commands[1]);
        }
    }
}
