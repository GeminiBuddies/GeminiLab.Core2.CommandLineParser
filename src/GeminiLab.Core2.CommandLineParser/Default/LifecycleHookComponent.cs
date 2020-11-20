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

        IEnumerable<IAttributeCategory<PreParsingAttribute>.MemberWithAttribute> IAttributeCategory<PreParsingAttribute>.Options {
            set { _pre = value.Select(mwa => mwa.Target).OfType<MethodInfo>().FirstOrDefault(); }
        }

        IEnumerable<IAttributeCategory<PostParsingAttribute>.MemberWithAttribute> IAttributeCategory<PostParsingAttribute>.Options {
            set { _post = value.Select(mwa => mwa.Target).OfType<MethodInfo>().FirstOrDefault(); }
        }
    }
}
