using System;
using System.Collections.Generic;
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
            public Option1 Opt5 { get; set; }
            [Command()]
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

    }
}
