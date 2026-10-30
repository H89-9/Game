namespace NightDispatcher.Core;

public enum TileType
{
    Empty,
    Wire,
    Generator,
    House
}

public class Tile
{
    public TileType Type { get; }
    public Direction Connections { get; private set; }
    public bool IsPowered { get; internal set; }

    public Tile(TileType type, Direction connections = Direction.None)
    {
        Type = type;
        Connections = connections;
    }

    public bool HasConnection(Direction direction)
    {
        return direction != Direction.None && (Connections & direction) == direction;
    }

    public bool Rotate()
    {
        if (Type != TileType.Wire)
            return false;

        Direction rotated = Direction.None;
        if (HasConnection(Direction.Up)) rotated |= Direction.Right;
        if (HasConnection(Direction.Right)) rotated |= Direction.Down;
        if (HasConnection(Direction.Down)) rotated |= Direction.Left;
        if (HasConnection(Direction.Left)) rotated |= Direction.Up;

        Connections = rotated;
        return true;
    }
}
