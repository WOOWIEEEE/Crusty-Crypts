using System.IO;
using OpenTK.Audio;
using System.Collections.Generic;
using OpenTK.Audio.OpenAL;

namespace Dungeon
{
    partial class Window
    {
        AudioContext Context;

        static Dictionary<string, int> Buffers;
        static Dictionary<string, int> Sources;

        void SetListener()
        {
            AL.Listener(ALListener3f.Position, 0f, 0f, 0f);
            AL.Listener(ALListener3f.Velocity, 0f, 0f, 0f);
        }
        int LoadBuffer(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            // chunk 0
            string chunkID = new string(reader.ReadChars(4));
            int fileSize = reader.ReadInt32();
            int riffType = reader.ReadInt32();
            // chunk 1
            int fmtID = reader.ReadInt32();
            int fmtSize = reader.ReadInt32();
            int fmtCode = reader.ReadInt16();
            int channels = reader.ReadInt16();
            int rate = reader.ReadInt32();
            int byteRate = reader.ReadInt32();
            int fmtBlockAlign = reader.ReadInt16();
            int bitDepth = reader.ReadInt16();
            // chunk 2
            string dataID = new string(reader.ReadChars(4));
            int bytes = reader.ReadInt32();
            // DATA!
            byte[] Sound_Data = reader.ReadBytes(bytes);

            int id = AL.GenBuffer();
            AL.BufferData(id, ALFormat.Stereo16, Sound_Data, Sound_Data.Length, rate);
            reader.Dispose();
            return id;
        }
        static void Play(int source, int buffer)
        {
            AL.Source(source, ALSourcei.Buffer, buffer);
            AL.SourcePlay(source);
        }
        int CreateSource(float vol)
        {
            int id = AL.GenSource();
            AL.Source(id, ALSourcef.Gain, vol);
            AL.Source(id, ALSourcef.Pitch, 1f);
            AL.Source(id, ALSource3f.Position, 0f, 0f, 0f);
            AL.Source(id, ALSource3f.Velocity, 0f, 0f, 0f);
            return id;
        }
    }
}
