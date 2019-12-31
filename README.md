# ArgsToClass

## Description
Parse command line arguments and create class data from it.

## Build status
|Status|
|:----:|
|![](https://circleci.com/gh/uttne/ArgsToClass.svg?style=shield&circle-token=9f378b9751964e0a8781b0242b0a5d7667923787)|

## Usage
This library usage is berry easy.  
Simply define a class and use the corresponding options.
```C#
namespace Sample
{
    public class MainCommand
    {
        public bool Help { get; set; }
        public string Name { get; set; }
        public int Repeat { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var parser = new ArgsParser<MainCommand>();

            IArgsData<MainCommand> data;
            try
            {
                // This parses command line arguments and maps them to classes.
                // args == new string[]{"--help", "--name", "John", "--repeat", "1"};
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
        }
    }
}
```

If you need to add a description or short name to an option, use attributes.

```C#
namespace Sample
{
    [Description(@"Sample option description.
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

    public class Program
    {
        public static void Main(string[] args)
        {
            // args == new string[]{"--help", "-n", "John", "--repeat", "1"};

            var parser = new ArgsParser<Option>();
            
            IArgsData<Option> data;
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

            // data.Option.Help   == false
            // data.Option.Name   == "John"
            // data.Option.Repeat == 1

            if (data.Option.Help)
            {
                var helpTextGen = new HelpTextGenerator<Option>();
                var helpText = helpTextGen.GetHelpText(data);
                Console.WriteLine(helpText);
            }
            else
            {
                for (var i = 0; i < data.Option.Repeat; ++i)
                {
                    Console.WriteLine($"Hi! {data.Option.Name}!");
                }
            }

            Console.ReadKey();
        }
    }
}
```

You may need subcommands. In that case, define it with attributes.

```C#
namespace Sample
{
    using SubCommandExtensionRun;

    namespace SubCommandExtensionRun
    {
        [Description(@"Sample main command description.
More description.")]
        public class MainCommand
        {
            public bool Help { get; set; }

            [SubCommand]
            [Description("This command say hello.")]
            public Hello Hello { get; set; }

            [SubCommand]
            [Description("This command say bye.")]
            public Bye Bye{ get; set; }
        }

        [Description(@"Hello command long description.
More description.")]
        public class Hello
        {
            public bool Help { get; set; }

            [Option(shortName: 'n', longName: "name")]
            [Description("Name option description.")]
            public string Name { get; set; }

            [Option(shortName: 'r')]
            [Description("Repeat option description.")]
            public int Repeat { get; set; }
        }

        [Description(@"Bye command long description.
More description.")]
        public class Bye
        {
            public bool Help { get; set; }

            [Option(shortName: 'n', longName: "name")]
            [Description("Name option description.")]
            public string Name { get; set; }

            [Option(shortName: 'r')]
            [Description("Repeat option description.")]
            public int Repeat { get; set; }
        }

        public static class Extensions
        {
            public static void Run(this MainCommand command)
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

            public static void Run(this Hello command)
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

            public static void Run(this Bye command)
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

    public class Program
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

            // If you define Run as an extension method or class method, the Run command of the command specified by the argument will be called.
            data.RunCommand();
        }
    }
}

```

## Option format
The optional format supports.

### Option format with value
```
-o value
-o "value text"
-o=value
-o="value text"
-option value
-option "value text"
-option=value
-option="value text"
--option value
--option "value text"
--option=value
--option="value text"
/o value
/o "value text"
/o=value
/o="value text"
/option value
/option "value text"
/option=value
/option="value text"
```

### Switch format
```
-s
-s+
-s-
-switch
-switch+
-switch-
--switch
--switch+
--switch-
/s
/s+
/s-
/switch
/switch+
/switch-
```

Please try using!!