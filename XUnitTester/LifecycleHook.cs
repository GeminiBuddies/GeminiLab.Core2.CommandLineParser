using System.Collections.Generic;
using GeminiLab.Core2.CommandLineParser;
using GeminiLab.Core2.CommandLineParser.Default;
using Xunit;

namespace XUnitTester {
    public static class LifecycleHook {
        class HookedOption {
            public List<string> Queue { get; set; } = new List<string>();

            [PreParsing]
            public void PreParsing() {
                Queue.Add("pre");
            }

            [PostParsing]
            public void PostParsing() {
                Queue.Add("post");
            }

            [ShortOption('z'), ParameterRequired]
            public void Zulu(string z) {
                Queue.Add($"z:{z}");
            }
        }

        [Fact]
        public static void LifecycleHookTest() {
            var options = CommandLineParser<HookedOption>.DoParse("-zzulu");

            Assert.Equal(new string[] { "pre", "z:zulu", "post" }, options.Queue);
        }
    }
}
