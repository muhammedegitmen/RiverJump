using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverJump.Controls;

namespace RiverJump;

public class GameManager
{
    public Map _map;
    public readonly Hero _hero;
    private readonly Wave _wave;
    private readonly Camera _camera;
    private readonly List<Component> _gameComponents;
    private readonly Texture2D _pixel;

    private static int levelIndex = 0;
    private static float totalTimeSpend = 0;
    private const byte LEVEL_AMOUNT = 3;

    private float timeSpend = 0;
    private bool LevelEnded { get; set; }
    public GameManager()
    {
        if (levelIndex == 0)    totalTimeSpend = 0;
        
        _map = new(levelIndex, out Component[] additions);
        _hero = new(Globals.Content.Load<Texture2D>("hero"), _map.playerSpawn);
        _wave = new(Globals.Content.Load<Texture2D>("wave"), _map.playerSpawn + (Vector2.UnitY * Map.TILE_SIZE * 2));

        _camera = new(Globals.Viewport)
        {
            Follow = _hero,
            Position = _hero.position
        };

        _pixel = new Texture2D(Globals.GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);

        // Components

        var font = Globals.Content.Load<SpriteFont>("font");
        var fontBig = Globals.Content.Load<SpriteFont>("fontBig");
        var retryButton = new Button(Globals.Content.Load<Texture2D>("button"), font)
        {
            Text = "Retry",
            Active = false
        };
        retryButton.Position = new Vector2( 
            (Globals.WindowSize.X/2) - (retryButton._texture.Width/2), 
            (Globals.WindowSize.Y*2/3) - (retryButton._texture.Height/2) );
        retryButton.Click += RestartTheLevel;

        var nextButton = new Button(Globals.Content.Load<Texture2D>("button"), font)
        {
            Text = "Next",
            Active = false
        };
        nextButton.Position = new Vector2( 
            (Globals.WindowSize.X*2/3) - (nextButton._texture.Width/2), 
            (Globals.WindowSize.Y*2/3) - (nextButton._texture.Height/2) );
        nextButton.Click += NextLevel;

        //
        var gameTimer = new SimpleText(font)
        {
            Text = "0",
            Active = true
        };
        gameTimer.Position = new Vector2( 
            (64) - (gameTimer.Size.X/2), 
            (64) - (gameTimer.Size.Y/2) );

        //
        var levelScore = new SimpleText(fontBig)
        {
            Text = "Level Completion Time",
            PenColour = Color.White,
            Active = false
        };
        levelScore.Position = new Vector2( 
            (Globals.WindowSize.X/2) - (levelScore.Size.X/2), 
            (Globals.WindowSize.Y*1/3) - (levelScore.Size.Y/2) );
        var scoreTimer = new SimpleText(fontBig)
        {
            Text = "0",
            PenColour = Color.White,
            Active = false
        };
        scoreTimer.Position = new Vector2( 
            (Globals.WindowSize.X/2) - (scoreTimer.Size.X/2), 
            (Globals.WindowSize.Y*1/3) - (scoreTimer.Size.Y/2) + 64 );
        var gameScore = new SimpleText(fontBig)
        {
            Text = "You Win!",
            PenColour = Color.White,
            Active = false
        };
        gameScore.Position = new Vector2( 
            (Globals.WindowSize.X/2) - (gameScore.Size.X/2), 
            (Globals.WindowSize.Y*1/3) - (gameScore.Size.Y/2) - 192 );
        var totalTimer = new SimpleText(fontBig)
        {
            Text = "0",
            PenColour = Color.White,
            Active = false
        };
        totalTimer.Position = new Vector2( 
            (Globals.WindowSize.X/2) - (totalTimer.Size.X/2), 
            (Globals.WindowSize.Y*1/3) - (totalTimer.Size.Y/2) - 128);

        // --v
        _gameComponents =
        [
            retryButton,
            gameTimer,
            scoreTimer,
            nextButton,
            levelScore,
            gameScore,
            totalTimer,
            .. additions,
        ];
    }

    public void Update(GameTime gameTime)
    {
        foreach (var component in _gameComponents)
            if (component.Active)   component.Update(gameTime);

        if (LevelEnded) return;

        _hero.Update();
        _wave.Update();
        _camera.UpdateCamera(Globals.Viewport);

        //

        timeSpend += Globals.Time;
        (_gameComponents[1] as SimpleText).Text = timeSpend.ToString("f1").Replace(",",".");

        if (LoseCondition)  Losing();
        if (WinCondition)   Wining();
    }

    public void Draw(GameTime gameTime)
    {
        Globals.SpriteBatch.Begin(transformMatrix : _camera.Transform);

        _map.Draw();
        _hero.Draw();

        foreach (var component in _gameComponents)
            if (component.Active && !component.UI)   component.Draw(gameTime, Globals.SpriteBatch);

        _wave.Draw();

        Globals.SpriteBatch.End();
        if (LevelEnded)
        {
            // === Transparent ===
            Globals.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            Globals.SpriteBatch.Draw(_pixel, new Rectangle(0, 0, Globals.Viewport.Width, Globals.Viewport.Height), Color.Black * 0.5f);

            Globals.SpriteBatch.End();
        }
        // === UI ===
        Globals.SpriteBatch.Begin();

        foreach (var component in _gameComponents)
            if (component.Active && component.UI)   component.Draw(gameTime, Globals.SpriteBatch);

        Globals.SpriteBatch.End();
    }

    //

    public bool LoseCondition => _hero.position.Y >= _wave.position.Y;
    public bool WinCondition => _hero.position.Y + (Map.TILE_SIZE * 2) <= _map.winThreshold;

    public void EndGame()
    {
        LevelEnded = true;

        _gameComponents[0].Active = true;
        _gameComponents[1].Active = false;
        _gameComponents[2].Active = true;
        _gameComponents[4].Active = true;

        (_gameComponents[2] as SimpleText).Text = (_gameComponents[1] as SimpleText).Text;
    }

    public void Wining()
    {
        EndGame();

        (_gameComponents[0] as Button).Text = "Again";
        //(_gameComponents[2] as SimpleText).Text = "Completion Time\n" + (_gameComponents[2] as SimpleText).Text;

        _gameComponents[3].Active = true;

        var retryButton = (_gameComponents[0] as Button);
        retryButton.Position = new Vector2( 
            (Globals.WindowSize.X*1/3) - (retryButton._texture.Width/2), 
            (Globals.WindowSize.Y*2/3) - (retryButton._texture.Height/2) );

        //

        if (levelIndex + 1 == LEVEL_AMOUNT)
        // Last level
        {
            (_gameComponents[3] as Button).Text = "Restart";

            _gameComponents[5].Active = true;
            _gameComponents[6].Active = true;
            (_gameComponents[6] as SimpleText).Text = (totalTimeSpend + timeSpend).ToString("f1");
            // (_gameComponents[2] as SimpleText).Text = "You win with the total time of\n" + (totalTimeSpend + timeSpend).ToString("f1")
            //  + "\nCompletion Time\n" + timeSpend.ToString("f1");
        }
    }

    public void Losing()
    {
        EndGame();
        
        (_gameComponents[4] as SimpleText).Text = "Croaked";
        // (_gameComponents[2] as SimpleText).Text = "Croaked\n" + (_gameComponents[2] as SimpleText).Text;
    }

    public void RestartTheLevel(object sender, System.EventArgs e)
    {
        Globals.Game.NewGame();
    }
    public void NextLevel(object sender, System.EventArgs e)
    {
        if (levelIndex + 1 == LEVEL_AMOUNT)
            levelIndex = 0;
        else
            { levelIndex++; totalTimeSpend += timeSpend; }

        Globals.Game.NewGame();
    }
    public void Final()
    {
        
    }

    public void DeleteComponent(Func<Component, bool> Filter)
    {
        for (byte i = 0; i < _gameComponents.Count; i++)
        {
            if (Filter.Invoke(_gameComponents[i]))
            {
                _gameComponents[i].Active = false;
                _gameComponents.RemoveAt(i);
                return;
            }
        }
    }
}
