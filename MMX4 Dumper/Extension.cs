using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace MMX4_Dumper
{
    static class Extension
    {

        public static string extractString(this byte[] bytes)
        {
            StringBuilder s = new StringBuilder();
            foreach (byte b in bytes)
            {
                if (b == 0x00)
                    break;

                s.Append(Convert.ToChar(b));
            }
            return s.ToString();
        }

        public static int extractInt32(this byte[] bytes, int index = 0)
        {
            return (bytes[index + 3] << 24) + (bytes[index + 2] << 16) + (bytes[index + 1] << 8) + bytes[index + 0];
        }


        public static Int16 extractInt16(this byte[] bytes, int index = 0)
        {
            return (short)((bytes[index + 1] << 8) + bytes[index + 0]);
        }



        public static void saveText(this string text, string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(text);            
            }
        }



        public static string ReplaceAll(
    this string source, string[] oldValues, string[] newValues)
        {
            // error checking etc removed for brevity

            string pattern =
                string.Join("|", oldValues.Select(Regex.Escape).ToArray());

            return Regex.Replace(source, pattern, m =>
            {
                int index = Array.IndexOf(oldValues, m.Value);
                return newValues[index];
            });
        }

        //public static byte[] extractPiece(this System.IO.FileStream fs, int offset, int length)
        //{
        //    byte[] data = new byte[length];
        //    //fs.Position = offset;
        //    fs.Read(data, 0, length);

        //    return data;
        //}

        public static byte[] extractPiece(this FileStream ms, int offset, int length)
        {
            byte[] data = new byte[length];
            //fs.Position = offset;
            ms.Read(data, 0, length);

            return data;
        }

        public static byte[] extractPiece(this MemoryStream ms, int offset, int length)
        {
            byte[] data = new byte[length];
            //fs.Position = offset;
            ms.Read(data, 0, length);

            return data;
        }



        public static void Save(this byte[] data, string path)
        {
            using (FileStream fs = File.Create(path))
            {
                fs.Write(data, 0, data.Length);
            }
        }

        public static string ToAscii(this string str)
        {

            var encoder = ASCIIEncoding.GetEncoding("us-ascii",
                new EncoderReplacementFallback(string.Empty),
                new DecoderExceptionFallback());

            string cleanString = encoder.GetString(encoder.GetBytes(str));
            return cleanString;
        }





        public static byte[] nameToByteArray(this string name, int size)
        {
            char[] n = name.ToCharArray();
            byte[] x = new byte[size];
            for (int i = 0; i < size; i++)
            {
                if (i < n.Length)
                    x[i] = (byte)n[i];
                else if (i == n.Length)
                    x[i] = 0x00;
                else
                    x[i] = 0xCD;
            }
            return x;
        }


        public static byte[] int32ToByteArray(this int value)
        {
            byte[] result = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                result[i] = (byte)((value >> i * 8) & 0xFF);
            }
            return result;
        }


        public static byte[] int16ToByteArray(this short value)
        {
            byte[] result = new byte[2];

            for (int i = 0; i < 2; i++)
            {
                result[i] = (byte)((value >> i * 8) & 0xFF);
            }
            return result;
        }

    }
}
