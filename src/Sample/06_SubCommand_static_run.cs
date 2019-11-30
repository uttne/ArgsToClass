﻿using System;
using ArgsToClass;
using ArgsToClass.Attributes;
using ArgsToClass.Exceptions;

namespace Sample
{
    using SubCommandStaticRun;

    namespace SubCommandStaticRun
    {
        [Description(@"Sample main command description.
More description.")]
        class MainCommand
        {
            public bool Help { get; set; }

            [SubCommand]
            [Description("This command say hello.")]
            public Hello Hello { get; set; }

            [SubCommand]
            [Description("This command say bye.")]
            public Bye Bye{ get; set; }
            public static void Run(MainCommand command)
            {
                if (command.Help)
                {
                    var helpTextGen = new HelpTextGenerator<MainCommand>();
                    var helpText = helpTextGen.GetHelpText();
                    Console.WriteLine(helpText);
                }
                else
                {
                    Console.WriteLine("Specify a command.");
                }

                Console.ReadKey();
            }
        }

        [Description(@"Hello command long description.
More description.")]
        class Hello
        {
            public bool Help { get; set; }

            [Option(shortName: 'n', longName: "name")]
            [Description("Name option description.")]
            public string Name { get; set; }

            [Option(shortName: 'r')]
            [Description("Repeat option description.")]
            public int Repeat { get; set; }

            public static void Run(Hello command)
            {
                if (command.Help)
                {
                    var helpTextGen = new HelpTextGenerator<Hello>();
                    var helpText = helpTextGen.GetHelpText();
                    Console.WriteLine(helpText);
                }
                else
                {
                    Console.WriteLine($"Hello! {command.Name}!");
                }

                Console.ReadKey();
            }
        }

        [Description(@"Bye command long description.
More description.")]
        class Bye
        {
            public bool Help { get; set; }

            [Option(shortName: 'n', longName: "name")]
            [Description("Name option description.")]
            public string Name { get; set; }

            [Option(shortName: 'r')]
            [Description("Repeat option description.")]
            public int Repeat { get; set; }

            public static void Run(Bye command)
            {
                if (command.Help)
                {
                    var helpTextGen = new HelpTextGenerator<Bye>();
                    var helpText = helpTextGen.GetHelpText();
                    Console.WriteLine(helpText);
                }
                else
                {
                    Console.WriteLine($"Bye! {command.Name}!");
                }

                Console.ReadKey();
            }
        }
    }

    class Sample06
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
