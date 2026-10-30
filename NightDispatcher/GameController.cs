using System;
using NightDispatcher.Core;

namespace NightDispatcher;

public class GameController
{
    public GameState State { get; private set; }
    public event EventHandler? StateChanged;

    public GameController()
    {
        State = LevelFactory.CreateFirstLevel();
    }

    public void Rotate(int x, int y)
    {
        if (State.RotateTile(x, y)) StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Tick()
    {
        if (State.AdvanceSecond()) StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Restart()
    {
        State = LevelFactory.CreateFirstLevel();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
