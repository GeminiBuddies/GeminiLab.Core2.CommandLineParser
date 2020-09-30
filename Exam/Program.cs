using System;
using GeminiLab.Core2.CommandLineParser.Default;
using GeminiLab.Core2.CommandLineParser;

namespace Exam {
    class A {
        [ShortOption('m', Parameter = OptionParameter.Required)]
        [LongOption("message", Parameter = OptionParameter.Required)]
        public string Message;

        [ShortOption('a')]
        public bool All;

        [ShortOption('f', Parameter = OptionParameter.Optional)]
        [LongOption("foo", Parameter = OptionParameter.Optional)]
        public string Foo;
    }

    class Program {
        public static int Main(string[] args) {
            var parser = new CommandLineParser<A>();

            var a = parser.Parse("-am", "commit message", "-f", "--foo=123", "--foo", "--foo=");
            
            return 0;
        }
    }
}
