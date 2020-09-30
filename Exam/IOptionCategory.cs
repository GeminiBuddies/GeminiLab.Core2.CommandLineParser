using System.Collections.Generic;
using System.Reflection;

namespace Exam {
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
