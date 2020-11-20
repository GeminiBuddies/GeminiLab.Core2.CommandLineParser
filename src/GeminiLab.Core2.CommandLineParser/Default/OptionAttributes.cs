using System;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser.Default {
    [AttributeUsage(SupportedTargets, AllowMultiple = true)]
    public class ShortOptionAttribute : ParsingAttribute {
        public ShortOptionAttribute(char option) {
            Option = option;
        }

        public char Option { get; set; }
    }

    [AttributeUsage(SupportedTargets, AllowMultiple = true)]
    public class LongOptionAttribute : ParsingAttribute {
        public LongOptionAttribute(string option) {
            Option = option;
        }

        public string Option { get; set; }
    }

    [AttributeUsage(SupportedTargets, AllowMultiple = true)]
    public class SwitchAttribute : ParsingAttribute { }

    [AttributeUsage(SupportedTargets, AllowMultiple = true)]
    public class ParameterRequiredAttribute : ParsingAttribute { }

    [AttributeUsage(SupportedTargets, AllowMultiple = true)]
    public class ParameterOptionalAttribute : ParsingAttribute { }
}
