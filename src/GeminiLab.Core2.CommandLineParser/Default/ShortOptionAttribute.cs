using System;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser.Default {
    [AttributeUsage(SupportedTargets, AllowMultiple = true)]
    public class ShortOptionAttribute : OptionAttribute {
        public ShortOptionAttribute(char option, OptionParameter parameter = OptionParameter.None) {
            Option = option;
            Parameter = parameter;
        }

        public char            Option    { get; set; }
        public OptionParameter Parameter { get; set; }
        public string?         Default   { get; set; } = null;
    }
}
