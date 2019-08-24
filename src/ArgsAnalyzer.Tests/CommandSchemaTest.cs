using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ArgsAnalyzer.Tests
{
    public class CommandSchemaTest
    {
        [Fact]
        public void EqualsTest()
        {
            var commands = new CommandSchema[2];

            for (int i = 0; i < commands.Length; ++i)
            {
                commands[i] = new CommandSchemaBuilder()
                {
                    Commands = new[]
                    {
                        new CommandSchemaBuilder()
                        {
                            Name = "command1",
                            Description = "description1",
                            PropertyInfo = typeof(CommandSchema).GetProperty(nameof(CommandSchema.PropertyInfo)),
                            Commands = new[]
                            {
                                new CommandSchemaBuilder().Build(),
                                null,
                            },
                            Options = new []
                            {
                                new OptionSchemaBuilder().Build(),
                                null,
                            }
                        }.Build(),
                        new CommandSchemaBuilder()
                        {
                            Name = "command2",
                            Description = "description2",
                            PropertyInfo = typeof(CommandSchema).GetProperty(nameof(CommandSchema.PropertyInfo)),
                            Commands = new[]
                            {
                                new CommandSchemaBuilder().Build(),
                                null,
                            },
                            Options = new []
                            {
                                new OptionSchemaBuilder().Build(),
                                null,
                            }
                        }.Build(),
                        new CommandSchemaBuilder().Build(),
                        null,
                    },
                    Options = new[]
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
