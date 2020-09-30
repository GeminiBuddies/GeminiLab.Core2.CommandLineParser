namespace Exam {
    public interface IConfigurable<in TConfig> {
        void Config(TConfig config);
    }
}
