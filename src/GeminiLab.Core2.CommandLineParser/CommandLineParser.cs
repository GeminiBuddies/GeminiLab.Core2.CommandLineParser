using System;
using System.Collections.Generic;
using System.Reflection;
using GeminiLab.Core2.GetOpt;

namespace GeminiLab.Core2.CommandLineParser {
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class OptionAttribute : Attribute {
        public char Option { get; set; } = '\0';
        public string? LongOption { get; set; } = null;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class GetOptErrorHandlerAttribute : Attribute {
        public static bool IsValidErrorHandler(MethodInfo handler) {
            var para = handler.GetParameters();
            return para.Length == 2
                   && !para[0].IsIn && !para[0].IsOut && !para[0].IsLcid && para[0].ParameterType == typeof(GetOptError)
                   && !para[1].IsIn && !para[1].IsOut && !para[1].IsLcid && para[1].ParameterType == typeof(GetOptResult)
                ;
        }
    }

    internal class CommandLineParserTypeMetaInfo {
        public OptGetter Opt;
        public Dictionary<char, PropertyInfo> ShortOptionTargets;
        public Dictionary<string, PropertyInfo> LongOptionTargets;
        public IList<MethodInfo> ErrorHandlers;

        public CommandLineParserTypeMetaInfo(OptGetter opt, Dictionary<char, PropertyInfo> shortOptionTargets, Dictionary<string, PropertyInfo> longOptionTargets, IList<MethodInfo> errorHandlers) {
            Opt = opt;
            ShortOptionTargets = shortOptionTargets;
            LongOptionTargets = longOptionTargets;
            ErrorHandlers = errorHandlers;
        }
    }

    public delegate bool CommandLineParserErrorHandler(GetOptError error, GetOptResult result);

    public static class CommandLineParser<T> {
        // How to tell analyzers I DO KNOW WHAT I AM DOING??!!
        // ReSharper disable StaticMemberInGenericType
        private static readonly object InternalLock = new object();
        private static CommandLineParserTypeMetaInfo? Info;
        // ReSharper restore StaticMemberInGenericType

        private static CommandLineParserTypeMetaInfo GenerateOptGetter() {
            var opt = new OptGetter();
            var shortOptionTargets = new Dictionary<char, PropertyInfo>();
            var longOptionTargets = new Dictionary<string, PropertyInfo>();

            var type = typeof(T);

            var props = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props) {
                var propType = prop.PropertyType;
                OptionType optionType;
                
                if (propType == typeof(bool)) {
                    optionType = OptionType.Switch;
                } else if (propType == typeof(string)) {
                    optionType = OptionType.Parameterized;
                } else if (propType == typeof(string[])) {
                    optionType = OptionType.MultiParameterized;
                } else {
                    continue;
                }

                foreach (var attr in prop.GetCustomAttributes<OptionAttribute>(true)) {
                    if (attr.Option != '\0') {
                        shortOptionTargets[attr.Option] = prop;
                        opt.AddOption(attr.Option, optionType);
                    }

                    if (attr.LongOption != null) {
                        longOptionTargets[attr.LongOption] = prop;
                        opt.AddOption(attr.LongOption, optionType);
                    }
                }
            }

            var handlers = new List<MethodInfo>();
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods) {
                if (method.GetCustomAttribute<GetOptErrorHandlerAttribute>() != null) {
                    if (GetOptErrorHandlerAttribute.IsValidErrorHandler(method)) {
                        handlers.Add(method);
                    }
                }
            }

            return new CommandLineParserTypeMetaInfo(opt, shortOptionTargets, longOptionTargets, handlers);
        }

        public static T Parse(params string[] args) {
            return Parse(args, null);
        }

        public static T Parse(string[] args, CommandLineParserErrorHandler? errorHandler) {
            lock (InternalLock) {
                if (Info == null) Info = GenerateOptGetter();

                Info.Opt.BeginParse(args);

                T rv = Activator.CreateInstance<T>();

                GetOptError err;
                while ((err = Info.Opt.GetOpt(out var result)) != GetOptError.EndOfArguments) {
                    if (err == GetOptError.NoError) {
                        PropertyInfo prop;

                        switch (result.Type) {
                        case GetOptResultType.ShortOption:
                        case GetOptResultType.LongAlias:    // not supposed to happen, but handle it anyway
                            prop = Info.ShortOptionTargets[result.Option];
                            break;
                        case GetOptResultType.LongOption:
                            prop = Info.LongOptionTargets[result.LongOption!];
                            break;
                        // case GetOptResultType.Values:
                        // case GetOptResultType.Invalid:
                        default:
                            continue;
                        }

                        prop.SetValue(rv, result.OptionType switch {
                            OptionType.Switch => (object)true,
                            OptionType.Parameterized => result.Argument,
                            OptionType.MultiParameterized => result.Arguments,
                            _ => throw new ArgumentOutOfRangeException(),
                        });
                    } else {
                        bool exit = false;

                        foreach (var handler in Info.ErrorHandlers) {
                            if (handler.Invoke(rv, new object[] {err, result}) is bool b) exit |= b;
                        }

                        if (errorHandler != null) {
                            exit |= errorHandler.Invoke(err, result);
                        }

                        if (exit) break;
                    }
                }

                Info.Opt.EndParse();
                return rv;
            }
        }
    }
}
