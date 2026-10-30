using NightDispatcher.Core;
using NUnit.Framework;

namespace NightDispatcher.Tests;

[TestFixture]
public class GameStateTests
{
    private static GameState ThreeCellBoard(Direction middle, int maxMoves = 10, int seconds = 60)
    {
        Tile[,] tiles = new Tile[3, 1];
        tiles[0, 0] = new Tile(TileType.Generator, Direction.Right);
        tiles[1, 0] = new Tile(TileType.Wire, middle);
        tiles[2, 0] = new Tile(TileType.House, Direction.Left);
        return new GameState(tiles, maxMoves, seconds);
    }

    [Test]
    public void WireRotatesClockwise()
    {
        var tile = new Tile(TileType.Wire, Direction.Up | Direction.Right);
        Assert.That(tile.Rotate(), Is.True);
        Assert.That(tile.Connections, Is.EqualTo(Direction.Right | Direction.Down));
    }

    [Test]
    public void FourRotationsRestoreOriginalShape()
    {
        var tile = new Tile(TileType.Wire, Direction.Up | Direction.Left);
        for (int i = 0; i < 4; i++) tile.Rotate();
        Assert.That(tile.Connections, Is.EqualTo(Direction.Up | Direction.Left));
    }

    [Test]
    public void GeneratorCannotRotate()
    {
        var tile = new Tile(TileType.Generator, Direction.Right);
        Assert.That(tile.Rotate(), Is.False);
        Assert.That(tile.Connections, Is.EqualTo(Direction.Right));
    }

    [Test]
    public void HouseIsNotPoweredThroughUnconnectedWire()
    {
        GameState game = ThreeCellBoard(Direction.Up | Direction.Down);
        Assert.That(game.GetTile(0, 0).IsPowered, Is.True);
        Assert.That(game.GetTile(2, 0).IsPowered, Is.False);
        Assert.That(game.Status, Is.EqualTo(GameStatus.Playing));
    }

    [Test]
    public void TurningWireConnectsHouseAndWins()
    {
        GameState game = ThreeCellBoard(Direction.Up | Direction.Down);
        Assert.That(game.RotateTile(1, 0), Is.True);
        Assert.That(game.GetTile(2, 0).IsPowered, Is.True);
        Assert.That(game.Status, Is.EqualTo(GameStatus.Won));
        Assert.That(game.Moves, Is.EqualTo(1));
    }

    [Test]
    public void CannotTurnGeneratorOrOutsideBoard()
    {
        GameState game = ThreeCellBoard(Direction.Up | Direction.Down);
        Assert.That(game.RotateTile(0, 0), Is.False);
        Assert.That(game.RotateTile(-1, 0), Is.False);
        Assert.That(game.RotateTile(3, 1), Is.False);
        Assert.That(game.Moves, Is.Zero);
    }

    [Test]
    public void MoveLimitEndsGameIfHouseNotConnected()
    {
        GameState game = ThreeCellBoard(Direction.Up | Direction.Right, maxMoves: 1);
        Assert.That(game.RotateTile(1, 0), Is.True);
        Assert.That(game.Status, Is.EqualTo(GameStatus.Lost));
        Assert.That(game.RotateTile(1, 0), Is.False);
    }

    [Test]
    public void TimerEndsGame()
    {
        GameState game = ThreeCellBoard(Direction.Up | Direction.Down, seconds: 1);
        Assert.That(game.AdvanceSecond(), Is.True);
        Assert.That(game.SecondsRemaining, Is.Zero);
        Assert.That(game.Status, Is.EqualTo(GameStatus.Lost));
        Assert.That(game.AdvanceSecond(), Is.False);
    }

    [Test]
    public void LevelOneCanBeSolvedWithinMoveLimit()
    {
        GameState game = LevelFactory.CreateFirstLevel();
        Assert.That(game.Status, Is.EqualTo(GameStatus.Playing));

        // Поворачиваем провода на заранее известной правильной трассе.
        Rotate(game, 1, 2, 3);
        Rotate(game, 1, 1, 3);
        Rotate(game, 2, 1, 1);
        Rotate(game, 3, 1, 1);
        Rotate(game, 3, 2, 1);
        Rotate(game, 4, 2, 1);

        Assert.That(game.Status, Is.EqualTo(GameStatus.Won));
        Assert.That(game.GetTile(5, 2).IsPowered, Is.True);
        Assert.That(game.Moves, Is.LessThanOrEqualTo(game.MaxMoves));
    }

    private static void Rotate(GameState game, int x, int y, int times)
    {
        for (int i = 0; i < times; i++)
            Assert.That(game.RotateTile(x, y), Is.True);
    }
}
