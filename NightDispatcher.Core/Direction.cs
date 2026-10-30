using System;

namespace NightDispatcher.Core;

[Flags]
public enum Direction
{
    None = 0,
    Up = 1,
    Right = 2,
    Down = 4,
    Left = 8
}
