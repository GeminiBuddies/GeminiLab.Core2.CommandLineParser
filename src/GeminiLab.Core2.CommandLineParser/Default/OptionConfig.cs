namespace GeminiLab.Core2.CommandLineParser.Default {
    public class OptionConfig {
        public char ShortPrefix { get; set; } = '-';

        public string LongPrefix { get; set; } = "--";

        // The parameter delimiter for Long Options.
        public string ParameterDelimiter { get; set; } = "=";
    }
}
