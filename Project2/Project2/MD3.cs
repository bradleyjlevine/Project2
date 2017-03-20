using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Project2
{
    public struct Animation
    {
        public int startFrame;
        public int endFrame;
        public int nextFrame;
        public float interpolation;
        public int fps;
        public int currentFrame;
    };

    public class MD3
    {
        public Model lowerModel;
        public Model upperModel;
        public Model headModel;
        public Model gunModel;
        public Animation[] animations;
        public int currentAnimation;
        bool TEST = false;

        public void LoadAnimation(string f)
        {
            animations = new Animation[25];

            try
            {
                //opens model.txt
                StreamReader modelFile = new StreamReader(f);

                //starts reading the model.txt file
                while (modelFile.Peek() > -1)
                {
                    //gets a line
                    string file = modelFile.ReadLine();

                    //checks if it is the animation configuration file
                    if (file.EndsWith("animation.cfg"))
                    {
                        StreamReader animationFile = new StreamReader(file);
                        int i = 0;

                        //starts reading the animation configuration file
                        while (animationFile.Peek() > -1 && i < 25)
                        {
                            string line = animationFile.ReadLine();

                            if (!line.StartsWith("//") && line.Length > 0 && Char.IsDigit(line[0]))
                            {
                                char[] symbol = { '\t' };

                                //breaks up the line in to 4 or more tokens
                                string[] tokens = line.Split(symbol);

                                //sets the information for the animation
                                if (tokens.Length >= 4)
                                {
                                    animations[i].startFrame = Convert.ToInt32(tokens[0]);
                                    animations[i].currentFrame = animations[i].startFrame;
                                    animations[i].nextFrame = Convert.ToInt32(animations[i].currentFrame + 1);
                                    animations[i].endFrame = animations[i].startFrame + Convert.ToInt32(tokens[1]) - 1;
                                    animations[i].fps = Convert.ToInt32(tokens[3]);
                                }
                                else
                                {
                                    throw new Exception("Error in file: animation.cfg");
                                }

                                i++;
                            }
                        }

                        animationFile.Close();
                    }
                }

                if(TEST)Console.WriteLine("Read animation.cfg");

                //goes back and makes changes to LEGS_WALKCR animation
                for (AnimationTypes i = AnimationTypes.LEGS_WALKCR; i < AnimationTypes.MAX_ANIMATIONS; i++)
                    animations[(int)i].startFrame -= animations[(int)AnimationTypes.TORSO_GESTURE].startFrame;

                modelFile.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        public void SetAnimation(AnimationTypes a)
        {
            currentAnimation = (int)a;

            if (a <= AnimationTypes.BOTH_DEAD3) //both models
            {
                lowerModel.startFrame = animations[currentAnimation].startFrame;
                lowerModel.currentFrame = animations[currentAnimation].currentFrame;
                lowerModel.endFrame = animations[currentAnimation].endFrame;
                lowerModel.nextFrame = animations[currentAnimation].nextFrame;

                upperModel.startFrame = animations[currentAnimation].startFrame;
                upperModel.currentFrame = animations[currentAnimation].currentFrame;
                upperModel.endFrame = animations[currentAnimation].endFrame;
                upperModel.nextFrame = animations[currentAnimation].nextFrame;

            }
            else if (a > AnimationTypes.BOTH_DEAD3 && a <= AnimationTypes.TORSO_STAND2) //upper only
            {
                upperModel.startFrame = animations[currentAnimation].startFrame;
                upperModel.currentFrame = animations[currentAnimation].currentFrame;
                upperModel.endFrame = animations[currentAnimation].endFrame;
                upperModel.nextFrame = animations[currentAnimation].nextFrame;
            }
            else if (a > AnimationTypes.TORSO_STAND2) //lowerModel only
            {
                lowerModel.startFrame = animations[currentAnimation].startFrame;
                lowerModel.currentFrame = animations[currentAnimation].currentFrame;
                lowerModel.endFrame = animations[currentAnimation].endFrame;
                lowerModel.nextFrame = animations[currentAnimation].nextFrame;
            }
        }

        public void Update(float elapsedSeconds)
        {
            float frameFraction = elapsedSeconds * animations[currentAnimation].fps;
            UpdateFrame(lowerModel, frameFraction);
        }

        public void UpdateFrame(Model currentModel, float frameFraction)
        {
            currentModel.interpolation = currentModel.interpolation + frameFraction;
            if (currentModel.interpolation > 1)
            {
                currentModel.interpolation = 0;
                currentModel.currentFrame = currentModel.nextFrame;
                currentModel.nextFrame++;
                if (currentModel.nextFrame > currentModel.endFrame)
                {
                    currentModel.nextFrame = currentModel.startFrame;
                }
            }
        }

        public void Render(BasicEffect basicEffect, GraphicsDevice GraphicsDevice)
        {
            Matrix currentMatrix = Matrix.Identity;
            Matrix nextMatrix = Matrix.Identity;
            DrawAllModels(basicEffect, lowerModel, currentMatrix, nextMatrix, GraphicsDevice);
        }

        public void DrawAllModels(BasicEffect basicEffect, Model currentModel, Matrix currentMatrix, Matrix nextMatrix, GraphicsDevice GraphicsDevice)
        {
            DrawModel(basicEffect, currentModel, currentMatrix, nextMatrix, GraphicsDevice);
            for (int k = 0; k < currentModel.tags.Length; k++)
            {
                if (currentModel.links != null)
                {
                    if (currentModel.links[k] != null)
                    {
                        int currentTag = currentModel.currentFrame * currentModel.tags.Length + k;
                        int nextTag = currentModel.nextFrame * currentModel.tags.Length + k;

                        currentMatrix = currentModel.tags[currentTag].rotation * currentMatrix;
                        nextMatrix = currentModel.tags[nextTag].rotation * nextMatrix;
                        DrawAllModels(basicEffect, currentModel.links[k], currentMatrix, nextMatrix, GraphicsDevice);
                    }
                }
            }
        }

        public void DrawModel(BasicEffect basicEffect, Model currentModel, Matrix currentMatrix, Matrix nextMatrix, GraphicsDevice GraphicsDevice)
        {
            VertexPositionNormalTexture[] meshVertices;
            Texture2D currentTexture;
            int currentOffset, nextOffset;

            for (int i = 0; i < currentModel.meshes.Length; i++)
            {
                if (currentModel.meshes[i].texture != -1)
                {
                    currentTexture = currentModel.textures[i];
                    currentOffset = currentModel.currentFrame * (currentModel.meshes[i].header.vertexCount);
                    nextOffset = currentModel.nextFrame * (currentModel.meshes[i].header.vertexCount);
                    meshVertices = new VertexPositionNormalTexture[currentModel.meshes[i].header.triangleCount * 3];

                    for (int triangleNumber = 0, k = 0; triangleNumber < currentModel.meshes[i].header.triangleCount; triangleNumber++)
                    {
                        for (int vertexNumber = 0; vertexNumber < 3; vertexNumber++, k++)
                        {
                            int currentVertex = triangleNumber * 3 + vertexNumber;
                            Vector4 shortcutVertex1 = currentModel.meshes[i].vertices[currentVertex + currentOffset].vertex;
                            Vector3 currentVertexPos = new Vector3(shortcutVertex1.X, shortcutVertex1.Y, shortcutVertex1.Z);
                            Vector4 shortcutVertex2 = currentModel.meshes[i].vertices[currentVertex + nextOffset].vertex;
                            Vector3 nextVertexPos = new Vector3(shortcutVertex2.X, shortcutVertex2.Y, shortcutVertex2.Z);

                            currentVertexPos = Vector3.Transform(currentVertexPos, currentMatrix);
                            nextVertexPos = Vector3.Transform(nextVertexPos, nextMatrix);

                            int currentNormalIndex0 = currentModel.meshes[i].vertices[currentVertex + currentOffset].normal[0];
                            int currentNormalIndex1 = currentModel.meshes[i].vertices[currentVertex + currentOffset].normal[1];
                            Vector3 currentNormal = new Vector3(Model.normals[currentNormalIndex0, currentNormalIndex1].X, Model.normals[currentNormalIndex0, currentNormalIndex1].Y, Model.normals[currentNormalIndex0, currentNormalIndex1].Z);

                            int nextNormalIndex0 = currentModel.meshes[i].vertices[currentVertex + nextOffset].normal[0];
                            int nextNormalIndex1 = currentModel.meshes[i].vertices[currentVertex + nextOffset].normal[1];
                            Vector3 nextNormal = new Vector3(Model.normals[nextNormalIndex0, nextNormalIndex1].X, Model.normals[nextNormalIndex0, nextNormalIndex1].Y, Model.normals[nextNormalIndex0, nextNormalIndex1].Z);

                            currentNormal = Vector3.TransformNormal(currentNormal, currentMatrix);
                            nextNormal = Vector3.TransformNormal(nextNormal, nextMatrix);

                            Vector3 finalVertexPos = Vector3.Lerp(currentVertexPos, nextVertexPos, 0.5f);
                            Vector3 finalNormal = Vector3.Lerp(currentNormal, nextNormal, 0.5f);

                            //i believe this is what we want
                            meshVertices[currentVertex] = new VertexPositionNormalTexture(finalVertexPos, finalNormal, currentModel.meshes[i].textureCoordinates[currentModel.meshes[i].triangleVertices[k]]);
                        }
                    }
                    VertexBuffer vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), meshVertices.Length, BufferUsage.WriteOnly);
                    vertexBuffer.SetData(meshVertices);
                    GraphicsDevice.SetVertexBuffer(vertexBuffer);
                    basicEffect.Texture = currentTexture;

                    foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, meshVertices, 0, meshVertices.Length / 3);
                    }

                }


            }
        }
    }
}