using System;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public class LongOptionConfig {
        public string Prefix { get; set; } = "--";

        public string ParameterDelimiter { get; set; } = "=";
    }
}
