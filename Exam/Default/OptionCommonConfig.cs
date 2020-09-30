namespace Exam {
    public class OptionCommonConfig {
        public string Prefix { get; set; }
        public bool IgnoreUnknownOption { get; set; } = true;
        public string DefaultValueForOptionalParameter { get; set; } = null;
        public string DefaultValueForRequiredParameter { get; set; } = null;
        public bool ContinueWhenFailedToGetRequiredParameter { get; set; } = true;
    }
}
