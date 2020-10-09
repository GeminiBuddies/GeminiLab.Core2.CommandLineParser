using System;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser.Default {
    [AttributeUsage(SupportedTargets, AllowMultiple = true)]
    public class LongOptionAttribute : ParsingAttribute {
        public LongOptionAttribute(string option, OptionParameter parameter = OptionParameter.None) {
            Option = option;
            Parameter = parameter;
        }

        public string          Option    { get; set; }
        public OptionParameter Parameter { get; set; }
    }
}
