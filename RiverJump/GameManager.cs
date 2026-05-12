using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverJump;

public class GameManager
{
    private Map _map;
    private readonly Hero _hero;
    private readonly Camera _camera;

    private int levelIndex = 0;
    public GameManager()
    {
        _map = new(levelIndex);
        _hero = new(Globals.Content.Load<Texture2D>("hero"), _map.playerSpawn);

        _camera = new(Globals.Viewport);
        _camera.Follow = _hero;
        _camera.Position = _hero.position;
    }

    public void Update()
    {
        _hero.Update();
        _camera.UpdateCamera(Globals.Viewport);
    }

    public void Draw()
    {
        Globals.SpriteBatch.Begin(transformMatrix : _camera.Transform);
        _map.Draw();
        _hero.Draw();
        Globals.SpriteBatch.End();
    }
}
