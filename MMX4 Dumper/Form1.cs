using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;

namespace MMX4_Dumper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        DUMP dump = new DUMP();

        private void Form1_Load(object sender, EventArgs e)
        {

        }



        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Selecione o Arquivo";

            ofd.Filter = ".BIN|*.BIN";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                dump.Dump(ofd.FileName);
                MessageBox.Show("Dumpado com sucesso!");
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            ofd.Title = "Selecione o Arquivo";
            fbd.Description = "Selecione uma pasta para salvar o arquivo";

            ofd.Filter = "DUMPInfo.xml|DUMPInfo.xml";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    dump.Insert(ofd.FileName, fbd.SelectedPath);
                    MessageBox.Show("Inserido com sucesso!");
                }
            }
        }


    }
}
