using System;

namespace GeminiLab.Core2.CommandLineParser.Custom {
    public interface IOptionCategoryBase {
        int TryConsume(Span<string> args, object target);
    }
}
