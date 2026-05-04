using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverJump;

public class Map
{
    private readonly RenderTarget2D _target;
    public static readonly int TILE_SIZE = 128;
    public static readonly int[,] TILES =
    {
        {1,1,1,1,1,1,1,1,1,1},
        {1,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,1},
        {1,0,0,2,0,0,2,0,0,1},
        {1,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,2,2,2,0,1},
        {1,0,0,2,2,1,1,1,2,1},
        {1,2,2,1,1,1,1,1,1,1},
    };

    private static Rectangle[,] Colliders { get; } = new Rectangle[TILES.GetLength(0), TILES.GetLength(1)];
    public Map()
    {
        _target = new(Globals.GraphicsDevice, TILES.GetLength(1) * TILE_SIZE, TILES.GetLength(0) * TILE_SIZE);

        var tile1tex = Globals.Content.Load<Texture2D>("tile1");
        var tile2tex = Globals.Content.Load<Texture2D>("tile2");

        Globals.GraphicsDevice.SetRenderTarget(_target);
        Globals.GraphicsDevice.Clear(Color.Transparent);
        Globals.SpriteBatch.Begin();

        for (int x = 0; x < TILES.GetLength(0); x++)
        {
            for (int y = 0; y < TILES.GetLength(1); y++)
            {
                if (TILES[x,y] == 0) continue;
                var posX = y * TILE_SIZE;
                var posY = x * TILE_SIZE;
                var tex = TILES[x,y] == 1? tile1tex : tile2tex;
                Globals.SpriteBatch.Draw(tex, new Vector2(posX , posY), Color.White);
                Colliders[x, y] = new(posX, posY, TILE_SIZE, TILE_SIZE);
            }
        }

        Globals.SpriteBatch.End();
        Globals.GraphicsDevice.SetRenderTarget(null);
    }

    public static List<Rectangle> GetNearestColliders(Rectangle bounds)
    {
        int leftTile = (int)Math.Floor((float)bounds.Left / TILE_SIZE);
        int rightTile = (int)Math.Ceiling((float)bounds.Right / TILE_SIZE) - 1;
        int topTile = (int)Math.Floor((float)bounds.Top / TILE_SIZE);
        int bottomTile = (int)Math.Ceiling((float)bounds.Bottom / TILE_SIZE) - 1;

        leftTile = MathHelper.Clamp(leftTile, 0, TILES.GetLength(1));
        rightTile = MathHelper.Clamp(rightTile, 0, TILES.GetLength(1));
        topTile = MathHelper.Clamp(topTile, 0, TILES.GetLength(0));
        bottomTile = MathHelper.Clamp(bottomTile, 0, TILES.GetLength(0));

        List<Rectangle> result = [];

        for (int x = topTile; x <= bottomTile; x++)
        {
            for (int y = leftTile; y <= rightTile; y++)
            {
                if (TILES[x, y] != 0) result.Add(Colliders[x, y]);
            }
        }

        return result;
    }
    public void Draw()
    {
        Globals.SpriteBatch.Draw(_target, Vector2.Zero, Color.White);
    }
}
