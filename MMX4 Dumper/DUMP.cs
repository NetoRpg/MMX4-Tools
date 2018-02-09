using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

// 20 Caracteres por linha

namespace MMX4_Dumper
{
    class DUMP
    {

        private Dictionary<string, byte> bytes = new Dictionary<string, byte>();

        public DUMP()
        {
            GenerateBytes();
        }

        public void Dump(string _path)
        {


            Dictionary<byte, byte> dic = GenerateDic();


            using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(_path)))
            {
                List<short> pointers = new List<short>();
                short end = ms.extractPiece(0, 2).extractInt16();
                pointers.Add(end);

                while (ms.Position < end)
                {
                    pointers.Add(ms.extractPiece(0, 2).extractInt16());
                }

                string s = String.Empty;

                string name = Path.GetFileNameWithoutExtension(_path);
                string path = Path.Combine(Path.GetDirectoryName(_path), name);
                string filename;
                string[] names = new string[pointers.Count - 1];

                Directory.CreateDirectory(path);

                for (int i = 0; i < pointers.Count - 1; i++)
                {
                    filename =  name + "_" + i.ToString() + ".txt";
                    names[i] = filename;
                    s = ExtractText(ms.extractPiece(0, pointers[i + 1] - pointers[i]));
                    s.saveText(Path.Combine(path,filename));
                }

                GenerateXML(path, names);

            }
        }


        public void Insert(string path, string savePath)
        {

            List<string> names = ReadXML(path);

            //foreach (string n in names)
            //    System.Windows.Forms.MessageBox.Show(n);

            List<short> pointers = new List<short>();
            string basePath = Path.GetDirectoryName(path);
            byte[] data;

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Position = names.Count * 2;
                    for (int i = 1; i < names.Count; i++)
                    {
                        using (StreamReader sr = new StreamReader(Path.Combine(basePath, names[i])))
                        {
                            //System.Windows.Forms.MessageBox.Show(names[i]);
                            pointers.Add((short)ms.Position);

                            string s = sr.ReadToEnd().ReplaceAll(
                                new[] { "\r", "\n", "[ENDLINE]", "[ENDLINE2]", "[ENDBOX]", "[ENDBOX2]", "[ENDTEXT]", "[ENDTEXT2]" },
                                new[] { "", "", "[a]", "[A]", "[b]", "[B]", "[c]", "[C]" });

                            s = VerifyTags(s);

                            data = TextToByte(s);
                            ms.Write(data, 0, data.Length);
                            //TextToByte(s).Save(Path.Combine(savePath, names[0]));
                        }
                    }

                    pointers.Add((short)ms.Position);
                    ms.Write((32824).int32ToByteArray(), 0, 4);

                    ms.Position = 0;
                    foreach (short p in pointers)
                    {
                        ms.Write(p.int16ToByteArray(), 0, 2);
                    }

                    ms.ToArray().Save(Path.Combine(savePath, names[0]));
            }
        }


        private string VerifyTags(string s)
        {

            List<string[]> tags = new List<string[]>()
            {
                new string[] { "ENDLINE", "[a]" },
                new string[] { "ENDLINE2", "[A]" },
                new string[] { "ENDBOX", "[b]" },
                new string[] { "ENDBOX2", "[B]" },
                new string[] { "ENDTEXT", "[c]" },
                new string[] { "ENDTEXT2", "[C]" }
            };

            foreach (string[] tag in tags)
            {
                Match r = Regex.Match(s, String.Format(@"\[{0}[^\]]", tag[0]));
                if (r.Success)
                    s = s.Replace(String.Format("[{0}", tag[0]), String.Format("[{0}]", tag[1]));

                r = Regex.Match(s, String.Format(@"[^\[]{0}\]", tag[0]));
                if (r.Success)
                    s = s.Replace(String.Format("{0}]", tag[0]), String.Format("[{0}]", tag[1]));

                r = Regex.Match(s, String.Format(@"[^\[]{0}[^\]]", tag[0]));
                if (r.Success)
                    s = s.Replace(String.Format("{0}", tag[0]), String.Format("[{0}]", tag[1]));
            }

            return s;
        }


        private string[] genKey(int len)
        { 
            string[] data = new string[len];

            int i = 0;
            for (char c = 'A'; c <= 'Z' || i < len; c++, i++)
            {
                data[i] = Convert.ToChar(c).ToString();
            }

            return data;
        }


        private void GenerateBytes()
        {
            for (int i = 0; i < 256; i++)
            {
                bytes.Add(i.ToString("X2"), (byte)i);
            }
        }

        private Dictionary<byte, byte> GenerateDic(bool reverse = false)
        {
            Dictionary<byte, byte> dic = new Dictionary<byte, byte>();

            string x;

            using (StreamReader sr = new StreamReader(@"MMX4.tbl"))
            {

                while ((x = sr.ReadLine()) != null)
                {

                    Match r = Regex.Match(x, @"^([A-Fa-f0-9]*)\=(.*)$");
                    if (r.Success)
                    {
                        if (reverse)
                        {
                            string ds = r.Groups[2].ToString();
                            byte ds2 = bytes[r.Groups[1].ToString()];
                            byte [] a = Encoding.UTF8.GetBytes(r.Groups[2].ToString());
                            //byte b = a.Length == 3 ? a[2] : a.Length == 2 ? a[1] : a[0];

                            dic.Add((byte)Convert.ToChar(r.Groups[2].ToString()), bytes[r.Groups[1].ToString()]);
                        }
                        else
                            dic.Add(bytes[r.Groups[1].ToString()], (byte)Convert.ToChar(r.Groups[2].ToString()));
                    }
                }
            }

            return dic;
        }


        private Dictionary<char, byte> InsertDic()
        {
            Dictionary<char, byte> dic = new Dictionary<char, byte>();

            string x;

            using (StreamReader sr = new StreamReader(@"MMX4.tbl"))
            {

                while ((x = sr.ReadLine()) != null)
                {

                    Match r = Regex.Match(x, @"^([A-Fa-f0-9]*)\=(.*)$");
                    if (r.Success)
                    {
                            dic.Add(Convert.ToChar(r.Groups[2].ToString()), bytes[r.Groups[1].ToString()]);
                    }
                }
            }

            return dic;
        }

        private Dictionary<byte, char> DumpDic()
        {
            Dictionary<byte, char> dic = new Dictionary<byte, char>();

            string x;

            using (StreamReader sr = new StreamReader(@"MMX4.tbl"))
            {

                while ((x = sr.ReadLine()) != null)
                {

                    Match r = Regex.Match(x, @"^([A-Fa-f0-9]*)\=(.*)$");
                    if (r.Success)
                    {
                        dic.Add(bytes[r.Groups[1].ToString()], Convert.ToChar(r.Groups[2].ToString()));
                    }
                }
            }

            return dic;
        }


        private string ExtractText(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            //Dictionary<byte, byte> dic = GenerateDic();
            Dictionary<byte, char> dic = DumpDic();
            byte last = 0xFF;
            for (int i = 0; i < data.Length; i += 2)
            {
                //if (data[i + 1] != last && data[i + 1] != 0x50 && data[i + 1] != 0x58 && data[i + 1] != 0x20 && data[i + 1] != 0x28 && data[i + 1] != 0xA0 && data[i + 1] != 0xA8)
                //{
                //    last = data[i + 1];
                //    sb.Append("[" + last.ToString("X2") + "]");
                //}
                Dictionary<byte, string> dic2 = new Dictionary<byte, string> { { 0x20, "ENDBOX" }, { 0x28, "ENDBOX2" }, { 0x50, "ENDLINE" }, { 0x58, "ENDLINE2" }, { 0xA0, "ENDTEXT" }, { 0xA8, "ENDTEXT2" } };

                if (i == 0 || last != data[i + 1] && (data[i - 1] == 0x50 || data[i - 1] == 0x58 || data[i - 1] == 0x20 || data[i - 1] == 0x28 || data[i - 1] == 0xA0 || data[i - 1] == 0xA8))
                {
                    last = data[i + 1];
                    sb.Append("[" + last.ToString("X2") + "]");
                }

                if (dic.ContainsKey(data[i]))
                    sb.Append(Convert.ToChar(dic[data[i]]));
                else
                    sb.Append(data[i]);


                if (dic2.ContainsKey(data[i + 1]))
                    sb.Append("["+ dic2[data[i + 1]] + "]\r\n");

            }

            return sb.ToString();
        }


        private byte[] TextToByte(string text)
        {
            List<byte> data = new List<byte>();
            //Dictionary<byte, byte> dic = GenerateDic(true);
            Dictionary<char, byte> dic = InsertDic();
            Dictionary<string, byte> dic2 = new Dictionary<string, byte> {{ "[b]", 0x20 }, { "[B]", 0x28 }, { "[a]", 0x50 }, { "[A]", 0x58 }, { "[c]", 0xA0 }, { "[C]", 0xA8 } };


            //string[] find = new string[6] { "[a]", "[A]", "[b]", "[B]", "[c]", "[C]" };
            //byte[] replace = new byte[6] { 0x50, 0x58, 0x20, 0x28, 0xA0, 0xA8 };

            byte second = 0x00, defaul = 0x00;
            int i = 0;
            while (i < text.Length)
            {

                


                Match teste = Regex.Match(text.Substring(i, 4), @"^\[([A-Fa-f0-9]{2})\]$");

                if (teste.Success)
                {
                    //MessageBox.Show(teste.Groups[1].ToString());
                    defaul = bytes[teste.Groups[1].ToString()];
                    i += 4;
                }
                //MessageBox.Show(text.Substring(i, 1));

                string s = text.Substring(i + 1, 3);
                string s2 = text.Substring(i, 3);


                if (dic2.ContainsKey(s2))
                {
                    second = dic2[s2];
                    i += 3;
                }

                try
                {
                    data.Add(dic[Convert.ToChar(text.Substring(i, 1))]);
                }
                catch(Exception ex)
                {
                    throw new Exception("O caractere \"" + text.Substring(i, 1) + "\" não existe na tabela!");
                }

                if (dic2.ContainsKey(s))
                {
                    second = dic2[s];
                    i += 3;
                }
                else if (text.Substring(i, 1) == " ")
                    second = (byte)(defaul + 0x01);
                else
                    second = defaul;



                //byte testes = (byte)Convert.ToChar(text.Substring(i, 1));
                //byte testes2 = (byte)Convert.ToChar("[");
                //string teste4 = text.Substring(i, 1);
                //bool testes3 = dic2.ContainsKey(s);



                data.Add(second);
                i++;
            }

            return data.ToArray();
        }


        private void GenerateXML(string path, string[] NAMES)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            using (XmlWriter writer = XmlWriter.Create(Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) ,"DUMPInfo.xml"), settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("DUMP");
                writer.WriteAttributeString("Name", Path.GetFileName(path) + ".BIN");
                for (int i = 0; i < NAMES.Length; i++)
                {
                    writer.WriteStartElement("File");
                    writer.WriteAttributeString("Name", NAMES[i]);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
            }

        }


        private List<string> ReadXML(string path)
        {
            List<string> names = new List<string>();

            using (XmlReader reader = XmlReader.Create(path))
            {

                while (reader.Read())
                {
                    if (reader.Name.Equals("DUMP") && (reader.NodeType == XmlNodeType.Element))
                    {
                        names.Add(reader.GetAttribute("Name"));
                    }
                    else if (reader.Name.Equals("File") && (reader.NodeType == XmlNodeType.Element))
                    {
                        names.Add(reader.GetAttribute("Name"));
                    }
                }
            }


            return names;
        }


    }
}
