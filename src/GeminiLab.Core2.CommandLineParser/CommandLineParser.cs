using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeminiLab.Core2.CommandLineParser.Default;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser {
    public class CommandLineParser<T> where T : new() {
        private static CommandLineParser<T>? _defaultParser = null;

        public static T DoParse(ReadOnlySpan<string> args) {
            return (_defaultParser ??= new CommandLineParser<T>()).Parse(args);
        }

        public static T DoParse(params string[] args) {
            return (_defaultParser ??= new CommandLineParser<T>()).Parse(args);
        }

        private bool _evaluated = false;

        private List<IOptionCategory>                      _optionCategories  = null!;
        private List<(Type ExceptionType, object Handler)> _exceptionHandlers = null!;

        [Obsolete("Use method 'Parse' instead")]
        public T ParseFromSpan(ReadOnlySpan<string> args) {
            return Parse(args);
        }

        public T Parse(ReadOnlySpan<string> args) {
            return Parse(args.ToArray());
        }

        private class ComponentInfo {
            public ComponentInfo(Type type, Type? configType = null, object? config = null) {
                Type = type;
                ConfigType = configType;
                Config = config;
            }

            public Type    Type       { get; set; }
            public Type?   ConfigType { get; set; }
            public object? Config     { get; set; }
        }

        private Dictionary<Type, int> _componentIndex = new Dictionary<Type, int>();
        private List<ComponentInfo>   _components     = new List<ComponentInfo>();

        public CommandLineParser<T> Use<TComponent>()
            where TComponent : new() {
            _evaluated = false;

            var componentType = typeof(TComponent);

            if (_componentIndex.TryGetValue(componentType, out var index)) {
                _components[index].ConfigType = null;
                _components[index].Config = null;
            } else {
                _componentIndex[componentType] = _components.Count;
                _components.Add(new ComponentInfo(componentType));
            }

            return this;
        }

        public CommandLineParser<T> Use<TComponent, TConfig>(TConfig config)
            where TComponent : IConfigurable<TConfig>, new() {
            _evaluated = false;

            var componentType = typeof(TComponent);
            var configType = typeof(TConfig);

            if (_componentIndex.TryGetValue(componentType, out var index)) {
                _components[index].ConfigType = configType;
                _components[index].Config = config;
            } else {
                _componentIndex[componentType] = _components.Count;
                _components.Add(new ComponentInfo(componentType, configType, config));
            }

            return this;
        }

        private List<(MemberInfo MemberInfo, ParsingAttribute Attribute)> GetAttributesFromMemberInfos(IEnumerable<MemberInfo> memberInfos) {
            var result = new List<(MemberInfo MemberInfo, ParsingAttribute Attribute)>();

            foreach (var memberInfo in memberInfos) {
                var attrs = memberInfo.GetCustomAttributes(typeof(ParsingAttribute)).ToArray();
                foreach (var attr in attrs) {
                    result.Add((memberInfo, (ParsingAttribute) attr));
                }
            }

            return result;
        }

        private List<(MemberInfo MemberInfo, ParsingAttribute Attribute)> GetAttributes() {
            var result = new List<(MemberInfo MemberInfo, ParsingAttribute Attribute)>();

            var typeOfT = typeof(T);
            result.AddRange(GetAttributesFromMemberInfos(typeOfT.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)));
            result.AddRange(GetAttributesFromMemberInfos(typeOfT.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)));
            result.AddRange(GetAttributesFromMemberInfos(typeOfT.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)));

            return result;
        }

        private void EvaluateMetaInfo() {
            _evaluated = true;

            _optionCategories = new List<IOptionCategory>();
            _exceptionHandlers = new List<(Type ExceptionType, object Handler)>();

            var attributes = GetAttributes();

            foreach (var componentInfo in _components) {
                var componentType = componentInfo.Type;
                var instance = Activator.CreateInstance(componentType);

                if (componentInfo.ConfigType != null) {
                    var configurableType = typeof(IConfigurable<>).MakeGenericType(componentInfo.ConfigType);
                    configurableType.GetMethod(nameof(IConfigurable<object>.Config))!.Invoke(instance, new[] { componentInfo.Config });
                }

                foreach (var ifType in componentType.GetInterfaces()) {
                    // AttributeCategory
                    if (ifType.IsConstructedGenericType && ifType.GetGenericTypeDefinition() == typeof(IAttributeCategory<>)) {
                        var attributeType = ifType.GetGenericArguments()[0];
                        var mwaType = typeof(IAttributeCategory<>.MemberWithAttribute).MakeGenericType(attributeType);
                        var mwaListType = typeof(List<>).MakeGenericType(mwaType);
                        var mwaListAdder = mwaListType.GetMethod(nameof(List<object>.Add))!;
                        var mwaCtor = mwaType.GetConstructor(new[] { attributeType, typeof(MemberInfo) })!;

                        var mwaList = mwaListType.GetConstructor(Array.Empty<Type>())!.Invoke(Array.Empty<object>());

                        foreach (var (memberInfo, attribute) in attributes) {
                            if (attributeType.IsInstanceOfType(attribute)) {
                                mwaListAdder.Invoke(mwaList, new[] { mwaCtor.Invoke(new object[] { attribute, memberInfo }) });
                            }
                        }

                        ifType.GetProperty(nameof(IAttributeCategory<ParsingAttribute>.Options))!.GetSetMethod().Invoke(instance, new[] { mwaList });
                    }

                    if (ifType.IsConstructedGenericType && ifType.GetGenericTypeDefinition() == typeof(IExceptionHandler<>)) {
                        _exceptionHandlers.Add((ifType.GetGenericArguments()[0], instance));
                    }

                    if (ifType == typeof(IOptionCategory)) {
                        _optionCategories.Add((IOptionCategory) instance);
                    }
                }
            }
        }

        public T Parse(params string[] args) {
            if (!_evaluated) {
                EvaluateMetaInfo();
            }

            var workplace = args.ToArray().AsSpan();
            int len = workplace.Length;
            int ptr = 0;
            var rv = new T();

            while (ptr < len) {
                var current = workplace[ptr..];
                int consumed = 0;

                try {
                    foreach (var cat in _optionCategories) {
                        consumed = cat.TryConsume(current, rv);

                        if (consumed > 0) {
                            break;
                        }
                    }

                    if (consumed <= 0) {
                        throw new UnknownOptionException(args.ToArray(), ptr, workplace[ptr]);
                    }
                } catch (ParsingException e) {
                    var eType = e.GetType();
                    var finalResult = ExceptionHandlerResult.Throw;

                    foreach (var (type, handler) in _exceptionHandlers) {
                        if (eType == type || eType.IsSubclassOf(type)) {
                            finalResult = (ExceptionHandlerResult) handler.GetType().GetMethod(nameof(IExceptionHandler<ParsingException>.OnException))!.Invoke(handler, new object[] { e, rv });

                            if (finalResult != ExceptionHandlerResult.CallNextHandler) {
                                break;
                            }
                        }
                    }

                    if (finalResult == ExceptionHandlerResult.ContinueParsing) {
                        consumed = 1;
                    } else if (finalResult == ExceptionHandlerResult.GracefullyBreak) {
                        break;
                    } else {
                        throw;
                    }
                }

                ptr += consumed;
            }

            return rv;
        }

        private void LoadDefaultConfigs() {
            Use<ShortOptionCategory, ShortOptionConfig>(new ShortOptionConfig { Prefix = "-" });
            Use<LongOptionCategory, LongOptionConfig>(new LongOptionConfig { Prefix = "--", ParameterSeparator = "=" });
            Use<TailArgumentsCategory, TailArgumentsConfig>(new TailArgumentsConfig { TailMark = "--" });
            Use<NonOptionArgumentCategory>();

            Use<UnknownOptionHandlerComponent>();
        }

        public CommandLineParser() : this(true) { }

        public CommandLineParser(bool loadDefaultConfigs) {
            if (loadDefaultConfigs) {
                LoadDefaultConfigs();
            }
        }
    }
}
