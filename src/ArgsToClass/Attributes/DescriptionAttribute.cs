using System;

namespace ArgsToClass.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DescriptionAttribute : Attribute
    {
        public string Description { get; }
        public string OneLineDescription { get; }

        public DescriptionAttribute(string description,string oneLineDescription=null)
        {
            Description = description;
            OneLineDescription = oneLineDescription;
        }
    }
}