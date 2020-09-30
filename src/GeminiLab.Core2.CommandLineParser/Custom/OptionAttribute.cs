using System;

namespace GeminiLab.Core2.CommandLineParser.Custom {
    public abstract class OptionAttribute : Attribute {
        public const AttributeTargets SupportedTargets = AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field;
    }
}
