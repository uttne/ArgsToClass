# ArgsParser

## Usage

```

class Option
{
    public bool Help { get; set; }
}


var parser = new ArgsParser<Option>();

var option = parser.Parse(args);
```
