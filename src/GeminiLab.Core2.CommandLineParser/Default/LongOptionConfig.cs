using System;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public class LongOptionConfig {
        public string Prefix { get; set; } = "--";

        [Obsolete("Has not effect")]
        public bool IgnoreUnknownOption { get; set; } = false;

        [Obsolete("Use 'ParameterDelimiter' instead")]
        public string ParameterSeparator { get; set; } = "=";

        public string ParameterDelimiter {
#pragma warning disable 618
            get => ParameterSeparator;
            set => ParameterSeparator = value;
#pragma warning restore 618
        }
    }
}
