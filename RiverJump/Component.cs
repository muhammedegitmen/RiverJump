using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverJump;

public abstract class Component
{
    public bool Active { get; set; } = true;
    public bool UI { get; set; } = true;
    public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

    public abstract void Update(GameTime gameTime);
}
