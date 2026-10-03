namespace Checkers
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            btnlogin = new Button();
            btnsignup = new Button();
            txtusername = new TextBox();
            txtpassword = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            SuspendLayout();
            // 
            // btnlogin
            // 
            btnlogin.BackColor = Color.Transparent;
            btnlogin.Font = new Font("Modern No. 20", 8.999999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnlogin.ForeColor = Color.Black;
            btnlogin.Location = new Point(314, 264);
            btnlogin.Name = "btnlogin";
            btnlogin.Size = new Size(115, 37);
            btnlogin.TabIndex = 4;
            btnlogin.Text = "LOG IN";
            btnlogin.UseVisualStyleBackColor = false;
            btnlogin.Click += button1_Click;
            // 
            // btnsignup
            // 
            btnsignup.Font = new Font("Modern No. 20", 8.999999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnsignup.Location = new Point(314, 322);
            btnsignup.Name = "btnsignup";
            btnsignup.Size = new Size(115, 37);
            btnsignup.TabIndex = 5;
            btnsignup.Text = "SIGN UP";
            btnsignup.UseVisualStyleBackColor = true;
            btnsignup.Click += btnsignup_Click;
            // 
            // txtusername
            // 
            txtusername.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtusername.Location = new Point(265, 154);
            txtusername.Name = "txtusername";
            txtusername.Size = new Size(223, 29);
            txtusername.TabIndex = 6;
            txtusername.TextChanged += txtusername_TextChanged;
            // 
            // txtpassword
            // 
            txtpassword.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtpassword.Location = new Point(265, 216);
            txtpassword.Name = "txtpassword";
            txtpassword.Size = new Size(223, 29);
            txtpassword.TabIndex = 7;
            txtpassword.TextChanged += txtpassword_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = SystemColors.ActiveBorder;
            label1.Font = new Font("Modern No. 20", 8.999999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = SystemColors.ActiveCaptionText;
            label1.Location = new Point(265, 136);
            label1.Name = "label1";
            label1.Size = new Size(112, 15);
            label1.TabIndex = 8;
            label1.Text = "Enter Username : ";
            label1.Click += label1_Click_1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Modern No. 20", 8.999999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(265, 198);
            label2.Name = "label2";
            label2.Size = new Size(110, 15);
            label2.TabIndex = 9;
            label2.Text = "Enter Password : ";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = SystemColors.ActiveBorder;
            label3.Font = new Font("Modern No. 20", 8.999999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.ForeColor = SystemColors.ActiveCaptionText;
            label3.Location = new Point(362, 304);
            label3.Name = "label3";
            label3.Size = new Size(25, 15);
            label3.TabIndex = 10;
            label3.Text = "OR";
            label3.Click += label3_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveBorder;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(800, 450);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtpassword);
            Controls.Add(txtusername);
            Controls.Add(btnsignup);
            Controls.Add(btnlogin);
            DoubleBuffered = true;
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnlogin;
        private Button btnsignup;
        private TextBox txtusername;
        private TextBox txtpassword;
        private Label label1;
        private Label label2;
        private Label label3;
    }
}
