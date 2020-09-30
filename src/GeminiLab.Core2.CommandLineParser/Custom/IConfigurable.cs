namespace GeminiLab.Core2.CommandLineParser.Custom {
    public interface IConfigurable<in TConfig> {
        void Config(TConfig config);
    }
}
