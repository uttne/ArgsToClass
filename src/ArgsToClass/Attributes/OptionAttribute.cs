using System;

namespace ArgsToClass.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionAttribute : SchemaAttribute
    {
        /// <summary>
        /// Option property attribute
        /// </summary>
        /// <param name="shortName"></param>
        /// <param name="longName"></param>
        public OptionAttribute(char shortName = (char)0, string longName = null)
        {
            ShortName = shortName;
            LongName = longName;
        }

        public char ShortName { get; }
        public string LongName { get; }
    }
}