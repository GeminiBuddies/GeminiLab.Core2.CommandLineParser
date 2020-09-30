using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeminiLab.Core2.CommandLineParser.Default;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser {
    public class CommandLineParser<T> where T : new() {
        private class CategoryConfig {
            public Type    CategoryType  { get; set; }
            public Type    AttributeType { get; set; }
            public Type?   ConfigType    { get; set; }
            public object? Config        { get; set; }

            public IOptionCategoryBase Instance { get; set; }
            public List<OptionInDest>  Options  { get; set; }

            public CategoryConfig(Type categoryType, Type attributeType, Type? configType, object? config) {
                CategoryType = categoryType;
                AttributeType = attributeType;
                ConfigType = configType;
                Config = config;

                Instance = null!;
                Options = null!;
            }
        }

        private class OptionInDest {
            public OptionInDest(Attribute attribute, Type actualType, Type attributeType, MemberInfo target) {
                Attribute = attribute;
                ActualType = actualType;
                AttributeType = attributeType;
                Target = target;
            }

            public Attribute  Attribute     { get; set; }
            public Type       ActualType    { get; set; }
            public Type       AttributeType { get; set; }
            public MemberInfo Target        { get; set; }
        }


        private readonly Dictionary<Type, CategoryConfig> _configByCategoryType  = new Dictionary<Type, CategoryConfig>();
        private readonly Dictionary<Type, CategoryConfig> _configByAttributeType = new Dictionary<Type, CategoryConfig>();

        private bool                      _evaluated = false;
        private List<IOptionCategoryBase> _categories;

        private void RemoveExistingConfigs(Type categoryType, Type attributeType) {
            CategoryConfig config;
            if (_configByCategoryType.TryGetValue(categoryType, out config)) {
                _configByCategoryType.Remove(categoryType);
                _configByAttributeType.Remove(config.AttributeType);
            }

            if (_configByAttributeType.TryGetValue(attributeType, out config)) {
                _configByAttributeType.Remove(attributeType);
                _configByCategoryType.Remove(config.CategoryType);
            }
        }

        public CommandLineParser<T> Use<TOptionCategory, TOptionAttribute>()
            where TOptionCategory : IOptionCategory<TOptionAttribute>, new()
            where TOptionAttribute : OptionAttribute {
            _evaluated = false;

            var attributeType = typeof(TOptionAttribute);
            var categoryType = typeof(TOptionCategory);

            RemoveExistingConfigs(categoryType, attributeType);

            var categoryConfig = new CategoryConfig(categoryType, attributeType, null, null);

            _configByCategoryType[categoryType] = categoryConfig;
            _configByAttributeType[attributeType] = categoryConfig;

            return this;
        }


        public CommandLineParser<T> Use<TOptionCategory, TOptionAttribute, TConfig>(TConfig config)
            where TOptionCategory : IOptionCategory<TOptionAttribute>, IConfigurable<TConfig>, new()
            where TOptionAttribute : OptionAttribute {
            _evaluated = false;

            var attributeType = typeof(TOptionAttribute);
            var categoryType = typeof(TOptionCategory);
            var configType = typeof(TConfig);

            RemoveExistingConfigs(categoryType, attributeType);

            var categoryConfig = new CategoryConfig(categoryType, attributeType, configType, config);

            _configByCategoryType[categoryType] = categoryConfig;
            _configByAttributeType[attributeType] = categoryConfig;

            return this;
        }
        
        private IList<OptionInDest> ReadOptionsFromMemberInfos(IEnumerable<MemberInfo> memberInfos) {
            var options = new List<OptionInDest>();

            foreach (var memberInfo in memberInfos) {
                var attrs = memberInfo.GetCustomAttributes(typeof(OptionAttribute)).ToArray();
                foreach (var attr in attrs) {
                    var type = attr.GetType();
                    var actualType = type;

                    while (type != null && type != typeof(OptionAttribute)) {
                        if (_configByAttributeType.ContainsKey(type)) {
                            options.Add(new OptionInDest(attr, actualType, type, memberInfo));

                            break;
                        }

                        type = type.BaseType;
                    }
                }
            }

            return options;
        }

        private IList<OptionInDest> ReadOptions() {
            var options = new List<OptionInDest>();

            var typeOfT = typeof(T);
            options.AddRange(ReadOptionsFromMemberInfos(typeOfT.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)));
            options.AddRange(ReadOptionsFromMemberInfos(typeOfT.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)));
            options.AddRange(ReadOptionsFromMemberInfos(typeOfT.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)));

            return options;
        }

        private void EvaluateCategories(IList<OptionInDest> options) {
            foreach (var (categoryType, categoryConfig) in _configByCategoryType) {
                var instance = (IOptionCategoryBase) Activator.CreateInstance(categoryType);

                categoryConfig.Instance = instance;
                categoryConfig.Options = new List<OptionInDest>();

                if (categoryConfig.ConfigType != null) {
                    typeof(IConfigurable<>).MakeGenericType(categoryConfig.ConfigType).GetMethod(nameof(IConfigurable<int>.Config))?.Invoke(instance, new[] { categoryConfig.Config });
                }
            }

            foreach (var option in options) {
                _configByAttributeType[option.AttributeType].Options.Add(option);
            }

            foreach (var (categoryType, categoryConfig) in _configByCategoryType) {
                var optionType = typeof(IOptionCategory<>.Option).MakeGenericType(categoryConfig.AttributeType);
                var optionCtor = optionType.GetConstructor(new[] { categoryConfig.AttributeType, typeof(MemberInfo) });

                var listType = typeof(List<>).MakeGenericType(optionType);
                var listAdder = listType.GetMethod(nameof(List<int>.Add));

                var optionList = listType.GetConstructor(Array.Empty<Type>())!.Invoke(null);

                foreach (var option in categoryConfig.Options) {
                    listAdder!.Invoke(optionList, new[] { optionCtor!.Invoke(new object[] { option.Attribute, option.Target }) });
                }

                categoryType.GetProperty(nameof(IOptionCategory<OptionAttribute>.Options))!.GetSetMethod()!.Invoke(categoryConfig.Instance, new[] { optionList });
            }

            _categories = _configByCategoryType.Select(x => x.Value.Instance).ToList();
        }

        private void EvaluateMetaInfo() {
            _evaluated = true;

            EvaluateCategories(ReadOptions());
        }

        public T ParseFromSpan(ReadOnlySpan<string> args) {
            if (!_evaluated) EvaluateMetaInfo();

            int len = args.Length;
            int ptr = 0;
            var rv = new T();

            while (ptr < len) {
                var current = args[ptr..];
                int consumed = -1;

                foreach (var cat in _categories) {
                    if (cat.Match(args[ptr])) {
                        consumed = cat.Consume(current, rv);
                        break;
                    }
                }

                if (consumed <= 0) { // avoid endless loop when IOptionCategory.Consume returns 0 by error
                    consumed = 1;
                }

                ptr += consumed;
            }

            return rv;
        }

        public T Parse(params string[] args) {
            return ParseFromSpan(new ReadOnlySpan<string>(args));
        }

        private void LoadDefaultConfigs() {
            Use<ShortOptionCategory, ShortOptionAttribute, ShortOptionConfig>(new ShortOptionConfig { Prefix = "-" });
            Use<LongOptionCategory, LongOptionAttribute, LongOptionConfig>(new LongOptionConfig { Prefix = "--", ParameterSeparator = "=" });
        }

        public CommandLineParser() : this(false) { }

        public CommandLineParser(bool disableDefaultConfigs) {
            if (!disableDefaultConfigs) LoadDefaultConfigs(); 
            _categories = null!;
        }
    }
}
