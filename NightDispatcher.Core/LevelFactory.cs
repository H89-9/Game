namespace NightDispatcher.Core;

public static class LevelFactory
{
    public static GameState CreateFirstLevel()
    {
        Tile[,] cells = new Tile[6, 5];
        for (int x = 0; x < 6; x++)
        for (int y = 0; y < 5; y++)
            cells[x, y] = new Tile(TileType.Empty);

        // Генератор и дом нельзя поворачивать.
        cells[0, 2] = new Tile(TileType.Generator, Direction.Right);
        cells[5, 2] = new Tile(TileType.House, Direction.Left);

        // Между ними проложена цепь, но часть проводов повернута неправильно.
        cells[1, 2] = new Tile(TileType.Wire, Direction.Up | Direction.Right);
        cells[1, 1] = new Tile(TileType.Wire, Direction.Down | Direction.Left);
        cells[2, 1] = new Tile(TileType.Wire, Direction.Up | Direction.Down);
        cells[3, 1] = new Tile(TileType.Wire, Direction.Right | Direction.Down);
        cells[3, 2] = new Tile(TileType.Wire, Direction.Left | Direction.Up);
        cells[4, 2] = new Tile(TileType.Wire, Direction.Up | Direction.Down);

        return new GameState(cells, maxMoves: 20, seconds: 180);
    }
}
