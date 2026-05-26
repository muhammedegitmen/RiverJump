using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RiverJump.Controls;

public class SimpleText : Component
{
    #region Fields

    public SpriteFont _font;

    #endregion

    #region Properties

    public Color PenColour { get; set; }

    public Vector2 Position { get; set; }

    public Vector2 Size { get; set; } = new(256, 32);

    public Rectangle Rectangle
    {
        get
        {
            return new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        }
    }

    public string Text { get; set; }

    #endregion

    #region Methods

    public SimpleText(SpriteFont font)
    {
        _font = font;

        PenColour = Color.Black;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (!string.IsNullOrEmpty(Text))
        {
            var x = (Rectangle.X + (Rectangle.Width / 2) - (_font.MeasureString(Text).X / 2));
            var y = (Rectangle.Y + (Rectangle.Height / 2) - (_font.MeasureString(Text).Y / 2));

            spriteBatch.DrawString(_font, Text, new Vector2(x, y), PenColour);
        }
    }

    public override void Update(GameTime gameTime) {}

    #endregion
}
