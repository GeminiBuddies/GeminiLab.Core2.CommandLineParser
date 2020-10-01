using System;
using System.Collections.Generic;
using System.Reflection;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public class ShortOptionCategory : DefaultCategoryBase, IOptionCategory<ShortOptionAttribute>, IConfigurable<ShortOptionConfig> {
        private          char                                      _prefix;
        private readonly Dictionary<char, OptionInDefaultCategory> _options = new Dictionary<char, OptionInDefaultCategory>();

        private ShortOptionConfig _config = null!;
        
        public bool Match(string item) {
            return item.Length >= 2 && item[0] == _prefix && item[1] != _prefix;
        }

        public int Consume(ReadOnlySpan<string> args, object target) {
            var content = args[0].AsSpan(1);
            var len = content.Length;
            var nextStringConsumed = false;

            int ptr = 0;
            while (ptr < len && _options.TryGetValue(content[ptr], out var option)) {
                if (option.Parameter == OptionParameter.None) {
                    SetMember(target, option.Target);

                    ptr += 1;
                } else if (option.Parameter == OptionParameter.Optional) {
                    string? param = null;
                    
                    if (ptr + 1 < len) {
                        param = content[(ptr + 1)..].ToString();
                    }
                    
                    SetMember(target, option.Target, param);

                    ptr = len;
                } else if (option.Parameter == OptionParameter.Required) {
                    var param = "";
                    
                    if (ptr + 1 < len) {
                        param = content[(ptr + 1)..].ToString();
                    } else if (args.Length >= 2) {
                        param = args[1];
                        nextStringConsumed = true;
                    } else {
                        throw new FoobarException();
                    }
                    
                    SetMember(target, option.Target, param);

                    ptr = len;
                } else {
                    throw new FoobarException();
                }
            }

            if (ptr < len) {
                if (!_config.IgnoreUnknownOption) throw new FoobarException();
            }
            
            return nextStringConsumed ? 2 : 1;
        }

        public IEnumerable<IOptionCategory<ShortOptionAttribute>.Option> Options {
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
