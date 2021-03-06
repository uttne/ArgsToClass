﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArgsToClass.Attributes;
using Xunit;

namespace ArgsToClass.Tests
{
    using SchemaParserTestSpace;
    namespace SchemaParserTestSpace
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
            [SubCommand()]
            [Attributes.Description("Com1 description")]
            public Command1 Com1 { get; set; }
            [SubCommand()]
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
            [SubCommand()]
            public Command1 Com1 { get; set; }
        }

        //-------------------------

        public class MainCommand01
        {
            [SubCommand]
            public SubCommand100 SubCommand100 { get; set; }
            public string Name { get; set; }
        }

        public class SubCommand100
        {
            [SubCommand]
            public SubCommand101 SubCommand101 { get; set; }
            public string Name { get; set; }
        }

        public class SubCommand101
        {
            [SubCommand]
            public SubCommand100 SubCommand100 { get; set; }
            public string Name { get; set; }
        }
    }

    public class SchemaParserTest
    {
        [Fact]
        public void ParseTest()
        {
            var parser = new SchemaParser<OptionRoot>();

            var actual = parser.Parse();

            var type = typeof(OptionRoot);
            var expected = new CommandSchema("Root description",typeof(OptionRoot),
                SchemaParser.GetOptionSchemata(type),
                SchemaParser.GetExtraSchemata(type)
                );
            
            Assert.Equal(expected, actual.root);
        }

        [Fact]
        public void GetOptionSchemataTest()
        {
            var type = typeof(OptionRoot);
            var options = SchemaParser.GetOptionSchemata(type).ToArray();

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
            var command = SchemaParser.GetSubCommandSchemata(type).ToArray();

            SubCommandSchema[] expected = {
                new SubCommandSchemaBuilder()
                {
                    Name = "com1",
                    Description = "Com1 description",
                    PropertyInfo = type.GetProperty(nameof(OptionRoot.Com1)),
                    Options = new[]
                    {
                        new OptionSchema(default,"opt1",null,typeof(Command1).GetProperty(nameof(Command1.Opt1))),
                    }
                }.Build(),
                new SubCommandSchemaBuilder()
                {
                    Name = "com2",
                    Description = null,
                    PropertyInfo = type.GetProperty(nameof(OptionRoot.Com2)),
                    Options = new[]
                    {
                        new OptionSchema(default,"opt1",null,typeof(Command2).GetProperty(nameof(Command2.Opt1))),
                    }
                }.Build(),
            };
            Assert.Equal(expected, command);
        }
    }
}
