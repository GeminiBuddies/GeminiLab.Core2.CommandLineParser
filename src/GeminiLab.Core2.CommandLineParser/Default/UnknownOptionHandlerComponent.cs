using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeminiLab.Core2.CommandLineParser.Custom;
using GeminiLab.Core2.CommandLineParser.Util;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public class UnknownOptionHandlerComponent : IExceptionHandler<UnknownOptionException, UnknownOptionHandlerAttribute> {
        private MethodInfo? _handler;

        public ExceptionHandlerResult OnException(UnknownOptionException exception, object target) {
            if (_handler == null) {
                return ExceptionHandlerResult.CallNextHandler;
            }

            return MemberAccessor.InvokeFunction<UnknownOptionException, ExceptionHandlerResult>(_handler, target, exception);
        }

        public void SetAttributedMembers(IEnumerable<AttributedMember<UnknownOptionHandlerAttribute>> members) {
            _handler = members.Select(x => x.Target).OfType<MethodInfo>().FirstOrDefault();
        }
    }
}
