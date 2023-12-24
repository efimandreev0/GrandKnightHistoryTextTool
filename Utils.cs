using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GKHTextTool
{
    internal static class Utils
    {
        public static string ReadValueAfterEqual(string input)
        {
            int equalIndex = input.IndexOf("="); // Находим индекс символа "="

            if (equalIndex >= 0)
            {
                string value = input.Substring(equalIndex + 1).Trim(); // Получаем подстроку после "="
                return value;
            }

            return null;
        }

        public static bool IsMultipleOf16(int number)
        {
            return number % 16 == 0;
        }
        public static long SkipNullBytes(BinaryReader reader)
        {
            long startPosition = reader.BaseStream.Position;

            while (reader.PeekChar() != -1 && reader.ReadByte() == 0x00)
            {
                // Пропускаем нулевой байт
            }

            long endPosition = reader.BaseStream.Position;
            reader.BaseStream.Position = startPosition; // возвращаем позицию на начало
            return endPosition;
        }
        public static string ReadString(BinaryReader binaryReader, Encoding encoding)
        {
            if (encoding == null) throw new ArgumentNullException("encoding");

            List<byte> data = new List<byte>();

            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
            {
                data.Add(binaryReader.ReadByte());

                string partialString = encoding.GetString(data.ToArray(), 0, data.Count);

                if (partialString.Length > 0 && partialString.Last() == '\0')
                    return encoding.GetString(data.SkipLast(encoding.GetByteCount("\0")).ToArray());
            }
            throw new InvalidDataException("Hit end of stream while reading null-terminated string.");
        }
        private static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (source == null) throw new ArgumentNullException("source");

            Queue<TSource> queue = new Queue<TSource>();

            foreach (TSource item in source)
            {
                queue.Enqueue(item);

                if (queue.Count > count) yield return queue.Dequeue();
            }
        }
    }
}
