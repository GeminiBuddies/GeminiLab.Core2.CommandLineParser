namespace GeminiLab.Core2.CommandLineParser.Custom {
    public enum ExceptionHandlerResult {
        MayContinue = 0,
        MayBreak    = 1,
        MustThrow   = 2,
    }

    public interface IExceptionHandler<in TException> where TException : ParsingException {
        ExceptionHandlerResult OnException(TException exception);
    }

    public interface IExceptionHandler<in TException, TAttribute> : IExceptionHandler<TException>, IAttributeCategory<TAttribute> where TException : ParsingException where TAttribute : ParsingAttribute { }
}
