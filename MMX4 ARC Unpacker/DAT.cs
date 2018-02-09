using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace MMX_Unpacker
{
    class DAT
    {
        string PATH, NAME;
        List<string> NAMES = new List<string>();

        public DAT(string path)
        {
            this.PATH = path;        
        }

        public void Unpack()
        {

            using (FileStream fs = new FileStream(PATH, FileMode.Open, FileAccess.Read))
            {
                using (MemoryStream ms = new MemoryStream(fs.extractPiece(0, 2048)))
                {
                    byte[] data = new byte[8];
                    this.NAME = Path.GetFileNameWithoutExtension(PATH);
                    string path = Path.Combine(Path.GetDirectoryName(PATH), NAME);
                    string file;
                    int i = 0;

                    Directory.CreateDirectory(path);

                    while (ms.Read(data, 0, 8) == 8)
                    {
                        if (data[0] == data[1] && data[4] == data[5])
                            break;

                        fs.Position = data.extractInt32(0) * 2048;

                        file = String.Format("{0}_{1}.ARC", NAME, i);
                        NAMES.Add(file);

                        fs.extractPiece(0, data.extractInt32(4)).Save(Path.Combine(path, file));
                        i++;
                    }

                    WriteXML(path);
                }
            }
        }

        public void Repack(string _path)
        {
            ReadXML();

            using (FileStream fs = File.Create(Path.Combine(_path, NAME)))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    fs.Position = 2048;
                    foreach (string name in NAMES)
                    {
                        string path = Path.Combine(Path.GetDirectoryName(PATH), name);
                        List<byte> file = new List<byte>(File.ReadAllBytes(path));

                        ms.Write(((int)fs.Position / 2048).int32ToByteArray(), 0, 4);
                        ms.Write(file.Count.int32ToByteArray(), 0, 4);

                        while (file.Count % 2048 != 0)
                            file.Add(0x00);

                        fs.Write(file.ToArray(), 0, file.Count);
                    }

                    fs.Position = 0;
                    fs.Write(ms.ToArray(), 0, (int)ms.Position);
                }
            }

        }

        public void WriteXML(string path)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            using (XmlWriter writer = XmlWriter.Create(Path.Combine(path, "DATInfo.xml"), settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("DAT");
                writer.WriteAttributeString("Name", Path.GetFileName(Path.GetFileName(PATH)));
                for (int i = 0; i < NAMES.Count; i++)
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

        public void ReadXML()
        {
            using (XmlReader reader = XmlReader.Create(PATH))
            {

                while (reader.Read())
                {
                    if (reader.Name.Equals("DAT") && (reader.NodeType == XmlNodeType.Element))
                    {
                        NAME = reader.GetAttribute("Name");
                    }
                    else if (reader.Name.Equals("File") && (reader.NodeType == XmlNodeType.Element))
                    {
                        NAMES.Add(reader.GetAttribute("Name"));
                    }
                }
            }
        }

    }
}
