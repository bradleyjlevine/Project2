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

        Matrix current = Matrix.Identity;
        Matrix next = Matrix.Identity;

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

                Console.WriteLine("Read animation.cfg");

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

        public void Update()
        {

        }

        public void Render(BasicEffect basicEffect)
        {
            DrawAllModels(lowerModel, current, next);
        }

        public void DrawAllModels(Model lowModel, Matrix currentMatrix, Matrix nextMatrix)
        {
            DrawModel(lowModel, currentMatrix, nextMatrix);
        }

        public void DrawModel(Model currentModel, Matrix currentMatrix, Matrix nextMatrix)
        {
            VertexPositionNormalTexture[] meshVertices;
            Texture2D currentTexture;
            VertexPositionNormalTexture[] currentMeshVertices;
            int currentOffset, nextOffset;

            for(int i=0; i<currentModel.meshes.Length; i++)
            {
                if (currentModel.meshes[i].texture != -1)
                {
                    currentTexture = currentModel.textures[currentModel.meshes[i].texture];
                    currentOffset = currentModel.currentFrame * (currentModel.meshes[i].header.vertexCount);
                    nextOffset = currentModel.nextFrame * (currentModel.meshes[i].header.vertexCount);

                    currentMeshVertices = new VertexPositionNormalTexture[currentModel.meshes[i].header.triangleCount*3];
                    for(int triangleNumber = 0; triangleNumber < currentModel.meshes[i].header.triangleCount; triangleNumber++)
                    {
                        for(int vertexNumber=0; vertexNumber<3; vertexNumber++)
                        {

                        }
                    }
                }


            }
        }
    }
}