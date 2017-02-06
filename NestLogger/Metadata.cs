using System;

namespace NESTLogger
{
    public class Metadata
    {
        public DateTimeOffset CreatedTimestamp { get; set; }

        [String(Analyzer = "keyword")]
        public string Type { get; internal set; }

        [String(Analyzer = "keyword")]
        public string Env { get; internal set; }

        [String(Analyzer = "keyword")]
        public string System { get; internal set; }
    }
}