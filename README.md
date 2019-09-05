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
## Option
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


## Switch
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

