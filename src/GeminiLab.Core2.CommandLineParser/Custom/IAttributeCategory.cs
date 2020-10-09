using System.Collections.Generic;
using System.Reflection;

namespace GeminiLab.Core2.CommandLineParser.Custom {
    public interface IAttributeCategory<TAttribute> where TAttribute : ParsingAttribute {
        public class MemberWithAttribute {
            public TAttribute Attribute { get; set; }
            public MemberInfo Target    { get; set; }

            public MemberWithAttribute(TAttribute attribute, MemberInfo target) {
                Attribute = attribute;
                Target = target;
            }
        }

        IEnumerable<MemberWithAttribute> Options { set; }
    }
}
