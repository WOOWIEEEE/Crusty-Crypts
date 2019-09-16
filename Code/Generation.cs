using System;
using OpenTK;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace Dungeon
{
    partial class Window
    {
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
    }
}
