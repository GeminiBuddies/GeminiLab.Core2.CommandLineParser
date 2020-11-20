using System.Collections.Generic;
using System.Reflection;

namespace GeminiLab.Core2.CommandLineParser.Custom {
    public class AttributedMember<TAttribute> where TAttribute : ParsingAttribute {
        public TAttribute Attribute { get; set; }
        public MemberInfo Target    { get; set; }

        public AttributedMember(TAttribute attribute, MemberInfo target) {
            Attribute = attribute;
            Target = target;
        }

        public void Deconstruct(out TAttribute attribute, out MemberInfo target) => (attribute, target) = (Attribute, Target);
    }

    public interface IAttributeCategory<TAttribute> where TAttribute : ParsingAttribute {
        void SetAttributedMembers(IEnumerable<AttributedMember<TAttribute>> members);
    }
}
