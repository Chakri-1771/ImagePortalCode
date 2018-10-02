namespace ImageResizer
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblSelectImage = new System.Windows.Forms.Label();
            this.lblSaveImage = new System.Windows.Forms.Label();
            this.txtSelectImage = new System.Windows.Forms.TextBox();
            this.txtSaveImage = new System.Windows.Forms.TextBox();
            this.btnSelectImage = new System.Windows.Forms.Button();
            this.btnSaveImage = new System.Windows.Forms.Button();
            this.lblWidth = new System.Windows.Forms.Label();
            this.lblHeight = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.btnResize = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblSelectImage
            // 
            this.lblSelectImage.AutoSize = true;
            this.lblSelectImage.Location = new System.Drawing.Point(12, 28);
            this.lblSelectImage.Name = "lblSelectImage";
            this.lblSelectImage.Size = new System.Drawing.Size(69, 13);
            this.lblSelectImage.TabIndex = 0;
            this.lblSelectImage.Text = "Select Image";
            // 
            // lblSaveImage
            // 
            this.lblSaveImage.AutoSize = true;
            this.lblSaveImage.Location = new System.Drawing.Point(12, 72);
            this.lblSaveImage.Name = "lblSaveImage";
            this.lblSaveImage.Size = new System.Drawing.Size(64, 13);
            this.lblSaveImage.TabIndex = 1;
            this.lblSaveImage.Text = "Save Image";
            // 
            // txtSelectImage
            // 
            this.txtSelectImage.Location = new System.Drawing.Point(108, 25);
            this.txtSelectImage.Name = "txtSelectImage";
            this.txtSelectImage.Size = new System.Drawing.Size(525, 20);
            this.txtSelectImage.TabIndex = 2;
            // 
            // txtSaveImage
            // 
            this.txtSaveImage.Location = new System.Drawing.Point(108, 69);
            this.txtSaveImage.Name = "txtSaveImage";
            this.txtSaveImage.Size = new System.Drawing.Size(525, 20);
            this.txtSaveImage.TabIndex = 3;
            // 
            // btnSelectImage
            // 
            this.btnSelectImage.Location = new System.Drawing.Point(670, 23);
            this.btnSelectImage.Name = "btnSelectImage";
            this.btnSelectImage.Size = new System.Drawing.Size(75, 23);
            this.btnSelectImage.TabIndex = 4;
            this.btnSelectImage.Text = "...";
            this.btnSelectImage.UseVisualStyleBackColor = true;
            this.btnSelectImage.Click += new System.EventHandler(this.btnSelectImage_Click);
            // 
            // btnSaveImage
            // 
            this.btnSaveImage.Location = new System.Drawing.Point(670, 69);
            this.btnSaveImage.Name = "btnSaveImage";
            this.btnSaveImage.Size = new System.Drawing.Size(75, 23);
            this.btnSaveImage.TabIndex = 5;
            this.btnSaveImage.Text = "...";
            this.btnSaveImage.UseVisualStyleBackColor = true;
            this.btnSaveImage.Click += new System.EventHandler(this.btnSaveImage_Click);
            // 
            // lblWidth
            // 
            this.lblWidth.AutoSize = true;
            this.lblWidth.Location = new System.Drawing.Point(57, 147);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(35, 13);
            this.lblWidth.TabIndex = 6;
            this.lblWidth.Text = "Width";
            // 
            // lblHeight
            // 
            this.lblHeight.AutoSize = true;
            this.lblHeight.Location = new System.Drawing.Point(271, 143);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(38, 13);
            this.lblHeight.TabIndex = 7;
            this.lblHeight.Text = "Height";
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(483, 143);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(31, 13);
            this.lblType.TabIndex = 8;
            this.lblType.Text = "Type";
            // 
            // txtWidth
            // 
            this.txtWidth.Location = new System.Drawing.Point(139, 140);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(100, 20);
            this.txtWidth.TabIndex = 9;
            // 
            // txtHeight
            // 
            this.txtHeight.Location = new System.Drawing.Point(342, 140);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(100, 20);
            this.txtHeight.TabIndex = 11;
            // 
            // cmbType
            // 
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Location = new System.Drawing.Point(533, 139);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(121, 21);
            this.cmbType.TabIndex = 12;
            // 
            // btnResize
            // 
            this.btnResize.Location = new System.Drawing.Point(274, 238);
            this.btnResize.Name = "btnResize";
            this.btnResize.Size = new System.Drawing.Size(75, 23);
            this.btnResize.TabIndex = 13;
            this.btnResize.Text = "Resize";
            this.btnResize.UseVisualStyleBackColor = true;
            this.btnResize.Click += new System.EventHandler(this.btnResize_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(468, 238);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 14;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 352);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnResize);
            this.Controls.Add(this.cmbType);
            this.Controls.Add(this.txtHeight);
            this.Controls.Add(this.txtWidth);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.lblHeight);
            this.Controls.Add(this.lblWidth);
            this.Controls.Add(this.btnSaveImage);
            this.Controls.Add(this.btnSelectImage);
            this.Controls.Add(this.txtSaveImage);
            this.Controls.Add(this.txtSelectImage);
            this.Controls.Add(this.lblSaveImage);
            this.Controls.Add(this.lblSelectImage);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSelectImage;
        private System.Windows.Forms.Label lblSaveImage;
        private System.Windows.Forms.TextBox txtSelectImage;
        private System.Windows.Forms.TextBox txtSaveImage;
        private System.Windows.Forms.Button btnSelectImage;
        private System.Windows.Forms.Button btnSaveImage;
        private System.Windows.Forms.Label lblWidth;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.Button btnResize;
        private System.Windows.Forms.Button btnSave;
    }
}

