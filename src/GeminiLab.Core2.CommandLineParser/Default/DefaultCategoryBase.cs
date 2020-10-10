using System.Reflection;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public abstract class DefaultCategoryBase {
        protected class OptionInDefaultCategory {
            public OptionInDefaultCategory(MemberInfo target, OptionParameter parameter) {
                Target = target;
                Parameter = parameter;
            }

            public MemberInfo      Target    { get; set; }
            public OptionParameter Parameter { get; set; }
        }
    }
}
