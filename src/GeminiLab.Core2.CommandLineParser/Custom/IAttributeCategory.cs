using System.Collections.Generic;
using System.Reflection;

namespace GeminiLab.Core2.CommandLineParser.Custom {
    public class MemberWithAttribute<TAttribute> where TAttribute : ParsingAttribute {
        public TAttribute Attribute { get; set; }
        public MemberInfo Target    { get; set; }

        public MemberWithAttribute(TAttribute attribute, MemberInfo target) {
            Attribute = attribute;
            Target = target;
        }
    }

    public interface IAttributeCategory<TAttribute> where TAttribute : ParsingAttribute {
        IEnumerable<MemberWithAttribute<TAttribute>> Options { set; }
    }
}
