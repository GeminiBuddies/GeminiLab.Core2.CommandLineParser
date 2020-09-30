using System;
using System.Collections;
using System.Collections.Generic;
using GeminiLab.Core2.Collections;

namespace Exam {
    [AttributeUsage(SupportedTargets, AllowMultiple = true)]
    public class ShortOptionAttribute : OptionAttribute {
        public ShortOptionAttribute(char option) {
            Option = option;
        }

        public char Option { get; set; }
    }

    public class ShortOptionConfig : OptionCommonConfig { }

    public class ShortOptionCategory : IOptionCategory<ShortOptionAttribute>, IConfigurable<ShortOptionConfig> {
        public bool Match(string item) {
            return true;
        }

        public int Consume(ReadOnlySpan<string> args, object target) {
            Console.WriteLine($"get {args[0]}");
            return 1;
        }

        public IEnumerable<IOptionCategory<ShortOptionAttribute>.Option> Options {
            set {
                value.ForEach(v => Console.WriteLine(v.Attribute.Option));
            }
        }

        public void Config(ShortOptionConfig config) {
            Console.WriteLine(config.Prefix);
        }
    }

    [AttributeUsage(SupportedTargets, AllowMultiple = true)]
    public class LongOptionAttribute : OptionAttribute {
        public string Option { get; set; }
    }

    class A {
        [ShortOption('f')] private string F;
    }

    class Program {
        public static int Main(string[] args) {
            var parser = new CommandLineParser<A>();

            parser.Parse("-f");

            return 0;
        }
    }
}
