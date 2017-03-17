using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using Paloma;

namespace Project2
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        MD3 model = new MD3();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        BasicEffect basicEffect;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.Alpha = 1.0f;
            basicEffect.LightingEnabled = true;
            basicEffect.DirectionalLight0.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
            basicEffect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);
            basicEffect.DirectionalLight0.SpecularColor = new Vector3(0.5f, 0.5f, 0.5f);
            basicEffect.DirectionalLight0.Direction = new Vector3(MathHelper.ToRadians(-45f), MathHelper.ToRadians(20f), MathHelper.ToRadians(30f));
            basicEffect.Texture = this.model.lowerModel.textures.
            basicEffect.TextureEnabled = true;
            basicEffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), GraphicsDevice.DisplayMode.AspectRatio, 1f, 1000f);
            basicEffect.View = Matrix.CreateLookAt(new Vector3(0, 0, -200f), new Vector3(0, 0, 0), Vector3.Up);
            basicEffect.World = Matrix.CreateWorld(new Vector3(0, 0, -100f), Vector3.Forward, Vector3.Up);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Model[] model = new Model[4];

            Console.WriteLine("Enter the name of the file: ");
            string modelFile = Console.ReadLine();
            StreamReader fs = new StreamReader(modelFile);

            try
            {
                int i = 0, index = 0;
                while(fs.Peek() > -1)
                {
                    string file = fs.ReadLine();
                    if (file.EndsWith(".md3"))
                    {
                        model[i] = new Model();
                        model[i].LoadModel(file);
                        
                    }
                    else if(file.EndsWith(".skin"))
                    {
                        StreamReader skinFs = new StreamReader(file);

                        while (skinFs.Peek() > -1)
                        {
                            string skinFile = skinFs.ReadLine();
                            if (!skinFile.StartsWith("tag_"))
                            {
                                string name = skinFile.Remove(skinFile.IndexOf(','));

                                skinFile = skinFile.Substring(skinFile.IndexOf(',') + 1);
                                model[i].textures.Add(Model.LoadTexture(GraphicsDevice, skinFile));
                                Console.WriteLine("Read skin: {0}", skinFile);

                                for (int j = 0; j < model[i].header.meshCount; j++)
                                    if (model[i].meshes[j].header.name.Equals(name))
                                        model[i].meshes[j].texture = index;

                                index++;
                            }
                        }
                        skinFs.Close();
                        i++;
                    }
                }
                
                fs.Close();

                this.model.lowerModel = model[0];
                this.model.upperModel = model[1];
                this.model.headModel = model[2];
                this.model.gunModel = model[3];

                this.model.LoadAnimation(modelFile);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            model.Render(basicEffect, GraphicsDevice);
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
