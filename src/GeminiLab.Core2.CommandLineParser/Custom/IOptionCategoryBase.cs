using System;

namespace GeminiLab.Core2.CommandLineParser.Custom {
    public interface IOptionCategoryBase {
        bool Match(string item);
        int Consume(ReadOnlySpan<string> args, object target);
    }
}
