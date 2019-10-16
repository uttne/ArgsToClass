using System.Collections.Generic;
using System.Linq;
using ArgsToClass.Attributes;
using Xunit;

namespace ArgsToClass.Tests
{
    public class ArgsParserTest
    {
        public class Epsilon
        {
            public Epsilon(string arg)
            {

            }
        }

        public class Eta
        {
            public Eta(string arg)
            {

            }

            public string Omicron { get; set; }

        }

        public class Theta
        {
            public Theta(string arg)
            {

            }

            public string Pi { get; set; }

            [Command]
            public Rho Rho { get; set; }
            
        }

        public class Iota
        {
            public Iota(string arg)
            {

            }

            public string Sigma { get; set; }

            [Command]
            public Tau Tau { get; set; }
        }

        public class Mu
        {

        }

        public class Rho
        {
            public string Upsilon { get; set; }
            public string Phi { get; set; }
            
        }

        public class Tau
        {
            public string Chi { get; set; }
            public Psi Psi { get; set; }
        }

        public class Psi
        {
            public string Omega { get; set; }
        }

        public class Option
        {
            [Option(shortName: 'a', longName: "Alpha", description: "description", isDefault: true)]
            public string Alpha { get; set; }

            [Option(shortName: 'b', longName: "Beta")]
            public int Beta { get; set; }

            [Option(shortName: 'g')]
            public bool Gamma { get; set; }

            [Option(longName:"Delta")]
            public double Delta { get; set; }

            public Epsilon Epsilon { get; set; }

            [Option(longName: "ZetaAlpha")]
            public string Zeta { get; set; }

            public string ZetaBeta { get; set; }

            [Command(name: "Eta",description:"description")]
            public Eta Eta { get; set; }

            [Command(name: "Theta")]
            public Theta Theta { get; set; }

            [Command(name: "IotaAlpha")]
            public Iota Iota { get; set; }

            [OptionIgnore]
            public string Kappa { get; set; }

            [Option]
            [OptionIgnore]
            public string Lambda { get; set; }

            [Command]
            [OptionIgnore]
            public Mu Mu { get; set; }

            public string Nu { get; } = "Nu";

            private string Xi { get; set; } = "Xi";

        }

        [Fact]
        public void Constructor()
        {
            var parser = new ArgsParser<Option>();
        }

        public class ParseToTokensTestOption
        {
            [Option(description:"help show",longName:"help",shortName:'h')]
            public bool Help { get; set; }

            [Option(description: "alpha switch", longName: "alpha", shortName: 'a')]
            public bool Alpha { get; set; }

            [Option(description: "beta switch", longName: "beta", shortName: 'b')]
            public bool Beta { get; set; }
            
            [Option(description: "gamma switch", longName: "gamma", shortName: 'g')]
            public bool Gamma { get; set; }

            [Option(description: "ab switch", longName: "ab")]
            public bool Ab { get; set; }
        }

        [Fact]
        public void ParseToTokensTest()
        {
            var schemaParser = new SchemaParser<ParseToTokensTestOption>();

            var rootSchema = schemaParser.Parse();

            string[] args = {"-help"};
            var actual = ArgsParser.ParseToTokenSchemaPairs(rootSchema, args);

            TokenBase[] expected = {
                new OptionToken('h',"help","help show",true,"true"),
            };
            
            Assert.Equal(expected, actual.Select(x=>x.Item1).ToArray());
            ;
        }

        [Fact]
        public void ParseTest()
        {
            var parser = new ArgsParser<Option>();

            string[] args = new []{"-a" , "value"};
            var actual = parser.Parse(args);

            Assert.Equal("value", actual.Option.Alpha);
        }

        [Fact]
        public void SelectOptionSchemaTest()
        {
            var schemaParser = new SchemaParser<ParseToTokensTestOption>();

            var rootSchema = schemaParser.Parse();

            {
                var schema = new[]
                {
                    rootSchema.Options.First(x => x.LongName == "alpha"),
                };

                var arg = ArgToken.Create("--alpha");

                var actual = ArgsParser.SelectOptionSchema(rootSchema,arg);

                Assert.Equal(schema,actual);
            }

            {
                var schema = new[]
                {
                    rootSchema.Options.First(x => x.LongName == "alpha"),
                };

                var arg = ArgToken.Create("-alpha");

                var actual = ArgsParser.SelectOptionSchema(rootSchema, arg);

                Assert.Equal(schema, actual);
            }

            {
                var schema = new[]
                {
                    rootSchema.Options.First(x => x.LongName == "alpha"),
                };

                var arg = ArgToken.Create("/alpha");

                var actual = ArgsParser.SelectOptionSchema(rootSchema, arg);

                Assert.Equal(schema, actual);
            }

            {
                var schema = new[]
                {
                    rootSchema.Options.First(x => x.LongName == "alpha"),
                };

                var arg = ArgToken.Create("-alpha");

                var actual = ArgsParser.SelectOptionSchema(rootSchema, arg);

                Assert.Equal(schema, actual);
            }

            {
                var schema = new[]
                {
                    rootSchema.Options.First(x => x.LongName == "alpha"),
                };

                var arg = ArgToken.Create("-a");

                var actual = ArgsParser.SelectOptionSchema(rootSchema, arg);

                Assert.Equal(schema, actual);
            }

            {
                var schema = new[]
                {
                    rootSchema.Options.First(x => x.LongName == "alpha"),
                };

                var arg = ArgToken.Create("/a");

                var actual = ArgsParser.SelectOptionSchema(rootSchema, arg);

                Assert.Equal(schema, actual);
            }
        }
    }
}
