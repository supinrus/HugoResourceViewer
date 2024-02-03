using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Hugo_Viewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            pal = new byte[768];
            pic = null;
            offsetsInFile = null;
            sizesInFile = null;
            archive = false;
            numPal = null;
        }

        private string myfld;

        private byte[] pal;
        private byte[] pic;
        private int w, h;
        private int typePic;
        private bool archive;
        private UInt32[] offsetsInFile;
        private UInt32[] sizesInFile;
        private int[] numPal;
        private int countPal;

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void openArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                button1.Enabled = false;
                button2.Enabled = false;
                archive = true;
                myfld = openFileDialog1.FileName;
                pictureBox1.Image = null;
                comboBox1.Items.Clear();
                listBox1.Items.Clear();
                comboBox2.Items.Clear();
                pic = null;
                try
                {
                    using (FileStream fp = new FileStream(myfld, FileMode.Open, FileAccess.Read))
                    {
                        byte[] head = new byte[7];
                        fp.Read(head, 0, 7);
                        string he = Encoding.ASCII.GetString(head, 0, 6);
                        if (he == "ITERES")
                        {
                            head = new byte[12];
                            fp.Seek(6, SeekOrigin.Begin);
                            fp.Read(head, 0, 12);
                            UInt32 p1 = BitConverter.ToUInt32(head, 0);
                            UInt32 p2 = BitConverter.ToUInt32(head, 4);
                            UInt32 num = BitConverter.ToUInt32(head, 8);
                            offsetsInFile = new UInt32[num];
                            sizesInFile = new UInt32[num];
                            byte[] buf = new byte[p1 * p2];
                            fp.Seek(0x12, SeekOrigin.Begin);
                            fp.Read(buf, 0, (int)(p1 * p2));
                            int i = 0;
                            countPal = 0;
                            numPal = new int[num];
                            for (int k = 0; k < num; ++k)
                            {
                                sizesInFile[k] = BitConverter.ToUInt32(buf, i);
                                offsetsInFile[k] = BitConverter.ToUInt32(buf, i + 4) + p1 * p2;
                                i = i + 12;
                                string newname = "";
                                while (buf[i] != 0)
                                {
                                    newname = newname + (char)buf[i];
                                    ++i;
                                }
                                ++i;
                                listBox1.Items.Add(newname);
                                if (testPal(newname, offsetsInFile[k]))
                                {
                                    numPal[countPal] = k;
                                    countPal++;
                                    comboBox2.Items.Add(newname);
                                }
                            }
                            listBox1.Enabled = true;
                            comboBox2.SelectedIndex = 0;
                        }
                        else
                        {
                            he = Encoding.ASCII.GetString(head, 0, 7);
                            if (he == "BIGFILE")
                            {
                                fp.Seek(7, SeekOrigin.Begin);
                                fp.Read(head, 0, 4);
                                UInt32 fOffset = BitConverter.ToUInt32(head, 0);
                                UInt32 num = ((UInt32)fp.Length - fOffset)/0x10C;
                                offsetsInFile = new UInt32[num];
                                sizesInFile = new UInt32[num];
                                fp.Seek(fOffset, SeekOrigin.Begin);
                                byte[] buf = new byte[num * 0x10C];
                                fp.Read(buf, 0, (int)(num * 0x10C));
                                countPal = 0;
                                numPal = new int[num];
                                for (int k = 0; k < num; ++k)
                                {
                                    sizesInFile[k] = BitConverter.ToUInt32(buf, k * 0x10C + 0x108);
                                    offsetsInFile[k] = BitConverter.ToUInt32(buf, k * 0x10C + 0x104);
                                    int i = k * 0x10C;
                                    string newname = "";
                                    while ((buf[i] != 0)&&(i < k * 0x10C + 0x104))
                                    {
                                        newname = newname + (char)buf[i];
                                        ++i;
                                    }
                                    listBox1.Items.Add(newname);
                                    if (testPal(newname, offsetsInFile[k]))
                                    {
                                        numPal[countPal] = k;
                                        countPal++;
                                        comboBox2.Items.Add(newname);
                                    }
                                }
                                listBox1.Enabled = true;
                                comboBox2.SelectedIndex = 0;
                            }
                            else
                            {
                                fp.Seek(0, SeekOrigin.Begin);
                                fp.Read(head, 0, 4);
                                UInt32 fOffset = BitConverter.ToUInt32(head, 0) + 4;
                                if (fOffset != 134284558)
                                {
                                    fp.Seek(fOffset, SeekOrigin.Begin);
                                    fp.Read(head, 0, 4);
                                    UInt32 num = BitConverter.ToUInt32(head, 0);
                                    offsetsInFile = new UInt32[num];
                                    sizesInFile = new UInt32[num];
                                    countPal = 0;
                                    numPal = new int[num];
                                    byte[] buf = new byte[num*0x48];
                                    fp.Read(buf, 0, (int)num * 0x48);
                                    for (int k = 0; k < num; ++k)
                                    {
                                        sizesInFile[k] = BitConverter.ToUInt32(buf, k * 0x48 + 0x44);
                                        offsetsInFile[k] = BitConverter.ToUInt32(buf, k * 0x48 + 0x40);
                                        int i = k * 0x48;
                                        string newname = "";
                                        while ((buf[i] != 0) && (i < k * 0x48 + 0x40))
                                        {
                                            newname = newname + (char)buf[i];
                                            ++i;
                                        }
                                        listBox1.Items.Add(newname);
                                        if (testPal(newname, offsetsInFile[k]))
                                        {
                                            numPal[countPal] = k;
                                            countPal++;
                                            comboBox2.Items.Add(newname);
                                        }
                                    }
                                    listBox1.Enabled = true;
                                    comboBox2.SelectedIndex = 0;
                                }
                                else
                                {
                                    fp.Seek(-0x8000, SeekOrigin.End);
                                    byte[] tmp = new byte[11];
                                    fp.Read(tmp, 0, 11);
                                    string tmpstr = Encoding.ASCII.GetString(tmp, 0, 11);
                                    if (tmpstr == "charset.pcc")
                                    {
                                        UInt32 num = 0;
                                        string fname = "started";
                                        byte[] buf;
                                        while (fname != "")
                                        {
                                            fname = "";
                                            buf = new byte[40];
                                            fp.Seek(-0x8000 + num * 40, SeekOrigin.End);
                                            fp.Read(buf, 0, 40);
                                            for (int i = 0; buf[i] != 0; ++i)
                                            {
                                                fname = fname + (char)buf[i];
                                            }
                                            if (fname != "")
                                            {
                                                num++;
                                            }
                                        }
                                        offsetsInFile = new UInt32[num];
                                        sizesInFile = new UInt32[num];
                                        countPal = 0;
                                        numPal = new int[num];
                                        buf = new byte[40 * num];
                                        fp.Seek(-0x8000, SeekOrigin.End);
                                        fp.Read(buf, 0, (int)(40 * num));
                                        for (int k = 0; k < num; ++k)
                                        {
                                            sizesInFile[k] = BitConverter.ToUInt32(buf, k * 40 + 36);
                                            offsetsInFile[k] = BitConverter.ToUInt32(buf, k * 40 + 32);
                                            int i = k * 40;
                                            string newname = "";
                                            while ((buf[i] != 0) && (i < k * 40 + 32))
                                            {
                                                newname = newname + (char)buf[i];
                                                ++i;
                                            }
                                            listBox1.Items.Add(newname);
                                            if (testPal(newname, offsetsInFile[k]))
                                            {
                                                numPal[countPal] = k;
                                                countPal++;
                                                comboBox2.Items.Add(newname);
                                            }
                                        }
                                        listBox1.Enabled = true;
                                        comboBox2.SelectedIndex = 0;
                                    }
                                    else
                                    {
                                        fp.Seek(-0x10000, SeekOrigin.End);
                                        fp.Read(tmp, 0, 11);
                                        tmpstr = Encoding.ASCII.GetString(tmp, 0, 11);
                                        if (tmpstr == "charset.pcc")
                                        {
                                            UInt32 num = 0;
                                            string fname = "started";
                                            byte[] buf;
                                            while (fname != "")
                                            {
                                                fname = "";
                                                buf = new byte[0x48];
                                                fp.Seek(-0x10000 + num * 0x48, SeekOrigin.End);
                                                fp.Read(buf, 0, 0x48);
                                                for (int i = 0; buf[i] != 0; ++i)
                                                {
                                                    fname = fname + (char)buf[i];
                                                }
                                                if (fname != "")
                                                {
                                                    num++;
                                                }
                                            }
                                            offsetsInFile = new UInt32[num];
                                            sizesInFile = new UInt32[num];
                                            countPal = 0;
                                            numPal = new int[num];
                                            buf = new byte[0x48 * num];
                                            fp.Seek(-0x10000, SeekOrigin.End);
                                            fp.Read(buf, 0, (int)(0x48 * num));
                                            for (int k = 0; k < num; ++k)
                                            {
                                                sizesInFile[k] = BitConverter.ToUInt32(buf, k * 0x48 + 0x44);
                                                offsetsInFile[k] = BitConverter.ToUInt32(buf, k * 0x48 + 0x40);
                                                int i = k * 0x48;
                                                string newname = "";
                                                while ((buf[i] != 0) && (i < k * 0x48 + 0x44))
                                                {
                                                    newname = newname + (char)buf[i];
                                                    ++i;
                                                }
                                                listBox1.Items.Add(newname);
                                                if (testPal(newname, offsetsInFile[k]))
                                                {
                                                    numPal[countPal] = k;
                                                    countPal++;
                                                    comboBox2.Items.Add(newname);
                                                }
                                            }
                                            listBox1.Enabled = true;
                                            comboBox2.SelectedIndex = 0;
                                        }
                                        else
                                        {
                                            MessageBox.Show(myfld + "is not Hugo archive");
                                        }
                                    }
                                }
                            }
                        }
                        fp.Close();
                    }
                }
                catch (IOException)
                {
                    MessageBox.Show(myfld + " is not opened.");
                }
            }
        }

        private void openFoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                button1.Enabled = false;
                button2.Enabled = false;
                archive = false;
                myfld = folderBrowserDialog1.SelectedPath;
                string [] fn = Directory.GetFiles(myfld);
                pictureBox1.Image = null;
                comboBox1.Items.Clear();
                listBox1.Items.Clear();
                comboBox2.Items.Clear();
                pic = null;
                offsetsInFile = new UInt32[fn.Count()];
                sizesInFile = new UInt32[fn.Count()];
                numPal = new int[fn.Count()];
                countPal = 0;
                for (int i = 0; i < fn.Count(); ++i)
                {
                    offsetsInFile[i] = 0;
                    sizesInFile[i] = 0;
                    string newfn = fn[i].Remove(0,myfld.Length+1);
                    if (testExt(newfn))
                    {
                        listBox1.Items.Add(newfn);
                    }
                    if (testPal(newfn,0))
                    {
                        numPal[countPal] = i;
                        countPal++;
                        comboBox2.Items.Add(newfn);
                    }
                }
                if (listBox1.Items.Count != 0)
                {
                    listBox1.Enabled = true;
                }
                else
                {
                    listBox1.Enabled = false;
                }
                if (comboBox2.Items.Count != 0)
                {
                    comboBox2.SelectedIndex = 0;
                }
            }
        }

        private bool testExt(string fn)
        {
            string ext = fn.Remove(0, fn.Length - 4);
            ext = ext.ToLower();
            if ((ext == ".cgf") || (ext == ".raw") || (ext == ".lzp") || (ext == ".pal") || (ext == ".ti2") || (ext == ".ti4")
                || (ext == ".til") || (ext == ".cbr") || (ext == ".blk") || (ext == ".pbr") || (ext == ".pic") || (ext == ".brs"))
            {
                return true;
            }
            return false;
        }

        private bool testPal(string fn, UInt32 palOffset)
        {
            string ext = fn.Remove(0, fn.Length - 4);
            ext = ext.ToLower();
            if ((ext == ".raw") || (ext == ".lzp") || (ext == ".pal") || (ext == ".til") || (ext == ".ti2") || (ext == ".ti4") || (ext == ".blk") || (ext == ".pic"))
            {
                return true;
            }
            else if ((ext == ".cgf"))
            {
                cgfFile tmp;
                if (archive)
                {
                    tmp = new cgfFile(myfld, palOffset);
                }
                else
                {
                    tmp = new cgfFile(myfld + '\\' + fn, palOffset);
                }
                if (tmp.getNumPal() != 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = false; ;
            string fn = listBox1.Items[listBox1.SelectedIndex].ToString();
            string ext = fn.Remove(0, fn.Length - 4);
            ext = ext.ToLower();
            pictureBox1.Image = null;
            comboBox1.Items.Clear();
            if (archive)
            {
                button1.Enabled = true;
            }
            if (ext == ".cgf")
            {
                typePic = 2;
                cgfFile c;
                if (archive)
                {
                    c = new cgfFile(myfld, offsetsInFile[listBox1.SelectedIndex]);
                }
                else
                {
                    c = new cgfFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex]);
                }
                if (c.getStatus())
                {
                    button2.Enabled = true;
                    pic = c.getPicture(0, out w, out h);
                    if (pic != null)
                    {
                        pictureBox1.Image = new Bitmap(w, h);
                    }
                    else
                    {
                        pictureBox1.Image = null;
                    }
                    pictureBox1.Visible = true;
                    for (int i = 0; i < h; ++i)
                        for (int j = 0; j < w; ++j)
                        {
                            int a = pic[i * w * 2 + j * 2 + 1];
                            int r = pal[pic[i * w * 2 + j * 2] * 3 + 2];
                            int g = pal[pic[i * w * 2 + j * 2] * 3 + 1];
                            int b = pal[pic[i * w * 2 + j * 2] * 3];
                            ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                        }
                    pictureBox1.Refresh();
                    comboBox1.Items.Clear();
                    for (int i = 0; i < c.getNum(); ++i)
                    {
                        comboBox1.Items.Add(i);
                    }
                    comboBox1.SelectedIndex = 0;
                }
            }
            else if ((ext == ".til") || (ext == ".ti2") || (ext == ".ti4"))
            {
                typePic = 1;
                tilFile c;
                if (archive)
                {
                    c = new tilFile(myfld, offsetsInFile[listBox1.SelectedIndex]);
                }
                else
                {
                    c = new tilFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex]);
                }
                if (c.getStatus())
                {
                    button2.Enabled = true;
                    pic = c.getPicture(0, out w, out h);
                    pictureBox1.Image = new Bitmap(w, h);
                    pictureBox1.Visible = true;
                    for (int i = 0; i < h; ++i)
                        for (int j = 0; j < w; ++j)
                        {
                            int a = 255;
                            int r = pal[pic[i * w + j] * 3 + 2];
                            int g = pal[pic[i * w + j] * 3 + 1];
                            int b = pal[pic[i * w + j] * 3];
                            ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                        }
                    pictureBox1.Refresh();
                    comboBox1.Items.Clear();
                    for (int i = 0; i < c.getNum(); ++i)
                    {
                        comboBox1.Items.Add(i);
                    }
                    comboBox1.SelectedIndex = 0;
                }
            }
            else if ((ext == ".raw") || (ext == ".blk") || (ext == ".pic"))
            {
                typePic = 1;
                rawFile c;
                if (archive)
                {
                    c = new rawFile(myfld, offsetsInFile[listBox1.SelectedIndex]);
                }
                else
                {
                    c = new rawFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex]);
                }
                if (c.getStatus())
                {
                    button2.Enabled = true;
                    pic = c.getPicture(0, out w, out h);
                    pictureBox1.Image = new Bitmap(w, h);
                    pictureBox1.Visible = true;
                    for (int i = 0; i < h; ++i)
                        for (int j = 0; j < w; ++j)
                        {
                            int a = 255;
                            int r = pal[pic[i * w + j] * 3 + 2];
                            int g = pal[pic[i * w + j] * 3 + 1];
                            int b = pal[pic[i * w + j] * 3];
                            ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                        }
                    pictureBox1.Refresh();
                    comboBox1.Items.Clear();
                    for (int i = 0; i < c.getNum(); ++i)
                    {
                        comboBox1.Items.Add(i);
                    }
                    comboBox1.SelectedIndex = 0;
                }
            }
            else if (ext == ".lzp")
            {
                typePic = 1;
                lzpFile c;
                if (archive)
                {
                    c = new lzpFile(myfld, offsetsInFile[listBox1.SelectedIndex]);
                }
                else
                {
                    c = new lzpFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex]);
                }
                if (c.getStatus())
                {
                    button2.Enabled = true;
                    pic = c.getPicture(0, out w, out h);
                    pictureBox1.Image = new Bitmap(w, h);
                    pictureBox1.Visible = true;
                    for (int i = 0; i < h; ++i)
                        for (int j = 0; j < w; ++j)
                        {
                            int a = 255;
                            int r = pal[pic[i * w + j] * 3 + 2];
                            int g = pal[pic[i * w + j] * 3 + 1];
                            int b = pal[pic[i * w + j] * 3];
                            ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                        }
                    pictureBox1.Refresh();
                    comboBox1.Items.Clear();
                    for (int i = 0; i < c.getNum(); ++i)
                    {
                        comboBox1.Items.Add(i);
                    }
                    comboBox1.SelectedIndex = 0;
                }
            }
            else if (ext == ".cbr")
            {
                typePic = 2;
                cbrFile c;
                if (archive)
                {
                    c = new cbrFile(myfld, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                }
                else
                {
                    c = new cbrFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                }
                if (c.getStatus())
                {
                    button2.Enabled = true;
                    pic = c.getPicture(0, out w, out h);
                    pictureBox1.Image = new Bitmap(w, h);
                    pictureBox1.Visible = true;
                    for (int i = 0; i < h; ++i)
                        for (int j = 0; j < w; ++j)
                        {
                            int a = pic[i * w * 2 + j * 2 + 1];
                            int r = pal[pic[i * w * 2 + j * 2] * 3 + 2];
                            int g = pal[pic[i * w * 2 + j * 2] * 3 + 1];
                            int b = pal[pic[i * w * 2 + j * 2] * 3];
                            ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                        }
                    pictureBox1.Refresh();
                    comboBox1.Items.Clear();
                    for (int i = 0; i < c.getNum(); ++i)
                    {
                        comboBox1.Items.Add(i);
                    }
                    comboBox1.SelectedIndex = 0;
                }
                else
                {
                    pbrFile c2;
                    if (archive)
                    {
                        c2 = new pbrFile(myfld, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                    }
                    else
                    {
                        c2 = new pbrFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                    }
                    if (c2.getStatus())
                    {
                        button2.Enabled = true;
                        pic = c2.getPicture(0, out w, out h);
                        pictureBox1.Image = new Bitmap(w, h);
                        pictureBox1.Visible = true;
                        for (int i = 0; i < h; ++i)
                            for (int j = 0; j < w; ++j)
                            {
                                int a = pic[i * w * 2 + j * 2 + 1];
                                int r = pal[pic[i * w * 2 + j * 2] * 3 + 2];
                                int g = pal[pic[i * w * 2 + j * 2] * 3 + 1];
                                int b = pal[pic[i * w * 2 + j * 2] * 3];
                                ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                            }
                        pictureBox1.Refresh();
                        comboBox1.Items.Clear();
                        for (int i = 0; i < c2.getNum(); ++i)
                        {
                            comboBox1.Items.Add(i);
                        }
                        comboBox1.SelectedIndex = 0;
                    }
                }
            }
            else if (ext == ".pbr")
            {
                typePic = 2;
                pbrFile c;
                if (archive)
                {
                    c = new pbrFile(myfld, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                }
                else
                {
                    c = new pbrFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                }
                if (c.getStatus())
                {
                    button2.Enabled = true;
                    pic = c.getPicture(0, out w, out h);
                    pictureBox1.Image = new Bitmap(w, h);
                    pictureBox1.Visible = true;
                    for (int i = 0; i < h; ++i)
                        for (int j = 0; j < w; ++j)
                        {
                            int a = pic[i * w * 2 + j * 2 + 1];
                            int r = pal[pic[i * w * 2 + j * 2] * 3 + 2];
                            int g = pal[pic[i * w * 2 + j * 2] * 3 + 1];
                            int b = pal[pic[i * w * 2 + j * 2] * 3];
                            ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                        }
                    pictureBox1.Refresh();
                    comboBox1.Items.Clear();
                    for (int i = 0; i < c.getNum(); ++i)
                    {
                        comboBox1.Items.Add(i);
                    }
                    comboBox1.SelectedIndex = 0;
                }
            }
            else if (ext == ".brs")
            {
                typePic = 2;
                brsFile c;
                if (archive)
                {
                    c = new brsFile(myfld, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                }
                else
                {
                    c = new brsFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                }
                if (c.getStatus())
                {
                    button2.Enabled = true;
                    pic = c.getPicture(0, out w, out h);
                    pictureBox1.Image = new Bitmap(w, h);
                    pictureBox1.Visible = true;
                    for (int i = 0; i < h; ++i)
                        for (int j = 0; j < w; ++j)
                        {
                            int a = pic[i * w * 2 + j * 2 + 1];
                            int r = pal[pic[i * w * 2 + j * 2] * 3 + 2];
                            int g = pal[pic[i * w * 2 + j * 2] * 3 + 1];
                            int b = pal[pic[i * w * 2 + j * 2] * 3];
                            ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                        }
                    pictureBox1.Refresh();
                    comboBox1.Items.Clear();
                    for (int i = 0; i < c.getNum(); ++i)
                    {
                        comboBox1.Items.Add(i);
                    }
                    comboBox1.SelectedIndex = 0;
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string fn = listBox1.Items[listBox1.SelectedIndex].ToString();
            string ext = fn.Remove(0, fn.Length - 4);
            ext = ext.ToLower();
            if (ext == ".cgf")
            {
                cgfFile c;
                if (archive)
                {
                    c = new cgfFile(myfld, offsetsInFile[listBox1.SelectedIndex]);
                }
                else
                {
                    c = new cgfFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex]);
                }
                if (c.getStatus())
                {
                    pic = c.getPicture(comboBox1.SelectedIndex, out w, out h);
                    if ((pic != null)&&(w != 0)&&(h != 0))
                    {
                        pictureBox1.Image = new Bitmap(w, h);
                    }
                    else
                    {
                        pictureBox1.Image = null;
                    }
                    pictureBox1.Visible = true;
                    for (int i = 0; i < h; ++i)
                        for (int j = 0; j < w; ++j)
                        {
                            int a = pic[i * w * 2 + j * 2 + 1];
                            int r = pal[pic[i * w * 2 + j * 2] * 3 + 2];
                            int g = pal[pic[i * w * 2 + j * 2] * 3 + 1];
                            int b = pal[pic[i * w * 2 + j * 2] * 3];
                            ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                        }
                    pictureBox1.Refresh();
                }
            }
            else if ((ext == ".til") || (ext == ".ti2") || (ext == ".ti4"))
            {
                tilFile c;
                if (archive)
                {
                    c = new tilFile(myfld, offsetsInFile[listBox1.SelectedIndex]);
                }
                else
                {
                    c = new tilFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex]);
                }
                if (c.getStatus())
                {
                    pic = c.getPicture(comboBox1.SelectedIndex, out w, out h);
                    pictureBox1.Image = new Bitmap(w, h);
                    pictureBox1.Visible = true;
                    for (int i = 0; i < h; ++i)
                        for (int j = 0; j < w; ++j)
                        {
                            int a = 255;
                            int r = pal[pic[i * w + j] * 3 + 2];
                            int g = pal[pic[i * w + j] * 3 + 1];
                            int b = pal[pic[i * w + j] * 3];
                            ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                        }
                    pictureBox1.Refresh();
                }
            }
            else if (ext == ".lzp")
            {
                lzpFile c;
                if (archive)
                {
                    c = new lzpFile(myfld, offsetsInFile[listBox1.SelectedIndex]);
                }
                else
                {
                    c = new lzpFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex]);
                }
                if (c.getStatus())
                {
                    pic = c.getPicture(comboBox1.SelectedIndex, out w, out h);
                    pictureBox1.Image = new Bitmap(w, h);
                    pictureBox1.Visible = true;
                    for (int i = 0; i < h; ++i)
                        for (int j = 0; j < w; ++j)
                        {
                            int a = 255;
                            int r = pal[pic[i * w + j] * 3 + 2];
                            int g = pal[pic[i * w + j] * 3 + 1];
                            int b = pal[pic[i * w + j] * 3];
                            ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                        }
                    pictureBox1.Refresh();
                }
            }
            else if (ext == ".cbr")
            {
                cbrFile c;
                if (archive)
                {
                    c = new cbrFile(myfld, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                }
                else
                {
                    c = new cbrFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                }
                if (c.getStatus())
                {
                    pic = c.getPicture(comboBox1.SelectedIndex, out w, out h);
                    if (pic != null)
                    {
                        pictureBox1.Image = new Bitmap(w, h);
                    }
                    else
                    {
                        pictureBox1.Image = null;
                    }
                    pictureBox1.Visible = true;
                    for (int i = 0; i < h; ++i)
                        for (int j = 0; j < w; ++j)
                        {
                            int a = pic[i * w * 2 + j * 2 + 1];
                            int r = pal[pic[i * w * 2 + j * 2] * 3 + 2];
                            int g = pal[pic[i * w * 2 + j * 2] * 3 + 1];
                            int b = pal[pic[i * w * 2 + j * 2] * 3];
                            ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                        }
                    pictureBox1.Refresh();
                }
                else
                {
                    pbrFile c2;
                    if (archive)
                    {
                        c2 = new pbrFile(myfld, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                    }
                    else
                    {
                        c2 = new pbrFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                    }
                    if (c2.getStatus())
                    {
                        pic = c2.getPicture(comboBox1.SelectedIndex, out w, out h);
                        if (pic != null)
                        {
                            pictureBox1.Image = new Bitmap(w, h);
                        }
                        else
                        {
                            pictureBox1.Image = null;
                        }
                        pictureBox1.Visible = true;
                        for (int i = 0; i < h; ++i)
                            for (int j = 0; j < w; ++j)
                            {
                                int a = pic[i * w * 2 + j * 2 + 1];
                                int r = pal[pic[i * w * 2 + j * 2] * 3 + 2];
                                int g = pal[pic[i * w * 2 + j * 2] * 3 + 1];
                                int b = pal[pic[i * w * 2 + j * 2] * 3];
                                ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                            }
                        pictureBox1.Refresh();
                    }
                }
            }
            else if (ext == ".pbr")
            {
                pbrFile c;
                if (archive)
                {
                    c = new pbrFile(myfld, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                }
                else
                {
                    c = new pbrFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                }
                if (c.getStatus())
                {
                    pic = c.getPicture(comboBox1.SelectedIndex, out w, out h);
                    if (pic != null)
                    {
                        pictureBox1.Image = new Bitmap(w, h);
                    }
                    else
                    {
                        pictureBox1.Image = null;
                    }
                    pictureBox1.Visible = true;
                    for (int i = 0; i < h; ++i)
                        for (int j = 0; j < w; ++j)
                        {
                            int a = pic[i * w * 2 + j * 2 + 1];
                            int r = pal[pic[i * w * 2 + j * 2] * 3 + 2];
                            int g = pal[pic[i * w * 2 + j * 2] * 3 + 1];
                            int b = pal[pic[i * w * 2 + j * 2] * 3];
                            ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                        }
                    pictureBox1.Refresh();
                }
            }
            else if (ext == ".brs")
            {
                brsFile c;
                if (archive)
                {
                    c = new brsFile(myfld, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                }
                else
                {
                    c = new brsFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                }
                if (c.getStatus())
                {
                    pic = c.getPicture(comboBox1.SelectedIndex, out w, out h);
                    if (pic != null)
                    {
                        pictureBox1.Image = new Bitmap(w, h);
                    }
                    else
                    {
                        pictureBox1.Image = null;
                    }
                    pictureBox1.Visible = true;
                    for (int i = 0; i < h; ++i)
                        for (int j = 0; j < w; ++j)
                        {
                            int a = pic[i * w * 2 + j * 2 + 1];
                            int r = pal[pic[i * w * 2 + j * 2] * 3 + 2];
                            int g = pal[pic[i * w * 2 + j * 2] * 3 + 1];
                            int b = pal[pic[i * w * 2 + j * 2] * 3];
                            ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                        }
                    pictureBox1.Refresh();
                }
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string fn = comboBox2.Items[comboBox2.SelectedIndex].ToString();
            string ext = fn.Remove(0, fn.Length - 4);
            ext = ext.ToLower();
            if (ext == ".cgf")
            {
                cgfFile c;
                if (archive)
                {
                    c = new cgfFile(myfld, offsetsInFile[numPal[comboBox2.SelectedIndex]]);
                }
                else
                {
                    c = new cgfFile(myfld + '\\' + fn, offsetsInFile[numPal[comboBox2.SelectedIndex]]);
                }
                if (c.getStatus())
                {
                    pal = c.getPal();
                    if (pic != null)
                    {
                        pictureBox1.Image = new Bitmap(w, h);
                        for (int i = 0; i < h; ++i)
                            for (int j = 0; j < w; ++j)
                            {
                                int a = 255;
                                if (typePic == 2)
                                {
                                    a = pic[i * w * typePic + j * typePic + 1];
                                }
                                int r = pal[pic[i * w * typePic + j * typePic] * 3 + 2];
                                int g = pal[pic[i * w * typePic + j * typePic] * 3 + 1];
                                int b = pal[pic[i * w * typePic + j * typePic] * 3];
                                ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                            }
                        pictureBox1.Refresh();
                    }
                }
            }
            else if ((ext == ".til") || (ext == ".ti2") || (ext == ".ti4"))
            {
                tilFile c;
                if (archive)
                {
                    c = new tilFile(myfld, offsetsInFile[numPal[comboBox2.SelectedIndex]]);
                }
                else
                {
                    c = new tilFile(myfld + '\\' + fn, offsetsInFile[numPal[comboBox2.SelectedIndex]]);
                }
                if (c.getStatus())
                {
                    pal = c.getPal();
                    if (pic != null)
                    {
                        pictureBox1.Image = new Bitmap(w, h);
                        for (int i = 0; i < h; ++i)
                            for (int j = 0; j < w; ++j)
                            {
                                int a = 255;
                                if (typePic == 2)
                                {
                                    a = pic[i * w * typePic + j * typePic + 1];
                                }
                                int r = pal[pic[i * w * typePic + j * typePic] * 3 + 2];
                                int g = pal[pic[i * w * typePic + j * typePic] * 3 + 1];
                                int b = pal[pic[i * w * typePic + j * typePic] * 3];
                                ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                            }
                        pictureBox1.Refresh();
                    }
                }
            }
            else if ((ext == ".raw") || (ext == ".blk") || (ext == ".pic"))
            {
                rawFile c;
                if (archive)
                {
                    c = new rawFile(myfld, offsetsInFile[numPal[comboBox2.SelectedIndex]]);
                }
                else
                {
                    c = new rawFile(myfld + '\\' + fn, offsetsInFile[numPal[comboBox2.SelectedIndex]]);
                }
                if (c.getStatus())
                {
                    pal = c.getPal();
                    if (pic != null)
                    {
                        pictureBox1.Image = new Bitmap(w, h);
                        for (int i = 0; i < h; ++i)
                            for (int j = 0; j < w; ++j)
                            {
                                int a = 255;
                                if (typePic == 2)
                                {
                                    a = pic[i * w * typePic + j * typePic + 1];
                                }
                                int r = pal[pic[i * w * typePic + j * typePic] * 3 + 2];
                                int g = pal[pic[i * w * typePic + j * typePic] * 3 + 1];
                                int b = pal[pic[i * w * typePic + j * typePic] * 3];
                                ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                            }
                        pictureBox1.Refresh();
                    }
                }
            }
            else if (ext == ".lzp")
            {
                lzpFile c;
                if (archive)
                {
                    c = new lzpFile(myfld, offsetsInFile[numPal[comboBox2.SelectedIndex]]);
                }
                else
                {
                    c = new lzpFile(myfld + '\\' + fn, offsetsInFile[numPal[comboBox2.SelectedIndex]]);
                }
                if (c.getStatus())
                {
                    pal = c.getPal();
                    if (pic != null)
                    {
                        pictureBox1.Image = new Bitmap(w, h);
                        for (int i = 0; i < h; ++i)
                            for (int j = 0; j < w; ++j)
                            {
                                int a = 255;
                                if (typePic == 2)
                                {
                                    a = pic[i * w * typePic + j * typePic + 1];
                                }
                                int r = pal[pic[i * w * typePic + j * typePic] * 3 + 2];
                                int g = pal[pic[i * w * typePic + j * typePic] * 3 + 1];
                                int b = pal[pic[i * w * typePic + j * typePic] * 3];
                                ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                            }
                        pictureBox1.Refresh();
                    }
                }
            }
            else if (ext == ".pal")
            {
                palFile c;
                if (archive)
                {
                    c = new palFile(myfld, offsetsInFile[numPal[comboBox2.SelectedIndex]]);
                }
                else
                {
                    c = new palFile(myfld + '\\' + fn, offsetsInFile[numPal[comboBox2.SelectedIndex]]);
                }
                if (c.getStatus())
                {
                    pal = c.getPal();
                    if (pic != null)
                    {
                        pictureBox1.Image = new Bitmap(w, h);
                        for (int i = 0; i < h; ++i)
                            for (int j = 0; j < w; ++j)
                            {
                                int a = 255;
                                if (typePic == 2)
                                {
                                    a = pic[i * w * typePic + j * typePic + 1];
                                }
                                int r = pal[pic[i * w * typePic + j * typePic] * 3 + 2];
                                int g = pal[pic[i * w * typePic + j * typePic] * 3 + 1];
                                int b = pal[pic[i * w * typePic + j * typePic] * 3];
                                ((Bitmap)pictureBox1.Image).SetPixel(j, i, Color.FromArgb(a, r, g, b));
                            }
                        pictureBox1.Refresh();
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (FileStream fp = new FileStream(myfld, FileMode.Open, FileAccess.Read))
                {
                    string fn = listBox1.Items[listBox1.SelectedIndex].ToString();
                    fp.Seek(offsetsInFile[listBox1.SelectedIndex], SeekOrigin.Begin);
                    byte[] buf = new byte[sizesInFile[listBox1.SelectedIndex]];
                    fp.Read(buf, 0, (int)sizesInFile[listBox1.SelectedIndex]);
                    int i = 0;
                    for (int k = 0; k < fn.Length; ++k)
                    {
                        if (fn[k] == '\\')
                        {
                            i = k + 1;
                        }
                    }
                    fn = fn.Remove(0, i);
                    MessageBox.Show(fn);
                    saveFileDialog1.Filter = fn + '|' + fn;
                    saveFileDialog1.FileName = fn;
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            using (FileStream fpnew = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write))
                            {
                                fpnew.Write(buf, 0, (int)(sizesInFile[listBox1.SelectedIndex]));
                                fpnew.Close();
                            }
                        }
                        catch (IOException)
                        {
                            MessageBox.Show(saveFileDialog1.FileName + " is not saved.");
                        }
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show(myfld + " is not opened.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string fldpng = folderBrowserDialog1.SelectedPath;
                string fn = listBox1.Items[listBox1.SelectedIndex].ToString();
                string ext = fn.Remove(0, fn.Length - 4);
                ext = ext.ToLower();
                if (ext == ".cgf")
                {
                    cgfFile c;
                    if (archive)
                    {
                        c = new cgfFile(myfld, offsetsInFile[listBox1.SelectedIndex]);
                    }
                    else
                    {
                        c = new cgfFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex]);
                    }
                    if (c.getStatus())
                    {
                        Bitmap img = null;
                        for (int k = 0; k < c.getNum(); ++k)
                        {
                            pic = c.getPicture(k, out w, out h);
                            if ((pic != null) && (w != 0) && (h != 0))
                            {

                                img = new Bitmap(w, h);
                            }
                            else
                            {
                                img = null;
                            }
                            for (int i = 0; i < h; ++i)
                                for (int j = 0; j < w; ++j)
                                {
                                    int a = pic[i * w * 2 + j * 2 + 1];
                                    int r = pal[pic[i * w * 2 + j * 2] * 3 + 2];
                                    int g = pal[pic[i * w * 2 + j * 2] * 3 + 1];
                                    int b = pal[pic[i * w * 2 + j * 2] * 3];
                                    img.SetPixel(j, i, Color.FromArgb(a, r, g, b));
                                }
                            if (img != null)
                            {
                                string tmp = fn.Remove(fn.Length - 4, 4);
                                int j = 0;
                                for (int i = 0; i < tmp.Length; ++i)
                                {
                                    if (fn[i] == '\\')
                                    {
                                        j = i + 1;
                                    }
                                }
                                tmp = tmp.Remove(0, j);
                                string newpngname = fldpng + '\\' + tmp + '_' + k.ToString() + ".png";
                                img.Save(newpngname);
                                img.Dispose();
                            }
                        }
                    }
                }
                else if ((ext == ".til") || (ext == ".ti2") || (ext == ".ti4"))
                {
                    tilFile c;
                    if (archive)
                    {
                        c = new tilFile(myfld, offsetsInFile[listBox1.SelectedIndex]);
                    }
                    else
                    {
                        c = new tilFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex]);
                    }
                    if (c.getStatus())
                    {
                        Bitmap img = null;
                        for (int k = 0; k < c.getNum(); ++k)
                        {
                            pic = c.getPicture(k, out w, out h);
                            if ((pic != null) && (w != 0) && (h != 0))
                            {
                                img = new Bitmap(w, h);
                            }
                            else
                            {
                                img = null;
                            }
                            for (int i = 0; i < h; ++i)
                                for (int j = 0; j < w; ++j)
                                {
                                    int a = 255;
                                    int r = pal[pic[i * w + j] * 3 + 2];
                                    int g = pal[pic[i * w + j] * 3 + 1];
                                    int b = pal[pic[i * w + j] * 3];
                                    img.SetPixel(j, i, Color.FromArgb(a, r, g, b));
                                }
                            if (img != null)
                            {
                                string tmp = fn.Remove(fn.Length - 4, 4);
                                int j = 0;
                                for (int i = 0; i < tmp.Length; ++i)
                                {
                                    if (fn[i] == '\\')
                                    {
                                        j = i + 1;
                                    }
                                }
                                tmp = tmp.Remove(0, j);
                                string newpngname = fldpng + '\\' + tmp + '_' + k.ToString() + ".png";
                                img.Save(newpngname);
                                img.Dispose();
                            }
                        }
                    }
                }
                else if ((ext == ".raw") || (ext == ".blk") || (ext == ".pic"))
                {
                    rawFile c;
                    if (archive)
                    {
                        c = new rawFile(myfld, offsetsInFile[listBox1.SelectedIndex]);
                    }
                    else
                    {
                        c = new rawFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex]);
                    }
                    if (c.getStatus())
                    {
                        Bitmap img = null;
                        for (int k = 0; k < 1; ++k)
                        {
                            pic = c.getPicture(k, out w, out h);
                            if ((pic != null) && (w != 0) && (h != 0))
                            {
                                img = new Bitmap(w, h);
                            }
                            else
                            {
                                img = null;
                            }
                            for (int i = 0; i < h; ++i)
                                for (int j = 0; j < w; ++j)
                                {
                                    int a = 255;
                                    int r = pal[pic[i * w + j] * 3 + 2];
                                    int g = pal[pic[i * w + j] * 3 + 1];
                                    int b = pal[pic[i * w + j] * 3];
                                    img.SetPixel(j, i, Color.FromArgb(a, r, g, b));
                                }
                            if (img != null)
                            {
                                string tmp = fn.Remove(fn.Length - 4, 4);
                                int j = 0;
                                for (int i = 0; i < tmp.Length; ++i)
                                {
                                    if (fn[i] == '\\')
                                    {
                                        j = i + 1;
                                    }
                                }
                                tmp = tmp.Remove(0, j);
                                string newpngname = fldpng + '\\' + tmp + '_' + k.ToString() + ".png";
                                img.Save(newpngname);
                                img.Dispose();
                            }
                        }
                    }
                }
                else if (ext == ".lzp")
                {
                    lzpFile c;
                    if (archive)
                    {
                        c = new lzpFile(myfld, offsetsInFile[listBox1.SelectedIndex]);
                    }
                    else
                    {
                        c = new lzpFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex]);
                    }
                    if (c.getStatus())
                    {
                        Bitmap img = null;
                        for (int k = 0; k < c.getNum(); ++k)
                        {
                            pic = c.getPicture(k, out w, out h);
                            if ((pic != null) && (w != 0) && (h != 0))
                            {
                                img = new Bitmap(w, h);
                            }
                            else
                            {
                                img = null;
                            }
                            for (int i = 0; i < h; ++i)
                                for (int j = 0; j < w; ++j)
                                {
                                    int a = 255;
                                    int r = pal[pic[i * w + j] * 3 + 2];
                                    int g = pal[pic[i * w + j] * 3 + 1];
                                    int b = pal[pic[i * w + j] * 3];
                                    img.SetPixel(j, i, Color.FromArgb(a, r, g, b));
                                }
                            if (img != null)
                            {
                                string tmp = fn.Remove(fn.Length - 4, 4);
                                int j = 0;
                                for (int i = 0; i < tmp.Length; ++i)
                                {
                                    if (fn[i] == '\\')
                                    {
                                        j = i + 1;
                                    }
                                }
                                tmp = tmp.Remove(0, j);
                                string newpngname = fldpng + '\\' + tmp + '_' + k.ToString() + ".png";
                                img.Save(newpngname);
                                img.Dispose();
                            }
                        }
                    }
                }
                else if (ext == ".cbr")
                {
                    cbrFile c;
                    if (archive)
                    {
                        c = new cbrFile(myfld, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                    }
                    else
                    {
                        c = new cbrFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                    }
                    if (c.getStatus())
                    {
                        Bitmap img = null;
                        for (int k = 0; k < c.getNum(); ++k)
                        {
                            pic = c.getPicture(k, out w, out h);
                            if ((pic != null) && (w != 0) && (h != 0))
                            {
                                img = new Bitmap(w, h);
                            }
                            else
                            {
                                img = null;
                            }
                            for (int i = 0; i < h; ++i)
                                for (int j = 0; j < w; ++j)
                                {
                                    int a = pic[i * w * 2 + j * 2 + 1];
                                    int r = pal[pic[i * w * 2 + j * 2] * 3 + 2];
                                    int g = pal[pic[i * w * 2 + j * 2] * 3 + 1];
                                    int b = pal[pic[i * w * 2 + j * 2] * 3];
                                    img.SetPixel(j, i, Color.FromArgb(a, r, g, b));
                                }
                            if (img != null)
                            {
                                string tmp = fn.Remove(fn.Length - 4, 4);
                                int j = 0;
                                for (int i = 0; i < tmp.Length; ++i)
                                {
                                    if (fn[i] == '\\')
                                    {
                                        j = i + 1;
                                    }
                                }
                                tmp = tmp.Remove(0, j);
                                string newpngname = fldpng + '\\' + tmp + '_' + k.ToString() + ".png";
                                img.Save(newpngname);
                                img.Dispose();
                            }
                        }
                    }
                    else
                    {
                        pbrFile c2;
                        if (archive)
                        {
                            c2 = new pbrFile(myfld, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                        }
                        else
                        {
                            c2 = new pbrFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                        }
                        if (c2.getStatus())
                        {
                            Bitmap img = null;
                            for (int k = 0; k < c2.getNum(); ++k)
                            {
                                pic = c2.getPicture(k, out w, out h);
                                if ((pic != null) && (w != 0) && (h != 0))
                                {
                                    img = new Bitmap(w, h);
                                }
                                else
                                {
                                    img = null;
                                }
                                for (int i = 0; i < h; ++i)
                                    for (int j = 0; j < w; ++j)
                                    {
                                        int a = pic[i * w * 2 + j * 2 + 1];
                                        int r = pal[pic[i * w * 2 + j * 2] * 3 + 2];
                                        int g = pal[pic[i * w * 2 + j * 2] * 3 + 1];
                                        int b = pal[pic[i * w * 2 + j * 2] * 3];
                                        img.SetPixel(j, i, Color.FromArgb(a, r, g, b));
                                    }
                                if (img != null)
                                {
                                    string tmp = fn.Remove(fn.Length - 4, 4);
                                    int j = 0;
                                    for (int i = 0; i < tmp.Length; ++i)
                                    {
                                        if (fn[i] == '\\')
                                        {
                                            j = i + 1;
                                        }
                                    }
                                    tmp = tmp.Remove(0, j);
                                    string newpngname = fldpng + '\\' + tmp + '_' + k.ToString() + ".png";
                                    img.Save(newpngname);
                                    img.Dispose();
                                }
                            }
                        }
                    }
                }
                else if (ext == ".pbr")
                {
                    pbrFile c;
                    if (archive)
                    {
                        c = new pbrFile(myfld, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                    }
                    else
                    {
                        c = new pbrFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                    }
                    if (c.getStatus())
                    {
                        Bitmap img = null;
                        for (int k = 0; k < c.getNum(); ++k)
                        {
                            pic = c.getPicture(k, out w, out h);
                            if ((pic != null) && (w != 0) && (h != 0))
                            {
                                img = new Bitmap(w, h);
                            }
                            else
                            {
                                img = null;
                            }
                            for (int i = 0; i < h; ++i)
                                for (int j = 0; j < w; ++j)
                                {
                                    int a = pic[i * w * 2 + j * 2 + 1];
                                    int r = pal[pic[i * w * 2 + j * 2] * 3 + 2];
                                    int g = pal[pic[i * w * 2 + j * 2] * 3 + 1];
                                    int b = pal[pic[i * w * 2 + j * 2] * 3];
                                    img.SetPixel(j, i, Color.FromArgb(a, r, g, b));
                                }
                            if (img != null)
                            {
                                string tmp = fn.Remove(fn.Length - 4, 4);
                                int j = 0;
                                for (int i = 0; i < tmp.Length; ++i)
                                {
                                    if (fn[i] == '\\')
                                    {
                                        j = i + 1;
                                    }
                                }
                                tmp = tmp.Remove(0, j);
                                string newpngname = fldpng + '\\' + tmp + '_' + k.ToString() + ".png";
                                img.Save(newpngname);
                                img.Dispose();
                            }
                        }
                    }
                }
                else if (ext == ".brs")
                {
                    brsFile c;
                    if (archive)
                    {
                        c = new brsFile(myfld, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                    }
                    else
                    {
                        c = new brsFile(myfld + '\\' + fn, offsetsInFile[listBox1.SelectedIndex], sizesInFile[listBox1.SelectedIndex]);
                    }
                    if (c.getStatus())
                    {
                        Bitmap img = null;
                        for (int k = 0; k < c.getNum(); ++k)
                        {
                            pic = c.getPicture(k, out w, out h);
                            if ((pic != null) && (w != 0) && (h != 0))
                            {
                                img = new Bitmap(w, h);
                            }
                            else
                            {
                                img = null;
                            }
                            for (int i = 0; i < h; ++i)
                                for (int j = 0; j < w; ++j)
                                {
                                    int a = pic[i * w * 2 + j * 2 + 1];
                                    int r = pal[pic[i * w * 2 + j * 2] * 3 + 2];
                                    int g = pal[pic[i * w * 2 + j * 2] * 3 + 1];
                                    int b = pal[pic[i * w * 2 + j * 2] * 3];
                                    img.SetPixel(j, i, Color.FromArgb(a, r, g, b));
                                }
                            if (img != null)
                            {
                                string tmp = fn.Remove(fn.Length - 4, 4);
                                int j = 0;
                                for (int i = 0; i < tmp.Length; ++i)
                                {
                                    if (fn[i] == '\\')
                                    {
                                        j = i + 1;
                                    }
                                }
                                tmp = tmp.Remove(0, j);
                                string newpngname = fldpng + '\\' + tmp + '_' + k.ToString() + ".png";
                                img.Save(newpngname);
                                img.Dispose();
                            }
                        }
                    }
                }
            }
        }
    }
    public class cgfFile
    {
        private struct cgfHead
        {
            public string name;
            public UInt32 bt;
            public UInt32 num;
            public UInt32 sizedata;
            public UInt32 size;
            public UInt32 numpal;
            public UInt32 unk;
        };

        private struct cgfData
        {
            public UInt32 posx;
            public UInt32 posy;
            public UInt32 width;
            public UInt32 height;
            public UInt32 unk;
            public UInt32 offset;
        };
        private cgfHead head;
        private bool st;
        private string fn;
        private UInt32 cgfOffset;
        public cgfFile(string fileName, UInt32 myOffset)
        {
            fn = fileName;
            st = true;
            cgfOffset = myOffset;
            try
            {
                using (FileStream fp = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
                    byte[] buf = new byte[28];
                    fp.Seek(cgfOffset, SeekOrigin.Begin);
                    fp.Read(buf, 0, 28);
                    head.name = Encoding.Default.GetString(buf, 0, 4);
                    head.bt = BitConverter.ToUInt32(buf, 4);
                    head.num = BitConverter.ToUInt32(buf, 8);
                    head.sizedata = BitConverter.ToUInt32(buf, 12);
                    head.size = BitConverter.ToUInt32(buf, 16);
                    head.numpal = BitConverter.ToUInt32(buf, 20);
                    head.unk = BitConverter.ToUInt32(buf, 24);
                    fp.Close();
                }
            }
            catch (IOException)
            {
                MessageBox.Show(fn + " is not opened.");
                st = false;
            }
        }
        public bool getStatus()
        {
            return st;
        }
        public byte[] getPicture(int num, out int width, out int height)
        {
            width = 0;
            height = 0;
            cgfData d1;
            cgfData d2;
            byte[] pic = null;
            try
            {
                using (FileStream fp = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
                    byte[] databuf = new byte[24];
                    fp.Seek(cgfOffset + 28 + num * 24, SeekOrigin.Begin);
                    fp.Read(databuf, 0, 24);
                    d1.posx = BitConverter.ToUInt32(databuf, 0);
                    d1.posy = BitConverter.ToUInt32(databuf, 4);
                    d1.width = BitConverter.ToUInt32(databuf, 8);
                    d1.height = BitConverter.ToUInt32(databuf, 12);
                    d1.unk = BitConverter.ToUInt32(databuf, 16);
                    d1.offset = BitConverter.ToUInt32(databuf, 20);
                    if (num == head.num - 1)
                    {
                        d2.posx = 0;
                        d2.posy = 0;
                        d2.width = 0;
                        d2.height = 0;
                        d2.unk = 0;
                        d2.offset = 28 + head.sizedata + head.size;
                    }
                    else
                    {
                        fp.Seek(cgfOffset + 28 + (num + 1) * 24, SeekOrigin.Begin);
                        fp.Read(databuf, 0, 24);
                        d2.posx = BitConverter.ToUInt32(databuf, 0);
                        d2.posy = BitConverter.ToUInt32(databuf, 4);
                        d2.width = BitConverter.ToUInt32(databuf, 8);
                        d2.height = BitConverter.ToUInt32(databuf, 12);
                        d2.unk = BitConverter.ToUInt32(databuf, 16);
                        d2.offset = BitConverter.ToUInt32(databuf, 20);
                    }
                    width = (int)d1.width;
                    height = (int)d1.height;
                    pic = new byte[width * height * 2];
                    int sz = (int)d2.offset - (int)d1.offset;
                    if (sz == 0)
                    {
                        width = 0;
                        height = 0;
                        fp.Close();
                        return null;
                    }
                    byte[] buf = new byte[sz];
                    fp.Seek(cgfOffset + 28 + head.sizedata + d1.offset, SeekOrigin.Begin);
                    fp.Read(buf, 0, sz);
                    for (int i = 0, j = 0; i < d1.width * d1.height; )
                    {
                        sz = (int)BitConverter.ToUInt32(buf,j);
                        int k = 4, nmb;
                        while (k < sz)
                        {
                            if (buf[j + k] == 0)
                            {
                                k++;
                                nmb = buf[j + k];
                                k++;
                                if ((nmb == 0) && (sz == 6))
                                {
                                    nmb = 1;
                                }
                                while (nmb != 0)
                                {
                                    pic[i * 2] = 0;
                                    pic[i * 2 + 1] = 0;
                                    ++i;
                                    --nmb;
                                }
                            }
                            else if (buf[j + k] == 1)
                            {
                                k++;
                                nmb = buf[j + k];
                                k++;
                                while (nmb != 0)
                                {
                                    pic[i * 2] = buf[j + k];
                                    pic[i * 2 + 1] = buf[j + k + 1];
                                    ++i;
                                    k = k + 2;
                                    --nmb;
                                }
                            }
                            else if (buf[j + k] == 2)
                            {
                                k++;
                                nmb = buf[j + k];
                                k++;
                                while (nmb != 0)
                                {
                                    pic[i * 2] = buf[j + k];
                                    pic[i * 2 + 1] = buf[j + k + 1];
                                    ++i;
                                    --nmb;
                                }
                                k = k + 2;
                            }
                            else if (buf[j + k] == 3)
                            {
                                k++;
                                nmb = buf[j + k];
                                k++;
                                while (nmb != 0)
                                {
                                    pic[i * 2] = buf[j + k];
                                    pic[i * 2 + 1] = 255;
                                    ++i;
                                    ++k;
                                    --nmb;
                                }
                            }
                            else if (buf[j + k] == 4)
                            {
                                k++;
                                nmb = buf[j + k];
                                k++;
                                while (nmb != 0)
                                {
                                    pic[i * 2] = buf[j + k];
                                    pic[i * 2 + 1] = 255;
                                    ++i;
                                    --nmb;
                                }
                                ++k;
                            }
                        }
                        j = j + k;
                        while (i % d1.width != 0)
                        {
                            pic[i * 2] = 0;
                            pic[i * 2 + 1] = 0;
                            ++i;
                        }
                    }
                    fp.Close();
                }
            }
            catch (IOException)
            {
                MessageBox.Show(fn + " is not opened.");
                st = false;
            }
            return pic;
        }
        public byte[] getPal()
        {
            byte[] pal = null;
            byte[] buf = new byte[256 * 4];
            if (head.numpal != 0)
            {
                try
                {
                    using (FileStream fp = new FileStream(fn, FileMode.Open, FileAccess.Read))
                    {
                        fp.Seek(cgfOffset + 28 + head.size + head.sizedata, SeekOrigin.Begin);
                        fp.Read(buf, 0, (int)head.numpal * 4);
                        pal = new byte[256 * 3];
                        for (int i = 0; i < head.numpal; ++i)
                        {
                            pal[i * 3] = buf[i * 4];
                            pal[i * 3 + 1] = buf[i * 4 + 1];
                            pal[i * 3 + 2] = buf[i * 4 + 2];
                        }
                        fp.Close();
                    }
                }
                catch (IOException)
                {
                    MessageBox.Show(fn + " is not opened.");
                    st = false;
                }
            }
            return pal;
        }
        public int getNumPal()
        {
            return (int)head.numpal;
        }
        public int getNum()
        {
            return (int)head.num;
        }
    }
    public class tilFile
    {
        private struct tilHead
        {
            public UInt32 size;
    		public UInt16 unk1;
    		public UInt16 numfr;
	    	public UInt16 width;
	    	public UInt16 height;
	    	public UInt16 minfps;
	    	public UInt16 fps;
	    	public byte[] unk2;
	    	public UInt16 wi2;
	    	public UInt16 he2;
	    	public byte[] unk3;
            public byte[] pal;
        }
        private tilHead head;
        private string fn;
        private bool st;
        private UInt32 tilOffset;
        public tilFile(string fileName, UInt32 myOffset)
        {
            tilOffset = myOffset;
            head.unk2 = new byte[4];
            head.unk3 = new byte[8];
            head.pal = new byte[768];
            fn = fileName;
            st = true;
            try
            {
                using (FileStream fp = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
                    byte[] buf = new byte[32];
                    fp.Seek(tilOffset, SeekOrigin.Begin);
                    fp.Read(buf, 0, 32);
                    head.size = BitConverter.ToUInt32(buf, 0);
                    head.unk1 = BitConverter.ToUInt16(buf, 4);
                    head.numfr = (UInt16)(BitConverter.ToUInt16(buf, 6) + 2);
                    head.width = BitConverter.ToUInt16(buf, 8);
                    head.height = BitConverter.ToUInt16(buf, 10);
                    head.minfps = BitConverter.ToUInt16(buf, 12);
                    head.fps = BitConverter.ToUInt16(buf, 14);
                    head.unk2[0] = buf[16];
                    head.unk2[1] = buf[17];
                    head.unk2[2] = buf[18];
                    head.unk2[3] = buf[19];
                    head.wi2 = BitConverter.ToUInt16(buf, 21);
                    head.he2 = BitConverter.ToUInt16(buf, 23);
                    head.unk3[0] = buf[24];
                    head.unk3[1] = buf[25];
                    head.unk3[2] = buf[26];
                    head.unk3[3] = buf[27];
                    head.unk3[4] = buf[28];
                    head.unk3[5] = buf[29];
                    head.unk3[6] = buf[30];
                    head.unk3[7] = buf[31];
                    fp.Read(head.pal, 0, 768);
                    fp.Close();
                    //head.wi2 = (UInt16)(head.wi2 / 256 + (head.he2 % 256) * 256);
                    //head.he2 = (UInt16)(head.he2 / 256 + (head.unk3[0] % 256) * 256);
                }
            }
            catch (IOException)
            {
                MessageBox.Show(fn + " is not opened.");
                st = false;
            }
        }
        public bool getStatus()
        {
            return st;
        }
        public byte[] getPal()
        {
            byte[] pal = new byte[768];
            for (int i = 0; i < 256; ++i)
            {
                pal[i * 3] = head.pal[i * 3 + 2];
                pal[i * 3 + 1] = head.pal[i * 3 + 1];
                pal[i * 3 + 2] = head.pal[i * 3];
            }
            return pal;
        }
        public int getNumPal()
        {
            return 256;
        }
        public int getNum()
        {
            return (int)head.numfr;
        }
        public byte[] getPicture(int num, out int width, out int height)
        {
            width = head.width;
            height = head.height;
            byte[] pic = new byte[width * height];
            byte[] buf = new byte[head.wi2 * head.he2 * 2];
            try
            {
                using (FileStream fp = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
                    fp.Seek(tilOffset + 0x320 + num * head.wi2 * head.he2 * 2, SeekOrigin.Begin);
                    fp.Read(buf, 0, head.wi2 * head.he2 * 2);
                    for (int i = 0; i < head.he2; ++i)
                    {
                        for (int j = 0; j < head.wi2; ++j)
                        {
                            UInt16 z = BitConverter.ToUInt16(buf, i * head.wi2 * 2 + j * 2);
                            for (int k = 0; k < 0x10; ++k)
                            {
                                fp.Seek(tilOffset + 0x320 + head.numfr * head.wi2 * head.he2 * 2 + k * 16 + z * 256, SeekOrigin.Begin);
                                fp.Read(pic, head.width * i * 16 + j * 16 + k * head.width, 0x10);
                            }
                        }
                    }
                    fp.Close();
                }
            }
            catch (IOException)
            {
                MessageBox.Show(fn + " is not opened.");
                st = false;
            }
            return pic;
        }
    }
    public class rawFile
    {
        private class rawHead
        {
            public char[] name;
            public UInt16 version;
            public UInt16 width;
            public UInt16 height;
            public UInt16 numpal;
            public byte[] unk;
            public byte[] pal;
            public rawHead()
            {
                name = new char[6];
                unk = new byte[18];
                pal = new byte[768];
            }
        }
        private bool st;
        private string fn;
        private rawHead head;
        private UInt32 rawOffset;
        public rawFile(string fileName, UInt32 myOffset)
        {
            rawOffset = myOffset;
            fn = fileName;
            st = true;
            try
            {
                using (FileStream fp = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
                    head = new rawHead();
                    byte[] buf = new byte[14];
                    fp.Seek(rawOffset, SeekOrigin.Begin);
                    fp.Read(buf, 0, 14);
                    for (int i = 0; i < 6; ++i)
                    {
                        head.name[i] = (char)buf[i];
                    }
                    head.version = (UInt16)(buf[6] * 256 + buf[7]);
                    head.width = (UInt16)(buf[8] * 256 + buf[9]);
                    head.height = (UInt16)(buf[10] * 256 + buf[11]);
                    head.numpal = (UInt16)(buf[12] * 256 + buf[13]);
                    fp.Read(head.unk, 0, 18);
                    fp.Read(head.pal, 0, head.numpal*3);
                    fp.Close();
                }
            }
            catch (IOException)
            {
                MessageBox.Show(fn + " is not opened.");
                st = false;
            }
        }
        public bool getStatus()
        {
            return st;
        }
        public byte[] getPal()
        {
            byte[] pal = new byte[768];
            for (int i = 0; i < 256; ++i)
            {
                pal[i * 3] = head.pal[i * 3 + 2];
                pal[i * 3 + 1] = head.pal[i * 3 + 1];
                pal[i * 3 + 2] = head.pal[i * 3];
            }
            return pal;
        }
        public int getNumPal()
        {
            return head.numpal;
        }
        public int getNum()
        {
            return 1;
        }
        public byte[] getPicture(int num, out int width, out int height)
        {
            width = head.width;
            height = head.height;
            byte[] pic = new byte[width * height];
            try
            {
                using (FileStream fp = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
                    fp.Seek(rawOffset + 0x20 + head.numpal * 3, SeekOrigin.Begin);
                    fp.Read(pic, 0, width * height);
                    fp.Close();
                }
            }
            catch (IOException)
            {
                MessageBox.Show(fn + " is not opened.");
                st = false;
            }
            return pic;
        }
    }
    public class lzpFile
    {
        private class lzpHead
        {
            public UInt32 num;
            public UInt32 width;
            public UInt32 height;
            public UInt32 fps;
            public byte[] unk;
            public byte[] pal;
            public lzpHead()
            {
                unk = new byte[16];
                pal = new byte[0x300];
            }
        }
        private bool st;
        private string fn;
        private UInt32[] offset;
        private lzpHead head;
        private UInt32 lzpOffset;
        public lzpFile(string fileName, UInt32 myOffset)
        {
            lzpOffset = myOffset;
            fn = fileName;
            st = true;
            try
            {
                using (FileStream fp = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
                    head = new lzpHead();
                    byte[] buf = new byte[16];
                    fp.Seek(lzpOffset, SeekOrigin.Begin);
                    fp.Read(buf, 0, 16);
                    head.num = BitConverter.ToUInt32(buf, 0);
                    head.width = BitConverter.ToUInt32(buf, 4);
                    head.height = BitConverter.ToUInt32(buf, 8);
                    head.fps = BitConverter.ToUInt32(buf, 12);
                    fp.Read(head.unk, 0, 16);
                    fp.Read(head.pal, 0, 0x300);
                    offset = new UInt32[head.num];
                    /*fp.Seek(-head.num * 4, SeekOrigin.End);
                    byte[] buf2 = new byte[head.num * 4];
                    fp.Read(buf2, 0, (int)head.num * 4);
                    for (int i = 0; i < head.num; ++i)
                    {
                        offset[i] = BitConverter.ToUInt32(buf2, i * 4);
                    }*/
                    UInt32 currentOffset = 0x320;
                    for (int i = 0; i < head.num; ++i)
                    {
                        offset[i] = currentOffset;
                        fp.Seek(lzpOffset + currentOffset, SeekOrigin.Begin);
                        fp.Read(buf, 0, 4);
                        UInt32 mysz = BitConverter.ToUInt32(buf, 0);
                        currentOffset = currentOffset + mysz + 4;
                    }
                    fp.Close();
                    if (head.width == 0)
                    {
                        head.width = 320;
                    }
                    if (head.height == 0)
                    {
                        head.height = 240;
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show(fn + " is not opened.");
                st = false;
            }
        }
        public bool getStatus()
        {
            return st;
        }
        public byte[] getPal()
        {
            byte[] pal = new byte[768];
            for (int i = 0; i < 256; ++i)
            {
                pal[i * 3] = head.pal[i * 3 + 2];
                pal[i * 3 + 1] = head.pal[i * 3 + 1];
                pal[i * 3 + 2] = head.pal[i * 3];
            }
            return pal;
        }
        public int getNumPal()
        {
            return 256;
        }
        public int getNum()
        {
            return (int)head.num;
        }
        public byte[] getPicture(int num, out int width, out int height)
        {
            width = (int)head.width;
            height = (int)head.height;
            byte[] pic = new byte[width * height * 2];
            try
            {
                using (FileStream fp = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
                    byte[] buf1 = new byte[4];
                    UInt32 sz;
                    fp.Seek(lzpOffset + offset[num], SeekOrigin.Begin);
                    fp.Read(buf1, 0, 4);
                    sz = BitConverter.ToUInt32(buf1, 0);
                    byte[] buf = new byte[sz*2];
                    fp.Read(buf, 0, (int)sz);
                    fp.Close();
                    byte[] zbuf = new byte[0x1000];
                    for (int i = 0, j = 0, k = 0xFEE; j < head.width * head.height; )
                    {
                        UInt32 bt = (UInt32)(buf[i] + 0xFF00);
                        ++i;
                        while ((bt > 0xFF) && (j < head.width * head.height))
                        {
                            if ((bt & 1) != 0)
                            {
                                bt = bt / 2;
                                pic[j] = buf[i];
                                zbuf[k] = buf[i];
                                ++i;
                                ++j;
                                ++k;
                                k = k & 0xFFF;
                            }
                            else
                            {
                                bt = bt / 2;
                                UInt32 b1 = buf[i];
                                UInt32 b2 = buf[i + 1];
                                i = i + 2;
                                UInt32 rz = b2 / 16;
                                b1 = b1 + 0x100 * rz;
                                b2 = b2 % 16;
                                b2 = b2 + 3;
                                while (b2 != 0)
                                {
                                    pic[j] = zbuf[b1];
                                    zbuf[k] = zbuf[b1];
                                    ++j;
                                    ++k;
                                    --b2;
                                    ++b1;
                                    k = k & 0xFFF;
                                    b1 = b1 & 0xFFF;
                                }
                            }
                        }
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show(fn + " is not opened.");
                st = false;
            }
            return pic;
        }
    }
    public class palFile
    {
        private byte[] mypal;
        private bool st;
        private UInt32 palOffset;
        public palFile(string fileName, UInt32 myOffset)
        {
            string fn = fileName;
            palOffset = myOffset;
            st = true;
            mypal = new byte[0x300];
            try
            {
                using (FileStream fp = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
                    byte[] ch = new byte[16];
                    fp.Seek(palOffset, SeekOrigin.Begin);
                    fp.Read(ch, 0, 16);
                    string check = "";
                    for (int i = 0; i < 10; ++i)
                    {
                        check = check + (char)ch[i];
                    }
                    if (check == "CPAL768STD")
                    {
                        fp.Seek(palOffset + 10, SeekOrigin.Begin);
                        fp.Read(mypal, 0, 0x300);
                    }
                    else
                    {
                        check = "";
                        for (int i = 0; i < 12; ++i)
                        {
                            check = check + (char)ch[i];
                        }
                        if (check == "CPAL256X3STD")
                        {
                            fp.Seek(palOffset + 12, SeekOrigin.Begin);
                            fp.Read(mypal, 0, 0x300);
                        }
                        else if (check == "CPAL038X3STD")
                        {
                            fp.Seek(palOffset + 12, SeekOrigin.Begin);
                            fp.Read(mypal, 0, 38 * 3);
                        }
                        else if (check == "CPAL109X3STD")
                        {
                            fp.Seek(palOffset + 12, SeekOrigin.Begin);
                            fp.Read(mypal, 0, 109 * 3);
                        }
                    }
                    fp.Close();
                }
            }
            catch (IOException)
            {
                MessageBox.Show(fn + " is not opened.");
                st = false;
            }
        }
        public byte[] getPal()
        {
            byte[] pal = new byte[768];
            for (int i = 0; i < 256; ++i)
            {
                pal[i * 3] = mypal[i * 3];
                pal[i * 3 + 1] = mypal[i * 3 + 1];
                pal[i * 3 + 2] = mypal[i * 3 + 2];
            }
            return pal;
        }
        public bool getStatus()
        {
            return st;
        }
    }
    public class cbrFile
    {
        private class cbrHead
        {
            public UInt32 offsetOfOffsets;
            public byte[] unk;
            public cbrHead()
            {
                unk = new byte[8];
            }
        }
        private struct cbrData
        {
            public UInt32 width;
            public UInt32 height;
            public UInt32 posX;
            public UInt32 posY;
        }
        private bool st;
        private string fn;
        private long number;
        private cbrHead head;
        private UInt32[] offset;
        private UInt32 cbrOffset;
        private UInt32 cbrSize;
        public cbrFile(string fileName, UInt32 myOffset, UInt32 mySize)
        {
            head = new cbrHead();
            st = true;
            fn = fileName;
            cbrOffset = myOffset;
            cbrSize = mySize;
            try
            {
                using (FileStream fp = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
                    if (cbrSize == 0)
                    {
                        cbrSize = (UInt32)fp.Length;
                    }
                    byte[] buf = new byte[4];
                    fp.Seek(cbrOffset, SeekOrigin.Begin);
                    fp.Read(buf, 0, 4);
                    head.offsetOfOffsets = BitConverter.ToUInt32(buf, 0);
                    fp.Read(head.unk, 0, 8);
                    UInt32 tmp = BitConverter.ToUInt32(head.unk, 0);
                    if (tmp == 0)
                    {
                        st = false;
                    }
                    else
                    {
                        number = (cbrSize - head.offsetOfOffsets) / 4;
                        byte[] buf2 = new byte[number * 4];
                        fp.Seek(cbrOffset + head.offsetOfOffsets, SeekOrigin.Begin);
                        fp.Read(buf2, 0, (int)number * 4);
                        offset = new UInt32[number + 1];
                        for (int i = 0; i < number; ++i)
                        {
                            offset[i] = BitConverter.ToUInt32(buf2, i * 4);
                        }
                        offset[number] = head.offsetOfOffsets;
                    }
                    fp.Close();
                }
            }
            catch (IOException)
            {
                MessageBox.Show(fn + " is not opened.");
                st = false;
            }
        }
        public bool getStatus()
        {
            return st;
        }
        public int getNum()
        {
            return (int)number;
        }
        public byte[] getPicture(int num, out int width, out int height)
        {
            byte[] pic = null;
            width = 0;
            height = 0;
            try
            {
                using (FileStream fp = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
                    cbrData d;
                    int sz = (int)(offset[num + 1] - offset[num]);
                    byte[] buf = new byte[sz];
                    fp.Seek(cbrOffset + offset[num], SeekOrigin.Begin);
                    fp.Read(buf, 0, sz);
                    fp.Close();
                    d.width = BitConverter.ToUInt32(buf, 0);
                    d.height = BitConverter.ToUInt32(buf, 4);
                    d.posX = BitConverter.ToUInt32(buf, 8);
                    d.posY = BitConverter.ToUInt32(buf, 12);
                    width = (int)d.width;
                    height = (int)d.height;
                    pic = new byte[d.width * d.height * 2];
                    for (int i = 0; i < d.height; ++i)
                    {
                        for (int j = 0; j < d.width; ++j)
                        {
                            pic[(i * d.width + j) * 2] = 0;
                            pic[(i * d.width + j) * 2 + 1] = 0;
                        }
                    }
                    int k = 16;
                    for (int i = 0; i < d.height; ++i)
                    {
                        UInt16 j = BitConverter.ToUInt16(buf, k);
                        UInt16 n = BitConverter.ToUInt16(buf, k + 2);
                        k = k + 4;
                        while (n > 0)
                        {
                            pic[(i * d.width + j) * 2] = buf[k];
                            if (buf[k] != 0)
                            {
                                pic[(i * d.width + j) * 2 + 1] = 255;
                            }
                            else
                            {
                                pic[(i * d.width + j) * 2 + 1] = 0;
                            }
                            ++k;
                            ++j;
                            --n;
                        }
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show(fn + " is not opened.");
                st = false;
            }
            return pic;
        }
    }
    public class pbrFile
    {
        private class pbrHead
        {
            public UInt32 offsetOfOffsets;
            public byte[] unk;
            public pbrHead()
            {
                unk = new byte[12];
            }
        }
        private struct pbrData
        {
            public UInt16 posX;
            public UInt16 posY;
            public UInt16 width;
            public UInt16 height;
        }
        private bool st;
        private string fn;
        private long number;
        private pbrHead head;
        private UInt32[] offset;
        private UInt32 pbrOffset;
        private UInt32 pbrSize;
        public pbrFile(string fileName, UInt32 myOffset, UInt32 mySize)
        {
            head = new pbrHead();
            st = true;
            fn = fileName;
            pbrOffset = myOffset;
            pbrSize = mySize;
            try
            {
                using (FileStream fp = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
                    if (pbrSize == 0)
                    {
                        pbrSize = (UInt32)fp.Length;
                    }
                    byte[] buf = new byte[4];
                    fp.Seek(pbrOffset, SeekOrigin.Begin);
                    fp.Read(buf, 0, 4);
                    head.offsetOfOffsets = BitConverter.ToUInt32(buf, 0);
                    fp.Read(head.unk, 0, 12);
                    number = (pbrSize - head.offsetOfOffsets) / 4;
                    byte[] buf2 = new byte[number * 4];
                    fp.Seek(pbrOffset + head.offsetOfOffsets, SeekOrigin.Begin);
                    fp.Read(buf2, 0, (int)number * 4);
                    offset = new UInt32[number + 1];
                    for (int i = 0; i < number; ++i)
                    {
                        offset[i] = BitConverter.ToUInt32(buf2, i * 4);
                    }
                    offset[number] = head.offsetOfOffsets;
                    fp.Close();
                }
            }
            catch (IOException)
            {
                MessageBox.Show(fn + " is not opened.");
                st = false;
            }
        }
        private byte mysar4(byte b)
        {
            byte tmp = b;
            for (int i = 0; i < 4; ++i)
            {
                tmp = (byte)((tmp / 2) + (tmp & 0x80));
            }
            return tmp;
        }
        public bool getStatus()
        {
            return st;
        }
        public int getNum()
        {
            return (int)number;
        }
        public byte[] getPicture(int num, out int width, out int height)
        {
            byte[] pic = null;
            width = 0;
            height = 0;
            try
            {
                using (FileStream fp = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
                    pbrData d;
                    int sz = (int)(offset[num + 1] - offset[num]);
                    byte[] buf = new byte[sz];
                    fp.Seek(pbrOffset + offset[num], SeekOrigin.Begin);
                    fp.Read(buf, 0, sz);
                    fp.Close();
                    d.posX = BitConverter.ToUInt16(buf, 0);
                    d.posY = BitConverter.ToUInt16(buf, 2);
                    d.width = BitConverter.ToUInt16(buf, 4);
                    d.height = BitConverter.ToUInt16(buf, 6);
                    width = (int)d.width;
                    height = (int)d.height;
                    pic = new byte[d.width * d.height * 2];
                    for (int i = 0; i < d.height; ++i)
                    {
                        for (int j = 0; j < d.width; ++j)
                        {
                            pic[(i * d.width + j) * 2] = 0;
                            pic[(i * d.width + j) * 2 + 1] = 0;
                        }
                    }
                    int k = 8;
                    for (int i = 0; i < d.height; ++i)
                    {
                        UInt16 j = BitConverter.ToUInt16(buf, k);
                        UInt16 n = BitConverter.ToUInt16(buf, k + 2);
                        k = k + 4;
                        while (n > 0)
                        {
                            byte t = buf[k];
                            ++k;
                            --n;
                            if (t >= 0x80)
                            {
                                t = (byte)(t ^ 0xFF);
                                t = (byte)(t + 1);
                                if (t < 0x40)
                                {
                                    while (t > 0)
                                    {
                                        pic[(i * d.width + j) * 2] = buf[k];
                                        if (buf[k] == 0)
                                        {
                                            pic[(i * d.width + j) * 2 + 1] = 0;
                                        }
                                        else
                                        {
                                            pic[(i * d.width + j) * 2 + 1] = 255;
                                        }
                                        ++j;
                                        ++k;
                                        --n;
                                        --t;
                                    }
                                }
                                else
                                {
                                    t = (byte)(t - 0x40);
                                    pic[(i * d.width + j) * 2] = buf[k];
                                    if (buf[k] == 0)
                                    {
                                        pic[(i * d.width + j) * 2 + 1] = 0;
                                    }
                                    else
                                    {
                                        pic[(i * d.width + j) * 2 + 1] = 255;
                                    }
                                    ++j;
                                    ++k;
                                    --n;
                                    while (t > 0)
                                    {
                                        byte t2 = buf[k];
                                        ++k;
                                        --n;
                                        byte t3 = mysar4(t2);
                                        pic[(i * d.width + j) * 2] = (byte)(pic[(i * d.width + j - 1) * 2] + t3);
                                        if (pic[(i * d.width + j) * 2] == 0)
                                        {
                                            pic[(i * d.width + j) * 2 + 1] = 0;
                                        }
                                        else
                                        {
                                            pic[(i * d.width + j) * 2 + 1] = 255;
                                        }
                                        ++j;
                                        t2 = mysar4((byte)(t2 * 16));
                                        pic[(i * d.width + j) * 2] = (byte)(pic[(i * d.width + j - 1) * 2] + t2);
                                        if (pic[(i * d.width + j) * 2] == 0)
                                        {
                                            pic[(i * d.width + j) * 2 + 1] = 0;
                                        }
                                        else
                                        {
                                            pic[(i * d.width + j) * 2 + 1] = 255;
                                        }
                                        ++j;
                                        --t;
                                    }
                                }
                            }
                            else
                            {
                                while (t > 0)
                                {
                                    pic[(i * d.width + j) * 2] = buf[k];
                                    if (buf[k] == 0)
                                    {
                                        pic[(i * d.width + j) * 2 + 1] = 0;
                                    }
                                    else
                                    {
                                        pic[(i * d.width + j) * 2 + 1] = 255;
                                    }
                                    ++j;
                                    --t;
                                }
                                ++k;
                                --n;
                            }
                        }
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show(fn + " is not opened.");
                st = false;
            }
            return pic;
        }
    }
    public class brsFile
    {
        private struct brsData
        {
            public UInt32 width;
            public UInt32 height;
            public UInt32 posX;
            public UInt32 posY;
        }
        private bool st;
        private string fn;
        private long number;
        private UInt32[] offset;
        private UInt32 brsOffset;
        private UInt32 brsSize;
        public brsFile(string fileName, UInt32 myOffset, UInt32 mySize)
        {
            st = true;
            fn = fileName;
            brsOffset = myOffset;
            brsSize = mySize;
            try
            {
                using (FileStream fp = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
                    if (brsSize == 0)
                    {
                        brsSize = (UInt32)fp.Length;
                    }
                    byte[] buf = new byte[16];
                    long i = 0;
                    number = 0;
                    while (i < brsSize)
                    {
                        number = number + 1;
                        fp.Seek(brsOffset + i, SeekOrigin.Begin);
                        fp.Read(buf, 0, 16);
                        UInt32 w = BitConverter.ToUInt32(buf, 0);
                        UInt32 h = BitConverter.ToUInt32(buf, 4);
                        i = i + w * h + 16;
                    }
                    i = 0;
                    offset = new UInt32[number + 1];
                    long newnum = 0;
                    while (i < brsSize)
                    {
                        offset[newnum] = (UInt32)i;
                        newnum = newnum + 1;
                        fp.Seek(brsOffset + i, SeekOrigin.Begin);
                        fp.Read(buf, 0, 16);
                        UInt32 w = BitConverter.ToUInt32(buf, 0);
                        UInt32 h = BitConverter.ToUInt32(buf, 4);
                        i = i + w * h + 16;
                    }
                    offset[number] = (UInt32)brsSize;
                    fp.Close();
                }
            }
            catch (IOException)
            {
                MessageBox.Show(fn + " is not opened.");
                st = false;
            }
        }
        public bool getStatus()
        {
            return st;
        }
        public int getNum()
        {
            return (int)number;
        }
        public byte[] getPicture(int num, out int width, out int height)
        {
            byte[] pic = null;
            width = 0;
            height = 0;
            try
            {
                using (FileStream fp = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {
                    brsData d;
                    int sz = (int)(offset[num + 1] - offset[num]);
                    byte[] buf = new byte[sz];
                    fp.Seek(brsOffset + offset[num], SeekOrigin.Begin);
                    fp.Read(buf, 0, sz);
                    fp.Close();
                    d.width = BitConverter.ToUInt32(buf, 0);
                    d.height = BitConverter.ToUInt32(buf, 4);
                    d.posX = BitConverter.ToUInt32(buf, 8);
                    d.posY = BitConverter.ToUInt32(buf, 12);
                    width = (int)d.width;
                    height = (int)d.height;
                    pic = new byte[d.width * d.height * 2];
                    for (int i = 0; i < d.height; ++i)
                    {
                        for (int j = 0; j < d.width; ++j)
                        {
                            pic[(i * d.width + j) * 2] = 0;
                            pic[(i * d.width + j) * 2 + 1] = 0;
                        }
                    }
                    int k = 16;
                    for (int i = 0; i < d.height; ++i)
                    {
                        for (int j = 0; j < d.width; ++j)
                        {
                            pic[(i * d.width + j) * 2] = buf[k];
                            if (buf[k] == 0)
                            {
                                pic[(i * d.width + j) * 2 + 1] = 0;
                            }
                            else
                            {
                                pic[(i * d.width + j) * 2 + 1] = 255;
                            }
                            ++k;
                        }
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show(fn + " is not opened.");
                st = false;
            }
            return pic;
        }
    }
}
