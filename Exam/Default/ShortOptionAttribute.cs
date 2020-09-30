using System;

namespace Exam.Default {
    [AttributeUsage(SupportedTargets, AllowMultiple = true)]
    public class ShortOptionAttribute : OptionAttribute {
        public ShortOptionAttribute(char option) {
            Option = option;
        }

        public char Option { get; set; }
        public OptionParameter Parameter { get; set; } = OptionParameter.None;
    }
}
