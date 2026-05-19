using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RiverJump;

public class Wave(Texture2D texture, Vector2 position) : Sprite(texture, position)
{
    private const float SPEED = 150f;

    private void UpdatePosition()
    {
        var newPos = position + (Vector2.UnitY * -SPEED * Globals.Time);

        position = newPos;
    }

    private void PlayerCollision()
    {
        
    }

    public void Update()
    {
        UpdatePosition();
    }
}
