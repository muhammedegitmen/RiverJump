using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverJump.Controls;

namespace RiverJump;

public class GameManager
{
    private Map _map;
    private readonly Hero _hero;
    private readonly Wave _wave;
    private readonly Camera _camera;
    private List<Component> _gameComponents;

    private int levelIndex = 0;
    private bool LevelEnded { get; set; }
    public GameManager()
    {
        _map = new(levelIndex);
        _hero = new(Globals.Content.Load<Texture2D>("hero"), _map.playerSpawn);
        _wave = new(Globals.Content.Load<Texture2D>("wave"), _map.playerSpawn + (Vector2.UnitY * Map.TILE_SIZE * 2));

        _camera = new(Globals.Viewport)
        {
            Follow = _hero,
            Position = _hero.position
        };

        // Components

        var retryButton = new Button(Globals.Content.Load<Texture2D>("button"), Globals.Content.Load<SpriteFont>("font"))
        {
            Position = new Vector2( 350, 250 ),
            Text = "Retry",
            Active = false
        };
        retryButton.Click += RestartTheLevel;

        _gameComponents = new()
        {
            retryButton
        };
    }

    public void Update(GameTime gameTime)
    {
        foreach (var component in _gameComponents)
            if (component.Active)   component.Update(gameTime);

        if (LevelEnded) return;

        _hero.Update();
        _camera.UpdateCamera(Globals.Viewport);

        if (LoseCondition)  Losing();
        if (WinCondition)   Wining();
    }

    public void Draw(GameTime gameTime)
    {
        Globals.SpriteBatch.Begin(transformMatrix : _camera.Transform);

        _map.Draw();
        _hero.Draw();

        foreach (var component in _gameComponents)
            if (component.Active)   component.Draw(gameTime, Globals.SpriteBatch);

        Globals.SpriteBatch.End();
    }

    //

    public bool LoseCondition => _hero.position.Y >= _wave.position.Y;
    public bool WinCondition => _hero.position.Y <= _map.winThreshold;

    public void EndGame()
    {
        LevelEnded = true;

        _gameComponents[0].Active = true;
    }

    public void Wining()
    {
        EndGame();


    }

    public void Losing()
    {
        EndGame();


    }

    public void RestartTheLevel(object sender, System.EventArgs e)
    {
        
    }
}
