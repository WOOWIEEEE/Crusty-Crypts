using System;
using OpenTK;
using System.Linq;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Dungeon
{
    partial class Window
    {
        Dictionary<char, Character> CharList;
        Dictionary<string, Texture> TextureList;

        double ToNDC(double x, int n)
        {
            return 2 * x / n - 1;
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
            for (int i = 0; i < w.player.MaxMove; i++)
            {
                int n = w.player.MaxMove - i;
                if (n <= w.player.CurMove)
                {
                    DrawTexture(new Vector2(0, -4 + Height - (w.player.MaxHealth + 2) * 32 - i * 32), new Vector2(4f, 4f), TextureList["Boot2"], Color.White, 0);
                }
                else
                {
                    DrawTexture(new Vector2(0, -4 + Height - (w.player.MaxHealth + 2) * 32 - i * 32), new Vector2(4f, 4f), TextureList["Boot1"], Color.White, 0);
                }
            }
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
            GL.Translate(ToNDC(880, Width), 0, 0);

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
                    if (w.Mod[my, mx] == 13)
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
                string ID = "";
                double angle = 0;
                switch (w.CrossBowList[i].State)
                {
                    case 0: ID = "Crossbow1"; break;
                    case 1: ID = "Crossbow2"; break;
                    case 2: ID = "Crossbow3"; break;
                    case 3: ID = "Crossbow4"; break;
                }
                switch (w.CrossBowList[i].Direction)
                {
                    case 0: angle = Math.PI; break;
                    case 1: angle = Math.PI / 2; break;
                    case 2: angle = 0; break;
                    case 3: angle = Math.PI * 3 / 2; break;
                }
                DrawTexture(new Vector2(w.CrossBowList[i].Position.X - w.CameraPos.X, 10 - w.CrossBowList[i].Position.Y + w.CameraPos.Y) * 32, new Vector2(4f, 4f), TextureList[ID], c, angle);
            }
            for (int i = 0; i < w.BulletList.Count(); i++)
            {
                if (w.LightMap[(int)w.BulletList[i].Position.Y, (int)w.BulletList[i].Position.X] != 1) continue;
                double angle = 0;
                switch (w.BulletList[i].Direction)
                {
                    case 0: angle = Math.PI; break;
                    case 1: angle = Math.PI / 2; break;
                    case 2: angle = 0; break;
                    case 3: angle = Math.PI * 3 / 2; break;
                }
                DrawTexture(new Vector2(w.BulletList[i].Position.X - w.CameraPos.X, 10 - w.BulletList[i].Position.Y + w.CameraPos.Y) * 32, new Vector2(4f, 4f), TextureList["Bolt"], c, angle);
            }
            for (int i = 0; i < w.BlockList.Count(); i++)
            {
                if (w.LightMap[(int)w.BlockList[i].Position.Y, (int)w.BlockList[i].Position.X] != 1) continue;
                DrawTexture(new Vector2(w.BlockList[i].Position.X - w.CameraPos.X, 10 - w.BlockList[i].Position.Y + w.CameraPos.Y) * 32, new Vector2(4f, 4f), TextureList["Block1"], c, 0);
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
