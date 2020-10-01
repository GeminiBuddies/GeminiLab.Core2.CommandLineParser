using System;
using System.Collections.Generic;
using System.Reflection;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public class LongOptionCategory : DefaultCategoryBase, IOptionCategory<LongOptionAttribute>, IConfigurable<LongOptionConfig> {
        private          string                                      _prefix  = null!;
        private readonly Dictionary<string, OptionInDefaultCategory> _options = new Dictionary<string, OptionInDefaultCategory>();

        private LongOptionConfig _config = null!;
        
        public bool Match(string item) {
            return item.Length > _prefix.Length && item.StartsWith(_prefix);
        }

        public int Consume(ReadOnlySpan<string> args, object target) {
            var content = args[0].AsSpan(_prefix.Length);
            var sepIndex = content.IndexOf(_config.ParameterSeparator.AsSpan());
            var nextStringConsumed = false;

            var optionStr = (sepIndex > 0 ? content[..sepIndex] : content).ToString();
            if (_options.TryGetValue(optionStr, out var option)) {
                if (option.Parameter == OptionParameter.None) {
                    SetMember(target, option.Target);
                } else if (option.Parameter == OptionParameter.Optional) {
                    string? param = null;

                    if (sepIndex > 0) {
                        param = content[(sepIndex + 1)..].ToString();
                    }
                    
                    SetMember(target, option.Target, param);
                } else if (option.Parameter == OptionParameter.Required) {
                    var param = "";

                    if (sepIndex > 0) {
                        param = content[(sepIndex + 1)..].ToString();
                    } else if (args.Length >= 2) {
                        param = args[1];
                        nextStringConsumed = true;
                    } else {
                        throw new FoobarException();
                    }
                    
                    SetMember(target, option.Target, param);
                } else {
                    throw new FoobarException();
                }
            } else {
                if (!_config.IgnoreUnknownOption) throw new FoobarException();
            }

            return nextStringConsumed ? 2 : 1;
        }

        public IEnumerable<IOptionCategory<LongOptionAttribute>.Option> Options {
            set {
                foreach (var option in value) {
                    _options[option.Attribute.Option] = new OptionInDefaultCategory(option.Target, option.Attribute.Parameter);
                }
            }
        }

        public void Config(LongOptionConfig config) {
            _prefix = config.Prefix;
            _config = config;
        }
    }
}
