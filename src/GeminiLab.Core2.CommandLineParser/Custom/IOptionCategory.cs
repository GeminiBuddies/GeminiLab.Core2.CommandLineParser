using System.Collections.Generic;
using System.Reflection;

namespace GeminiLab.Core2.CommandLineParser.Custom {
    public interface IOptionCategory<TOptionAttribute> : IOptionCategoryBase where TOptionAttribute : OptionAttribute {
        public class Option {
            public TOptionAttribute Attribute { get; set; }
            public MemberInfo       Target    { get; set; }

            public Option(TOptionAttribute attribute, MemberInfo target) {
                Attribute = attribute;
                Target = target;
            }
        }

        IEnumerable<Option> Options { set; }
    }
}
