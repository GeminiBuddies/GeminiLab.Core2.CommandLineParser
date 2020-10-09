using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public class UnknownOptionHandlerComponent : IExceptionHandler<UnknownOptionException, UnknownOptionHandlerAttribute> {
        private MethodInfo? _handler = null;

        public ExceptionHandlerResult OnException(UnknownOptionException exception, object target) {
            if (_handler == null) return ExceptionHandlerResult.CallNextHandler;

            return (ExceptionHandlerResult) _handler.Invoke(target, new object[] { exception });
        }

        protected static bool IsQualifiedHandlerMethod(MethodInfo m) {
            var paramList = m.GetParameters();

            return m.ReturnType == typeof(ExceptionHandlerResult)
                && paramList.Length == 1
                && !paramList[0].IsOut
                && paramList[0].ParameterType == typeof(UnknownOptionException);
        }

        public IEnumerable<IAttributeCategory<UnknownOptionHandlerAttribute>.MemberWithAttribute> Options {
            set { _handler = (MethodInfo?) value.FirstOrDefault(x => x.Target is MethodInfo m && IsQualifiedHandlerMethod(m))?.Target; }
        }
    }
}
