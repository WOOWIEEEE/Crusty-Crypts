using System;
using OpenTK;
using System.IO;
using System.Linq;
using System.Drawing;
using OpenTK.Graphics;
using System.Threading;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Dungeon
{
      partial class Window
     {
        //Variables
        int Width;
        int Height;
        int ButtonPos;

        static int GameState;

        double FPS;
        double MoveTime;
        double UpdateTime;

        static double rp;
        static double dp;
        static double op;

        static string Seed;
        static string MaxRoom;

        bool P1;
        bool P2;
        bool P3;
        bool P4;
        bool P5;
        bool P6;
        bool P7;
        bool P8;
        bool P9;
        bool P0;
        bool ShowFPS;
        bool BackSpace;

        static bool rd;
        static bool dd;
        static bool IsLinear;

        static World w;

        static Thread MapGen;

        static Random rnd;

        GameWindow gw;

        static List<Tile> Open;
        static List<Tile> Closed;
        static List<Tile> Null;

        Dictionary<char, Character> CharList;
        Dictionary<string, Texture> TextureList;

        //Functions
        public Window()
        {
            Width = 1056;
            Height = 352;
            gw = new GameWindow(Width, Height, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 8));
            gw.WindowBorder = WindowBorder.Fixed;
            gw.ClientRectangle = new Rectangle(0, 0, Width, Height);
            gw.Title = "Crusty Crypts";
            gw.Load += Load;
            gw.RenderFrame += Update;
            gw.KeyUp += KeyUp;
            gw.Closing += Closing;
            gw.KeyDown += KeyDown;
            gw.Run(200, 200);
        }

 
        static void OpenSurrounding(Tile t, int parent, Vector2 e)
        {
            Vector2[] Dir = new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };
            for (int i = 0; i < Dir.Count(); i++)
            {
                double ia = t.Position.X + Dir[i].X + (t.Position.Y + Dir[i].Y) * w.MapSize.X;
                if (Open.Exists(x => x.Position.X == t.Position.X + Dir[i].X && x.Position.Y == t.Position.Y + Dir[i].Y))
                {
                    Tile nt = Open.Find(x => x.ID == ia);
                    Open.Remove(nt);
                    int newg = t.G + 1;
                    if (newg < nt.G)
                    {
                        nt.G = newg;
                        nt.F = nt.G + nt.H;
                        nt.ParentID = parent;
                    }
                    Open.Add(nt);
                    continue;
                }
                if (Null.Exists(x => x.Position.X == t.Position.X + Dir[i].X && x.Position.Y == t.Position.Y + Dir[i].Y))
                {
                    Tile nt = Null.Find(x => x.ID == ia);
                    Null.Remove(nt);
                    nt.H = (int)Math.Abs(e.X + e.Y - nt.Position.X - nt.Position.Y);
                    nt.G = t.G + 1;
                    nt.F = nt.H + nt.G;
                    nt.ParentID = parent;
                    Open.Add(nt);
                }
            }
        }

        static List<Vector2> FindPath(Vector2 s, Vector2 e, int limit)
        {
            List<Vector2> Path = new List<Vector2>();
            if (e == s)
            {
                return Path;
            }
            Open = new List<Tile>();
            Closed = new List<Tile>();
            Null = new List<Tile>();
            for (int x = 0; x < w.MapSize.X; x++)
            {
                for (int y = 0; y < w.MapSize.Y; y++)
                {
                    if (w.Map[y, x] == 2)
                    {
                        Null.Add(new Tile(new Vector2(x, y), (int)w.MapSize.X));
                    }
                }
            }
            Tile st = Null.Find(x => x.Position == s);
            Null.Remove(st);
            Closed.Add(st);
            OpenSurrounding(st, st.ID, e);
            int Count = 0;
            while (Open.Count() > 0)
            {
                int ID = Open[0].ID;
                double MinF = Open[0].F;
                for (int i = 1; i < Open.Count(); i++)
                {
                    if (Open[i].F < MinF)
                    {
                        MinF = Open[i].F;
                        ID = Open[i].ID;
                    }
                    else if (Open[i].F == MinF)
                    {
                        if (Open[i].H < Open.Find(x => x.ID == ID).H)
                        {
                            MinF = Open[i].F;
                            ID = Open[i].ID;
                        }
                    }
                }
                Tile t = Open.Find(x => x.ID == ID);
                Closed.Add(t);
                Open.Remove(t);
                OpenSurrounding(t, ID, e);
                if (Count == limit || t.Position == e)
                {
                    int oid = (int)(s.X + s.Y * w.MapSize.X);
                    int nid = ID;
                    while (nid != oid)
                    {
                        Tile p = Closed.Find(x => x.ID == nid);
                        Path.Add(p.Position);
                        nid = p.ParentID;
                    }
                    return Path;
                }
                Count++;
            }
            return Path;
        }

        string UpdateString(string s, int max)
        {
            if (BackSpace && s.Length > 0)
            {
                return s.Substring(0, s.Length - 1);
            }
            if (s.Length >= max)
            {
                return s;
            }
            if (P1)
            {
                return s + "1";
            }
            if (P2)
            {
                return s + "2";
            }
            if (P3)
            {
                return s + "3";
            }
            if (P4)
            {
                return s + "4";
            }
            if (P5)
            {
                return s + "5";
            }
            if (P6)
            {
                return s + "6";
            }
            if (P7)
            {
                return s + "7";
            }
            if (P8)
            {
                return s + "8";
            }
            if (P9)
            {
                return s + "9";
            }
            if (P0)
            {
                return s + "0";
            }
            return s;
        }

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

        double ToNDC(double x, int n)
        {
            return 2 * x / n - 1;
        }
        double Clamp(double min, double max, double inp)
        {
            if (inp < min)
            {
                return min;
            }
            else if (inp > max)
            {
                return max;
            }
            return inp;
        }

        Vector2 Rotate(Vector2 P, Vector2 C, double angle)
        {
            float Sin = (float)Math.Sin(angle);
            float Cos = (float)Math.Cos(angle);
            Vector2 D = P - C;
            return new Vector2(D.X * Cos - D.Y * Sin, D.X * Sin + D.Y * Cos) + C;
        }

        Color Lighting(Vector2 p)
        {
            int a = 255;
            for (int i = 0; i < w.LightList.Count(); i++)
            {
                a = Math.Max(Math.Min((int)Math.Round(Vector2.DistanceSquared(w.LightList[i].Position, p) / w.LightList[0].Strength), a), 0);
            }
            return Color.FromArgb(a, 0, 0, 0);
        }

        //Form Functions
        void Load(object Sender, EventArgs e)
        {
            CharList = new Dictionary<char, Character>();
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
            TextureList.Add("Heart1", new Texture(IconsID, new Vector2(24, 8), new RectangleF(0, 0, 8, 8)));
            TextureList.Add("Heart2", new Texture(IconsID, new Vector2(24, 8), new RectangleF(8, 0, 8, 8)));
            TextureList.Add("Key", new Texture(IconsID, new Vector2(24, 8), new RectangleF(16, 0, 8, 8)));

            int Door1ID = LoadBitmap(Properties.Resources.Door1);
            TextureList.Add("Door1", new Texture(Door1ID, new Vector2(16, 8), new RectangleF(0, 0, 8, 8)));
            TextureList.Add("Door2", new Texture(Door1ID, new Vector2(16, 8), new RectangleF(8, 0, 8, 8)));

            int Crossbow1ID = LoadBitmap(Properties.Resources.Crossbow1);
            TextureList.Add("Crossbow1", new Texture(Crossbow1ID, new Vector2(16, 8), new RectangleF(0, 0, 8, 8)));
            TextureList.Add("Crossbow2", new Texture(Crossbow1ID, new Vector2(16, 8), new RectangleF(8, 0, 8, 8)));

            TextureList.Add("Wall1", new Texture(LoadBitmap(Properties.Resources.Wall1), new Vector2(8, 8), new RectangleF(0, 0, 8, 8)));        
            TextureList.Add("Title", new Texture(LoadBitmap(Properties.Resources.Title), new Vector2(80, 13), new RectangleF(0, 0, 80, 13)));
            TextureList.Add("Enemy1", new Texture(LoadBitmap(Properties.Resources.Enemy1), new Vector2(8, 8), new RectangleF(0, 0, 8, 8)));          

            using (StreamReader sr = new StreamReader(new MemoryStream(Properties.Resources.NFont)))
            {
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
            }
            GameState = 0;
            ButtonPos = 0;
            IsLinear = true;
            rnd = new Random();
            MaxRoom = "50";
            Seed = rnd.Next(999999).ToString();

            ShowFPS = false;

            Open = new List<Tile>();
            Closed = new List<Tile>();
            Null = new List<Tile>();
            MoveTime = 0.5;

            GL.Viewport(0, 0, Width, Height);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            MapGen = new Thread(GenerateMap);
        }
        void Update(object Sender, FrameEventArgs e)
        {       
            FPS = 1 / e.Time;
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(Color.FromArgb(255, 0, 0, 0));           
            if (GameState == 0 || GameState == 3)
            {
                DrawTexture(new Vector2(288, Height - 88), new Vector2(6f, 6f), TextureList["Title"], Color.White, 0);
                DrawText("V 0.0.1", new Vector2(710, Height - 90), 20f, 0, Color.White);
                if (GameState == 0)
                {
                    DrawTexture(new Vector2(483, Height - 166), new Vector2(3f, 3f), TextureList[(ButtonPos == 0) ? "BtnPlay1" : "BtnPlay2"], Color.White, 0);
                    DrawTexture(new Vector2(483, Height - 246), new Vector2(3f, 3f), TextureList[(ButtonPos == 1) ? "BtnExit1" : "BtnExit2"], Color.White, 0);
                }
                if (GameState == 3)
                {
                    DrawText("Seed: " + Seed, new Vector2(528, Height - 136), 20f, 1, (ButtonPos == 0) ? Color.White : Color.Gray);
                    DrawText("Max Rooms: " + MaxRoom, new Vector2(528, Height - 166), 20f, 1, (ButtonPos == 1) ? Color.White : Color.Gray);
                    DrawText("Is Linear: " + (!IsLinear ? "True" : "False"), new Vector2(528, Height - 196), 20f, 1, (ButtonPos == 2) ? Color.White : Color.Gray);
                    DrawTexture(new Vector2(483, Height - 280), new Vector2(3f, 3f), TextureList[(ButtonPos == 3) ? "BtnPlay1" : "BtnPlay2"], Color.White, 0);
                }
            }
            if (GameState == 1)
            {
                DrawGame();
                DrawMap();
                DrawUI();
            }
            if (GameState == 2)
            {
                DrawText("Adding Rooms " + Math.Round(rp * 100) + "%", new Vector2(528, Height - 100), 20f, 1, Color.White);
                DrawText("Total " + Math.Round((rp + dp + op) / 3 * 100) + "%", new Vector2(528, Height - 220), 20f, 1, Color.White);
                if (rd)
                {
                    DrawText("Adding Doors " + Math.Round(dp * 100) + "%", new Vector2(528, Height - 120), 20f, 1, Color.White);
                    if (dd)
                    {
                        DrawText("Adding Objects " + Math.Round(op * 100) + "%", new Vector2(528, Height - 140), 20f, 1, Color.White);
                    }
                }
            }        
            if (ShowFPS)
            {
                DrawText("FPS " + (int)FPS, new Vector2(0, 0), 20f, 0, Color.White);
            }
            gw.SwapBuffers();

            MoveTime += e.Time;
            UpdateTime += e.Time;
            if (GameState == 1)
            {
                if (UpdateTime >= 0.11)
                {
                    if (!w.player.Dead)
                    {
                        w.LightList[0].Strength = rnd.Next(9000, 14000) / 100000f;
                    }
                    bool CanPlayerMove = true;
                    for (int i = 0; i < w.SpikeList.Count(); i++)
                    {
                        if (w.SpikeList[i].State == 3)
                        {
                            w.SpikeList[i].State = 0;
                            CanPlayerMove = false;
                        }
                        if (w.SpikeList[i].State == 2)
                        {
                            w.SpikeList[i].State = 3;
                            CanPlayerMove = false;
                        }
                    }
                    if (w.player.IsWalk) w.player.IsWalk = false;
                    UpdateTime = 0;
                    if (MoveTime >= 0.16)
                    {
                        if ((w.player.Up || w.player.Down || w.player.Left || w.player.Right || (w.player.Enter && w.player.CurKey > 0)) && CanPlayerMove && !w.player.Dead)
                        {
                            if (w.player.Enter)
                            {
                                bool Open = false;
                                Vector2[] d = new Vector2[] {new Vector2(1,0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1),
                                                    new Vector2(1,1), new Vector2(-1, 1), new Vector2(-1, -1), new Vector2(1, -1)};
                                for (int i = 0; i < 8; i++)
                                {
                                    if(w.player.CurKey <= 0)
                                    {
                                        break;
                                    }
                                    Vector2 np = w.player.Position + d[i];
                                    if (np.X < 0 || np.X > w.MapSize.X - 1 || np.Y < 0 || np.Y > w.MapSize.Y - 1) continue;
                                    if (w.Map[(int)np.Y, (int)np.X] == 3 || w.Map[(int)np.Y, (int)np.X] == 4)
                                    {
                                        w.Map[(int)np.Y, (int)np.X] = 2;
                                        w.player.CurKey--;
                                        Open = true;
                                    }
                                }
                                if (!Open)
                                {
                                    return;
                                }
                            }
                            Vector2 NPP = w.player.Position;
                            Vector2 NCP = w.CameraPos;
                            if (w.player.Up)
                            {
                                NPP.Y--;
                                NCP.Y--;
                            }
                            else if (w.player.Down)
                            {
                                NPP.Y++;
                                NCP.Y++;
                            }
                            else if (w.player.Left)
                            {
                                NPP.X--;
                                NCP.X--;
                                w.player.IsRight = false;
                            }
                            else if (w.player.Right)
                            {
                                NPP.X++;
                                NCP.X++;
                                w.player.IsRight = true;
                            }
                            if (w.Map[(int)NPP.Y, (int)NPP.X] == 1 || w.Map[(int)NPP.Y, (int)NPP.X] == 0 || w.Map[(int)NPP.Y, (int)NPP.X] == 3 
                                || w.Map[(int)NPP.Y, (int)NPP.X] == 4 || w.EnemyList.Exists(n => n.Position == NPP) || w.CrossBowList.Exists(n => n.Position == NPP))
                            {
                                return;
                            }
                            for (int i = 0; i < w.SpikeList.Count(); i++)
                            {
                                if (NPP == w.SpikeList[i].Position && w.SpikeList[i].State == 1)
                                {
                                    w.player.CurHealth -= 1;
                                    if(w.player.CurHealth == 0)
                                    {
                                        w.player.Dead = true;
                                    }
                                }
                                if (w.SpikeList[i].State == 1) w.SpikeList[i].State = 2;
                                if (w.player.Position == w.SpikeList[i].Position) w.SpikeList[i].State = 1;
                            }
                            if(w.Mod[(int)NPP.Y, (int)NPP.X] == 13)
                            {
                                w.Mod[(int)NPP.Y, (int)NPP.X] = 0;
                                w.player.CurKey++;
                            }
                            if(w.player.Position != NPP) w.player.IsWalk = true;
                            w.CameraPos = NCP;
                            w.player.Position = NPP;
                            w.LightList[0].Position = NPP;
                            for (int i = 0; i < w.EnemyList.Count(); i++)
                            {
                                if (w.EnemyList[i].Activated)
                                {
                                    List<Vector2> a = FindPath(w.EnemyList[i].Position, w.player.Position, 300);
                                    if(a.Count() > 1)
                                    {
                                        w.EnemyList[i].Position = a.Last();
                                    }
                                }
                            }
                            MoveTime = 0;
                        }
                    }
                }
            }
        }
        void Closing(object Sender, EventArgs e)
        {
            if (MapGen.IsAlive)
            {
                MapGen.Abort();
            }
        }

        //Inputs     
        void KeyUp(object Sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case OpenTK.Input.Key.Number0: P0 = false; break;
                case OpenTK.Input.Key.Number1: P1 = false; break;
                case OpenTK.Input.Key.Number2: P2 = false; break;
                case OpenTK.Input.Key.Number3: P3 = false; break;
                case OpenTK.Input.Key.Number4: P4 = false; break;
                case OpenTK.Input.Key.Number5: P5 = false; break;
                case OpenTK.Input.Key.Number6: P6 = false; break;
                case OpenTK.Input.Key.Number7: P7 = false; break;
                case OpenTK.Input.Key.Number8: P8 = false; break;
                case OpenTK.Input.Key.Number9: P9 = false; break;
                case OpenTK.Input.Key.BackSpace: BackSpace = false; break;
                case OpenTK.Input.Key.Up: if (GameState == 1) w.player.Up = false; break;
                case OpenTK.Input.Key.Down: if (GameState == 1) w.player.Down = false; break;
                case OpenTK.Input.Key.Left: if (GameState == 1) w.player.Left = false; break;
                case OpenTK.Input.Key.Right: if (GameState == 1) w.player.Right = false; break;
                case OpenTK.Input.Key.Enter: if (GameState == 1) w.player.Enter = false; break;
            }
        }
        void KeyDown(object Sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case OpenTK.Input.Key.F9: w.Rev = w.Map; break;
                case OpenTK.Input.Key.Number0: P0 = true; break;
                case OpenTK.Input.Key.Number1: P1 = true; break;
                case OpenTK.Input.Key.Number2: P2 = true; break;
                case OpenTK.Input.Key.Number3: P3 = true; break;
                case OpenTK.Input.Key.Number4: P4 = true; break;
                case OpenTK.Input.Key.Number5: P5 = true; break;
                case OpenTK.Input.Key.Number6: P6 = true; break;
                case OpenTK.Input.Key.Number7: P7 = true; break;
                case OpenTK.Input.Key.Number8: P8 = true; break;
                case OpenTK.Input.Key.Number9: P9 = true; break;
                case OpenTK.Input.Key.BackSpace: BackSpace = true; break;
                case OpenTK.Input.Key.Up:
                    if (GameState == 0)
                    {
                        ButtonPos = (ButtonPos == 0) ? 1 : ButtonPos - 1;
                    }
                   
                    if (GameState == 1)
                    {
                        w.player.Up = true;
                    }
                    if (GameState == 3)
                    {
                        ButtonPos = (ButtonPos == 0) ? 3 : ButtonPos - 1;
                    }
                    break;
                case OpenTK.Input.Key.Down:
                    if (GameState == 0)
                    {
                        ButtonPos = (ButtonPos == 1) ? 0 : ButtonPos + 1;
                    }
                    if (GameState == 1)
                    {
                        w.player.Down = true;
                    }
                    if (GameState == 3)
                    {
                        ButtonPos = (ButtonPos == 3) ? 0 : ButtonPos + 1;
                    }
                    break;
                case OpenTK.Input.Key.Left: if (GameState == 1) w.player.Left = true; break;
                case OpenTK.Input.Key.Right: if (GameState == 1) w.player.Right = true; break;
                case OpenTK.Input.Key.Enter:
                    if (GameState == 0)
                    {
                        if(ButtonPos == 0)
                        {
                            GameState = 3;                          
                        }
                        else
                        {
                            gw.Close();
                        }
                    }
                    if (GameState == 1)
                    {
                        w.player.Enter = true;
                    }
                    if (GameState == 3)
                    {
                        if (ButtonPos == 2)
                        {
                            IsLinear = IsLinear ? false : true;
                        }
                        if (ButtonPos == 3)
                        {
                            if (Seed != "" && MaxRoom != "")
                            {
                                GameState = 2;
                                MapGen.Start();
                            }
                        }
                    }
                    break;
                case OpenTK.Input.Key.F10: ShowFPS = ShowFPS ? false : true; break;
            }
            if(GameState == 3)
            {
                if(ButtonPos == 0)
                {
                    Seed = UpdateString(Seed, 6);
                }
                if (ButtonPos == 1)
                {
                    MaxRoom = UpdateString(MaxRoom, 6);
                }
            }
        }
    }
}
