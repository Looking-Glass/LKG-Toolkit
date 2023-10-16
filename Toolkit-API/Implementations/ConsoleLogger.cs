using System;

namespace ToolkitAPI {
    public class ConsoleLogger : ILogger {
        public void Dispose()
        {
            // nothing to dispose
        }

        public void Log(string message) {
            Console.WriteLine(message);
        }

        public void LogException(Exception e) {
            Console.WriteLine(e.ToString());
        }
    }
}
