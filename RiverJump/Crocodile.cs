using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverJump;

public class Crocodile : Component
{
    #region Properties
    public Texture2D Open { get; set; }
    public Texture2D Close { get; set; }
    public Vector2 Position { get; set; }
    public Rectangle Rectangle { get; set; }
    #endregion

    public Crocodile(Texture2D _Open, Texture2D _Close, Vector2 _Position, Rectangle _Rectangle)
    {
        Open = _Open;
        Close = _Close;
        Position = _Position;
        Rectangle = _Rectangle;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (Globals.Game._gameManager._hero.position.Y > Rectangle.Top)
            spriteBatch.Draw(Open, Rectangle, Color.White);
        else
            spriteBatch.Draw(Close, Rectangle, Color.White);
    }

    public override void Update(GameTime gameTime) { }
}