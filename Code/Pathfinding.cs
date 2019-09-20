using System;
using OpenTK;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Dungeon
{
    partial class Window
    {
        static List<Tile> Open;
        static List<Tile> Closed;
        static List<Tile> Null;

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
    }
}
