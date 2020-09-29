using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Exam {
    public abstract class OptionAttribute : Attribute {
        public const AttributeTargets SupportedTargets = AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field;
    }

    public interface IOptionCategoryBase {
        bool Match(string item);
        int Consume(ReadOnlySpan<string> args, object target);
    }

    public interface IOptionCategory<TOptionAttribute> : IOptionCategoryBase where TOptionAttribute : OptionAttribute {
        struct Option {
            public TOptionAttribute Attribute;
            public MemberInfo       Target;

            public Option(TOptionAttribute attribute, MemberInfo target) {
                Attribute = attribute;
                Target = target;
            }
        }

        IEnumerable<Option> Options { set; }
    }

    public interface IConfigurable<in TConfig> {
        void Config(TConfig config);
    }

    public class OptionCommonConfig {
        public string Prefix { get; set; }
    }


    [AttributeUsage(SupportedTargets, AllowMultiple = true)]
    public class ShortOptionAttribute : OptionAttribute {
        public ShortOptionAttribute(char option) {
            Option = option;
        }

        public char Option { get; set; }
    }

    public class ShortOptionConfig : OptionCommonConfig { }

    public class ShortOptionCategory : IOptionCategory<ShortOptionAttribute>, IConfigurable<ShortOptionConfig> {
        public bool Match(string item) {
            return true;
        }

        public int Consume(ReadOnlySpan<string> args, object target) {
            Console.WriteLine($"get {args[0]}");
            return 1;
        }

        public IEnumerable<IOptionCategory<ShortOptionAttribute>.Option> Options { private get; set; }

        public void Config(ShortOptionConfig config) {
            Console.WriteLine(config.Prefix);
        }
    }

    [AttributeUsage(SupportedTargets, AllowMultiple = true)]
    public class LongOptionAttribute : OptionAttribute {
        public string Option { get; set; }
    }

    public class CommandLineParser<T> where T : new() {
        private readonly Dictionary<Type, Type>   _attributeTypeToCategoryType = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Type>   _categoryTypeToConfigType    = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, object> _categoryTypeToConfig        = new Dictionary<Type, object>();
        private readonly List<Type>               _categoryTypes               = new List<Type>();

        public CommandLineParser<T> Use<TOptionCategory, TOptionAttribute>()
            where TOptionCategory : IOptionCategory<TOptionAttribute>, new()
            where TOptionAttribute : OptionAttribute {
            _evaluated = false;

            var typeAttribute = typeof(TOptionAttribute);
            var typeCategory = typeof(TOptionCategory);

            _attributeTypeToCategoryType[typeAttribute] = typeCategory;
            _categoryTypes.Add(typeCategory);

            return this;
        }

        public CommandLineParser<T> Use<TOptionCategory, TOptionAttribute, TConfig>(TConfig config)
            where TOptionCategory : IOptionCategory<TOptionAttribute>, IConfigurable<TConfig>, new()
            where TOptionAttribute : OptionAttribute {
            _evaluated = false;

            var typeAttribute = typeof(TOptionAttribute);
            var typeCategory = typeof(TOptionCategory);
            var typeConfig = typeof(TConfig);

            _attributeTypeToCategoryType[typeAttribute] = typeCategory;
            _categoryTypeToConfigType[typeCategory] = typeConfig;
            _categoryTypeToConfig[typeCategory] = config;
            _categoryTypes.Add(typeCategory);

            return this;
        }

        private struct OptionInDest {
            public Attribute  Attribute;
            public Type       ActualType;
            public Type       AttributeType;
            public MemberInfo Target;
        }

        private bool                                                 _evaluated = false;
        private List<IOptionCategoryBase>                            _categories;
        private List<OptionInDest>                                   _options;
        private Dictionary<IOptionCategoryBase, IList<OptionInDest>> _optionsOfCategories;

        private void ReadOptionsFromMemberInfos(IEnumerable<MemberInfo> memberInfos) {
            foreach (var memberInfo in memberInfos) {
                var attrs = memberInfo.GetCustomAttributes(typeof(OptionAttribute)).ToArray();
                foreach (var attr in attrs) {
                    var type = attr.GetType();
                    var actualType = type;

                    while (type != null && type != typeof(OptionAttribute)) {
                        if (_attributeTypeToCategoryType.ContainsKey(type)) {
                            _options.Add(new OptionInDest {
                                Attribute = attr,
                                ActualType = actualType,
                                AttributeType = type,
                                Target = memberInfo
                            });

                            break;
                        }

                        type = type.BaseType;
                    }
                }
            }
        }

        private void ReadOptions() {
            _options = new List<OptionInDest>();

            var typeOfT = typeof(T);
            ReadOptionsFromMemberInfos(typeOfT.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            ReadOptionsFromMemberInfos(typeOfT.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            ReadOptionsFromMemberInfos(typeOfT.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        }

        private void ReadCategories() {
            _categories = new List<IOptionCategoryBase>();
            _optionsOfCategories = new Dictionary<IOptionCategoryBase, IList<OptionInDest>>();

            var categoriesTypesToInstances = new Dictionary<Type, IOptionCategoryBase>();

            foreach (var catType in _categoryTypes) {
                var catIns = (IOptionCategoryBase) Activator.CreateInstance(catType);

                _categories.Add(catIns);
                _optionsOfCategories[catIns!] = new List<OptionInDest>();
                categoriesTypesToInstances[catType] = catIns;

                if (_categoryTypeToConfig.TryGetValue(catType, out var config)) {
                    typeof(IConfigurable<>).MakeGenericType(_categoryTypeToConfigType[catType]).GetMethod(nameof(IConfigurable<int>.Config))?.Invoke(catIns, new[] { config });
                }
            }

            foreach (var option in _options) {
                _optionsOfCategories[categoriesTypesToInstances[_attributeTypeToCategoryType[option.AttributeType]]].Add(option);
            }

            foreach (var (catIns, options) in _optionsOfCategories) {
                var catType = catIns.GetType();
                var typeOfI = catType.GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IOptionCategory<>));
                var typeOfT = typeOfI.GetGenericArguments()[0];
                var optionType = typeof(IOptionCategory<>.Option).MakeGenericType(typeOfT);
                var optionCtor = optionType.GetConstructor(new[] { typeOfT, typeof(MemberInfo) });

                var listType = typeof(List<>).MakeGenericType(optionType);
                var listAdder = listType.GetMethod(nameof(List<int>.Add));

                var optionList = listType.GetConstructor(Array.Empty<Type>())!.Invoke(null);

                foreach (var option in options) {
                    listAdder!.Invoke(optionList, new[] { optionCtor!.Invoke(new object[] { option.Attribute, option.Target }) });
                }

                catType.GetProperty(nameof(IOptionCategory<OptionAttribute>.Options))!.GetSetMethod()!.Invoke(catIns, new[] { optionList });
            }
        }

        private void EvaluateMetaInfo() {
            _evaluated = true;

            ReadOptions();
            ReadCategories();
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
    }

    class A {
        [ShortOption('f')] private string F;
    }

    class Program {
        public static int Main(string[] args) {
            var parser = new CommandLineParser<A>();
            parser.Use<ShortOptionCategory, ShortOptionAttribute, ShortOptionConfig>(new ShortOptionConfig {
                Prefix = "/"
            });

            parser.Parse("/f");

            return 0;
        }
    }
}
