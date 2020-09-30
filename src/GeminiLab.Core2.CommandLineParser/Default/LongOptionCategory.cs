using System;
using System.Collections.Generic;
using System.Reflection;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public class LongOptionCategory : IOptionCategory<LongOptionAttribute>, IConfigurable<LongOptionConfig> {
        private          string                              _prefix          = null!;
        private readonly Dictionary<string, MemberInfo>      _targets         = new Dictionary<string, MemberInfo>();
        private readonly Dictionary<string, OptionParameter> _optionParameter = new Dictionary<string, OptionParameter>();

        private LongOptionConfig _config = null!;
        
        public bool Match(string item) {
            return item.Length > _prefix.Length && item.StartsWith(_prefix);
        }

        public int Consume(ReadOnlySpan<string> args, object target) {
            var content = args[0].AsSpan(_prefix.Length);
            var sepIndex = content.IndexOf(_config.ParameterSeparator.AsSpan());
            var nextStringConsumed = false;

            var option = sepIndex > 0 ? content[..sepIndex] : content;
            if (_targets.TryGetValue(option.ToString(), out var memberInfo)) {
                var parameter = _optionParameter[option.ToString()];

                if (parameter == OptionParameter.None) {
                    MemberSetter.SetMember(target, memberInfo);
                } else if (parameter == OptionParameter.Optional) {
                    var param = _config.DefaultValueForOptionalParameter;

                    if (sepIndex > 0) {
                        param = content[(sepIndex + 1)..].ToString();
                    }
                    
                    MemberSetter.SetMember(target, memberInfo, param);
                } else if (parameter == OptionParameter.Required) {
                    var param = "";

                    if (sepIndex > 0) {
                        param = content[(sepIndex + 1)..].ToString();
                    } else if (args.Length >= 2) {
                        param = args[1];
                        nextStringConsumed = true;
                    } else {
                        if (_config.ContinueWhenFailedToGetRequiredParameter) {
                            param = _config.DefaultValueForRequiredParameter;
                        } else {
                            throw new FoobarException();
                        }
                    }
                    
                    MemberSetter.SetMember(target, memberInfo, param);
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
                    _targets[option.Attribute.Option] = option.Target;
                    _optionParameter[option.Attribute.Option] = option.Attribute.Parameter;
                }
            }
        }

        public void Config(LongOptionConfig config) {
            _prefix = config.Prefix;
            _config = config;
        }
    }
}
