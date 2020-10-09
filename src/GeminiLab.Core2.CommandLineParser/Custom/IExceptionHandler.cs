namespace GeminiLab.Core2.CommandLineParser.Custom {
    public enum ExceptionHandlerResult {
        CallNextHandler = 0,
        ContinueParsing = 1,
        GracefullyBreak = 2,
        Throw           = 3,
    }

    public interface IExceptionHandler<in TException> where TException : ParsingException {
        ExceptionHandlerResult OnException(TException exception, object target);
    }

    public interface IExceptionHandler<in TException, TAttribute> : IExceptionHandler<TException>, IAttributeCategory<TAttribute> where TException : ParsingException where TAttribute : ParsingAttribute { }
}
