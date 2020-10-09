using GeminiLab.Core2.CommandLineParser.Custom;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public class UnknownOptionException : ParsingException {
        public UnknownOptionException(string[] arguments, int position, string option) {
            Arguments = arguments;
            Position = position;
            Option = option;
        }

        public string[] Arguments { get; }
        public int      Position  { get; }
        public string   Option    { get; }
    }
}
