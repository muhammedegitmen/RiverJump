using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RiverJump.Controls;

public class SimpleImage : Component
{
    #region Fields

    public Texture2D _texture;

    #endregion

    #region Properties

    public Vector2 Position { get; set; }

    public Rectangle Rectangle
    {
        get
        {
            return new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);
        }
    }

    #endregion

    #region Methods

    public SimpleImage(Texture2D texture) => _texture = texture;

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) => spriteBatch.Draw(_texture, Rectangle, Color.White);

    public override void Update(GameTime gameTime) {}

    #endregion
}
