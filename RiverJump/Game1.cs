using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RiverJump;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private GameManager _gameManager;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Globals.Game = this;
        Globals.WindowSize = new(1024,1024);//new(Map.TILES.GetLength(1) * Map.TILE_SIZE, Map.TILES.GetLength(0) * Map.TILE_SIZE);
        Globals.Viewport = new(0, 0, Globals.WindowSize.X, Globals.WindowSize.Y);
        _graphics.PreferredBackBufferWidth = Globals.WindowSize.X;
        _graphics.PreferredBackBufferHeight = Globals.WindowSize.Y;
        _graphics.ApplyChanges();
        
        // TODO: Add your initialization logic here
        
        Globals.Content = Content;
        base.Initialize();
    }

    protected override void LoadContent()
    {
        Globals.SpriteBatch = new SpriteBatch(GraphicsDevice);
        Globals.GraphicsDevice = GraphicsDevice;
        
        NewGame();
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        Globals.Update(gameTime);
        _gameManager.Update(gameTime);

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.SkyBlue);

        _gameManager.Draw(gameTime);

        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }

    public void NewGame()
    {
        _gameManager = new();
    }
}
