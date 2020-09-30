using System.Collections.Generic;
using System.Reflection;

namespace GeminiLab.Core2.CommandLineParser.Custom {
    public interface IOptionCategory<TOptionAttribute> : IOptionCategoryBase where TOptionAttribute : OptionAttribute {
        struct Option {
            public TOptionAttribute Attribute;
            public MemberInfo       Target;

            public Option(TOptionAttribute attribute, MemberInfo target) {
                Attribute = attribute;
                Target = target;
            }
        }

        IEnumerable<Option> Options { set; }
    }
}
