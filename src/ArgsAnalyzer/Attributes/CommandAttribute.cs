using System;
using System.Collections.Generic;
using System.Text;

namespace ArgsAnalyzer.Attributes
{
    public class CommandAttribute:Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public CommandAttribute(string name = null,string description = null)
        {
            Name = name;
            Description = description;
        }
    }
}
