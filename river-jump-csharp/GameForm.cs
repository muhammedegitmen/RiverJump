using System.Drawing.Drawing2D;

namespace RiverJump;

public sealed class GameForm : Form
{
    private const int WorldWidth = 480;
    private const int WorldHeight = 5600;
    private const float Gravity = 0.62f;
    private const float Jump = -14.8f;
    private const float HighJump = -21.4f;
    private const float WindJump = -18.7f;
    private const float MoveSpeed = 1.16f;
    private const float Friction = 0.9f;
    private const float MaxXSpeed = 8.4f;

    private readonly System.Windows.Forms.Timer timer = new();
    private readonly Random random = new();
    private readonly List<Platform> platforms = [];
    private readonly List<Particle> particles = [];
    private readonly HashSet<Keys> keys = [];

    private Player player = new();
    private float cameraY;
    private int score;
    private int bestScore;
    private GameState state = GameState.Menu;

    public GameForm()
    {
        Text = "River Jump";
        ClientSize = new Size(WorldWidth, 760);
        MinimumSize = new Size(360, 560);
        DoubleBuffered = true;
        KeyPreview = true;
        BackColor = Color.FromArgb(188, 238, 255);
        Font = new Font("Trebuchet MS", 10, FontStyle.Bold);

        bestScore = LoadBestScore();
        ResetWorld();

        timer.Interval = 16;
        timer.Tick += (_, _) =>
        {
            UpdateGame();
            Invalidate();
        };
        timer.Start();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        keys.Add(e.KeyCode);
        if (state != GameState.Playing && e.KeyCode is Keys.Space or Keys.Enter)
        {
            StartGame();
        }

        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        keys.Remove(e.KeyCode);
        base.OnKeyUp(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (state != GameState.Playing)
        {
            StartGame();
        }

        base.OnMouseDown(e);
    }

    private void StartGame()
    {
        score = 0;
        state = GameState.Playing;
        ResetWorld();
    }

    private void ResetWorld()
    {
        player = new Player
        {
            X = WorldWidth / 2f,
            Y = WorldHeight - 75,
            Width = 42,
            Height = 46,
            VelocityY = Jump,
            Facing = 1
        };

        cameraY = WorldHeight - ClientSize.Height;
        platforms.Clear();
        particles.Clear();
        BuildPlatforms();
    }

    private void BuildPlatforms()
    {
        platforms.Add(new Platform(PlatformType.Log, WorldWidth / 2f - 80, WorldHeight - 28, 160, 24));

        var y = WorldHeight - 150f;
        while (y > 130)
        {
            var roll = random.NextDouble();
            var width = roll > 0.82 ? 92 : 118 + (float)random.NextDouble() * 40;
            var x = 26 + (float)random.NextDouble() * (WorldWidth - width - 52);
            var type = PlatformType.Log;

            if (roll > 0.90)
            {
                type = PlatformType.Wind;
            }
            else if (roll > 0.77)
            {
                type = PlatformType.Mushroom;
            }
            else if (roll > 0.62 && y < WorldHeight - 650)
            {
                type = PlatformType.Gator;
            }
            else if (roll > 0.54)
            {
                type = PlatformType.Wave;
            }

            platforms.Add(new Platform(type, x, y, width, type == PlatformType.Gator ? 28 : 22));
            y -= 98 + (float)random.NextDouble() * 54;
        }

        platforms.Add(new Platform(PlatformType.Finish, 92, 46, WorldWidth - 184, 26));
    }

    private void UpdateGame()
    {
        if (state != GameState.Playing)
        {
            return;
        }

        var direction = 0f;
        if (keys.Contains(Keys.Left) || keys.Contains(Keys.A))
        {
            direction -= MoveSpeed;
        }

        if (keys.Contains(Keys.Right) || keys.Contains(Keys.D))
        {
            direction += MoveSpeed;
        }

        player.VelocityX += direction;
        player.VelocityX *= Friction;
        player.VelocityX = Math.Clamp(player.VelocityX, -MaxXSpeed, MaxXSpeed);

        if (Math.Abs(player.VelocityX) > 0.15f)
        {
            player.Facing = Math.Sign(player.VelocityX);
        }

        player.VelocityY += Gravity;
        player.X += player.VelocityX;
        player.Y += player.VelocityY;

        if (player.X < -player.Width)
        {
            player.X = WorldWidth;
        }

        if (player.X > WorldWidth)
        {
            player.X = -player.Width;
        }

        if (player.WindTicks > 0)
        {
            player.WindTicks--;
            player.VelocityY -= 0.18f;
            AddBurst(player.X, player.Y + player.Height, Color.FromArgb(210, Color.White), 1);
        }

        CollidePlatforms();

        var targetCamera = player.Y - ClientSize.Height * 0.42f;
        cameraY += (targetCamera - cameraY) * 0.14f;
        cameraY = Math.Clamp(cameraY, 0, WorldHeight - ClientSize.Height);

        score = Math.Max(score, Math.Max(0, (int)((WorldHeight - player.Y) / 10)));
        if (score > bestScore)
        {
            bestScore = score;
            SaveBestScore(bestScore);
        }

        for (var i = particles.Count - 1; i >= 0; i--)
        {
            var particle = particles[i];
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.VelocityY += 0.12f;
            particle.Life--;

            if (particle.Life <= 0)
            {
                particles.RemoveAt(i);
            }
        }

        if (player.Y - cameraY > ClientSize.Height + 90)
        {
            state = GameState.Lost;
        }

        if (player.Y < 88)
        {
            state = GameState.Won;
        }
    }

    private void CollidePlatforms()
    {
        var feet = player.Y + player.Height / 2f;
        var previousFeet = feet - player.VelocityY;

        foreach (var platform in platforms)
        {
            if (platform.Broken)
            {
                continue;
            }

            var hitX = player.X + player.Width / 2f > platform.X &&
                       player.X - player.Width / 2f < platform.X + platform.Width;
            var crossingTop = previousFeet <= platform.Y && feet >= platform.Y;
            var closeY = feet >= platform.Y && feet <= platform.Y + platform.Height + 18;

            if (!hitX || !closeY)
            {
                continue;
            }

            if (platform.Type == PlatformType.Wave)
            {
                state = GameState.Lost;
                return;
            }

            if (platform.Type == PlatformType.Gator && !crossingTop)
            {
                state = GameState.Lost;
                return;
            }

            if (player.VelocityY <= 0 || !crossingTop)
            {
                continue;
            }

            player.Y = platform.Y - player.Height / 2f;

            switch (platform.Type)
            {
                case PlatformType.Mushroom:
                    player.VelocityY = HighJump;
                    AddBurst(player.X, platform.Y, Color.FromArgb(246, 95, 95), 18);
                    break;
                case PlatformType.Wind:
                    player.VelocityY = WindJump;
                    player.WindTicks = 180;
                    AddBurst(player.X, platform.Y, Color.White, 24);
                    break;
                case PlatformType.Gator:
                    player.VelocityY = Jump * 0.95f;
                    platform.Broken = true;
                    AddBurst(platform.X + platform.Width / 2f, platform.Y, Color.FromArgb(103, 141, 88), 28);
                    break;
                case PlatformType.Finish:
                    state = GameState.Won;
                    break;
                default:
                    player.VelocityY = Jump;
                    AddBurst(player.X, platform.Y, Color.FromArgb(143, 211, 106), 8);
                    break;
            }

            return;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        DrawBackground(graphics);

        graphics.TranslateTransform(0, -cameraY);
        foreach (var platform in platforms)
        {
            DrawPlatform(graphics, platform);
        }

        DrawPlayer(graphics);
        DrawParticles(graphics);
        graphics.ResetTransform();

        DrawHud(graphics);
        if (state != GameState.Playing)
        {
            DrawOverlay(graphics);
        }
    }

    private void DrawBackground(Graphics graphics)
    {
        using var sky = new LinearGradientBrush(ClientRectangle, Color.FromArgb(220, 249, 255), Color.FromArgb(112, 199, 232), 90);
        graphics.FillRectangle(sky, ClientRectangle);

        using var waveBrush = new SolidBrush(Color.FromArgb(95, Color.White));
        for (var y = -20; y < ClientSize.Height + 80; y += 82)
        {
            DrawWaveLine(graphics, waveBrush, y, 12);
        }
    }

    private void DrawWaveLine(Graphics graphics, Brush brush, int y, int amp)
    {
        using var path = new GraphicsPath();
        path.StartFigure();
        path.AddLine(0, y, 0, y);
        for (var x = 0; x <= ClientSize.Width; x += 28)
        {
            path.AddBezier(x, y, x + 8, y - amp, x + 20, y - amp, x + 28, y);
        }

        path.AddLine(ClientSize.Width, y + 14, 0, y + 14);
        path.CloseFigure();
        graphics.FillPath(brush, path);
    }

    private void DrawPlatform(Graphics graphics, Platform platform)
    {
        if (platform.Broken)
        {
            return;
        }

        switch (platform.Type)
        {
            case PlatformType.Mushroom:
                DrawLog(graphics, platform);
                DrawMushroom(graphics, platform);
                break;
            case PlatformType.Wind:
                DrawWind(graphics, platform);
                break;
            case PlatformType.Gator:
                DrawGator(graphics, platform);
                break;
            case PlatformType.Wave:
                DrawHazardWave(graphics, platform);
                break;
            case PlatformType.Finish:
                DrawFinish(graphics, platform);
                break;
            default:
                DrawLog(graphics, platform);
                break;
        }
    }

    private void DrawLog(Graphics graphics, Platform platform)
    {
        using var brush = new SolidBrush(Color.FromArgb(141, 99, 63));
        using var pen = new Pen(Color.FromArgb(75, 51, 34), 3);
        graphics.FillRoundedRectangle(brush, platform.Bounds, 8);
        graphics.DrawRoundedRectangle(pen, platform.Bounds, 8);
        graphics.FillRectangle(Brushes.SaddleBrown, platform.X + 14, platform.Y + 6, platform.Width - 28, 3);
    }

    private void DrawMushroom(Graphics graphics, Platform platform)
    {
        var centerX = platform.X + platform.Width / 2f;
        using var stemBrush = new SolidBrush(Color.FromArgb(248, 239, 225));
        using var redBrush = new SolidBrush(Color.FromArgb(217, 78, 82));
        using var pen = new Pen(Color.FromArgb(36, 48, 71), 3);

        graphics.FillRoundedRectangle(stemBrush, new RectangleF(centerX - 18, platform.Y - 36, 36, 38), 8);
        graphics.DrawRoundedRectangle(pen, new RectangleF(centerX - 18, platform.Y - 36, 36, 38), 8);

        using var cap = new GraphicsPath();
        cap.AddBezier(centerX - 48, platform.Y - 28, centerX - 20, platform.Y - 82, centerX + 20, platform.Y - 82, centerX + 48, platform.Y - 28);
        cap.AddBezier(centerX + 48, platform.Y - 28, centerX + 40, platform.Y - 4, centerX - 40, platform.Y - 4, centerX - 48, platform.Y - 28);
        graphics.FillPath(redBrush, cap);
        graphics.DrawPath(pen, cap);

        foreach (var dx in new[] { -24, 0, 24 })
        {
            graphics.FillEllipse(Brushes.White, centerX + dx - 7, platform.Y - 38, 14, 14);
        }
    }

    private void DrawWind(Graphics graphics, Platform platform)
    {
        using var brush = new SolidBrush(Color.FromArgb(220, Color.White));
        using var pen = new Pen(Color.FromArgb(77, 158, 192), 3);
        graphics.FillRoundedRectangle(brush, new RectangleF(platform.X, platform.Y - 6, platform.Width, platform.Height + 12), 8);
        graphics.DrawRoundedRectangle(pen, new RectangleF(platform.X, platform.Y - 6, platform.Width, platform.Height + 12), 8);

        using var font = new Font(Font.FontFamily, 18, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.FromArgb(44, 125, 160));
        for (var i = 0; i < 5; i++)
        {
            graphics.DrawString("^", font, textBrush, platform.X + 14 + i * ((platform.Width - 28) / 4f), platform.Y - 3);
        }
    }

    private void DrawGator(Graphics graphics, Platform platform)
    {
        using var brush = new SolidBrush(Color.FromArgb(111, 168, 93));
        using var pen = new Pen(Color.FromArgb(36, 48, 71), 3);
        var points = new[]
        {
            new PointF(platform.X + 8, platform.Y + platform.Height),
            new PointF(platform.X + platform.Width - 18, platform.Y + platform.Height),
            new PointF(platform.X + platform.Width + 6, platform.Y + 10),
            new PointF(platform.X + platform.Width - 38, platform.Y + 4),
            new PointF(platform.X + 8, platform.Y + platform.Height)
        };
        graphics.FillPolygon(brush, points);
        graphics.DrawPolygon(pen, points);
        graphics.FillEllipse(Brushes.White, platform.X + platform.Width - 40, platform.Y - 3, 16, 16);
        graphics.DrawEllipse(pen, platform.X + platform.Width - 40, platform.Y - 3, 16, 16);
        graphics.FillEllipse(Brushes.Black, platform.X + platform.Width - 32, platform.Y + 4, 5, 5);
    }

    private void DrawHazardWave(Graphics graphics, Platform platform)
    {
        using var brush = new SolidBrush(Color.FromArgb(44, 125, 160));
        using var pen = new Pen(Color.FromArgb(36, 48, 71), 3);
        using var path = new GraphicsPath();
        path.StartFigure();
        path.AddLine(platform.X, platform.Y + platform.Height, platform.X, platform.Y + platform.Height);
        for (var x = platform.X; x <= platform.X + platform.Width; x += 28)
        {
            path.AddBezier(x, platform.Y + platform.Height, x + 7, platform.Y - 30, x + 21, platform.Y - 30, x + 28, platform.Y + platform.Height);
        }

        path.CloseFigure();
        graphics.FillPath(brush, path);
        graphics.DrawPath(pen, path);
    }

    private void DrawFinish(Graphics graphics, Platform platform)
    {
        using var brush = new SolidBrush(Color.FromArgb(255, 247, 223));
        using var pen = new Pen(Color.FromArgb(36, 48, 71), 4);
        graphics.FillRoundedRectangle(brush, platform.Bounds, 8);
        graphics.DrawRoundedRectangle(pen, platform.Bounds, 8);
        DrawCenteredText(graphics, "END", platform.Bounds, 18, Color.FromArgb(36, 48, 71));
    }

    private void DrawPlayer(Graphics graphics)
    {
        graphics.TranslateTransform(player.X, player.Y);
        graphics.ScaleTransform(player.Facing, 1);

        using var bodyBrush = new SolidBrush(Color.FromArgb(118, 200, 107));
        using var footBrush = new SolidBrush(Color.FromArgb(94, 183, 95));
        using var pen = new Pen(Color.FromArgb(36, 48, 71), 3);

        graphics.FillEllipse(bodyBrush, -21, -19, 42, 50);
        graphics.DrawEllipse(pen, -21, -19, 42, 50);
        graphics.FillEllipse(bodyBrush, -22, -25, 20, 20);
        graphics.FillEllipse(bodyBrush, 2, -25, 20, 20);
        graphics.DrawEllipse(pen, -22, -25, 20, 20);
        graphics.DrawEllipse(pen, 2, -25, 20, 20);
        graphics.FillEllipse(Brushes.White, -17, -21, 9, 9);
        graphics.FillEllipse(Brushes.White, 9, -21, 9, 9);
        graphics.FillEllipse(Brushes.Black, -13, -18, 4, 4);
        graphics.FillEllipse(Brushes.Black, 13, -18, 4, 4);
        graphics.DrawArc(pen, -12, -2, 24, 18, 15, 150);
        graphics.FillEllipse(footBrush, -32, 20, 22, 12);
        graphics.FillEllipse(footBrush, 10, 20, 22, 12);
        graphics.DrawEllipse(pen, -32, 20, 22, 12);
        graphics.DrawEllipse(pen, 10, 20, 22, 12);

        graphics.ResetTransform();
    }

    private void DrawParticles(Graphics graphics)
    {
        foreach (var particle in particles)
        {
            using var brush = new SolidBrush(particle.Color);
            graphics.FillEllipse(brush, particle.X - 3, particle.Y - 3, 6, 6);
        }
    }

    private void DrawHud(Graphics graphics)
    {
        using var font = new Font(Font.FontFamily, 18, FontStyle.Bold);
        using var smallFont = new Font(Font.FontFamily, 11, FontStyle.Bold);
        using var brush = new SolidBrush(Color.FromArgb(36, 48, 71));
        graphics.DrawString("River Jump", font, brush, 14, 12);
        graphics.DrawString($"{score} m", font, brush, ClientSize.Width / 2f - 38, 12);
        graphics.DrawString($"Best {bestScore} m", smallFont, brush, ClientSize.Width - 112, 18);
    }

    private void DrawOverlay(Graphics graphics)
    {
        using var overlayBrush = new SolidBrush(Color.FromArgb(190, 231, 249, 255));
        graphics.FillRectangle(overlayBrush, ClientRectangle);

        var title = state switch
        {
            GameState.Won => "Kazandin!",
            GameState.Lost => "Dusunce Bitti",
            _ => "River Jump"
        };

        var message = state switch
        {
            GameState.Won => "Nehri bitirdin. Yeniden baslamak icin tikla veya Space'e bas.",
            GameState.Lost => "Dalga, dusme veya timsah oyunu bitirdi. Tekrar dene.",
            _ => "A/D veya ok tuslari ile saga sola git. Baslamak icin tikla."
        };

        var panel = new RectangleF(ClientSize.Width / 2f - 180, ClientSize.Height / 2f - 95, 360, 190);
        using var panelBrush = new SolidBrush(Color.FromArgb(235, Color.White));
        using var pen = new Pen(Color.FromArgb(80, 36, 48, 71), 3);
        graphics.FillRoundedRectangle(panelBrush, panel, 8);
        graphics.DrawRoundedRectangle(pen, panel, 8);
        DrawCenteredText(graphics, title, new RectangleF(panel.X, panel.Y + 20, panel.Width, 50), 28, Color.FromArgb(36, 48, 71));
        DrawCenteredText(graphics, message, new RectangleF(panel.X + 20, panel.Y + 82, panel.Width - 40, 64), 12, Color.FromArgb(36, 48, 71));
        DrawCenteredText(graphics, "Baslamak icin tikla / Space", new RectangleF(panel.X, panel.Y + 145, panel.Width, 36), 13, Color.FromArgb(58, 157, 98));
    }

    private void DrawCenteredText(Graphics graphics, string text, RectangleF bounds, float size, Color color)
    {
        using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using var font = new Font(Font.FontFamily, size, FontStyle.Bold);
        using var brush = new SolidBrush(color);
        graphics.DrawString(text, font, brush, bounds, format);
    }

    private void AddBurst(float x, float y, Color color, int count)
    {
        for (var i = 0; i < count; i++)
        {
            particles.Add(new Particle
            {
                X = x,
                Y = y,
                VelocityX = ((float)random.NextDouble() - 0.5f) * 6,
                VelocityY = -(float)random.NextDouble() * 4 - 1,
                Life = 30 + random.Next(20),
                Color = color
            });
        }
    }

    private static int LoadBestScore()
    {
        var path = GetScorePath();
        if (!File.Exists(path))
        {
            return 0;
        }

        return int.TryParse(File.ReadAllText(path), out var value) ? value : 0;
    }

    private static void SaveBestScore(int value)
    {
        File.WriteAllText(GetScorePath(), value.ToString());
    }

    private static string GetScorePath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RiverJumpBestScore.txt");
    }
}

public static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, RectangleF rectangle, float radius)
    {
        using var path = CreateRoundedRectangle(rectangle, radius);
        graphics.FillPath(brush, path);
    }

    public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, RectangleF rectangle, float radius)
    {
        using var path = CreateRoundedRectangle(rectangle, radius);
        graphics.DrawPath(pen, path);
    }

    private static GraphicsPath CreateRoundedRectangle(RectangleF rectangle, float radius)
    {
        var diameter = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}

public sealed class Player
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public int Facing { get; set; } = 1;
    public int WindTicks { get; set; }
}

public sealed class Platform(PlatformType type, float x, float y, float width, float height)
{
    public PlatformType Type { get; } = type;
    public float X { get; } = x;
    public float Y { get; } = y;
    public float Width { get; } = width;
    public float Height { get; } = height;
    public bool Broken { get; set; }
    public RectangleF Bounds => new(X, Y, Width, Height);
}

public sealed class Particle
{
    public float X { get; set; }
    public float Y { get; set; }
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public int Life { get; set; }
    public Color Color { get; set; }
}

public enum PlatformType
{
    Log,
    Mushroom,
    Wind,
    Gator,
    Wave,
    Finish
}

public enum GameState
{
    Menu,
    Playing,
    Lost,
    Won
}
