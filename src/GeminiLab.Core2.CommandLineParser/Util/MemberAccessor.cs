using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using GeminiLab.Core2.CommandLineParser.Default;

namespace GeminiLab.Core2.CommandLineParser.Util {
    [ExcludeFromCodeCoverage]
    public static class MemberAccessor {
        public static void Assert(bool value) {
            if (!value) throw new DefaultException();
        }

        public static bool CheckParameterTypes(MethodInfo method, params Type[] argTypes) {
            var paramInfos = method.GetParameters();
            var paramCount = paramInfos.Length;

            if (paramInfos.Length != argTypes.Length) return false;

            for (int i = 0; i < paramCount; ++i) {
                var param = paramInfos[i];

                if (param.IsOut || param.IsIn || param.IsLcid || param.IsRetval) return false;

                if (!param.ParameterType.IsAssignableFrom(argTypes[i])) return false;
            }

            return true;
        }

        public static void SetProperty<T>(PropertyInfo property, object target, T value) {
            Assert(property.CanWrite);
            Assert(property.PropertyType.IsAssignableFrom(typeof(T)));

            property.GetSetMethod().Invoke(target, new object?[] { value });
        }

        public static void SetField<T>(FieldInfo field, object target, T value) {
            Assert(field.FieldType.IsAssignableFrom(typeof(T)));

            field.SetValue(target, value);
        }

        public static void InvokeMethod(MethodInfo method, object target) {
            Assert(CheckParameterTypes(method));

            method.Invoke(target, Array.Empty<object>());
        }

        public static void InvokeMethod<T0>(MethodInfo method, object target, T0 arg0) {
            Assert(CheckParameterTypes(method, typeof(T0)));

            method.Invoke(target, new object?[] { arg0 });
        }

        public static void InvokeMethod<T0, T1>(MethodInfo method, object target, T0 arg0, T1 arg1) {
            Assert(CheckParameterTypes(method, typeof(T0), typeof(T1)));

            method.Invoke(target, new object?[] { arg0, arg1 });
        }

        public static void InvokeMethod<T0, T1, T2>(MethodInfo method, object target, T0 arg0, T1 arg1, T2 arg2) {
            Assert(CheckParameterTypes(method, typeof(T0), typeof(T1), typeof(T2)));

            method.Invoke(target, new object?[] { arg0, arg1, arg2 });
        }

        public static TResult InvokeFunction<TResult>(MethodInfo method, object target) {
            Assert(CheckParameterTypes(method));
            Assert(method.ReturnType == typeof(TResult));

            return (TResult) method.Invoke(target, Array.Empty<object>());
        }

        public static TResult InvokeFunction<T0, TResult>(MethodInfo method, object target, T0 arg0) {
            Assert(CheckParameterTypes(method, typeof(T0)));
            Assert(method.ReturnType == typeof(TResult));

            return (TResult) method.Invoke(target, new object?[] { arg0 });
        }

        public static TResult InvokeFunction<T0, T1, TResult>(MethodInfo method, object target, T0 arg0, T1 arg1) {
            Assert(CheckParameterTypes(method, typeof(T0), typeof(T1)));
            Assert(method.ReturnType == typeof(TResult));

            return (TResult) method.Invoke(target, new object?[] { arg0, arg1 });
        }

        public static TResult InvokeFunction<T0, T1, T2, TResult>(MethodInfo method, object target, T0 arg0, T1 arg1, T2 arg2) {
            Assert(CheckParameterTypes(method, typeof(T0), typeof(T1), typeof(T2)));
            Assert(method.ReturnType == typeof(TResult));

            return (TResult) method.Invoke(target, new object?[] { arg0, arg1, arg2 });
        }

        public static void SetMember(MemberInfo member, object target) {
            switch (member) {
            case PropertyInfo property:
                SetProperty(property, target, true);
                break;
            case FieldInfo field:
                SetField(field, target, true);
                break;
            case MethodInfo method:
                if (method.GetParameters().Length == 0) {
                    InvokeMethod(method, target);
                } else {
                    InvokeMethod(method, target, true);
                }
                break;
            default:
                throw new DefaultException();
            }
        }

        public static void SetMember<T>(MemberInfo member, object target, T value) {
            switch (member) {
            case PropertyInfo property:
                SetProperty(property, target, value);
                break;
            case FieldInfo field:
                SetField(field, target, value);
                break;
            case MethodInfo method:
                InvokeMethod(method, target, value);
                break;
            default:
                throw new DefaultException();
            }
        }
    }
}
