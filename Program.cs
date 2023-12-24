using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace GKHTextTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args[0].Contains(".txt"))
            {
                Rebuild(args[0], args[1]);
            }
            else
            {
                Extract(args[0]);
            }
        }
        public static void Extract(string txt)
        {
            var reader = new BinaryReader(File.OpenRead(txt));
            string magic = Encoding.UTF8.GetString(reader.ReadBytes(4));
            int unk1 = reader.ReadInt32();
            int unk2 = reader.ReadInt32();
            int blockSize = unk1 + unk2;
            reader.BaseStream.Position += 8;
            int count = reader.ReadInt32();
            string[] strings = new string[count];
            reader.BaseStream.Position = Utils.SkipNullBytes(reader) - 1;
            string textSt = "//TextOffset = " + reader.BaseStream.Position.ToString() + "\n";
            File.WriteAllText(txt + ".txt", textSt);
            Console.WriteLine($"{reader.BaseStream.Position}");
            for (int i = 0; i < count; i++)
            {
                strings[i] = Utils.ReadString(reader, Encoding.UTF8);
                strings[i] = strings[i].Replace("\n", "<lf>").Replace("\r","<br>");
                File.AppendAllText(txt + ".txt", strings[i] + "\n");
            }
        }
        public static void Rebuild(string text, string file)
        {
            var writer = new BinaryWriter(File.OpenWrite(file));
            writer.BaseStream.Position = 0x20;
            string[] strings = File.ReadAllLines(text);
            int offset = int.Parse(Utils.ReadValueAfterEqual(strings[0]));
            writer.BaseStream.Position = offset;
            for (int i = 0; i < strings.Length - 1; i++)
            {
                if (i == 0)
                {
                    continue;
                }
                strings[i] = strings[i].Replace("<lf>", "\n").Replace("<br>","\r");
                writer.Write(Encoding.UTF8.GetBytes(strings[i]));
                writer.Write(new byte());
            }
            int blockSize = 0;
            while (Utils.IsMultipleOf16((int)writer.BaseStream.Length) == false)
            {
                if (Utils.IsMultipleOf16((int)writer.BaseStream.Length) == false)
                {
                    writer.Write(new byte());
                }
                else
                {
                    blockSize = (int)writer.BaseStream.Position - 32;
                    writer.Write("\u4645\u4F43\u0000\u0000\u1000\u0000\u0000\u0000");
                }
            }
            writer.BaseStream.Position = 4;
            writer.Write(blockSize);
            writer.BaseStream.Position += 12;
            writer.Write(strings.Length - 1);
        }
    }
}
