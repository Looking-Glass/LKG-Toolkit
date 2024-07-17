using System;

namespace LookingGlass.Toolkit {
    [Flags]
    public enum BridgeLoggingFlags : int {
        None = 0,
        Timing = 1,
        Messages = 2,
        Responses = 4,

        All = ~0
    }
}
