using Exam.Default;

namespace Exam {
    class A {
        [ShortOption('m', Parameter = OptionParameter.Required)]
        [LongOption("message", Parameter = OptionParameter.Required)]
        private string Message;

        [ShortOption('a')]
        private bool All;

        [ShortOption('f', Parameter = OptionParameter.Optional)]
        [LongOption("foo", Parameter = OptionParameter.Optional)]
        private string Foo;
    }

    class Program {
        public static int Main(string[] args) {
            var parser = new CommandLineParser<A>();

            parser.Parse("-am", "commit message", "-f", "--foo=123", "--foo", "--foo=");

            return 0;
        }
    }
}
