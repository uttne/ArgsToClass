using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Xunit;

namespace ArgsToClass.Tests
{
    class SubCommandSchemaBuilder : IEnumerable<SchemaBase>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public PropertyInfo PropertyInfo { get; set; }

        public IList<OptionSchema> Options { get; set; }

        public IList<ExtraSchema> Extras { get; set; }

        public Type Type { get; set; }
        public SubCommandSchema Build()
        {
            return new SubCommandSchema(Name, Description, Type ?? PropertyInfo?.PropertyType, PropertyInfo, Options?.ToArray(), Extras?.ToArray());
        }

        public string OneLineDescription { get; set; }

        public void Add(SchemaBase schema)
        {
            if (schema is OptionSchema option)
            {
                if(Options == null)
                    Options = new List<OptionSchema>();
                Options.Add(option);
            }
        }

        public IEnumerator<SchemaBase> GetEnumerator()
        {
            return (Options ?? new List<OptionSchema>())
                .OfType<SchemaBase>()
                .ToList()
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    class OptionSchemaBuilder
    {
        public char ShortName { get; set; }
        public string LongName { get; set; }
        public string Description { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public OptionSchema Build()
        {
            return new OptionSchema(ShortName, LongName, Description, PropertyInfo);
        }

        public string OneLineDescription { get; set; }
    }


    public class SchemaBaseTest : SchemaBase
    {
        public class Option
        {

        }

        [Theory]
        [InlineData("option", "option")]
        [InlineData("TestOption", "test-option")]
        [InlineData("testOption", "test-option")]
        [InlineData("Testoption", "testoption")]
        [InlineData("testoption", "testoption")]
        [InlineData("test-option", "test-option")]
        public void ConvertOptionNameTest(string src, string expected)
        {
            var actual = ConvertOptionName(src);
            Assert.Equal(expected, actual);
        }
    }
}
