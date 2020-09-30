using System;

namespace Exam {
    public abstract class OptionAttribute : Attribute {
        public const AttributeTargets SupportedTargets = AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field;
    }
}
