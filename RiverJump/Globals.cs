using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RiverJump;

public static class Globals
{
    public static float Time {get; set;}
    public static Game1 Game {get; set;}
    public static ContentManager Content {get; set;}
    public static SpriteBatch SpriteBatch {get; set;}
    public static GraphicsDevice GraphicsDevice {get; set;}
    public static Microsoft.Xna.Framework.Point WindowSize {get; set;}
    public static Viewport Viewport {get; set;}

    public static void Update(GameTime gt)
    {
        Time = (float)gt.ElapsedGameTime.TotalSeconds;
    }

}
