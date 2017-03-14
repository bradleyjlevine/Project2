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
                    }
                }

                Console.WriteLine("Read animation.cfg");

                //goes back and makes changes to LEGS_WALKCR animation
                for (AnimationTypes i = AnimationTypes.LEGS_WALKCR; i < AnimationTypes.MAX_ANIMATIONS; i++)
                    animations[(int)i].startFrame -= animations[(int)AnimationTypes.TORSO_GESTURE].startFrame;
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
    }
}