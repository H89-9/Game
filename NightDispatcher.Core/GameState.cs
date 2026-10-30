using System;
using System.Collections.Generic;

namespace NightDispatcher.Core;

public enum GameStatus
{
    Playing,
    Won,
    Lost
}

public class GameState
{
    private readonly Tile[,] tiles;
    private static readonly (int dx, int dy, Direction from, Direction to)[] Neighbors =
    {
        (0, -1, Direction.Up, Direction.Down),
        (1, 0, Direction.Right, Direction.Left),
        (0, 1, Direction.Down, Direction.Up),
        (-1, 0, Direction.Left, Direction.Right)
    };

    public int Width => tiles.GetLength(0);
    public int Height => tiles.GetLength(1);
    public int Moves { get; private set; }
    public int MaxMoves { get; }
    public int SecondsRemaining { get; private set; }
    public GameStatus Status { get; private set; }

    public GameState(Tile[,] initialTiles, int maxMoves, int seconds)
    {
        if (initialTiles == null) throw new ArgumentNullException(nameof(initialTiles));
        if (initialTiles.Length == 0) throw new ArgumentException("Поле не должно быть пустым.");
        if (maxMoves <= 0 || seconds <= 0) throw new ArgumentOutOfRangeException(nameof(maxMoves));

        tiles = new Tile[initialTiles.GetLength(0), initialTiles.GetLength(1)];
        int generatorCount = 0;
        int houseCount = 0;
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        {
            Tile source = initialTiles[x, y] ?? throw new ArgumentException("Нельзя оставлять пустые ссылки в поле.");
            tiles[x, y] = new Tile(source.Type, source.Connections);
            if (source.Type == TileType.Generator) generatorCount++;
            if (source.Type == TileType.House) houseCount++;
        }

        if (generatorCount != 1 || houseCount < 1)
            throw new ArgumentException("На поле нужен один генератор и хотя бы один дом.");

        MaxMoves = maxMoves;
        SecondsRemaining = seconds;
        Status = GameStatus.Playing;
        UpdatePower();
        if (AllHousesPowered()) Status = GameStatus.Won;
    }

    public Tile GetTile(int x, int y)
    {
        if (!Inside(x, y)) throw new ArgumentOutOfRangeException(nameof(x));
        return tiles[x, y];
    }

    public bool RotateTile(int x, int y)
    {
        if (Status != GameStatus.Playing || !Inside(x, y)) return false;
        if (!tiles[x, y].Rotate()) return false;

        Moves++;
        UpdatePower();

        if (AllHousesPowered())
            Status = GameStatus.Won;
        else if (Moves >= MaxMoves)
            Status = GameStatus.Lost;

        return true;
    }

    public bool AdvanceSecond()
    {
        if (Status != GameStatus.Playing) return false;
        SecondsRemaining--;
        if (SecondsRemaining <= 0)
        {
            SecondsRemaining = 0;
            Status = GameStatus.Lost;
        }
        return true;
    }

    private bool Inside(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    private bool AllHousesPowered()
    {
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
            if (tiles[x, y].Type == TileType.House && !tiles[x, y].IsPowered)
                return false;
        return true;
    }

    private void UpdatePower()
    {
        Queue<(int x, int y)> queue = new();

        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        {
            Tile tile = tiles[x, y];
            tile.IsPowered = tile.Type == TileType.Generator;
            if (tile.Type == TileType.Generator) queue.Enqueue((x, y));
        }

        while (queue.Count > 0)
        {
            (int x, int y) = queue.Dequeue();
            Tile current = tiles[x, y];

            foreach (var direction in Neighbors)
            {
                int nx = x + direction.dx;
                int ny = y + direction.dy;
                if (!Inside(nx, ny) || !current.HasConnection(direction.from)) continue;

                Tile next = tiles[nx, ny];
                if (next.Type == TileType.Empty || next.IsPowered || !next.HasConnection(direction.to)) continue;

                next.IsPowered = true;
                queue.Enqueue((nx, ny));
            }
        }
    }
}
