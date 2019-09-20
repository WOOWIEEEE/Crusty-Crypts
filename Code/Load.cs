using System;
using OpenTK;
using System.IO;
using OpenTK.Audio;
using System.Drawing;
using System.Threading;
using OpenTK.Audio.OpenAL;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Dungeon
{
    partial class Window
    {
        int LoadBitmap(Bitmap bmp)
        {
            BitmapData Data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int ID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, Data.Scan0);
            bmp.UnlockBits(Data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            return ID;
        }

        void LoadImages()
        {
            TextureList = new Dictionary<string, Texture>();
            int PlayerID = LoadBitmap(Properties.Resources.Player);
            TextureList.Add("PlayerL1", new Texture(PlayerID, new Vector2(32, 8), new RectangleF(0, 0, 8, 8)));
            TextureList.Add("PlayerL2", new Texture(PlayerID, new Vector2(32, 8), new RectangleF(8, 0, 8, 8)));
            TextureList.Add("PlayerR1", new Texture(PlayerID, new Vector2(32, 8), new RectangleF(16, 0, 8, 8)));
            TextureList.Add("PlayerR2", new Texture(PlayerID, new Vector2(32, 8), new RectangleF(24, 0, 8, 8)));

            int MenuButtonID = LoadBitmap(Properties.Resources.MenuButton);
            TextureList.Add("BtnPlay1", new Texture(MenuButtonID, new Vector2(60, 40), new RectangleF(0, 0, 30, 20)));
            TextureList.Add("BtnPlay2", new Texture(MenuButtonID, new Vector2(60, 40), new RectangleF(30, 0, 30, 20)));
            TextureList.Add("BtnExit1", new Texture(MenuButtonID, new Vector2(60, 40), new RectangleF(0, 20, 30, 20)));
            TextureList.Add("BtnExit2", new Texture(MenuButtonID, new Vector2(60, 40), new RectangleF(30, 20, 30, 20)));

            int Spike1ID = LoadBitmap(Properties.Resources.Spike1);
            TextureList.Add("Spike11", new Texture(Spike1ID, new Vector2(32, 8), new RectangleF(0, 0, 8, 8)));
            TextureList.Add("Spike12", new Texture(Spike1ID, new Vector2(32, 8), new RectangleF(8, 0, 8, 8)));
            TextureList.Add("Spike13", new Texture(Spike1ID, new Vector2(32, 8), new RectangleF(16, 0, 8, 8)));
            TextureList.Add("Spike14", new Texture(Spike1ID, new Vector2(32, 8), new RectangleF(24, 0, 8, 8)));

            int Floor1ID = LoadBitmap(Properties.Resources.Floor1);
            TextureList.Add("Floor1", new Texture(Floor1ID, new Vector2(40, 24), new RectangleF(0, 0, 8, 8)));
            TextureList.Add("Floor2", new Texture(Floor1ID, new Vector2(40, 24), new RectangleF(8, 0, 8, 8)));
            TextureList.Add("Floor3", new Texture(Floor1ID, new Vector2(40, 24), new RectangleF(16, 0, 8, 8)));
            TextureList.Add("Floor4", new Texture(Floor1ID, new Vector2(40, 24), new RectangleF(24, 0, 8, 8)));
            TextureList.Add("Floor5", new Texture(Floor1ID, new Vector2(40, 24), new RectangleF(32, 0, 8, 8)));
            TextureList.Add("Floor6", new Texture(Floor1ID, new Vector2(40, 24), new RectangleF(0, 8, 8, 8)));
            TextureList.Add("Floor7", new Texture(Floor1ID, new Vector2(40, 24), new RectangleF(8, 8, 8, 8)));
            TextureList.Add("Floor8", new Texture(Floor1ID, new Vector2(40, 24), new RectangleF(16, 8, 8, 8)));
            TextureList.Add("Floor9", new Texture(Floor1ID, new Vector2(40, 24), new RectangleF(24, 8, 8, 8)));
            TextureList.Add("Floor10", new Texture(Floor1ID, new Vector2(40, 24), new RectangleF(32, 8, 8, 8)));
            TextureList.Add("Floor11", new Texture(Floor1ID, new Vector2(40, 24), new RectangleF(0, 16, 8, 8)));
            TextureList.Add("Floor12", new Texture(Floor1ID, new Vector2(40, 24), new RectangleF(8, 16, 8, 8)));
            TextureList.Add("Floor13", new Texture(Floor1ID, new Vector2(40, 24), new RectangleF(16, 16, 8, 8)));

            int IconsID = LoadBitmap(Properties.Resources.Icons);
            TextureList.Add("Heart1", new Texture(IconsID, new Vector2(40, 8), new RectangleF(0, 0, 8, 8)));
            TextureList.Add("Heart2", new Texture(IconsID, new Vector2(40, 8), new RectangleF(8, 0, 8, 8)));
            TextureList.Add("Key", new Texture(IconsID, new Vector2(40, 8), new RectangleF(16, 0, 8, 8)));
            TextureList.Add("Boot1", new Texture(IconsID, new Vector2(40, 8), new RectangleF(24, 0, 8, 8)));
            TextureList.Add("Boot2", new Texture(IconsID, new Vector2(40, 8), new RectangleF(32, 0, 8, 8)));

            int Door1ID = LoadBitmap(Properties.Resources.Door1);
            TextureList.Add("Door1", new Texture(Door1ID, new Vector2(16, 8), new RectangleF(0, 0, 8, 8)));
            TextureList.Add("Door2", new Texture(Door1ID, new Vector2(16, 8), new RectangleF(8, 0, 8, 8)));

            int Crossbow1ID = LoadBitmap(Properties.Resources.Crossbow1);
            TextureList.Add("Crossbow1", new Texture(Crossbow1ID, new Vector2(40, 8), new RectangleF(0, 0, 8, 8)));
            TextureList.Add("Crossbow2", new Texture(Crossbow1ID, new Vector2(40, 8), new RectangleF(8, 0, 8, 8)));
            TextureList.Add("Bolt", new Texture(Crossbow1ID, new Vector2(40, 8), new RectangleF(16, 0, 8, 8)));
            TextureList.Add("Crossbow3", new Texture(Crossbow1ID, new Vector2(40, 8), new RectangleF(24, 0, 8, 8)));
            TextureList.Add("Crossbow4", new Texture(Crossbow1ID, new Vector2(40, 8), new RectangleF(32, 0, 8, 8)));

            TextureList.Add("Wall1", new Texture(LoadBitmap(Properties.Resources.Wall1), new Vector2(8, 8), new RectangleF(0, 0, 8, 8)));
            TextureList.Add("Title", new Texture(LoadBitmap(Properties.Resources.Title), new Vector2(80, 13), new RectangleF(0, 0, 80, 13)));
            TextureList.Add("Enemy1", new Texture(LoadBitmap(Properties.Resources.Enemy1), new Vector2(8, 8), new RectangleF(0, 0, 8, 8)));
            TextureList.Add("Block1", new Texture(LoadBitmap(Properties.Resources.Block1), new Vector2(8, 8), new RectangleF(0, 0, 8, 8)));
        }
        void LoadFonts()
        {
            CharList = new Dictionary<char, Character>();
            StreamReader sr = new StreamReader(new MemoryStream(Properties.Resources.NFont));
            string[] Lines = sr.ReadToEnd().Split('\n');
            int Len = Convert.ToInt16(Lines[3].Split(' ')[1].Split('=')[1]);
            int BasLin = Convert.ToInt32(Lines[1].Split(' ')[2].Split('=')[1]);
            int FontID = LoadBitmap(Properties.Resources.NFont1);
            for (int i = 4; i < 4 + Len; i++)
            {
                string[] L = Regex.Replace(Lines[i], @"\s+", " ").Split(' ');
                char Chr = (char)Convert.ToInt32(L[1].Split('=')[1]);
                Vector2 Pos = new Vector2(Convert.ToInt32(L[2].Split('=')[1]), Convert.ToInt32(L[3].Split('=')[1]));
                Vector2 Siz = new Vector2(Convert.ToInt32(L[4].Split('=')[1]), Convert.ToInt32(L[5].Split('=')[1]));
                Vector2 Off = new Vector2(Convert.ToInt32(L[6].Split('=')[1]), Convert.ToInt32(L[7].Split('=')[1]));
                int Adv = Convert.ToInt32(L[8].Split('=')[1]);
                CharList.Add(Chr, new Character(Chr, Siz, Off, Adv, BasLin, new Texture(FontID, new Vector2(512, 512), new RectangleF(Pos.X, Pos.Y, Siz.X, Siz.Y))));
            }
            sr.Dispose();
        }
        void LoadAudio()
        {
            Buffers = new Dictionary<string, int>();
            Sources = new Dictionary<string, int>();

            Context = new AudioContext();
            SetListener();
            Sources.Add("BackMusic", CreateSource(0.5f));
            Sources.Add("Player", CreateSource(1f));
            Buffers.Add("BackMusic", LoadBuffer(Properties.Resources.BackMusic));
            Buffers.Add("Walk", LoadBuffer(Properties.Resources.Walk));
            Buffers.Add("Key", LoadBuffer(Properties.Resources.Key));
            AL.Source(Sources["BackMusic"], ALSourceb.Looping, true);
        }
        void LoadKeys()
        {
            Keys = new Dictionary<string, bool>();
            Keys["Up"] = false;
            Keys["Down"] = false;
            Keys["Left"] = false;
            Keys["Right"] = false;
            Keys["0"] = false;
            Keys["1"] = false;
            Keys["2"] = false;
            Keys["3"] = false;
            Keys["4"] = false;
            Keys["5"] = false;
            Keys["6"] = false;
            Keys["7"] = false;
            Keys["8"] = false;
            Keys["9"] = false;
            Keys["BackSpace"] = false;
            Keys["Shift"] = false;
            Keys["Enter"] = false;
            Keys["Control"] = false;
        }
        void Load(object Sender, EventArgs e)
        {
            LoadImages();
            LoadAudio();
            LoadKeys();
            LoadFonts();

            GameState = 0;
            ButtonPos = 0;
            IsLinear = true;
            MaxRoom = "50";
            rnd = new Random();
            Seed = rnd.Next(999999).ToString();
            MapGen = new Thread(GenerateMap);

            Open = new List<Tile>();
            Closed = new List<Tile>();
            Null = new List<Tile>();

            GL.Viewport(0, 0, Width, Height);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }
    }
}
