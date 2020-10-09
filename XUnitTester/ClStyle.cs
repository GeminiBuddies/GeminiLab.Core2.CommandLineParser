using System.Collections.Generic;
using GeminiLab.Core2.CommandLineParser;
using GeminiLab.Core2.CommandLineParser.Default;
using Xunit;

namespace XUnitTester {
    public class ClStyleTestOptions {
        [LongOption("OptionFirst", OptionParameter.Required)]
        public string OptA = null;
        [LongOption("S")]
        [LongOption("OptionSecond")]
        public bool   OptB = false;
    }

    public static class ClStyle {
        private static void AssertLogQueue(Queue<string> logs, params string[] expected) {
            foreach (var s in expected) {
                Assert.NotEmpty(logs);
                Assert.Equal(s, logs.Dequeue());
            }

            Assert.Empty(logs);
        }

        [Fact]
        public static void NormalTest() {
            var args = new[] {
                "/OptionFirst:first",
                "/S",
            };
            
            var parser = new CommandLineParser<ClStyleTestOptions>(true)
                .Use<LongOptionCategory, LongOptionConfig>(new LongOptionConfig {
                    ParameterSeparator = ":",
                    Prefix = "/",
                });
            var result = parser.Parse(args);
            
            Assert.True(result.OptB);
            Assert.Equal("first", result.OptA);
        }
    }
}
