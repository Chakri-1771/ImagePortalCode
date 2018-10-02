using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageResizer
{
    public partial class Form1 : Form
    {
        Image img;
        string[] extn = { ".PNG", ".JPEG", ".JPG", ".GIF" };
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < extn.Length; i++)
            {
                cmbType.Items.Add(extn[i]);
            }
        }

        private void btnSelectImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog opn = new OpenFileDialog();
            opn.Filter = "images | *.png;*.jpg;*.jpeg;*.gif";
            if (opn.ShowDialog() == DialogResult.OK)
            {
                txtSelectImage.Text = opn.FileName;
                img = Image.FromFile(opn.FileName);
            }
        }

        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtSaveImage.Text = fbd.SelectedPath;
            }
        }

        private void btnResize_Click(object sender, EventArgs e)
        {
            int w = Convert.ToInt32(txtWidth.Text), h = Convert.ToInt32(txtHeight.Text);
            img = Resize(img, w, h);
            ((Button)sender).Enabled = false;
            MessageBox.Show("Image Resizede");

        }

        Image Resize(Image image,int w, int h)
        {

            Bitmap bmp = new Bitmap(w, h);
            Graphics graphic = Graphics.FromImage(bmp);
            graphic.DrawImage(image, 0, 0, w, h);
            graphic.Dispose();

            return bmp;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            int dot = 0, slash = 0;
            for (int j = txtSelectImage.Text.Length - 1; j >= 0; j--)
            {
                if (txtSelectImage.Text[j] == '.')
                {
                    dot = j;
                }
                else if(txtSelectImage.Text[j] == '\\')
                {
                    slash = j;
                    break;
                   
                }
            }

            img.Save(txtSaveImage.Text + "\\" + txtSelectImage.Text.Substring(slash + 1, dot - slash - 1) + extn[cmbType.SelectedIndex]);
            ((Button)sender).Enabled = false;
            MessageBox.Show("Image saved");
        }
    }
}
