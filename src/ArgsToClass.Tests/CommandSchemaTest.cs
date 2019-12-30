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
