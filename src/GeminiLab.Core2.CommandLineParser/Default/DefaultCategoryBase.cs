using System;
using System.Reflection;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public abstract class DefaultCategoryBase {
        protected class OptionInDefaultCategory {
            public OptionInDefaultCategory(MemberInfo target, OptionParameter parameter) {
                Target = target;
                Parameter = parameter;
            }
            
            public MemberInfo      Target    { get; set; }
            public OptionParameter Parameter { get; set; }
        }
        
        protected void Assert(bool boolean) {
            if (!boolean) throw new FoobarException();
        }

        protected void SetMember(object target, MemberInfo memberInfo) {
            switch (memberInfo) {
            case MethodInfo methodInfo:
                Assert(methodInfo.GetParameters().Length == 0);
                methodInfo.Invoke(target, Array.Empty<object>());
                return;
            case PropertyInfo propertyInfo:
                Assert(propertyInfo.CanWrite && propertyInfo.PropertyType == typeof(bool));
                propertyInfo.SetMethod.Invoke(target, new object[] { true });
                return;
            case FieldInfo fieldInfo:
                Assert(fieldInfo.FieldType == typeof(bool));
                fieldInfo.SetValue(target, true);
                return;
            }
        }
        
        protected void SetMember(object target, MemberInfo memberInfo, string? value) {
            switch (memberInfo) {
            case MethodInfo methodInfo:
                Assert(methodInfo.GetParameters().Length == 1 && methodInfo.GetParameters()[0].ParameterType == typeof(string));
                methodInfo.Invoke(target, new object?[] { value });
                return;
            case PropertyInfo propertyInfo:
                Assert(propertyInfo.CanWrite && propertyInfo.PropertyType == typeof(string));
                propertyInfo.SetMethod.Invoke(target, new object?[] { value });
                return;
            case FieldInfo fieldInfo:
                Assert(fieldInfo.FieldType == typeof(string));
                fieldInfo.SetValue(target, value);
                return;
            }
        }      
    }
}
