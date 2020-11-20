using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public class LifecycleHookComponent : IParsingHook, IAttributeCategory<PreParsingAttribute>, IAttributeCategory<PostParsingAttribute> {
        private MethodInfo? _pre, _post;

        public void OnParsingEvent(ParsingEvent parsingEvent, object target) {
            (parsingEvent switch {
                ParsingEvent.PreParsing  => _pre,
                ParsingEvent.PostParsing => _post,
                _                        => null,
            })?.Invoke(target, Array.Empty<object>());
        }

        public void SetAttributedMembers(IEnumerable<AttributedMember<PreParsingAttribute>> members) {
            _pre = members.Select(mwa => mwa.Target).OfType<MethodInfo>().FirstOrDefault();
        }

        public void SetAttributedMembers(IEnumerable<AttributedMember<PostParsingAttribute>> members) {
            _post = members.Select(mwa => mwa.Target).OfType<MethodInfo>().FirstOrDefault();
        }
    }
}
