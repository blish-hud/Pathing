using System;

namespace BhModule.Community.Pathing.Scripting {
    public struct ScriptMessage {

        public DateTime Timestamp { get; }
        public string   Message   { get; }

        /// <summary>
        /// The source script that triggered the message.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// The message log level.
        /// </summary>
        public ScriptMessageLogLevel LogLevel { get; }

        public ScriptMessage(string message, string source, DateTime timestamp, ScriptMessageLogLevel logLevel = ScriptMessageLogLevel.Info) {
            this.Message   = message;
            this.Source    = source;
            this.Timestamp = timestamp;
            this.LogLevel  = logLevel;
        }

    }
}
