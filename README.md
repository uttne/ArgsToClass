# ArgsToClass

## Description
Parse command line arguments and create class data from it.

## Build status
|Status|
|:----:|
|[![CircleCI](https://circleci.com/gh/uttne/ArgsToClass/tree/master.svg?style=svg)](https://circleci.com/gh/uttne/ArgsToClass/tree/master)|

## Usage

```C#
[Description(@"Sample option description.
More description.")]
public class Option
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
```
## Option format
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

## Switch format
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
