using System;
using ArgsToClass;
using ArgsToClass.Attributes;
using ArgsToClass.Exceptions;

namespace Sample
{
    using MainCommandInstanceRun;

    namespace MainCommandInstanceRun
    {
        [Description(@"Sample main command description.
More description.")]
        class MainCommand
        {
            public bool Help { get; set; }

            [Option(shortName: 'n', longName: "name")]
            [Description(@"Name option description.
More description.")]
            public string Name { get; set; }

            [Option(shortName: 'r')]
            [Description("Repeat option description.")]
            public int Repeat { get; set; }

            public void Run()
            {
                if (Help)
                {
                    var helpTextGen = new HelpTextGenerator<MainCommand>();
                    var helpText = helpTextGen.GetHelpText();
                    Console.WriteLine(helpText);
                }
                else
                {
                    for (var i = 0; i < Repeat; ++i)
                    {
                        Console.WriteLine($"Hi! {Name}!");
                    }
                }

                Console.ReadKey();
            }
        }
    }

    class Sample01
    {
        public void Main(string[] args)
        {
            var parser = new ArgsParser<MainCommand>();

            IArgsData<MainCommand> data;
            try
            {
                // This parses command line arguments and maps them to classes.
                data = parser.Parse(args);
            }
            catch (ArgsAnalysisException ex)
            {
                Console.WriteLine(ex);
                return;
            }

            data.RunCommand();
        }
    }
}
