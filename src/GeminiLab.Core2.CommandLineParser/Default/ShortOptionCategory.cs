using System;
using System.Collections.Generic;
using GeminiLab.Core2.CommandLineParser.Custom;
using GeminiLab.Core2.CommandLineParser.Util;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public class ShortOptionCategory : DefaultCategoryBase, IOptionCategory<ShortOptionAttribute>, IConfigurable<ShortOptionConfig> {
        private          char                                      _prefix;
        private readonly Dictionary<char, OptionInDefaultCategory> _options = new Dictionary<char, OptionInDefaultCategory>();

        private ShortOptionConfig _config = null!;

        public int TryConsume(Span<string> args, object target) {
            if (args[0].Length <= 1 || args[0][0] != _prefix || args[0][1] == _prefix) {
                return 0;
            }

            var content = args[0].AsSpan(1);
            var len = content.Length;
            var nextStringConsumed = false;

            int ptr = 0;
            while (ptr < len && _options.TryGetValue(content[ptr], out var option)) {
                if (option.Parameter == OptionParameter.Optional) {
                    string? param = null;

                    if (ptr + 1 < len) {
                        param = content[(ptr + 1)..].ToString();
                    }

                    MemberAccessor.SetMember(option.Target, target, param);

                    ptr = len;
                } else if (option.Parameter == OptionParameter.Required) {
                    string param;

                    if (ptr + 1 < len) {
                        param = content[(ptr + 1)..].ToString();
                    } else if (args.Length >= 2) {
                        param = args[1];
                        nextStringConsumed = true;
                    } else {
                        throw new DefaultException();
                    }

                    MemberAccessor.SetMember(option.Target, target, param);

                    ptr = len;
                } else { // if (option.Parameter == OptionParameter.None) 
                    MemberAccessor.SetMember(option.Target, target);

                    ptr += 1;
                }
            }

            if (ptr < len) {
                args[0] = args[0][ptr..];
                return 0;
            }

            return nextStringConsumed ? 2 : 1;
        }

        public IEnumerable<IOptionCategory<ShortOptionAttribute>.MemberWithAttribute> Options {
            set {
                foreach (var option in value) {
                    _options[option.Attribute.Option] = new OptionInDefaultCategory(option.Target, option.Attribute.Parameter);
                }
            }
        }

        public void Config(ShortOptionConfig config) {
            _prefix = config.Prefix[0];
            _config = config;
        }
    }
}
