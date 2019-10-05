# ArgsParser

## Usage

```
class Option
{
    public bool Help { get; set; }
    public string Name { get; set; }
    public int Repeat { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        // args == new []{ "--name", "test name", "--repeat", "1" }

        var parser = new ArgsParser<Option>();

        IArgsData<Option> data;
        try
        {
            data = parser.Parse(args);
        }
        catch(ArgsAnalysisException ex)
        {
            Console.WriteLine(ex.ToString());
            return;
        }

        Console.WriteLine($"Help   == {data.Option.Help}");     // Help == false
        Console.WriteLine($"Name   == {data.Option.Name}");     // Name == test name
        Console.WriteLine($"Repeat == {data.Option.Repeat}");   // Repeat == 1

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
