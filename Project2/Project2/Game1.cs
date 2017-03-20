/*
 * Authors:  Bradley Levine and Aidan Helm
 * Project 2:  Quaking in Your Boots
 * Files: Game1.cs, Program.cs, Model.cs, MD3.cs, TargaImage.cs, AnimationTypes.cs
 */

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
        //global variables
        MD3 model = new MD3();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        BasicEffect basicEffect;
        bool TEST = false;
        
        //constructor
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

            //gets file from user to load models
            Console.WriteLine("Enter the name of the file: ");
            string modelFile = Console.ReadLine();
            StreamReader fs = new StreamReader(modelFile);

            //starts reading file and looks for .md3 and .skin files
            try
            {
                int i = 0, index = 0;
                while (fs.Peek() > -1)
                {
                    string file = fs.ReadLine();
                    if (file.EndsWith(".md3"))
                    {
                        model[i] = new Model();
                        model[i].LoadModel(file);

                    }
                    else if (file.EndsWith(".skin"))
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
                                if(TEST)Console.WriteLine("Read skin: {0}", skinFile);

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

                //copies over the models into MD3 object
                this.model.lowerModel = model[0];
                this.model.upperModel = model[1];
                this.model.headModel = model[2];
                this.model.gunModel = model[3];

                //loads the animations 
                this.model.LoadAnimation(modelFile);

                //links the models
                this.model.lowerModel.Link("tag_torso", this.model.upperModel);
                this.model.upperModel.Link("tag_head", this.model.headModel);
                this.model.upperModel.Link("tag_weapon", this.model.gunModel);
            }
            catch (Exception e)
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
            
            //Translation Controls (W=forward, A=left, S=back, D=right, Q=up, E=down
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                basicEffect.View = basicEffect.View * Matrix.CreateTranslation(0, 0, 1);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                basicEffect.View = basicEffect.View * Matrix.CreateTranslation(0, 0, -1);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                basicEffect.View = basicEffect.View * Matrix.CreateTranslation(1, 0, 0);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                basicEffect.View = basicEffect.View * Matrix.CreateTranslation(-1, 0, 0);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                basicEffect.View = basicEffect.View * Matrix.CreateTranslation(0, -1, 0);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                basicEffect.View = basicEffect.View * Matrix.CreateTranslation(0, 1, 0);
            }



            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                basicEffect.View = basicEffect.View * Matrix.CreateRotationY(-0.05f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                basicEffect.View = basicEffect.View * Matrix.CreateRotationY(0.05f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                basicEffect.View = basicEffect.View * Matrix.CreateRotationX(-0.05f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                basicEffect.View = basicEffect.View * Matrix.CreateRotationX(+0.05f);
            }
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

            //draws model
            model.Update(gameTime.ElapsedGameTime.Milliseconds / 1000.0f);
            model.Render(basicEffect, GraphicsDevice);
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
