using System;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser.Default {
    [AttributeUsage(SupportedTargets, AllowMultiple = true)]
    public class LongOptionAttribute : OptionAttribute {
        public LongOptionAttribute(string option) {
            Option = option;
        }

        public string Option { get; set; }
        public OptionParameter Parameter { get; set; } = OptionParameter.None;
    }
}
