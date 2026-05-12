using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RiverJump;

public class Map
{
    private readonly RenderTarget2D _target;
    public static readonly int TILE_SIZE = 128;
    public static int[,] TILES;
    public Vector2 playerSpawn;

    private static Rectangle[,] Colliders;
    public Map(int levelIndex)
    {
        TILES = ConvertCsvToIntMatrix($"Content/LevelMap_{levelIndex}.csv");
        
        _target = new(Globals.GraphicsDevice, TILES.GetLength(1) * TILE_SIZE, TILES.GetLength(0) * TILE_SIZE);

        Texture2D[] texs =
        [
            Globals.Content.Load<Texture2D>("tile1"),
            Globals.Content.Load<Texture2D>("tile2"),
            Globals.Content.Load<Texture2D>("jumpShroom")
        ];

        Globals.GraphicsDevice.SetRenderTarget(_target);
        Globals.GraphicsDevice.Clear(Color.Transparent);
        Globals.SpriteBatch.Begin();

        Colliders = new Rectangle[TILES.GetLength(0), TILES.GetLength(1)];

        for (int x = 0; x < TILES.GetLength(0); x++)
        {
            for (int y = 0; y < TILES.GetLength(1); y++)
            {
                if (TILES[x,y] == 0) continue;
                if (TILES[x,y] == -1)
                {
                    playerSpawn = new(y * TILE_SIZE, x * TILE_SIZE);
                    continue;
                }

                var tex = texs[TILES[x,y] - 1];

                var posX = (y * TILE_SIZE) + ((TILE_SIZE - tex.Bounds.Width) * .5f);
                var posY = (x * TILE_SIZE) + (TILE_SIZE - tex.Bounds.Height);

                Globals.SpriteBatch.Draw(tex, new Vector2(posX , posY), Color.White);
                Colliders[x, y] = new((int)posX, posY, tex.Bounds.Width, tex.Bounds.Height);
            }
        }

        Globals.SpriteBatch.End();
        Globals.GraphicsDevice.SetRenderTarget(null);
    }
    private static int[,] ConvertCsvToIntMatrix(string filePath)
    {
        // Read all lines into a string array
        string[] lines = File.ReadAllLines(filePath);
        
        if (lines.Length == 0) return new int[0, 0];

        // Determine dimensions based on the first line
        int rowCount = lines.Length;
        int colCount = lines[0].Split(',').Length;
        
        int[,] matrix = new int[rowCount, colCount];

        for (int i = 0; i < rowCount; i++)
        {
            string[] values = lines[i].Split(',');
            for (int j = 0; j < colCount; j++)
            {
                // Parse strings to integers
                if (int.TryParse(values[j].Trim(), out int result))
                {
                    matrix[i, j] = result;
                }
            }
        }

        return matrix;
    }

    public static List<(Rectangle, int)> GetNearestColliders(Rectangle bounds)
    {
        int leftTile = (int)Math.Floor((float)bounds.Left / TILE_SIZE);
        int rightTile = (int)Math.Ceiling((float)bounds.Right / TILE_SIZE) - 1;
        int topTile = (int)Math.Floor((float)bounds.Top / TILE_SIZE);
        int bottomTile = (int)Math.Ceiling((float)bounds.Bottom / TILE_SIZE) - 1;

        leftTile = MathHelper.Clamp(leftTile, 0, TILES.GetLength(1));
        rightTile = MathHelper.Clamp(rightTile, 0, TILES.GetLength(1));
        topTile = MathHelper.Clamp(topTile, 0, TILES.GetLength(0));
        bottomTile = MathHelper.Clamp(bottomTile, 0, TILES.GetLength(0));

        List<(Rectangle, int)> result = [];

        for (int x = topTile; x <= bottomTile; x++)
        {
            for (int y = leftTile; y <= rightTile; y++)
            {
                if (TILES.GetLength(0) > x && TILES.GetLength(1) > y && TILES[x,y] != 0) result.Add((Colliders[x,y], TILES[x,y]));
            }
        }

        return result;
    }
    public void Draw()
    {
        Globals.SpriteBatch.Draw(_target, Vector2.Zero, Color.White);
    }
}
