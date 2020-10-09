using System;

namespace GeminiLab.Core2.CommandLineParser.Custom {
    public abstract class AttributeForParser : Attribute {
        public const AttributeTargets SupportedTargets = AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field;
    }
}
