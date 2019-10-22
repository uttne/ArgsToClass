using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArgsToClass.Attributes;
using Xunit;

namespace ArgsToClass.Tests
{
    public class SchemaParserTest
    {
        public class OptionRoot
        {
            public string Opt1 { get; set; }
            public int Opt2 { get; set; }
            [Option()]
            public string Opt3 { get; set; }
            [OptionIgnore]
            public string Opt4 { get; set; }
            [Option()]
            [Description("Opt5 description","Opt5 one line description")]
            public Option1 Opt5 { get; set; }
            [Command()]
            [Description("Com1 description","Com1 one line description")]
            public Command1 Com1 { get; set; }
            [Command()]
            public Command2 Com2 { get; set; }

        }

        public class Option1
        {

        }

        public class Command1
        {
            public string Opt1 { get; set; }
        }

        public class Command2
        {
            public string Opt1 { get; set; }
            [Command()]
            public Command1 Com1 { get; set; }
        }

        [Fact]
        public void ParseTest()
        {
            var parser = new SchemaParser<OptionRoot>();

            var actual = parser.Parse();

            Assert.NotNull(actual);

            Assert.Equal(4, actual.Options.Count);
            Assert.Equal(2, actual.Commands.Count);

            Assert.Equal(1, actual.Commands[0].Options.Count);
            Assert.Equal(0, actual.Commands[0].Commands.Count);

            Assert.Equal(1, actual.Commands[1].Options.Count);
            Assert.Equal(1, actual.Commands[1].Commands.Count);

            Assert.Equal(1, actual.Commands[1].Commands[0].Options.Count);
            Assert.Equal(0, actual.Commands[1].Commands[0].Commands.Count);
        }

        [Fact]
        public void GetOptionSchemataTest()
        {
            var type = typeof(OptionRoot);
            var options = SchemaParser<OptionRoot>.GetOptionSchemata(type).ToArray();

            OptionSchema[] expected = {
                new OptionSchema(default,"opt1",null,null,type.GetProperty("Opt1")),
                new OptionSchema(default,"opt2",null,null,type.GetProperty("Opt2")),
                new OptionSchema(default,"opt3",null,null,type.GetProperty("Opt3")),
                new OptionSchema(default,"opt5","Opt5 description","Opt5 one line description",type.GetProperty("Opt5")),
            };
            Assert.Equal(expected, options);
        }

        [Fact]
        public void GeCommandSchemataTest()
        {
            var type = typeof(OptionRoot);
            var command = SchemaParser<OptionRoot>.GetCommandSchemata(type).ToArray();

            CommandSchema[] expected = {
                new CommandSchema("com1","Com1 description","Com1 one line description",type.GetProperty(nameof(OptionRoot.Com1)),null,new []
                {
                    new OptionSchema(default,"opt1",null,null,typeof(Command1).GetProperty(nameof(Command1.Opt1))),
                }),
                new CommandSchema("com2",null,null,type.GetProperty(nameof(OptionRoot.Com2)),new []
                    {
                        new CommandSchema("com1",null,null,typeof(Command2).GetProperty(nameof(Command2.Com1)),null,new []
                        {
                            new OptionSchema(default,"opt1",null,null,typeof(Command1).GetProperty(nameof(Command1.Opt1))),
                        }),
                    },
                    new []
                    {
                        new OptionSchema(default,"opt1",null,null,typeof(Command2).GetProperty(nameof(Command2.Opt1))),
                    }),
            };
            Assert.Equal(expected, command);
        }
    }
}
