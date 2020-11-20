using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeminiLab.Core2.CommandLineParser.Custom;
using GeminiLab.Core2.CommandLineParser.Util;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public class NonOptionArgumentCategory : IOptionCategory<NonOptionArgumentAttribute> {
        private MemberInfo? _memberInfo;

        public int TryConsume(Span<string> args, object target) {
            if (_memberInfo == null) {
                return 0;
            }

            MemberAccessor.SetMember(_memberInfo, target, args[0]);
            return 1;
        }

        public void SetAttributedMembers(IEnumerable<AttributedMember<NonOptionArgumentAttribute>> members) {
            _memberInfo = members.FirstOrDefault()?.Target;
        }
    }
}
