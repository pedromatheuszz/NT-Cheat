namespace NT_Cheat
{
    partial class Form1
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.guna2BorderlessForm1 = new Guna.UI2.WinForms.Guna2BorderlessForm(this.components);
            this.lczxy7AnimatedButton11 = new LczxyCustom.Lczxy7AnimatedButton1();
            this.Username = new LczxyCustom.Lczxy7SmoothTextBox();
            this.Pass = new LczxyCustom.Lczxy7SmoothTextBox();
            this.guna2CirclePictureBox1 = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.status = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.guna2CirclePictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // guna2BorderlessForm1
            // 
            this.guna2BorderlessForm1.BorderRadius = 12;
            this.guna2BorderlessForm1.ContainerControl = this;
            this.guna2BorderlessForm1.DockIndicatorTransparencyValue = 0.6D;
            this.guna2BorderlessForm1.HasFormShadow = false;
            this.guna2BorderlessForm1.ResizeForm = false;
            this.guna2BorderlessForm1.TransparentWhileDrag = true;
            // 
            // lczxy7AnimatedButton11
            // 
            this.lczxy7AnimatedButton11.AnimationFillDirection = LczxyCustom.Lczxy7AnimatedButton1.FillDirection.BottomToTop;
            this.lczxy7AnimatedButton11.AnimationFillStyle = LczxyCustom.Lczxy7AnimatedButton1.FillStyle.Solid;
            this.lczxy7AnimatedButton11.AnimationSpeed = 1.5F;
            this.lczxy7AnimatedButton11.BackColor = System.Drawing.Color.Transparent;
            this.lczxy7AnimatedButton11.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(26)))));
            this.lczxy7AnimatedButton11.CornerRadius = 6;
            this.lczxy7AnimatedButton11.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lczxy7AnimatedButton11.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lczxy7AnimatedButton11.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))));
            this.lczxy7AnimatedButton11.InsideColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(16)))), ((int)(((byte)(18)))));
            this.lczxy7AnimatedButton11.Location = new System.Drawing.Point(127, 214);
            this.lczxy7AnimatedButton11.Name = "lczxy7AnimatedButton11";
            this.lczxy7AnimatedButton11.ShowToolTip = false;
            this.lczxy7AnimatedButton11.Size = new System.Drawing.Size(253, 40);
            this.lczxy7AnimatedButton11.TabIndex = 0;
            this.lczxy7AnimatedButton11.Text = "Login";
            this.lczxy7AnimatedButton11.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lczxy7AnimatedButton11.TextHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.lczxy7AnimatedButton11.ToolTipIcon = "";
            this.lczxy7AnimatedButton11.ToolTipMessage = "";
            this.lczxy7AnimatedButton11.Click += new System.EventHandler(this.lczxy7AnimatedButton11_Click_1);
            // 
            // Username
            // 
            this.Username.BackColor = System.Drawing.Color.Transparent;
            this.Username.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(16)))), ((int)(((byte)(18)))));
            this.Username.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(26)))));
            this.Username.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Username.CursorColor = System.Drawing.Color.White;
            this.Username.FocusedBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(85)))));
            this.Username.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(45)))));
            this.Username.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Username.ForeColor = System.Drawing.Color.White;
            this.Username.Location = new System.Drawing.Point(127, 138);
            this.Username.Name = "Username";
            this.Username.Padding = new System.Windows.Forms.Padding(8);
            this.Username.Password = false;
            this.Username.PlaceholderColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.Username.PlaceholderText = "Your Username...";
            this.Username.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(107)))), ((int)(((byte)(105)))));
            this.Username.SelectionLength = 0;
            this.Username.SelectionStart = 0;
            this.Username.Size = new System.Drawing.Size(253, 32);
            this.Username.TabIndex = 1;
            this.Username.TextColor = System.Drawing.Color.White;
            // 
            // Pass
            // 
            this.Pass.BackColor = System.Drawing.Color.Transparent;
            this.Pass.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(16)))), ((int)(((byte)(18)))));
            this.Pass.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(26)))));
            this.Pass.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Pass.CursorColor = System.Drawing.Color.White;
            this.Pass.FocusedBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(85)))));
            this.Pass.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(45)))));
            this.Pass.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Pass.ForeColor = System.Drawing.Color.White;
            this.Pass.Location = new System.Drawing.Point(127, 176);
            this.Pass.Name = "Pass";
            this.Pass.Padding = new System.Windows.Forms.Padding(8);
            this.Pass.Password = true;
            this.Pass.PlaceholderColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.Pass.PlaceholderText = "Your Pass...";
            this.Pass.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(107)))), ((int)(((byte)(105)))));
            this.Pass.SelectionLength = 0;
            this.Pass.SelectionStart = 0;
            this.Pass.Size = new System.Drawing.Size(253, 32);
            this.Pass.TabIndex = 2;
            this.Pass.TextColor = System.Drawing.Color.White;
            // 
            // guna2CirclePictureBox1
            // 
            this.guna2CirclePictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("guna2CirclePictureBox1.Image")));
            this.guna2CirclePictureBox1.ImageRotate = 0F;
            this.guna2CirclePictureBox1.Location = new System.Drawing.Point(206, 21);
            this.guna2CirclePictureBox1.Name = "guna2CirclePictureBox1";
            this.guna2CirclePictureBox1.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.guna2CirclePictureBox1.Size = new System.Drawing.Size(89, 98);
            this.guna2CirclePictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.guna2CirclePictureBox1.TabIndex = 3;
            this.guna2CirclePictureBox1.TabStop = false;
            // 
            // status
            // 
            this.status.AutoSize = true;
            this.status.BackColor = System.Drawing.Color.Transparent;
            this.status.ForeColor = System.Drawing.Color.Gray;
            this.status.Location = new System.Drawing.Point(238, 261);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(0, 13);
            this.status.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(12)))));
            this.ClientSize = new System.Drawing.Size(525, 298);
            this.Controls.Add(this.status);
            this.Controls.Add(this.guna2CirclePictureBox1);
            this.Controls.Add(this.Pass);
            this.Controls.Add(this.Username);
            this.Controls.Add(this.lczxy7AnimatedButton11);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.guna2CirclePictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Guna.UI2.WinForms.Guna2BorderlessForm guna2BorderlessForm1;
        private LczxyCustom.Lczxy7AnimatedButton1 lczxy7AnimatedButton11;
        private LczxyCustom.Lczxy7SmoothTextBox Pass;
        private LczxyCustom.Lczxy7SmoothTextBox Username;
        private Guna.UI2.WinForms.Guna2CirclePictureBox guna2CirclePictureBox1;
        private System.Windows.Forms.Label status;
    }
}

