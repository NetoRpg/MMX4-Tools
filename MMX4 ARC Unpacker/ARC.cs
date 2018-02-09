using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace MMX_Unpacker
{
    class ARC
    {
        string PATH, NAME;
        List<string> NAMES = new List<string>();
        List<int> OP = new List<int>();

        public ARC(string path)
        {
            this.PATH = path;
        }

        public void Unpack()
        {
            using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(PATH)))
            {
                int fileNumber, arcSize;

                fileNumber = ms.extractPiece(0, 4).extractInt32();
                arcSize = ms.extractPiece(0, 4).extractInt32();


                if(arcSize != ms.Length)
                    return;


                int[] sizes = new int[fileNumber];
                byte[] data = new byte[8];

                int i;
                for (i = 0; i < fileNumber; i++)
                {
                    data = ms.extractPiece(0, 8);
                    OP.Add(data.extractInt32(0));
                    sizes[i] = data.extractInt32(4);

                }

                string filename = Path.GetFileNameWithoutExtension(PATH);
                string path = Path.Combine(Path.GetDirectoryName(PATH), filename);
                string name;

                Directory.CreateDirectory(path);

                ms.Position = 2048;
                i = 0;
                foreach (int size in sizes)
                {
                    while (ms.Position % 2048 != 0)
                        ms.Position += 1;

                    name = String.Format("{0}_{1}.BIN", filename, i);
                    NAMES.Add(name);
                    ms.extractPiece(0, size).Save(Path.Combine(path, name));

                    i++;
                }

                WriteXML(path);

            }
        }

        public void Repack(string _path)
        {
            ReadXML();

            int[] sizes = new int[NAMES.Count];

            using (FileStream fs = File.Create(Path.Combine(_path, NAME)))
            {
                fs.Position = 2048;
                for (int i = 0; i < NAMES.Count; i++)
                {
                    string path = Path.Combine(Path.GetDirectoryName(PATH), NAMES[i]);
                    List<byte> file = new List<byte>(File.ReadAllBytes(path));

                    sizes[i] = file.Count;

                    while (file.Count % 2048 != 0)
                        file.Add(0x00);

                    fs.Write(file.ToArray(), 0, file.Count);
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(NAMES.Count.int32ToByteArray(), 0, 4);
                    ms.Write(((int)fs.Position).int32ToByteArray(), 0, 4);

                    for (int i = 0; i < NAMES.Count; i++)
                    {
                        ms.Write(OP[i].int32ToByteArray(), 0, 4);
                        ms.Write(sizes[i].int32ToByteArray(), 0, 4);
                    }

                    fs.Position = 0;
                    fs.Write(ms.ToArray(), 0, (int)ms.Position);

                }
            }
        }


        private void ReadXML()
        {

            using (XmlReader reader = XmlReader.Create(PATH))
            {

                while (reader.Read())
                {
                    if (reader.Name.Equals("ARC") && (reader.NodeType == XmlNodeType.Element))
                    {
                        NAME = reader.GetAttribute("Name");
                    }
                    else if (reader.Name.Equals("File") && (reader.NodeType == XmlNodeType.Element))
                    {
                        NAMES.Add(reader.GetAttribute("Name"));
                        OP.Add(Convert.ToInt32(reader.GetAttribute("Op")));
                    }
                }
            }

        }

        private void WriteXML(string path)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            using (XmlWriter writer = XmlWriter.Create(Path.Combine(path, "ARCInfo.xml"), settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("ARC");
                writer.WriteAttributeString("Name", Path.GetFileName(Path.GetFileName(PATH)));
                for (int i = 0; i < NAMES.Count; i++)
                {
                    writer.WriteStartElement("File");
                    writer.WriteAttributeString("Name", NAMES[i]);
                    writer.WriteAttributeString("Op", OP[i].ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
            }
        }


    }

}