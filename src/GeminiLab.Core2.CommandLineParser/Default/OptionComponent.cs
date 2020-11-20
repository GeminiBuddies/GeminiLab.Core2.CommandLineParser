using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeminiLab.Core2.CommandLineParser.Custom;
using GeminiLab.Core2.CommandLineParser.Util;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public class OptionComponent
        : IOptionCategory,
          IConfigurable<OptionConfig>,
          IAttributeCategory<ShortOptionAttribute>,
          IAttributeCategory<LongOptionAttribute>,
          IAttributeCategory<SwitchAttribute>,
          IAttributeCategory<ParameterRequiredAttribute>,
          IAttributeCategory<ParameterOptionalAttribute>,
          IParsingHook {
#region configs

        private char   _shortPrefix;
        private string _longPrefix     = null!;
        private string _paramDelimiter = null!;


        public void Config(OptionConfig config) {
            _shortPrefix = config.ShortPrefix;
            _longPrefix = config.LongPrefix;
            _paramDelimiter = config.ParameterDelimiter;
        }

#endregion

#region options

        protected class Option {
            public Option(MemberInfo target, OptionParameter parameter) {
                Target = target;
                Parameter = parameter;
            }

            public MemberInfo      Target    { get; set; }
            public OptionParameter Parameter { get; set; }
        }

        private readonly List<AttributedMember<ShortOptionAttribute>> _shortOptions    = new List<AttributedMember<ShortOptionAttribute>>();
        private readonly List<AttributedMember<LongOptionAttribute>>  _longOptions     = new List<AttributedMember<LongOptionAttribute>>();
        private readonly List<MemberInfo>                             _noParamOptions  = new List<MemberInfo>();
        private readonly List<MemberInfo>                             _paramOptions    = new List<MemberInfo>();
        private readonly List<MemberInfo>                             _optParamOptions = new List<MemberInfo>();

        public void SetAttributedMembers(IEnumerable<AttributedMember<ShortOptionAttribute>> members) => _shortOptions.AddRange(members);
        public void SetAttributedMembers(IEnumerable<AttributedMember<LongOptionAttribute>> members) => _longOptions.AddRange(members);
        public void SetAttributedMembers(IEnumerable<AttributedMember<SwitchAttribute>> members) => _noParamOptions.AddRange(members.Select(x => x.Target));
        public void SetAttributedMembers(IEnumerable<AttributedMember<ParameterRequiredAttribute>> members) => _paramOptions.AddRange(members.Select(x => x.Target));
        public void SetAttributedMembers(IEnumerable<AttributedMember<ParameterOptionalAttribute>> members) => _optParamOptions.AddRange(members.Select(x => x.Target));

        private bool                           _initialized    = false;
        private Dictionary<MemberInfo, Option> _optionByTarget = null!;
        private Dictionary<char, Option>       _optionByShort  = null!;
        private Dictionary<string, Option>     _optionByLong   = null!;

        private void Initialize() {
            _initialized = true;

            _optionByTarget = new Dictionary<MemberInfo, Option>();
            _optionByShort = new Dictionary<char, Option>();
            _optionByLong = new Dictionary<string, Option>();

            foreach (var (attribute, target) in _shortOptions) {
                if (_optionByTarget.TryGetValue(target, out var option)) {
                    _optionByShort[attribute.Option] = option;
                } else {
                    _optionByShort[attribute.Option] = _optionByTarget[target] = new Option(target, OptionParameter.None);
                }
            }

            foreach (var (attribute, target) in _longOptions) {
                if (_optionByTarget.TryGetValue(target, out var option)) {
                    _optionByLong[attribute.Option] = option;
                } else {
                    _optionByLong[attribute.Option] = _optionByTarget[target] = new Option(target, OptionParameter.None);
                }
            }

            foreach (var target in _noParamOptions) {
                if (_optionByTarget.TryGetValue(target, out var option)) {
                    option.Parameter = OptionParameter.None;
                }
            }

            foreach (var target in _paramOptions) {
                if (_optionByTarget.TryGetValue(target, out var option)) {
                    option.Parameter = OptionParameter.Required;
                }
            }

            foreach (var target in _optParamOptions) {
                if (_optionByTarget.TryGetValue(target, out var option)) {
                    option.Parameter = OptionParameter.Optional;
                }
            }
        }

        public void OnParsingEvent(ParsingEvent parsingEvent, object target) {
            if (parsingEvent == ParsingEvent.PreParsing && !_initialized) {
                Initialize();
            }
        }

#endregion

        public int TryConsume(Span<string> args, object target) {
            // it's a long option
            if (_longPrefix.Length > 0 && args[0].StartsWith(_longPrefix) && args[0].Length > _longPrefix.Length) {
                var content = args[0].AsSpan(_longPrefix.Length);
                var sepIndex = content.IndexOf(_paramDelimiter.AsSpan());
                var nextStringConsumed = false;

                var optionStr = (sepIndex > 0 ? content[..sepIndex] : content).ToString();
                if (_optionByLong.TryGetValue(optionStr, out var option)) {
                    if (option.Parameter == OptionParameter.Optional) {
                        string? param = null;

                        if (sepIndex > 0) {
                            param = content[(sepIndex + 1)..].ToString();
                        }

                        MemberAccessor.SetMember(option.Target, target, param);
                    } else if (option.Parameter == OptionParameter.Required) {
                        var param = "";

                        if (sepIndex > 0) {
                            param = content[(sepIndex + 1)..].ToString();
                        } else if (args.Length >= 2) {
                            param = args[1];
                            nextStringConsumed = true;
                        } else {
                            throw new DefaultException();
                        }

                        MemberAccessor.SetMember(option.Target, target, param);
                    } else { // if (option.Parameter == OptionParameter.None) 
                        MemberAccessor.SetMember(option.Target, target);
                    }
                } else {
                    return 0;
                }

                return nextStringConsumed ? 2 : 1;
            }

            // it's a short option
            if (_shortPrefix != '\0' && args[0].Length > 1 && args[0][0] == _shortPrefix && args[0][1] != _shortPrefix) {
                var content = args[0].AsSpan(1);
                var len = content.Length;
                var nextStringConsumed = false;

                int ptr = 0;
                while (ptr < len && _optionByShort.TryGetValue(content[ptr], out var option)) {
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
                    if (ptr > 0) args[0] = args[0][(ptr + 1)..];
                    return 0;
                }

                return nextStringConsumed ? 2 : 1;
            }

            // we do not know what it is
            return 0;
        }
    }
}
