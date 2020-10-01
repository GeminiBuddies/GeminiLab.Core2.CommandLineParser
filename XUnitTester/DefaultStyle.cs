using System.Collections.Generic;
using GeminiLab.Core2.CommandLineParser;
using GeminiLab.Core2.CommandLineParser.Default;
using Xunit;

namespace XUnitTester {
    public class TestOptions {
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

        [ShortOption('d', OptionParameter.Optional, Default = "d-fault")]
        [LongOption("delta", OptionParameter.Optional, Default = "delta-fault")]
        public string OptionD {
            set => Logs.Enqueue($"D:{value}");
        }

        [ShortOption('e', OptionParameter.Optional, Default = "e-fault")]
        [LongOption("echo", OptionParameter.Required)]
        public string OptionE {
            set => Logs.Enqueue($"E:{value}");
        }
    }

    public static class Test {
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
                "--echo",
                "echo",
                "--echo=echo",
            };
            var result = new CommandLineParser<TestOptions>().Parse(args);
            AssertLogQueue(result.Logs, "A:x", "B:True", "C:charlie", "B:True", "D:d-fault", "D:d", "D:d", "E:e-fault", "E:echo", "E:echo");
        }
    }
}
