using System;
using OpenTK;
using System.Linq;
using System.Drawing;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

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

        bool ShowFPS;

        GameWindow gw;

        Dictionary<string, bool> Keys;

        static World w;

        static Random rnd;

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

        void Frame()
        {
            if (GameState == 1 && UpdateTime >= 0.12)
            {
                bool CanPlayerMove = true;
                if (w.player.CurHealth == 0) w.player.Dead = true;
                if (!w.player.Dead) w.LightList[0].Strength = rnd.Next(9000, 14000) / 100000f;
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
                if(w.BulletList.Count() > 0)
                {
                    for(int i = w.BulletList.Count() - 1; i >= 0; i--)
                    {
                        w.BulletList[i].Position += w.BulletList[i].Velocity;
                        if(w.Map[(int)w.BulletList[i].Position.Y, (int)w.BulletList[i].Position.X] == 1)
                        {
                            w.BulletList.RemoveAt(i);
                            continue;
                        }
                        if (w.BulletList[i].Position == w.player.Position)
                        {
                            w.BulletList.RemoveAt(i);
                            w.player.CurHealth--;
                            continue;
                        }
                        if (w.CrossBowList.Exists(n => n.Position == w.BulletList[i].Position))
                        {
                            w.CrossBowList.Remove(w.CrossBowList.Find(n => n.Position == w.BulletList[i].Position));
                            w.BulletList.RemoveAt(i);
                            continue;
                        }
                        if (w.BlockList.Exists(n => n.Position == w.BulletList[i].Position))
                        {
                            w.BulletList.RemoveAt(i);
                            continue;
                        }
                    }
                    CanPlayerMove = false;
                }
                if (w.player.IsWalk) w.player.IsWalk = false;
                UpdateTime = 0;
                if (MoveTime >= 0.16)
                {
                    if (CanPlayerMove)
                    {
                        if (Keys["Control"])
                        {
                            w.player.MaxMove = 3;
                        }
                        else
                        {
                            w.player.MaxMove = 1;
                        }
                        if (w.player.CurMove >= w.player.MaxMove)
                        {
                            w.player.CurMove = 0;
                        }
                    }
                        Vector2 OP = w.player.Position;
                    if ((Keys["Up"] || Keys["Down"] || Keys["Left"] || Keys["Right"] || (Keys["Enter"] && w.player.CurKey > 0)) && CanPlayerMove && !w.player.Dead)
                    {
                        if (Keys["Enter"])
                        {
                            bool Open = false;
                            Vector2[] d = new Vector2[] { new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1) };
                            for (int i = 0; i < 4; i++)
                            {
                                if (w.player.CurKey <= 0) break;
                                Vector2 np = w.player.Position + d[i];
                                if (!OnMap(np)) continue;
                                if (w.Map[(int)np.Y, (int)np.X] == 3 || w.Map[(int)np.Y, (int)np.X] == 4)
                                {
                                    w.Map[(int)np.Y, (int)np.X] = 2;
                                    w.player.CurKey--;
                                    Open = true;
                                }
                            }
                            if (!Open) return;
                        }
                        Vector2 NPP = w.player.Position;
                        Vector2 NCP = w.CameraPos;
                        if (Keys["Up"])
                        {
                            NPP.Y--;
                            NCP.Y--;
                        }
                        else if (Keys["Down"])
                        {
                            NPP.Y++;
                            NCP.Y++;
                        }
                        else if (Keys["Left"])
                        {
                            NPP.X--;
                            NCP.X--;
                            w.player.IsRight = false;
                        }
                        else if (Keys["Right"])
                        {
                            NPP.X++;
                            NCP.X++;
                            w.player.IsRight = true;
                        }
                        if (w.Map[(int)NPP.Y, (int)NPP.X] == 1 || w.Map[(int)NPP.Y, (int)NPP.X] == 0 || w.Map[(int)NPP.Y, (int)NPP.X] == 3
                            || w.Map[(int)NPP.Y, (int)NPP.X] == 4 || w.EnemyList.Exists(n => n.Position == NPP) || w.CrossBowList.Exists(n => n.Position == NPP)) return;
                       
                        if (w.Mod[(int)NPP.Y, (int)NPP.X] == 13)
                        {
                            w.Mod[(int)NPP.Y, (int)NPP.X] = 0;
                            w.player.CurKey++;
                            Play(Sources["Player"], Buffers["Key"]);
                        }
                        if (w.player.Position != NPP)
                        {
                            Vector2 d = 2 * NPP - w.player.Position;
                            bool Fail = false;
                            for (int i = 0; i < w.BlockList.Count(); i++)
                            {
                                if (w.BlockList[i].Position != NPP) continue;
                                if (w.Map[(int)d.Y, (int)d.X] == 2 && !w.BlockList.Exists(n => n.Position == d) && !w.CrossBowList.Exists(n => n.Position == d)
                                     && !w.EnemyList.Exists(n => n.Position == d))
                                {
                                    w.BlockList[i].Position = d;
                                }
                                else
                                {
                                    Fail = true;
                                }
                            }
                            if (Fail) return;
                            w.player.IsWalk = true;
                            Play(Sources["Player"], Buffers["Walk"]);
                            if (Keys["Shift"])
                            {
                                for (int i = 0; i < w.BlockList.Count(); i++)
                                {
                                    if (w.player.Position - w.BlockList[i].Position == NPP - w.player.Position)
                                    {
                                        w.BlockList[i].Position = w.player.Position;
                                    }
                                }
                            }
                        }
                        w.player.CurMove++;

                        w.CameraPos = NCP;
                        w.player.Position = NPP;
                        w.LightList[0].Position = NPP;
                        MoveTime = 0;
                    }
                    if (CanPlayerMove)
                    {
                         if (w.player.CurMove >= w.player.MaxMove)
                        {

                            for (int i = 0; i < w.SpikeList.Count(); i++)
                            {
                                if (w.player.Position == w.SpikeList[i].Position && w.SpikeList[i].State == 1)
                                {
                                    w.player.CurHealth -= 1;
                                }
                                if (w.SpikeList[i].State == 1) w.SpikeList[i].State = 2;
                                if (OP == w.SpikeList[i].Position) w.SpikeList[i].State = 1;
                            }
                            for (int i = 0; i < w.EnemyList.Count(); i++)
                            {
                                if (w.EnemyList[i].Activated)
                                {
                                    List<Vector2> a = FindPath(w.EnemyList[i].Position, w.player.Position, 300);
                                    if (a.Count() > 1) if (a.Last() != w.player.Position) w.EnemyList[i].Position = a.Last();
                                }
                                if (w.LightMap[(int)w.EnemyList[i].Position.Y, (int)w.EnemyList[i].Position.X] == 1)
                                {
                                    w.EnemyList[i].Activated = true;
                                }
                            }
                            for (int i = 0; i < w.CrossBowList.Count(); i++)
                            {
                                if (w.CrossBowList[i].State == 0)
                                {
                                    bool Fail = false;
                                    if (((w.CrossBowList[i].Position.X != w.player.Position.X || w.player.Position.Y > w.CrossBowList[i].Position.Y) && w.CrossBowList[i].Direction == 0) ||
                                        ((w.CrossBowList[i].Position.Y != w.player.Position.Y || w.player.Position.X < w.CrossBowList[i].Position.X) && w.CrossBowList[i].Direction == 1) ||
                                        ((w.CrossBowList[i].Position.X != w.player.Position.X || w.player.Position.Y < w.CrossBowList[i].Position.Y) && w.CrossBowList[i].Direction == 2) ||
                                        ((w.CrossBowList[i].Position.Y != w.player.Position.Y || w.player.Position.X > w.CrossBowList[i].Position.X) && w.CrossBowList[i].Direction == 3)) continue;
                                    Vector2[] dir = new Vector2[] { new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0) };
                                    for (int d = 0; d < Math.Max(Math.Abs(w.CrossBowList[i].Position.Y - w.player.Position.Y), Math.Abs(w.CrossBowList[i].Position.X - w.player.Position.X)); d++)
                                    {
                                        if (w.Map[(int)(w.CrossBowList[i].Position.Y + d * dir[w.CrossBowList[i].Direction].Y), (int)(w.player.Position.X + d * dir[w.CrossBowList[i].Direction].X)] != 2)
                                        {
                                            Fail = true;
                                            break;
                                        }
                                    }
                                    if (Fail) continue;
                                    w.CrossBowList[i].State = 1;
                                    continue;
                                }
                                if (w.CrossBowList[i].State == 1)
                                {
                                    w.CrossBowList[i].State = 2;
                                    switch (w.CrossBowList[i].Direction)
                                    {
                                        case 0: w.BulletList.Add(new Bullet(w.CrossBowList[i].Position, new Vector2(0, -1), 0)); break;
                                        case 1: w.BulletList.Add(new Bullet(w.CrossBowList[i].Position, new Vector2(1, 0), 1)); break;
                                        case 2: w.BulletList.Add(new Bullet(w.CrossBowList[i].Position, new Vector2(0, 1), 2)); break;
                                        case 3: w.BulletList.Add(new Bullet(w.CrossBowList[i].Position, new Vector2(-1, 0), 3)); break;
                                    }
                                    continue;
                                }
                                if (w.CrossBowList[i].State == 3) w.CrossBowList[i].State = 0;
                                if (w.CrossBowList[i].State == 2) w.CrossBowList[i].State = 3;
                            }
                        }
                    }
                }
            }
        }

        static void Set(Vector2 p, int x)
        {
            w.Map[(int)p.Y, (int)p.X] = x;
        }

        static bool OnMap(Vector2 p)
        {
            if (p.X < 0 || p.X > w.MapSize.X - 1 || p.Y < 0 || p.Y > w.MapSize.Y - 1)
            {
                return false;
            }
            return true;
        }

        string UpdateString(string s, int max)
        {
            if (Keys["BackSpace"] && s.Length > 0)
            {
                return s.Substring(0, s.Length - 1);
            }
            if (s.Length >= max)
            {
                return s;
            }
            if (Keys["1"])
            {
                return s + "1";
            }
            if (Keys["2"])
            {
                return s + "2";
            }
            if (Keys["3"])
            {
                return s + "3";
            }
            if (Keys["4"])
            {
                return s + "4";
            }
            if (Keys["5"])
            {
                return s + "5";
            }
            if (Keys["6"])
            {
                return s + "6";
            }
            if (Keys["7"])
            {
                return s + "7";
            }
            if (Keys["8"])
            {
                return s + "8";
            }
            if (Keys["9"])
            {
                return s + "9";
            }
            if (Keys["0"])
            {
                return s + "0";
            }
            return s;
        }

        void Closing(object Sender, EventArgs e)
        {
            if (MapGen.IsAlive)
            {
                MapGen.Abort();
            }
        }
        void Update(object Sender, FrameEventArgs e)
        {
            FPS = 1 / e.Time;
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(Color.FromArgb(255, 0, 0, 0));
            if (GameState == 0 || GameState == 3)
            {
                DrawTexture(new Vector2(288, Height - 88), new Vector2(6f, 6f), TextureList["Title"], Color.White, 0);
                DrawText("V 0.0.2", new Vector2(710, Height - 90), 20f, 0, Color.White);
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
            Frame();
        }
        void KeyUp(object Sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case OpenTK.Input.Key.Number0: Keys["0"] = false; break;
                case OpenTK.Input.Key.Number1: Keys["1"] = false; break;
                case OpenTK.Input.Key.Number2: Keys["2"] = false; break;
                case OpenTK.Input.Key.Number3: Keys["3"] = false; break;
                case OpenTK.Input.Key.Number4: Keys["4"] = false; break;
                case OpenTK.Input.Key.Number5: Keys["5"] = false; break;
                case OpenTK.Input.Key.Number6: Keys["6"] = false; break;
                case OpenTK.Input.Key.Number7: Keys["7"] = false; break;
                case OpenTK.Input.Key.Number8: Keys["8"] = false; break;
                case OpenTK.Input.Key.Number9: Keys["9"] = false; break;
                case OpenTK.Input.Key.ShiftLeft: Keys["Shift"] = false; break;
                case OpenTK.Input.Key.ControlLeft: Keys["Control"] = false; break;
                case OpenTK.Input.Key.BackSpace: Keys["BackSpace"] = false; break;
                case OpenTK.Input.Key.Up: Keys["Up"] = false; break;
                case OpenTK.Input.Key.Down: Keys["Down"] = false; break;
                case OpenTK.Input.Key.Left: Keys["Left"] = false; break;
                case OpenTK.Input.Key.Right: Keys["Right"] = false; break;
                case OpenTK.Input.Key.Enter: Keys["Enter"] = false; break;
            }
        }
        void KeyDown(object Sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case OpenTK.Input.Key.F9: w.Rev = w.Map; break;
                case OpenTK.Input.Key.Number0: Keys["0"] = true; break;
                case OpenTK.Input.Key.Number1: Keys["1"] = true; break;
                case OpenTK.Input.Key.Number2: Keys["2"] = true; break;
                case OpenTK.Input.Key.Number3: Keys["3"] = true; break;
                case OpenTK.Input.Key.Number4: Keys["4"] = true; break;
                case OpenTK.Input.Key.Number5: Keys["5"] = true; break;
                case OpenTK.Input.Key.Number6: Keys["6"] = true; break;
                case OpenTK.Input.Key.Number7: Keys["7"] = true; break;
                case OpenTK.Input.Key.Number8: Keys["8"] = true; break;
                case OpenTK.Input.Key.Number9: Keys["9"] = true; break;
                case OpenTK.Input.Key.ShiftLeft: Keys["Shift"] = true; break;
                case OpenTK.Input.Key.BackSpace: Keys["BackSpace"] = true; break;
                case OpenTK.Input.Key.ControlLeft: Keys["Control"] = true; break;
                case OpenTK.Input.Key.Up:
                    Keys["Up"] = true;
                    if (GameState == 0) ButtonPos = (ButtonPos == 0) ? 1 : ButtonPos - 1;            
                    if (GameState == 3) ButtonPos = (ButtonPos == 0) ? 3 : ButtonPos - 1;
                    break;
                case OpenTK.Input.Key.Down:
                    Keys["Down"] = true;
                    if (GameState == 0) ButtonPos = (ButtonPos == 1) ? 0 : ButtonPos + 1;
                    if (GameState == 3) ButtonPos = (ButtonPos == 3) ? 0 : ButtonPos + 1;
                    break;
                case OpenTK.Input.Key.Left: Keys["Left"] = true; break;
                case OpenTK.Input.Key.Right: Keys["Right"] = true; break;
                case OpenTK.Input.Key.Enter:
                    Keys["Enter"] = true;
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
