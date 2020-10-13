using System;

namespace GeminiLab.Core2.CommandLineParser.Default {
    public class ShortOptionConfig {
        [Obsolete("Use 'PrefixChar' instead")]
        public string Prefix {
            get => new string(PrefixChar, 1);
            set => PrefixChar = value?.Length > 0 ? value[0] : throw new ArgumentOutOfRangeException(nameof(value));
        }

        public char PrefixChar { get; set; } = '-';

        [Obsolete("Has not effect")]
        public bool IgnoreUnknownOption { get; set; } = false;
    }
}
