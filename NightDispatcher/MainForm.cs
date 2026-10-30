using System;
using System.Drawing;
using System.Windows.Forms;
using NightDispatcher.Core;

namespace NightDispatcher;

public class MainForm : Form
{
    private readonly Panel screen = new() { Dock = DockStyle.Fill };
    private readonly System.Windows.Forms.Timer gameTimer = new() { Interval = 1000 };
    private GameController? controller;
    private BoardView? boardView;
    private Label? movesLabel;
    private Label? timeLabel;
    private Label? resultLabel;

    private static readonly Color BackgroundColor = Color.FromArgb(243, 244, 246);
    private static readonly Color TextColor = Color.FromArgb(43, 51, 59);
    private static readonly Color AccentColor = Color.FromArgb(53, 87, 110);

    public MainForm()
    {
        Text = "Ночной диспетчер: Энергосеть";
        ClientSize = new Size(940, 620);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = BackgroundColor;
        Font = new Font("Segoe UI", 10f);
        Controls.Add(screen);

        gameTimer.Tick += (_, _) => controller?.Tick();
        ShowMainMenu();
    }

    private void ClearScreen()
    {
        gameTimer.Stop();
        controller = null;
        boardView = null;
        movesLabel = null;
        timeLabel = null;
        resultLabel = null;

        while (screen.Controls.Count > 0)
        {
            Control old = screen.Controls[0];
            screen.Controls.RemoveAt(0);
            old.Dispose();
        }
    }

    private Label AddLabel(string text, int x, int y, int width, int height,
        float size = 12f, bool bold = false, Color? color = null)
    {
        var label = new Label
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            ForeColor = color ?? TextColor,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular)
        };
        screen.Controls.Add(label);
        return label;
    }

    private Button AddButton(string text, int x, int y, int width, int height,
        Action action, bool enabled = true)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            Enabled = enabled,
            FlatStyle = FlatStyle.Flat,
            BackColor = enabled ? Color.White : Color.FromArgb(232, 234, 237),
            ForeColor = enabled ? TextColor : Color.Gray,
            Cursor = enabled ? Cursors.Hand : Cursors.Default,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(188, 195, 201);
        button.FlatAppearance.BorderSize = 1;
        button.Click += (_, _) => action();
        screen.Controls.Add(button);
        return button;
    }

    private void ShowMainMenu()
    {
        ClearScreen();
        AddLabel("НОЧНОЙ ДИСПЕТЧЕР", 255, 148, 520, 55, 27f, true);
        AddLabel("Энергосеть", 378, 209, 260, 40, 18f, false, AccentColor);
        AddLabel("Восстановите подачу электричества в городе.", 278, 280, 500, 35, 11f);
        AddButton("Начать играть", 348, 360, 244, 48, ShowLevelSelect);
        AddButton("Выход", 348, 424, 244, 48, Close);
    }

    private void ShowLevelSelect()
    {
        ClearScreen();
        AddLabel("Выбор уровня", 72, 57, 600, 51, 24f, true);
        AddLabel("Открыт первый уровень. Остальные будут добавлены позднее.", 74, 109, 760, 32, 11f);

        for (int index = 0; index < 10; index++)
        {
            int number = index + 1;
            int x = 72 + (index % 5) * 160;
            int y = 200 + (index / 5) * 104;
            string caption = number == 1 ? "Уровень 1" : $"Уровень {number}\nНедоступен";
            AddButton(caption, x, y, 143, 72, ShowFirstLevel, number == 1);
        }

        AddButton("Назад", 72, 505, 144, 45, ShowMainMenu);
    }

    private void ShowFirstLevel()
    {
        ClearScreen();
        controller = new GameController();

        AddLabel("Уровень 1. Восстановление линии", 62, 38, 760, 47, 21f, true);
        AddLabel("Поверните провода так, чтобы генератор был соединён с домом.",
            63, 91, 800, 32, 11f);

        boardView = new BoardView(controller.State)
        {
            Location = new Point(62, 168)
        };
        boardView.CellClicked += (x, y) => controller?.Rotate(x, y);
        screen.Controls.Add(boardView);

        AddLabel("ЗАДАЧА", 540, 174, 300, 30, 14f, true);
        AddLabel("Нажмите на провод, чтобы повернуть его на 90°.",
            540, 214, 325, 60, 10f);
        movesLabel = AddLabel("", 540, 301, 300, 32, 12f);
        timeLabel = AddLabel("", 540, 345, 300, 32, 12f);
        resultLabel = AddLabel("", 540, 400, 330, 60, 11f, true);

        AddButton("Заново", 540, 478, 145, 45, () => controller?.Restart());
        AddButton("К уровням", 702, 478, 145, 45, ShowLevelSelect);

        controller.StateChanged += (_, _) => RefreshGame();
        RefreshGame();
        gameTimer.Start();
    }

    private void RefreshGame()
    {
        if (controller == null || boardView == null || movesLabel == null ||
            timeLabel == null || resultLabel == null) return;

        GameState state = controller.State;
        boardView.State = state;
        boardView.Invalidate();

        movesLabel.Text = $"Ходы: {state.Moves} / {state.MaxMoves}";
        timeLabel.Text = $"Время: {state.SecondsRemaining / 60}:{state.SecondsRemaining % 60:00}";

        if (state.Status == GameStatus.Won)
        {
            gameTimer.Stop();
            resultLabel.ForeColor = Color.FromArgb(37, 123, 88);
            resultLabel.Text = "Электричество восстановлено!\nУровень пройден.";
        }
        else if (state.Status == GameStatus.Lost)
        {
            gameTimer.Stop();
            resultLabel.ForeColor = Color.FromArgb(160, 72, 72);
            resultLabel.Text = "Попытка закончилась.\nНажмите «Заново».";
        }
        else
        {
            resultLabel.ForeColor = TextColor;
            resultLabel.Text = "Соедините все провода.";
        }

        if (state.Status == GameStatus.Playing && !gameTimer.Enabled)
            gameTimer.Start();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        gameTimer.Dispose();
        base.OnFormClosed(e);
    }
}
