using OpenTK;
using System.Linq;
using System.Collections.Generic;

namespace Dungeon
{
    public  partial class Window
    {
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
}
