using System;
using System.Collections.Generic;
using System.Text;
using ArgsToClass.Attributes;
using Xunit;

namespace ArgsToClass.Tests
{
    using ArgsParserTestSpace.Extra;
    namespace ArgsParserTestSpace.Extra
    {
        public class SubCommand0
        {
            public string Name { get; set; }
            [Extra]
            public List<string> ExtraX { get; set; }
        }

        public class SubCommand1
        {
            public string Name { get; set; }
            public string Extra { get; set; }
        }

        public class MainCommand
        {
            public string Name { get; set; }
            public List<string> Extra { get; set; }

            [SubCommand]
            public SubCommand0 Sub0 { get; set; }

            [SubCommand]
            public SubCommand1 Sub1 { get; set; }
        }
    }

    partial class ArgsParserTest
    {
        [Fact]
        public void Extra_parse_success()
        {
            var parser = new ArgsParser<MainCommand>();

            {
                var args = new[] {"extra0", "--name", "test", "extra1", "extra2"};
                var argsData = parser.Parse(args);

                Assert.Equal(new[] {"extra0", "extra1", "extra2"}, argsData.MainCommand.Extra);
            }

            {
                var args = new[] { "extra0", "--name", "test0", "sub0", "extra1", "--name", "test1", "extra2", "extra3" };
                var argsData = parser.Parse(args);

                Assert.Equal(new[] { "extra0" }, argsData.MainCommand.Extra);
                Assert.Equal(new[] { "extra1", "extra2", "extra3" }, ((SubCommand0)argsData.Command).ExtraX);
            }

            {
                var args = new[] { "extra0", "--name", "test0", "sub1", "extra1", "--name", "test1", "extra2", "extra3" };
                var argsData = parser.Parse(args);

                Assert.Equal(new[] { "extra0" }, argsData.MainCommand.Extra);
                Assert.Null(((SubCommand1)argsData.Command).Extra);
            }
        }
    }
}
