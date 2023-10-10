﻿using System;

namespace ToolkitAPI {
    public interface ILogger : IDisposable {
        public void Log(object obj) => Log(obj == null ? "null" : obj.ToString());
        public void Log(string message);
        public void LogException(Exception e);
    }
}
