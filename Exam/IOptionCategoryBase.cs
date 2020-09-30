using System;

namespace Exam {
    public interface IOptionCategoryBase {
        bool Match(string item);
        int Consume(ReadOnlySpan<string> args, object target);
    }
}
