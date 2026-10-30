using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using NightDispatcher.Core;

namespace NightDispatcher;

public class BoardView : Control
{
    public const int CellSize = 70;

    public GameState State { get; set; }
    public event Action<int, int>? CellClicked;

    public BoardView(GameState state)
    {
        State = state;
        Size = new Size(state.Width * CellSize, state.Height * CellSize);
        BackColor = Color.FromArgb(231, 233, 236);
        DoubleBuffered = true;
        Cursor = Cursors.Hand;
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button != MouseButtons.Left) return;

        int x = e.X / CellSize;
        int y = e.Y / CellSize;
        if (x >= 0 && x < State.Width && y >= 0 && y < State.Height)
            CellClicked?.Invoke(x, y);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var unpoweredPen = new Pen(Color.FromArgb(102, 115, 126), 8);
        using var poweredPen = new Pen(Color.FromArgb(43, 148, 111), 8);
        using var linePen = new Pen(Color.FromArgb(204, 209, 215), 1);
        using var regularBrush = new SolidBrush(Color.FromArgb(248, 249, 250));
        using var poweredBrush = new SolidBrush(Color.FromArgb(227, 243, 234));
        using var nodeBrush = new SolidBrush(Color.FromArgb(102, 115, 126));
        using var greenBrush = new SolidBrush(Color.FromArgb(43, 148, 111));
        using var sourceBrush = new SolidBrush(Color.FromArgb(212, 160, 68));
        using var labelFont = new Font("Segoe UI", 7f, FontStyle.Bold);

        unpoweredPen.StartCap = unpoweredPen.EndCap = LineCap.Round;
        poweredPen.StartCap = poweredPen.EndCap = LineCap.Round;

        for (int y = 0; y < State.Height; y++)
        for (int x = 0; x < State.Width; x++)
        {
            Tile tile = State.GetTile(x, y);
            int left = x * CellSize;
            int top = y * CellSize;
            int centerX = left + CellSize / 2;
            int centerY = top + CellSize / 2;

            e.Graphics.FillRectangle(tile.IsPowered ? poweredBrush : regularBrush,
                left + 1, top + 1, CellSize - 2, CellSize - 2);
            e.Graphics.DrawRectangle(linePen, left, top, CellSize - 1, CellSize - 1);

            if (tile.Type == TileType.Empty) continue;

            Pen wire = tile.IsPowered ? poweredPen : unpoweredPen;
            DrawConnections(e.Graphics, wire, tile, centerX, centerY);

            if (tile.Type == TileType.Generator)
            {
                e.Graphics.FillEllipse(sourceBrush, centerX - 13, centerY - 13, 26, 26);
                DrawCellLabel(e.Graphics, "ГЕНЕРАТОР", labelFont, left, top);
            }
            else if (tile.Type == TileType.House)
            {
                e.Graphics.FillRectangle(tile.IsPowered ? greenBrush : nodeBrush,
                    centerX - 12, centerY - 12, 24, 24);
                DrawCellLabel(e.Graphics, "ДОМ", labelFont, left, top);
            }
            else
            {
                e.Graphics.FillEllipse(tile.IsPowered ? greenBrush : nodeBrush,
                    centerX - 5, centerY - 5, 10, 10);
            }
        }
    }

    private static void DrawConnections(Graphics g, Pen pen, Tile tile, int x, int y)
    {
        int half = CellSize / 2;
        if (tile.HasConnection(Direction.Up)) g.DrawLine(pen, x, y, x, y - half);
        if (tile.HasConnection(Direction.Right)) g.DrawLine(pen, x, y, x + half, y);
        if (tile.HasConnection(Direction.Down)) g.DrawLine(pen, x, y, x, y + half);
        if (tile.HasConnection(Direction.Left)) g.DrawLine(pen, x, y, x - half, y);
    }

    private static void DrawCellLabel(Graphics g, string text, Font font, int left, int top)
    {
        TextRenderer.DrawText(g, text, font,
            new Rectangle(left + 2, top + 49, CellSize - 4, 18),
            Color.FromArgb(48, 59, 69),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }
}
