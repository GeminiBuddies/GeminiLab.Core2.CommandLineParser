using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public class NonOptionArgumentCategory : DefaultCategoryBase, IOptionCategory<NonOptionArgumentAttribute> {
        private MemberInfo? _memberInfo;
        
        public int TryConsume(Span<string> args, object target) {
            if (_memberInfo == null) {
                return 0;
            }
            
            SetMember(target, _memberInfo, args[0]);
            return 1;
        }

        public IEnumerable<IOptionCategory<NonOptionArgumentAttribute>.MemberWithAttribute> Options {
            set => _memberInfo = value.FirstOrDefault()?.Target;
        }
    }
}
