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
        [Description("Root description")]
        public class OptionRoot
        {
            public string Opt1 { get; set; }
            public int Opt2 { get; set; }
            [Option()]
            public string Opt3 { get; set; }
            [OptionIgnore]
            public string Opt4 { get; set; }
            [Option()]
            [Attributes.Description("Opt5 description")]
            public Option1 Opt5 { get; set; }
            [Command()]
            [Attributes.Description("Com1 description")]
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

            var type = typeof(OptionRoot);
            var expected = new RootSchema("Root description",typeof(OptionRoot),
                SchemaParser<OptionRoot>.GetCommandSchemata(type),
                SchemaParser<OptionRoot>.GetOptionSchemata(type)
                );
            
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetOptionSchemataTest()
        {
            var type = typeof(OptionRoot);
            var options = SchemaParser<OptionRoot>.GetOptionSchemata(type).ToArray();

            OptionSchema[] expected = {
                new OptionSchema(default,"opt1",null,type.GetProperty("Opt1")),
                new OptionSchema(default,"opt2",null,type.GetProperty("Opt2")),
                new OptionSchema(default,"opt3",null,type.GetProperty("Opt3")),
                new OptionSchema(default,"opt5","Opt5 description",type.GetProperty("Opt5")),
            };
            Assert.Equal(expected, options);
        }

        [Fact]
        public void GeCommandSchemataTest()
        {
            var type = typeof(OptionRoot);
            var command = SchemaParser<OptionRoot>.GetCommandSchemata(type).ToArray();

            CommandSchema[] expected = {
                new CommandSchema("com1","Com1 description",type.GetProperty(nameof(OptionRoot.Com1)),null,new []
                {
                    new OptionSchema(default,"opt1",null,typeof(Command1).GetProperty(nameof(Command1.Opt1))),
                }),
                new CommandSchema("com2",null,type.GetProperty(nameof(OptionRoot.Com2)),new []
                    {
                        new CommandSchema("com1",null,typeof(Command2).GetProperty(nameof(Command2.Com1)),null,new []
                        {
                            new OptionSchema(default,"opt1",null,typeof(Command1).GetProperty(nameof(Command1.Opt1))),
                        }),
                    },
                    new []
                    {
                        new OptionSchema(default,"opt1",null,typeof(Command2).GetProperty(nameof(Command2.Opt1))),
                    }),
            };
            Assert.Equal(expected, command);
        }
    }
}
