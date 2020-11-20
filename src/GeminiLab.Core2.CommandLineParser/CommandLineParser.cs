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
        private List<IParsingHook>                         _hooks             = null!;

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

        private ComponentInfo? FindComponentByConfigType(Type configType) {
            return _components.FirstOrDefault(info => info.ConfigType == configType);
        }

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

        public CommandLineParser<T> Use<TComponent>(object config)
            where TComponent : new() {
            _evaluated = false;

            var componentType = typeof(TComponent);
            var configType = config.GetType();

            if (_componentIndex.TryGetValue(componentType, out var index)) {
                _components[index].ConfigType = configType;
                _components[index].Config = config;
            } else {
                _componentIndex[componentType] = _components.Count;
                _components.Add(new ComponentInfo(componentType, configType, config));
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

        public CommandLineParser<T> Config(object config) {
            var componentInfo = FindComponentByConfigType(config.GetType());

            if (componentInfo != null) componentInfo.Config = config;

            return this;
        }

        public CommandLineParser<T> Config<TConfig>(TConfig config) {
            var componentInfo = FindComponentByConfigType(typeof(TConfig));

            if (componentInfo != null) componentInfo.Config = config;

            return this;
        }

        public CommandLineParser<T> Config<TComponent>(object config)
            where TComponent : new() {
            if (_componentIndex.TryGetValue(typeof(TComponent), out var index)) {
                var component = _components[index];

                component.ConfigType = config.GetType();
                component.Config = config;
            }

            return this;
        }

        public CommandLineParser<T> Config<TComponent, TConfig>(TConfig config)
            where TComponent : IConfigurable<TConfig>, new() {
            if (_componentIndex.TryGetValue(typeof(TComponent), out var index)) {
                var component = _components[index];

                component.ConfigType = typeof(TConfig);
                component.Config = config;
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
            _hooks = new List<IParsingHook>();

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
                        var mwaType = typeof(MemberWithAttribute<>).MakeGenericType(attributeType);
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

                    if (ifType == typeof(IParsingHook)) {
                        _hooks.Add((IParsingHook) instance);
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

            _hooks.ForEach(h => h.OnParsingEvent(ParsingEvent.PreParsing, rv));

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

            _hooks.ForEach(h => h.OnParsingEvent(ParsingEvent.PostParsing, rv));

            return rv;
        }

        private void LoadDefaultConfigs() {
            // for default values of config items, see definition of config classes
            Use<ShortOptionCategory, ShortOptionConfig>(new ShortOptionConfig());
            Use<LongOptionCategory, LongOptionConfig>(new LongOptionConfig());
            Use<TailArgumentsCategory, TailArgumentsConfig>(new TailArgumentsConfig());
            Use<NonOptionArgumentCategory>();

            Use<LifecycleHookComponent>();
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
