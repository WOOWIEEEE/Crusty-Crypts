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
    class Window
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

        static void GenerateMap()
        {
            //add seed
            w = new World(Convert.ToInt32(MaxRoom), IsLinear, Convert.ToInt32(Seed));
            rnd = new Random(w.Seed);
            rd = false;
            List<Vector2> DoorList1 = new List<Vector2>();
            List<Vector2> DoorList2 = new List<Vector2>();
            int DoorSpawnRate = 500;

            w.player.CurKey = rnd.Next(0, 10);

            for (int x = 47; x <= 53; x++)
            {
                for (int y = 47; y <= 53; y++)
                {
                    w.Map[y, x] = 2;
                }
            }
            Vector2[] Dir = new Vector2[] { new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0) };
            Bitmap[] BL = new Bitmap[] {Properties.Resources.Room1, Properties.Resources.Room2, Properties.Resources.Room3,
                                            Properties.Resources.Room4, Properties.Resources.Room5, Properties.Resources.Room6,
                                            Properties.Resources.Room7, Properties.Resources.Room8, Properties.Resources.Room9,
                                            Properties.Resources.Room10, Properties.Resources.Room11, Properties.Resources.Room12,
                                            Properties.Resources.Room13, Properties.Resources.Room14,  Properties.Resources.Room15,
                                            Properties.Resources.Room16,  Properties.Resources.Room17,  Properties.Resources.Room18,
                                            Properties.Resources.Room19,  Properties.Resources.Room20,   Properties.Resources.Room21};
            int[] OrderX;
            int[] OrderY;
            for (int i = 0; i < w.MaxRoom; i++)
            {
                rp = (double)(i + 1) / w.MaxRoom;
                Room r = LoadRoom(BL[rnd.Next(BL.Count())], rnd.Next(6));
                OrderX = RandomiseArray((int)w.MapSize.X);
                OrderY = RandomiseArray((int)w.MapSize.Y);
                for (int xi = 0; xi < w.MapSize.X; xi++)
                {
                    for (int yi = 0; yi < w.MapSize.Y; yi++)
                    {
                        int x = OrderX[xi];
                        int y = OrderY[yi];
                        if (w.Map[y, x] == 2) continue;
                        Vector2 n = new Vector2(x, y) + Dir[r.Direction];
                        if (n.X < 0 || n.X > w.MapSize.X - 1 || n.Y < 0 || n.Y > w.MapSize.Y - 1) continue;
                        if (w.Map[(int)n.Y, (int)n.X] == 2)
                        {
                            bool Fail = false;
                            for (int a = 0; a < r.Floor.Count(); a++)
                            {
                                n = new Vector2(x, y) + r.Floor[a];
                                if (n.X < 0 || n.X > w.MapSize.X - 1 || n.Y < 0 || n.Y > w.MapSize.Y - 1)
                                {
                                    Fail = true;
                                    break;
                                }
                                if (w.Map[(int)n.Y, (int)n.X] != 0)
                                {
                                    Fail = true;
                                    break;
                                }
                            }
                            for (int a = 0; a < r.Wall.Count(); a++)
                            {
                                n = new Vector2(x, y) + r.Wall[a];
                                if (n.X < 0 || n.X > w.MapSize.X - 1 || n.Y < 0 || n.Y > w.MapSize.Y - 1)
                                {
                                    Fail = true;
                                    break;
                                }
                                if (!(w.Map[(int)n.Y, (int)n.X] == 0 || w.Map[(int)n.Y, (int)n.X] == -1))
                                {
                                    Fail = true;
                                    break;
                                }
                            }
                            if (Fail) continue;
                            w.Map[y, x] = 2;
                            for (int a = 0; a < r.Floor.Count(); a++)
                            {
                                n = new Vector2(x, y) + r.Floor[a];
                                w.Map[(int)n.Y, (int)n.X] = 2;
                            }
                            for (int a = 0; a < r.Wall.Count(); a++)
                            {
                                n = new Vector2(x, y) + r.Wall[a];
                                w.Map[(int)n.Y, (int)n.X] = -1;
                            }
                            if (r.Direction == 1 || r.Direction == 3)
                            {
                                if (rnd.Next(1000) > DoorSpawnRate) DoorList1.Add(new Vector2(x, y));
                            }
                            else
                            {
                                if (rnd.Next(1000) > DoorSpawnRate) DoorList2.Add(new Vector2(x, y));
                            }
                            goto Finish;
                        }
                    }
                }
            Finish:
                continue;
            }
            rd = true;
            if (w.MapNonLinear)
            {
                int threshold = 50;
                OrderX = RandomiseArray((int)w.MapSize.X);
                OrderY = RandomiseArray((int)w.MapSize.Y);        
                for (int xi = 0; xi < w.MapSize.X; xi++)
                {
                    for (int yi = 0; yi < w.MapSize.Y; yi++)
                    {
                        dp = (double)(xi + 1) / w.MapSize.X;
                        int x = OrderX[xi];
                        int y = OrderY[yi];
                        if (x < w.MapSize.X - 3)
                        {
                            if (w.Map[y, x] == 2 && w.Map[y, x + 1] != 2 && w.Map[y, x + 2] == 2)
                            {
                                if (FindPath(new Vector2(x, y), new Vector2(x + 2, y), -1).Count() > threshold)
                                {
                                    w.Map[y, x + 1] = 2;
                                    if (rnd.Next(1000) > DoorSpawnRate) DoorList1.Add(new Vector2(x + 1, y));
                                }
                                goto End;
                            }
                            if (w.Map[y, x] == 2 && w.Map[y, x + 1] != 2 && w.Map[y, x + 2] != 2 && w.Map[y, x + 3] == 2)
                            {
                                if (FindPath(new Vector2(x, y), new Vector2(x + 3, y), -1).Count() > threshold)
                                {
                                    w.Map[y, x + 1] = 2;
                                    w.Map[y, x + 2] = 2;
                                    if (rnd.Next(1000) > DoorSpawnRate) DoorList1.Add(new Vector2(x + 1, y));
                                }
                            }
                        }
                    End:
                        if (y < w.MapSize.Y - 3)
                        {
                            if (w.Map[y, x] == 2 && w.Map[y + 1, x] != 2 && w.Map[y + 2, x] == 2)
                            {
                                if (FindPath(new Vector2(x, y), new Vector2(x, y + 2), -1).Count() > threshold)
                                {
                                    w.Map[y + 1, x] = 2;
                                    if (rnd.Next(1000) > DoorSpawnRate) DoorList2.Add(new Vector2(x, y + 1));
                                }                       
                                continue;
                            }
                            if (w.Map[y, x] == 2 && w.Map[y + 1, x] != 2 && w.Map[y + 2, x] != 2 && w.Map[y + 3, x] == 2)
                            {
                                if (FindPath(new Vector2(x, y), new Vector2(x, y + 3), -1).Count() > threshold)
                                {
                                    w.Map[y + 1, x] = 2;
                                    w.Map[y + 2, x] = 2;
                                    if (rnd.Next(1000) > DoorSpawnRate) DoorList2.Add(new Vector2(x, y + 1));
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                dp = 1;
            }
            for (int i = 0; i < DoorList1.Count(); i++)
            {
                w.Map[(int)DoorList1[i].Y, (int)DoorList1[i].X] = 3;
            }
            for (int i = 0; i < DoorList2.Count(); i++)
            {
                w.Map[(int)DoorList2[i].Y, (int)DoorList2[i].X] = 4;
            }
            dd = true;
            Vector2[] d = new Vector2[] {new Vector2(1,0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1),
                                                    new Vector2(1,1), new Vector2(-1, 1), new Vector2(-1, -1), new Vector2(1, -1)};
            w.EnemyList = new List<Enemy>();
            for (int x = 0; x < w.MapSize.X; x++)
            {
                for (int y = 0; y < w.MapSize.X; y++)
                {
                    op = (double)(x + 1) / w.MapSize.X;
                    if (w.Map[y, x] == 3 || w.Map[y, x] == 4)
                    {
                        continue;
                    }
                    if (w.Map[y, x] == 2)
                    {
                        int r = rnd.Next(0, 1000);
                        if (r > 900)
                        {
                            w.SpikeList.Add(new Spike(new Vector2(x, y)));
                        }
                        if (r > 780 && r <= 800)
                        {
                            w.CrossBowList.Add(new CrossBow(new Vector2(x, y)));
                        }
                        if (r > 895 && r <= 900)
                        {
                            w.EnemyList.Add(new Enemy(new Vector2(x, y)));
                        }
                        if (r > 0 && r <= 10)
                        {
                            w.Mod[y, x] = 1;
                        }
                        if (r > 10 && r <= 20)
                        {
                            w.Mod[y, x] = 2;
                        }
                        if (r > 20 && r <= 30)
                        {
                            w.Mod[y, x] = 3;
                        }
                        if (r > 30 && r <= 40)
                        {
                            w.Mod[y, x] = 4;
                        }
                        if (r > 40 && r <= 50)
                        {
                            w.Mod[y, x] = 5;
                        }
                        if (r > 50 && r <= 60)
                        {
                            w.Mod[y, x] = 6;
                        }
                        if (r > 60 && r <= 70)
                        {
                            w.Mod[y, x] = 7;
                        }
                        if (r > 70 && r <= 80)
                        {
                            w.Mod[y, x] = 8;
                        }
                        if (r > 80 && r <= 90)
                        {
                            w.Mod[y, x] = 9;
                        }
                        if (r > 90 && r <= 100)
                        {
                            w.Mod[y, x] = 10;
                        }
                        if (r > 100 && r <= 110)
                        {
                            w.Mod[y, x] = 11;
                        }
                        if (r > 110 && r <= 120)
                        {
                            w.Mod[y, x] = 12;
                        }
                        if (r > 890 && r <= 895)
                        {
                            w.Mod[y, x] = 13;
                        }
                        continue;
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 np = new Vector2(x, y) + d[i];
                        if (np.X < 0 || np.X > w.MapSize.X - 1 || np.Y < 0 || np.Y > w.MapSize.Y - 1) continue;
                        if (w.Map[(int)np.Y, (int)np.X] == 2)
                        {
                            w.Map[y, x] = 1;
                            break;
                        }
                    }
                }
            }
            w.Rev = new int[(int)w.MapSize.X, (int)w.MapSize.Y];
            Thread.Sleep(1000);
            GameState = 1;
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
        static Room LoadRoom(Bitmap b, int r)
        {
            Room room = new Room();
            switch (r)
            {
                case 1:
                    b.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    break;
                case 2:
                    b.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    break;
                case 3:
                    b.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    b.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    break;
                case 4:
                    b.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case 5:
                    b.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
            }
            for (int n = 0; n < b.Width * b.Height; n++)
            {
                int x = n % b.Width;
                int y = (n - x) / b.Width;
                Color c = b.GetPixel(x, y);
                if (c == Color.FromArgb(255, 255, 0, 0))
                {
                    room.Entrance = new Vector2(x, y);
                }
                if (c == Color.FromArgb(255, 255, 255, 255))
                {
                    room.Floor.Add(new Vector2(x, y));
                }
                if (c == Color.FromArgb(255, 0, 255, 0))
                {
                    room.Wall.Add(new Vector2(x, y));
                }
            }
            if (room.Entrance.Y == 0)
            {
                room.Direction = 0;
            }
            if (room.Entrance.X == b.Width - 1)
            {
                room.Direction = 1;
            }
            if (room.Entrance.Y == b.Height - 1)
            {
                room.Direction = 2;
            }
            if (room.Entrance.X == 0)
            {
                room.Direction = 3;
            }
            room.Localise();
            return room;
        }

        static int[] RandomiseArray(int s)
        {
            int[] Array = new int[s];
            for (int i = 0; i < Array.Count(); i++)
            {
                Array[i] = i;
            }
            for (int i = 0; i < Array.Count(); i++)
            {
                int temp = Array[i];
                int ri = rnd.Next(i, Array.Count());
                Array[i] = Array[ri];
                Array[ri] = temp;
            }
            return Array;
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

        //Draw Functions
        void DrawUI()
        {
            GL.Translate(ToNDC(1232, Width), 0, 0);
            for (int i = 0; i <= w.player.MaxHealth; i++)
            {
                if (i <= w.player.CurHealth)
                {
                    DrawTexture(new Vector2(0, -4 + Height - i * 32), new Vector2(4f, 4f), TextureList["Heart1"], Color.White, 0);
                }
                else
                {
                    DrawTexture(new Vector2(0, -4 + Height - i * 32), new Vector2(4f, 4f), TextureList["Heart2"], Color.White, 0);
                }
            }
            DrawTexture(new Vector2(0, -4 + Height - (w.player.MaxHealth + 1) * 32), new Vector2(4f, 4f), TextureList["Key"], Color.White, 0);
            DrawText("x" + w.player.CurKey, new Vector2(32, -4 + Height - (w.player.MaxHealth + 1) * 32), 20f, 0, w.player.CurKey == 0 ? Color.Red : Color.White);
            GL.LoadIdentity();
        }
        void DrawMap()
        {
            for (int x = 0; x < w.MapSize.X; x++)
            {
                for (int y = 0; y < w.MapSize.Y; y++)
                {
                    if (w.Rev[y, x] == 0) continue;
                    Color c = Color.Black;
                    switch (w.Rev[y, x])
                    {
                        case 1: c = Color.Gray; break;
                        case 2: c = Color.DarkGray; break;
                        case 3:
                        case 4: c = Color.Brown; break;
                    }
                    DrawRectangle(new Vector2(26, 26) + new Vector2(x, 100 - y) * 3, new Vector2(3, 3), c);
                }
            }

            DrawRectangle(new Vector2(26, 26) + new Vector2(w.player.Position.X, 100 - w.player.Position.Y) * 3, new Vector2(3, 3), Color.Blue);             
            GL.LoadIdentity();
        }
        void DrawGame()
        {
            Color c = Color.FromArgb(255, 255, 204, 102);
            GL.Translate(ToNDC(880,Width), 0, 0);
            
            //Draws Objects
            w.LightMap = new int[(int)w.MapSize.X, (int)w.MapSize.Y];
            List<Vector2> Draw = new List<Vector2>();
            for (int n = 0; n < 121; n++)
            {
                int x = n % 11;
                int y = (n - x) / 11;
                int mx = x + (int)w.CameraPos.X;
                int my = y + (int)w.CameraPos.Y;
                if (mx < 0 || mx > w.MapSize.X - 1 || my < 0 || my > w.MapSize.Y - 1) continue;
                if (w.Map[my, mx] == 2)
                {
                    int WallHit = 0;
                    for (int i = 0; i < w.LightList.Count(); i++)
                    {
                        int max = 10;                    
                        Vector2 Incr = (new Vector2(mx, my) - w.LightList[i].Position);
                        for (int a = 0; a < max; a++)
                        {
                            Vector2 v = Incr * ((float)a / (float)max);
                            Vector2 m = w.LightList[i].Position + new Vector2((float)Math.Round(v.X), (float)Math.Round(v.Y));
                            if (w.Map[(int)m.Y, (int)m.X] == 1 || w.Map[(int)m.Y, (int)m.X] == 3 || w.Map[(int)m.Y, (int)m.X] == 4)
                            {
                                WallHit++;
                                break;
                            }
                        }
                    }
                    if (WallHit == w.LightList.Count()) continue;
                    w.LightMap[my, mx] = 1;
                    w.Rev[my, mx] = 2;
                    string id = "Floor1";
                    switch (w.Mod[my, mx])
                    {
                        case 0: case 13: id = "Floor1"; break;
                        case 1: id = "Floor2"; break;
                        case 2: id = "Floor3"; break;
                        case 3: id = "Floor4"; break;
                        case 4: id = "Floor5"; break;
                        case 5: id = "Floor6"; break;
                        case 6: id = "Floor7"; break;
                        case 7: id = "Floor8"; break;
                        case 8: id = "Floor9"; break;
                        case 9: id = "Floor10"; break;
                        case 10: id = "Floor11"; break;
                        case 11: id = "Floor12"; break;
                        case 12: id = "Floor13"; break;
                    } 
                    DrawTexture(new Vector2(x, 10 - y) * 32, new Vector2(4f, 4f), TextureList[id], c, 0);
                    if(w.Mod[my, mx] == 13)
                    {
                        DrawTexture(new Vector2(x, 10 - y) * 32, new Vector2(4f, 4f), TextureList["Key"], c, 0);
                    }
                    Draw.Add(new Vector2(x, y));
                }
            }
            for (int n = 0; n < 121; n++)
            {
                int x = n % 11;
                int y = (n - x) / 11;
                int mx = x + (int)w.CameraPos.X;
                int my = y + (int)w.CameraPos.Y;
                if (mx < 0 || mx > w.MapSize.X - 1 || my < 0 || my > w.MapSize.Y - 1) continue;
                if (w.Map[my, mx] == 1 || w.Map[my, mx] == 3 || w.Map[my, mx] == 4)
                {
                    Vector2[] d = new Vector2[] {new Vector2(1,0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1),
                                                    new Vector2(1,1), new Vector2(-1, 1), new Vector2(-1, -1), new Vector2(1, -1)};
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 np = new Vector2(mx, my) + d[i];
                        if (np.X < 0 || np.X > w.MapSize.X - 1 || np.Y < 0 || np.Y > w.MapSize.Y - 1) continue;
                        if (w.LightMap[(int)np.Y, (int)np.X] == 1)
                        {
                            if (w.Map[my, mx] == 3 || w.Map[my, mx] == 4)
                            {
                                w.Rev[my, mx] = w.Map[my, mx];
                                DrawTexture(new Vector2(x, 10 - y) * 32, new Vector2(4f, 4f), TextureList[(w.Map[my, mx] == 3) ? "Door1" : "Door2"], c, 0);
                                Draw.Add(new Vector2(x, y));
                            }
                            if (w.Map[my, mx] == 1)
                            {
                                w.Rev[my, mx] = 1;
                                DrawTexture(new Vector2(x, 10 - y) * 32, new Vector2(4f, 4f), TextureList["Wall1"], c, 0);
                                Draw.Add(new Vector2(x, y));
                            }                  
                        }                
                    }
                }
            }
                    
            for (int i = 0; i < w.SpikeList.Count(); i++)
            {
                if (w.LightMap[(int)w.SpikeList[i].Position.Y, (int)w.SpikeList[i].Position.X] != 1) continue;
                string ID = "";
                switch (w.SpikeList[i].State)
                {
                    case 0: ID = "Spike11"; break;
                    case 1: ID = "Spike12"; break;
                    case 2: ID = "Spike13"; break;
                    case 3: ID = "Spike14"; break;
                }
                DrawTexture(new Vector2(w.SpikeList[i].Position.X - w.CameraPos.X, 10 - w.SpikeList[i].Position.Y + w.CameraPos.Y) * 32, new Vector2(4f, 4f), TextureList[ID], c, 0);
            }
            for (int i = 0; i < w.CrossBowList.Count(); i++)
            {
                if (w.LightMap[(int)w.CrossBowList[i].Position.Y, (int)w.CrossBowList[i].Position.X] != 1) continue;
                DrawTexture(new Vector2(w.CrossBowList[i].Position.X - w.CameraPos.X, 10 - w.CrossBowList[i].Position.Y + w.CameraPos.Y) * 32, new Vector2(4f, 4f), TextureList["Crossbow1"], c, 0);
            }
            for (int i = 0; i < w.EnemyList.Count(); i++)
            {
                if (w.LightMap[(int)w.EnemyList[i].Position.Y, (int)w.EnemyList[i].Position.X] != 1) continue;
                DrawTexture(new Vector2(w.EnemyList[i].Position.X - w.CameraPos.X, 10 - w.EnemyList[i].Position.Y + w.CameraPos.Y) * 32, new Vector2(4f, 4f), TextureList["Enemy1"], c, 0);
            }
            if (!w.player.Dead)
            {
                DrawTexture(new Vector2(160, 160), new Vector2(4f, 4f), TextureList[w.player.IsRight ? (w.player.IsWalk ? "PlayerR2" : "PlayerR1") : w.player.IsWalk ? "PlayerL2" : "PlayerL1"], c, 0);
            }

            //Draws Shadow
            Draw = Draw.Distinct().ToList();
            for (int n = 0; n < Draw.Count(); n++)
            {
                DrawRectangle(new Vector2(Draw[n].X, 10 - Draw[n].Y) * 32, new Vector2(32, 32), Lighting(Draw[n] + w.CameraPos));
            }
            GL.LoadIdentity();
        }
        void DrawRectangle(Vector2 p, Vector2 s, Color c)
        {
            GL.Color4(c);
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(ToNDC(p.X + s.X, Width), ToNDC(p.Y, Height));
            GL.Vertex2(ToNDC(p.X + s.X, Width), ToNDC(p.Y + s.Y, Height));
            GL.Vertex2(ToNDC(p.X, Width), ToNDC(p.Y + s.Y, Height));
            GL.Vertex2(ToNDC(p.X, Width), ToNDC(p.Y, Height));
            GL.End();
        }
        void DrawTexture(Vector2 p, Vector2 s, Texture t, Color c, double r)
        {
            GL.Color4(c);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, t.ID);
            GL.Begin(PrimitiveType.Quads);
            Vector2 Cen = new Vector2(p.X + t.Size.X * s.X / 2, p.Y + t.Size.Y * s.Y / 2);
            Vector2 v1 = Rotate(new Vector2(p.X + t.Size.X * s.X, p.Y), Cen, r);
            Vector2 v2 = Rotate(new Vector2(p.X + t.Size.X * s.X, p.Y + t.Size.Y * s.Y), Cen, r);
            Vector2 v3 = Rotate(new Vector2(p.X, p.Y + t.Size.Y * s.Y), Cen, r);
            Vector2 v4 = Rotate(new Vector2(p.X, p.Y), Cen, r);
            GL.TexCoord2(t.texco.X + t.texco.Width, t.texco.Y + t.texco.Height);
            GL.Vertex2(ToNDC(v1.X, Width), ToNDC(v1.Y, Height));
            GL.TexCoord2(t.texco.X + t.texco.Width, t.texco.Y);
            GL.Vertex2(ToNDC(v2.X, Width), ToNDC(v2.Y, Height));
            GL.TexCoord2(t.texco.X, t.texco.Y);
            GL.Vertex2(ToNDC(v3.X, Width), ToNDC(v3.Y, Height));
            GL.TexCoord2(t.texco.X, t.texco.Y + t.texco.Height);
            GL.Vertex2(ToNDC(v4.X, Width), ToNDC(v4.Y, Height));
            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }
        void DrawText(string text, Vector2 Pos, float sca, int p, Color col)
        {
            if (text == null || text.Length <= 0)
            {
                return;
            }
            float scale = sca / 60f;
            Vector2 cp = Pos;
            char[] ch = text.ToArray();
            if (p == 1 || p == 2)
            {
                double len = 0;
                foreach (char c in ch)
                {
                    len += CharList[c].advance;
                }
                if (p == 1)
                {
                    cp.X -= (int)(len * scale / 2);
                }
                if (p == 2)
                {
                    cp.X -= (int)(len * scale);
                }
            }
            int line = 0;
            foreach (char c in ch)
            {
                if (c == '\n')
                {
                    line++;
                    cp = new Vector2(Pos.X, Pos.Y - line * sca);
                }
                else
                {
                    DrawTexture(new Vector2(cp.X + CharList[c].offset.X * scale, cp.Y + (CharList[c].basel - CharList[c].size.Y - CharList[c].offset.Y) * scale), new Vector2(scale, scale), CharList[c].tex, col, 0);
                    cp.X += CharList[c].advance * scale;
                }
            }
        }

        //Classes
        class Tile
        {
            public Vector2 Position;
            public int ID;
            public int ParentID;
            public int F;
            public int G;
            public int H;
            public Tile(Vector2 p, int w)
            {
                Position = p;
                ID = (int)(p.X + p.Y * w);
                ParentID = 0;
                F = 0;
                G = 0;
                H = 0;
            }
        }
        class Room
        {
            public List<Vector2> Floor;
            public List<Vector2> Wall;
            public Vector2 Entrance;
            public int Direction;

            public Room()
            {
                Floor = new List<Vector2>();
                Wall = new List<Vector2>();
                Entrance = new Vector2(0, 0);
                Direction = 0;
            }
            public void Localise()
            {
                for (int i = 0; i < Floor.Count(); i++)
                {
                    Floor[i] -= Entrance;
                }
                for (int i = 0; i < Wall.Count(); i++)
                {
                    Wall[i] -= Entrance;
                }
            }
        }
        class Spike
        {
            public Vector2 Position;
            public int State;
            public Spike(Vector2 p)
            {
                Position = p;
                State = 0;
            }
        }
        class CrossBow
        {
            public Vector2 Position;
            public int State;
            public int Direction;
            public CrossBow(Vector2 p)
            {
                Position = p;
                State = 0;
            }
        }
        class Enemy
        {
            public Vector2 Position;
            public bool Activated;
            public Enemy(Vector2 p)
            {
                Activated = false;
                Position = p;
            }

        }
        class Light
        {
            public Vector2 Position;
            public float Strength;

            public Light(Vector2 p, float s)
            {
                Position = p;
                Strength = s;
            }
        }
        class Player
        {
            public Vector2 Position;
            public bool IsRight;
            public bool IsWalk;

            public bool Up;
            public bool Down;
            public bool Dead;
            public bool Left;
            public bool Enter;
            public bool Right;

            public int MaxHealth;
            public int CurHealth;
            public int CurKey;

            public Player(Vector2 p)
            {
                Position = p;
                MaxHealth = 4;
                CurHealth = 4;
                IsRight = true;
                IsWalk = false;
            }
        }
        class World
        {
            public List<CrossBow> CrossBowList;
            public List<Light> LightList;
            public List<Spike> SpikeList;
            public List<Enemy> EnemyList;
            public int MaxRoom;
            public int Seed;
            public bool MapNonLinear;
            public Player player;
            public Vector2 CameraPos;
            public int[,] Map;
            public int[,] Mod;
            public int[,] Rev;
            public int[,] LightMap;
            public Vector2 MapSize;
            public World(int mr, bool nl, int s)
            {
                MaxRoom = mr;
                MapNonLinear = nl;
                Seed = s;
                SpikeList = new List<Spike>();
                LightList = new List<Light>();
                CrossBowList = new List<CrossBow>();

                player = new Player(new Vector2(50, 50));
                CameraPos = player.Position - new Vector2(5, 5);
                LightList.Add(new Light(player.Position, 0.1f));

                MapSize = new Vector2(100, 100);
                Map = new int[(int)MapSize.X, (int)MapSize.Y];
                Mod = new int[(int)MapSize.X, (int)MapSize.Y];
            }
        }
    }
    class Shader
    {
        public int ProgramID;
        public int VertexShaderID;
        public int FragmentShaderID;

        public Shader(string vs, string fs)
        {
            VertexShaderID = LoadShader(vs, ShaderType.VertexShader);
            FragmentShaderID = LoadShader(fs, ShaderType.FragmentShader);
            ProgramID = GL.CreateProgram();
            GL.AttachShader(ProgramID, VertexShaderID);
            GL.AttachShader(ProgramID, FragmentShaderID);
            GL.LinkProgram(ProgramID);
            GL.ValidateProgram(ProgramID);
            BindAttribute(0, "position");
        }

        public void Start()
        {
            GL.UseProgram(ProgramID);
        }

        public void Stop()
        {
            GL.UseProgram(0);
        }

        public void BindAttribute(int attribute, string variable)
        {
            GL.BindAttribLocation(ProgramID, attribute, variable);
        }

        public void UniformF(string name, float value)
        {
            GL.Uniform1(GL.GetUniformLocation(ProgramID, name), value);
        }

        int LoadShader(string file, ShaderType type)
        {
            int ShaderID = GL.CreateShader(type);
            GL.ShaderSource(ShaderID, file);
            GL.CompileShader(ShaderID);
            return ShaderID;
        }
    }
    class Texture
    {
        public int ID;

        public RectangleF texco;

        public Vector2 Size;
        public Vector2 OrigSize;

        public Texture(int id, Vector2 size, RectangleF r)
        {
            ID = id;
            Size = new Vector2(r.Width, r.Height);
            texco = new RectangleF(r.X / size.X, r.Y / size.Y, r.Width / size.X, r.Height / size.Y);
            OrigSize = size;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Window w = new Window();
        }
    }
    class Character
    {
        public char id;

        public Vector2 size;
        public Vector2 offset;

        public int basel;
        public int advance;

        public Texture tex;

        public Character(char c, Vector2 s, Vector2 o, int a, int b, Texture t)
        {
            id = c;
            size = s;
            offset = o;
            advance = a;
            tex = t;
            basel = b;
        }
    }
    class FrameBuffer
    {
        public int FrameBufferID;
        public int TextureID;
        public int DepthTextureID;
        public Texture texture;
        public Vector2 Size;

        public FrameBuffer(Vector2 s)
        {
            Size = s;

            FrameBufferID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferID);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            TextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, (int)s.X, (int)s.Y, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedInt, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureID, 0);

            texture = new Texture(TextureID, new Vector2(s.X, s.Y), new RectangleF(0, 0, s.X, s.Y));
        }

        public void AttachDepSte()
        {
            DepthTextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, DepthTextureID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, (int)Size.X, (int)Size.Y, 0, OpenTK.Graphics.OpenGL.PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, DepthTextureID, 0);
        }
    }
}
