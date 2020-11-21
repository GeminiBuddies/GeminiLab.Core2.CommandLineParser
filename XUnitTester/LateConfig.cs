using System.Collections.Generic;
using GeminiLab.Core2.CommandLineParser;
using GeminiLab.Core2.CommandLineParser.Default;
using Xunit;

namespace XUnitTester {
    public class LateConfig {
        public class TestOptions {
            [ShortOption('a'), LongOption("a")]
            public bool A { get; set; }

            [ShortOption('b'), LongOption("bravo"), ParameterRequired]
            public string B { get; set; } = "";

            [LongOption("c"), Switch]
            public bool C { get; set; }
            
            [TailArguments]
            public IEnumerable<string> T { get; set; } = null!;
        }

        [Fact]
        public static void LateConfigTest() {
            var parserA = new CommandLineParser<TestOptions>()
                .Config((object) new OptionConfig { ShortPrefix = '/', LongPrefix = "-" })
                .Config<TailArgumentsCategory>(new TailArgumentsConfig { TailMark = "??" });

            var parserB = new CommandLineParser<TestOptions>()
                .Config<OptionConfig>(new OptionConfig { ShortPrefix = '-', LongPrefix = "/" })
                .Config<TailArgumentsCategory, TailArgumentsConfig>(new TailArgumentsConfig { TailMark = "-c" });

            var args = new[] { "/a", "-bravo=b", "-c", "??", "!!" };
            
            var optionsA = parserA.Parse(args);

            Assert.True(optionsA.A);
            Assert.Equal("b", optionsA.B);
            Assert.True(optionsA.C);
            Assert.Equal(new[] { "!!" }, optionsA.T);
            
            var optionsB = parserB.Parse(args);

            Assert.True(optionsB.A);
            Assert.Equal("ravo=b", optionsB.B);
            Assert.False(optionsB.C);
            Assert.Equal(new[] { "??", "!!" }, optionsB.T);
        }
    }
}
