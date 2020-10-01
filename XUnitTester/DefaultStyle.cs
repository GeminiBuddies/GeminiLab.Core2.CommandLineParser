using System.Collections.Generic;
using GeminiLab.Core2.CommandLineParser;
using GeminiLab.Core2.CommandLineParser.Default;
using Xunit;

namespace XUnitTester {
    public class DefaultStyleTestOptions {
        public Queue<string> Logs { get; } = new Queue<string>();

        [ShortOption('a', OptionParameter.Required)]
        [LongOption("alpha", OptionParameter.Required)]
        public string OptionA {
            set => Logs.Enqueue($"A:{value}");
        }

        [ShortOption('b')]
        public bool OptionB {
            set => Logs.Enqueue($"B:{value}");
        }

        [LongOption("bravo")]
        public void OptionBPlus() => Logs.Enqueue($"B:{true}");

        [ShortOption('c', OptionParameter.Required)]
        public void OptionC(string value) => Logs.Enqueue($"C:{value}");

        [ShortOption('d', OptionParameter.Optional)]
        [LongOption("delta", OptionParameter.Optional)]
        public string OptionD {
            set => Logs.Enqueue($"D:{value ?? "default"}");
        }

        [ShortOption('e', OptionParameter.Optional)]
        [LongOption("echo", OptionParameter.Required)]
        public string OptionE {
            set => Logs.Enqueue($"E:{value ?? "default"}");
        }

        [NonOptionArgument]
        public void NonOptionArgument(string value) => Logs.Enqueue($"NOA:{value}");
    }

    public static class DefaultStyle {
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
                "-ax",
                "-bc",
                "charlie",
                "--bravo",
                "-d",
                "-dd",
                "--delta=d",
                "-e",
                "echo",
                "--echo",
                "echo",
                "--echo=echo",
            };
            var result = new CommandLineParser<DefaultStyleTestOptions>().Parse(args);
            AssertLogQueue(result.Logs, "A:x", "B:True", "C:charlie", "B:True", "D:default", "D:d", "D:d", "E:default", "NOA:echo", "E:echo", "E:echo");
        }
    }
}
