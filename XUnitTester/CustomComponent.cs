using System;
using System.Collections.Generic;
using System.Linq;
using GeminiLab.Core2.CommandLineParser;
using GeminiLab.Core2.CommandLineParser.Custom;
using GeminiLab.Core2.CommandLineParser.Default;
using GeminiLab.Core2.CommandLineParser.Util;
using Xunit;

namespace XUnitTester {
    public class WowConfig {
        public string WowInvoker { get; set; } = WowComponent.Wow;
    }

    public class WowAttribute : ParsingAttribute { }

    public class WowComponent : IConfigurable<WowConfig>, IOptionCategory<WowAttribute> {
        public const string Wow = "WOW";

        public string WowInvoker { get; private set; } = Wow;

        private Action<object, string>? _setter = null;

        public void Config(WowConfig config) {
            WowInvoker = config.WowInvoker;
        }

        public int TryConsume(Span<string> args, object target) {
            if (args[0] == WowInvoker) {
                _setter?.Invoke(target, Wow);
                return 1;
            }

            return 0;
        }

        public void SetAttributedMembers(IEnumerable<AttributedMember<WowAttribute>> members) {
            _setter = (obj, str) => { MemberAccessor.SetMember(members.FirstOrDefault()?.Target ?? throw new InvalidOperationException(), obj, str); };
        }
    }

    public class WowOption {
        [Wow]
        public string Wow { get; set; } = null!;

        [ShortOption('z')]
        public bool Zulu { get; set; }
    }

    public static class CustomComponent {
        [Fact]
        public static void CustomComponentTest() {
            const string wowInvoker = "ready?";

            var parser = new CommandLineParser<WowOption>(false)
                .Use<OptionComponent, OptionConfig>(new OptionConfig { ShortPrefix = '/', LongPrefix = "" })
                .Use<WowComponent>()
                .Config<WowComponent, WowConfig>(new WowConfig { WowInvoker = wowInvoker });

            var wow = parser.Parse("/z", wowInvoker);

            Assert.Equal(WowComponent.Wow, wow.Wow);
            Assert.True(wow.Zulu);
        }
    }
}
