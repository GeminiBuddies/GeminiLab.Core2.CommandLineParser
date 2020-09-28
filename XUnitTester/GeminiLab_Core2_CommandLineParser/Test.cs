using System.Collections.Generic;
using System.Threading;
using GeminiLab.Core2;
using GeminiLab.Core2.CommandLineParser;
using GeminiLab.Core2.GetOpt;
using Xunit;

namespace XUnitTester.GeminiLab_Core2_CommandLineParser {
    public class TestOptions {
        public Queue<string> Logs { get; } = new Queue<string>();

        [Option(Option = 'a', LongOption = "alpha")]
        public string OptionA {
            set => Logs.Enqueue($"A:{value}");
        }

        [Option(Option = 'b', LongOption = "bravo")]
        public bool OptionB {
            set => Logs.Enqueue($"B:{value}");
        }
        [Option(Option = 'c')]
        public string OptionC {
            set => Logs.Enqueue($"C:{value}");
        }

        [Option(Option = 'd', LongOption = "delta")]
        public string[] OptionD {
            set => Logs.Enqueue($"D:{value.JoinBy(",")}");
        }

        [Option(LongOption = "echo")]
        public string OptionE {
            set => Logs.Enqueue($"E:{value}");
        }

        [GetOptErrorHandler]
        public void Handler(GetOptError err, GetOptResult res) {
            if (err != GetOptError.UnknownOption) Logs.Enqueue($"!:{err}");
        }

        [GetOptErrorHandler]
        public bool HandlerPlus(GetOptError err, GetOptResult res) {
            if (err == GetOptError.UnknownOption) {
                Logs.Enqueue("!!!!!!");
                return true;
            }

            return false;
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
            var result = CommandLineParser<TestOptions>.Parse("--echo", "echo", "--bravo", "--delta", "DE!", "FF!", "--alpha", "-bbb", "-b", "bb", "--crash-now-baby");
            AssertLogQueue(result.Logs, "E:echo", $"B:{true}", "D:DE!,FF!", $"!:{GetOptError.ValueExpected}", $"!:{GetOptError.UnexpectedAttachedValue}", $"B:{true}", $"!:{GetOptError.UnexpectedValue}", "!!!!!!");
        }
    }
}
