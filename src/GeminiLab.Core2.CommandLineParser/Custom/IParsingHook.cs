namespace GeminiLab.Core2.CommandLineParser.Custom {
    public enum ParsingEvent {
        PreParsing,
        PostParsing,
    }
    
    public interface IParsingHook {
        void OnParsingEvent(ParsingEvent parsingEvent, object target);
    }
}
