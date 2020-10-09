using System;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser.Default {
    [AttributeUsage(SupportedTargets, AllowMultiple = true)]
    public class ShortOptionAttribute : AttributeForParser {
        public ShortOptionAttribute(char option, OptionParameter parameter = OptionParameter.None) {
            Option = option;
            Parameter = parameter;
        }

        public char            Option    { get; set; }
        public OptionParameter Parameter { get; set; }
    }
}
