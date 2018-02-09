using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace MMX_Unpacker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void UnpackARC_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "ARC|*.ARC";
                ofd.Multiselect = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (string path in ofd.FileNames)
                    {
                        ARC arc = new ARC(path);
                        arc.Unpack();
                    }
                    SUnpacked();
                }
            }
        }

        private void RepackARC_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog ofd = new OpenFileDialog())
            {

                ofd.Filter = "ARCInfo.xml|ARCInfo.xml";

                if (ofd.ShowDialog() == DialogResult.OK)
                {

                    using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                    {
                        if (fbd.ShowDialog() == DialogResult.OK)
                        {
                            ARC arc = new ARC(ofd.FileName);
                            arc.Repack(fbd.SelectedPath);
                            SRepacked();
                        }
                    }
                }
            }
        }

        private void UnpackDAT_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "DAT|*.DAT";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    DAT dat = new DAT(ofd.FileName);
                    dat.Unpack();
                    SUnpacked();
                }
            }
        }

        private void RepackDAT_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {

                ofd.Filter = "DATInfo.xml|DATInfo.xml";

                if (ofd.ShowDialog() == DialogResult.OK)
                {

                    using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                    {
                        if (fbd.ShowDialog() == DialogResult.OK)
                        {
                            DAT dat = new DAT(ofd.FileName);
                            dat.Repack(fbd.SelectedPath);
                            SRepacked();
                        }
                    }
                }
            }
        }


        private void SUnpacked()
        {
            MessageBox.Show("successfully Unpacked!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void SRepacked()
        {
            MessageBox.Show("successfully Repacked!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}
