using System;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser.Default {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class UnknownOptionHandlerAttribute : ParsingAttribute { }
}
