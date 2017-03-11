/*
 * 
*/
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paloma;

namespace Project2
{
    public struct MD3Header
    {
        public string ID;       //  4 bytes "IDP3"
        public int version;     //  15
        public string file;     //  64 bytes
        public int flags;
        public int frameCount;
        public int tagCount;
        public int meshCount;
        public int skinCount;
        public int frameOffset;
        public int tagOffset;
        public int meshOffset;
        public int fileSize;
    };

    public struct Frame
    {
        public Vector4 minimums;
        public Vector4 maximums;
        public Vector4 position;
        public float scale;
        public string creator;      //  16 bytes
    };

    public struct Tag
    {
        public string name;     //  64 bytes
        public Vector4 position;
        public Matrix rotation;
    };

    public struct Skin
    {
        public string name;
        public int index;
    };

    public struct Vertex
    {
        public Vector4 vertex;  // stored as three 2-byte shorts
        public byte[] normal;
    };

    public struct MeshHeader
    {
        public string ID;       //  4 bytes
        public string name;     //  64 bytes
        public int flags;
        public int frameCount;
        public int skinCount;
        public int vertexCount;
        public int triangleCount;
        public int triangleOffset;
        public int skinOffset;
        public int textureVectorStart;
        public int vertexStart;
        public int meshSize;
    };

    public struct Mesh
    {
        public MeshHeader header;
        public Skin[] skins;
        public int[] triangleVertices;
        public Vector2[] textureCoordinates;
        public Vertex[] vertices;
        public int texture;
    };

    public class Model
    {
        public MD3Header header;
        public Frame[] frames;
        public Tag[] tags;
        public Mesh[] meshes;
        public Model[] links;

        int startFrame;
        int endFrame;
        int nextFrame;
        float interpolation;
        int currentFrame;

        public List<Texture2D> textures;
        static Vector4[,] normals;
        string file;

        public Model(string file)
        {
            this.file = file;
            this.textures = new List<Texture2D>();
        }

        public void LoadModel()
        {
            try
            {
                //create binary reader
                BinaryReader br = new BinaryReader(File.Open(file, FileMode.Open));

                //fill in the MD3 header struct
                header.ID = BytesToStringOp(br.ReadBytes(4));
                header.version = br.ReadInt32();
                header.file = BytesToStringOp(br.ReadBytes(64));
                header.flags = br.ReadInt32();
                header.frameCount = br.ReadInt32();
                header.tagCount = br.ReadInt32();
                header.meshCount = br.ReadInt32();
                header.skinCount = br.ReadInt32();
                header.frameOffset = br.ReadInt32();
                header.tagOffset = br.ReadInt32();
                header.meshOffset = br.ReadInt32();
                header.fileSize = br.ReadInt32();

                Console.WriteLine("Read MD3 file header.");

                frames = new Frame[header.frameCount];
                tags = new Tag[header.frameCount * header.tagCount];
                meshes = new Mesh[header.meshCount];

                br.BaseStream.Seek(header.frameOffset, SeekOrigin.Begin);

                //reading frames
                for (int i = 0; i < frames.Length; i++)
                {
                    frames[i].minimums = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), 1);
                    frames[i].maximums = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), 1);
                    frames[i].position = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), 1);
                    frames[i].scale = br.ReadSingle();
                    frames[i].creator = BytesToStringOp(br.ReadBytes(16));
                }

                Console.WriteLine("Read MD3 file frames.");

                //reading tags
                br.BaseStream.Seek(header.tagOffset, SeekOrigin.Begin);

                for (int i = 0; i < tags.Length; i++)
                {
                    tags[i].name = BytesToStringOp(br.ReadBytes(64));
                    tags[i].position = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), 1);
                    tags[i].rotation = new Matrix(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), 0,
                                                  br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), 0,
                                                  br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), 0,
                                                  0, 0, 0, 1);

                    //tags[i].rotation = Matrix.Transpose(tags[i].rotation);
                }

                Console.WriteLine("Read MD3 file tags.");

                //reading meshes
                br.BaseStream.Seek(header.meshOffset, SeekOrigin.Begin);
                long meshOffset = header.meshOffset;

                for (int i = 0; i < header.meshCount; i++)
                {
                    meshes[i].header.ID = BytesToStringOp(br.ReadBytes(4));
                    meshes[i].header.name = BytesToStringOp(br.ReadBytes(64));
                    meshes[i].header.flags = br.ReadInt32();
                    meshes[i].header.frameCount = br.ReadInt32();
                    meshes[i].header.skinCount = br.ReadInt32();
                    meshes[i].header.vertexCount = br.ReadInt32();
                    meshes[i].header.triangleCount = br.ReadInt32();
                    meshes[i].header.triangleOffset = br.ReadInt32();
                    meshes[i].header.skinOffset = br.ReadInt32();
                    meshes[i].header.textureVectorStart = br.ReadInt32();
                    meshes[i].header.vertexStart = br.ReadInt32();
                    meshes[i].header.meshSize = br.ReadInt32();

                    //reading in triangle indexes
                    br.BaseStream.Seek(meshes[i].header.triangleOffset + meshOffset, SeekOrigin.Begin);

                    meshes[i].triangleVertices = new int[meshes[i].header.triangleCount * 3];

                    for (int j = 0; j < meshes[i].triangleVertices.Length; j++)
                        meshes[i].triangleVertices[j] = br.ReadInt32();

                    //reading in skins
                    br.BaseStream.Seek(meshes[i].header.skinOffset + meshOffset, SeekOrigin.Begin);

                    meshes[i].skins = new Skin[meshes[i].header.skinCount];

                    for (int j = 0; j < meshes[i].header.skinCount; j++)
                    {
                        meshes[i].skins[j].name = BytesToStringOp(br.ReadBytes(64));
                        meshes[i].skins[j].index = br.ReadInt32();
                    }

                    //reading in texture coordinates
                    br.BaseStream.Seek(meshes[i].header.textureVectorStart + meshOffset, SeekOrigin.Begin);

                    meshes[i].textureCoordinates = new Vector2[meshes[i].header.vertexCount];

                    for (int j = 0; j < meshes[i].textureCoordinates.Length; j++)
                    {
                        meshes[i].textureCoordinates[j] = new Vector2(br.ReadSingle(), br.ReadSingle());
                    }

                    //reading vertices
                    br.BaseStream.Seek(meshes[i].header.vertexStart + meshOffset, SeekOrigin.Begin);

                    meshes[i].vertices = new Vertex[meshes[i].header.vertexCount * meshes[i].header.frameCount];

                    for(int j = 0; j < meshes[i].header.vertexCount * meshes[i].header.frameCount; j++)
                    {
                        meshes[i].vertices[j].vertex.X = br.ReadInt16() * (1.0f / 64);
                        meshes[i].vertices[j].vertex.Y = br.ReadInt16() * (1.0f / 64);
                        meshes[i].vertices[j].vertex.Z = br.ReadInt16() * (1.0f / 64);

                        meshes[i].vertices[j].normal = new byte[3];

                        byte lat = (byte)((float)br.ReadByte() * (2 * MathHelper.Pi) / 255), lng = (byte)((float)br.ReadByte() * (2 * MathHelper.Pi) / 255);

                        meshes[i].vertices[j].normal[0] = (byte)(Math.Cos(lat) * Math.Sin(lng));
                        meshes[i].vertices[j].normal[1] = (byte)(Math.Sin(lat) * Math.Sin(lng));
                        meshes[i].vertices[j].normal[2] = (byte)(Math.Cos(lng));
                    }

                    meshes[i].texture = -1;

                    meshOffset += meshes[i].header.meshSize;

                    Console.WriteLine("Read Mesh {0}", i);
                }

                Console.WriteLine("Read MD3 file");

                br.Close();

            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        public static Texture2D LoadTexture(GraphicsDevice device, string texturePath)
        {
            Texture2D texture;

            if (texturePath.ToLower().EndsWith(".tga"))
            {
                TargaImage image = new TargaImage(texturePath);
                texture = new Texture2D(device, image.Header.Width, image.Header.Height);
                Color[] data = new Color[image.Header.Height * image.Header.Width];
                for (int y = 0; y < image.Header.Height; y++)
                    for (int x = 0; x < image.Header.Width; x++)
                    {
                        System.Drawing.Color color = image.Image.GetPixel(x, y);
                        data[y * image.Header.Width + x] = new Color(color.R, color.G, color.B, color.A);
                    }
                image.Dispose();
                texture.SetData(data);
            }
            else
            {
                FileStream stream = new FileStream(texturePath, FileMode.Open);
                texture = Texture2D.FromStream(device, stream);
                stream.Close();
            }

            return texture;
        }

        public String BytesToStringOp(byte[] b)
        {
            string s = null;
            for (int i = 0; i < b.Length && b[i] != 0; i++)
                s += (char)b[i];
            return s;
        }
    }
}
