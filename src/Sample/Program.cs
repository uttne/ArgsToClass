using System;
using ArgsToClass;
using ArgsToClass.Attributes;
using ArgsToClass.Exceptions;

namespace Sample
{
    [Description(@"Sample main command description.
More description.")]
    public class MainCommand
    {
        public bool Help { get; set; }

        [Option(shortName:'n',longName:"name")]
        [Description(@"Name option description.
More description.")]
        public string Name { get; set; }

        [Option(shortName: 'r')]
        [Description("Repeat option description.")]
        public int Repeat { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // args == new string[]{"--help", "-n", "John", "--repeat", "1"};

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

            // data.MainCommand.Help   == false
            // data.MainCommand.Name   == "John"
            // data.MainCommand.Repeat == 1

            if (data.MainCommand.Help)
            {
                var helpTextGen = new HelpTextGenerator<MainCommand>();
                var helpText = helpTextGen.GetHelpText(data);
                Console.WriteLine(helpText);
            }
            else
            {
                for (var i = 0; i < data.MainCommand.Repeat; ++i)
                {
                    Console.WriteLine($"Hi! {data.MainCommand.Name}!");
                }
            }

            Console.ReadKey();
        }
    }
}
