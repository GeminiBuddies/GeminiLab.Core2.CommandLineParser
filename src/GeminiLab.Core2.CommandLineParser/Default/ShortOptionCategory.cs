using System;
using System.Collections.Generic;
using System.Reflection;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public class ShortOptionCategory : IOptionCategory<ShortOptionAttribute>, IConfigurable<ShortOptionConfig> {
        private          char                              _prefix;
        private readonly Dictionary<char, MemberInfo>      _targets         = new Dictionary<char, MemberInfo>();
        private readonly Dictionary<char, OptionParameter> _optionParameter = new Dictionary<char, OptionParameter>();

        private ShortOptionConfig _config = null!;
        
        public bool Match(string item) {
            return item.Length >= 2 && item[0] == _prefix && item[1] != _prefix;
        }

        public int Consume(ReadOnlySpan<string> args, object target) {
            var content = args[0].AsSpan(1);
            var len = content.Length;
            var nextStringConsumed = false;

            int ptr = 0;
            char option;
            while (ptr < len && _targets.TryGetValue(option = content[ptr], out var memberInfo)) {
                var parameter = _optionParameter[option];

                if (parameter == OptionParameter.None) {
                    MemberSetter.SetMember(target, memberInfo);

                    ptr += 1;
                } else if (parameter == OptionParameter.Optional) {
                    var param = _config.DefaultValueForOptionalParameter;
                    
                    if (ptr + 1 < len) {
                        param = content[(ptr + 1)..].ToString();
                    }
                    
                    MemberSetter.SetMember(target, memberInfo, param);

                    ptr = len;
                } else if (parameter == OptionParameter.Required) {
                    var param = "";
                    
                    if (ptr + 1 < len) {
                        param = content[(ptr + 1)..].ToString();
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
                    _targets[option.Attribute.Option] = option.Target;
                    _optionParameter[option.Attribute.Option] = option.Attribute.Parameter;
                }
            }
        }

        public void Config(ShortOptionConfig config) {
            _prefix = config.Prefix[0];
            _config = config;
        }
    }
}
