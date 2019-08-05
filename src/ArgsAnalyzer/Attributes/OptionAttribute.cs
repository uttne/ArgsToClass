using System;

namespace ArgsAnalyzer.Attributes
{

    [AttributeUsage(AttributeTargets.Property)]
    public class OptionAttribute : Attribute
    {
        /// <summary>
        /// Option property attribute
        /// </summary>
        /// <param name="shortName"></param>
        /// <param name="longName"></param>
        /// <param name="description"></param>
        /// <param name="isDefault"></param>
        public OptionAttribute(char shortName = (char)0, string longName = null, string description = null, bool isDefault = false)
        {
            ShortName = shortName;
            LongName = longName;
            Description = description;
            IsDefault = isDefault;
        }

        public string Description { get; }
        public bool IsDefault { get; }
        public char ShortName { get; }
        public string LongName { get; }
    }
}