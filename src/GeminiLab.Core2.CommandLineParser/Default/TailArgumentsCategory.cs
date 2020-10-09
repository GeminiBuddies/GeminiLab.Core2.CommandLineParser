using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public class TailArgumentsCategory : DefaultCategoryBase, IOptionCategory<TailArgumentsAttribute>, IConfigurable<TailArgumentsConfig> {
        private MemberInfo? _memberInfo;
        private string      _tailMark = null!;
        
        public int TryConsume(Span<string> args, object target) {
            if (_memberInfo == null) {
                return 0;
            }

            if (args[0] != _tailMark) {
                return 0;
            }
            
            SetMember(target, _memberInfo, args[1..].ToArray());
            return args.Length;
        }

        public IEnumerable<IOptionCategory<TailArgumentsAttribute>.MemberWithAttribute> Options {
            set => _memberInfo = value.FirstOrDefault()?.Target;
        }

        public void Config(TailArgumentsConfig config) {
            _tailMark = config.TailMark;
        }
    }
}
