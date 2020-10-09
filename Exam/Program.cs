using System;
using GeminiLab.Core2.CommandLineParser.Default;
using GeminiLab.Core2.CommandLineParser;
using GeminiLab.Core2.CommandLineParser.Custom;

namespace Exam {
    class A {
        [ShortOption('m', Parameter = OptionParameter.Required)]
        [LongOption("message", Parameter = OptionParameter.Required)]
        public string Message {
            set {
                Console.WriteLine($"message set to {value ?? "<null>"}");
            }
        }

        [ShortOption('a')]
        public bool All {
            set {
                Console.WriteLine($"all set to {value}");
            }
        }

        [ShortOption('f', Parameter = OptionParameter.Optional)]
        [LongOption("foo", Parameter = OptionParameter.Optional)]
        public string Foo {
            set {
                Console.WriteLine($"foo set to {value ?? "<null>"}");
            }
        }

        [UnknownOptionHandler]
        public ExceptionHandlerResult OnUnknownOption(UnknownOptionException exception) {
            Console.WriteLine($"unknown option {exception.Option} at position {exception.Position}");
            return ExceptionHandlerResult.ContinueParsing;
        }
    }

    class Program {
        public static int Main(string[] args) {
            var parser = new CommandLineParser<A>();

            var a = parser.Parse("-am", "commit message", "--unexpected", "-f", "--foo=123", "--foo", "--foo=");

            return 0;
        }
    }
}
