using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RiverJump;

public class Wave(Texture2D texture, Vector2 position) : Sprite(texture, position)
{
    private const float SPEED = 150f;
    private float delay = 3f;

    //

    private static readonly Vector2 WavingMovement = new(5f,2f);
    private Vector3[] _movementByLines = new Vector3[tilingY];
    private const byte tilingX = 8;
    private const byte tilingY = 3;

    //

    private void UpdatePosition()
    {
        var newPos = position + (Vector2.UnitY * -SPEED * Globals.Time);

        position = newPos;
    }

    private void UpdateWaves()
    {
        for (byte i = 0; i < tilingY; i++)
        {
            if (i % 2 == 0)
            {
                _movementByLines[i].X += WavingMovement.X;
                if (_movementByLines[i].X >= Texture.Width/2) _movementByLines[i].X -= Texture.Width;
            }
            else
            {
                _movementByLines[i].X -= WavingMovement.X;
                if (_movementByLines[i].X <= -Texture.Width/2) _movementByLines[i].X += Texture.Width;
            }

            if (_movementByLines[i].Z < 1)
            {
                _movementByLines[i].Y -= WavingMovement.Y * i * .75f;
                if (_movementByLines[i].Y <= -Texture.Height * .15f) _movementByLines[i].Z = 1;
            }
            else
            {
                _movementByLines[i].Y += WavingMovement.Y * i * .75f;
                if (_movementByLines[i].Y >= Texture.Height * .15f) _movementByLines[i].Z = 0;
            }
        }
    }

    public void Update()
    {
        UpdateWaves();

        if (delay > 0)
            delay -= Globals.Time;
        else
            UpdatePosition();
    }

    public override void Draw()
    {
        Vector2 tilingStart = new(position.X - (tilingX / 2 * Texture.Width/2), position.Y);

        for (byte j = 0; j < tilingY; j++)
        {
            for (byte i = 0; i < tilingX; i++)
            {
                Globals.SpriteBatch.Draw(Texture, 
                tilingStart + new Vector2(i * Texture.Width/2, j * Texture.Height/3) + 
                new Vector2(_movementByLines[j].X, _movementByLines[j].Y), Color.White);
            }
        }
    }
}
