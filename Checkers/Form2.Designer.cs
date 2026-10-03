namespace Checkers
{
    partial class Form2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            txtusername = new TextBox();
            txtfullname = new TextBox();
            label1 = new Label();
            label2 = new Label();
            textBox3 = new TextBox();
            label3 = new Label();
            label4 = new Label();
            btnEnterSignUp = new Button();
            btnback = new Button();
            SuspendLayout();
            // 
            // txtusername
            // 
            txtusername.Location = new Point(292, 186);
            txtusername.Name = "txtusername";
            txtusername.Size = new Size(209, 23);
            txtusername.TabIndex = 0;
            txtusername.TextChanged += textBox1_TextChanged;
            // 
            // txtfullname
            // 
            txtfullname.Location = new Point(292, 250);
            txtfullname.Name = "txtfullname";
            txtfullname.Size = new Size(209, 23);
            txtfullname.TabIndex = 1;
            txtfullname.TextChanged += textBox2_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = SystemColors.ActiveBorder;
            label1.Font = new Font("Modern No. 20", 8.999999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = SystemColors.ActiveCaptionText;
            label1.Location = new Point(292, 168);
            label1.Name = "label1";
            label1.Size = new Size(67, 15);
            label1.TabIndex = 9;
            label1.Text = "Username ";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = SystemColors.ActiveBorder;
            label2.Font = new Font("Modern No. 20", 8.999999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = SystemColors.ActiveCaptionText;
            label2.Location = new Point(292, 232);
            label2.Name = "label2";
            label2.Size = new Size(62, 15);
            label2.TabIndex = 10;
            label2.Text = "Fullname";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(292, 313);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(209, 23);
            textBox3.TabIndex = 11;
            textBox3.TextChanged += textBox3_TextChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = SystemColors.ActiveBorder;
            label3.Font = new Font("Modern No. 20", 8.999999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.ForeColor = SystemColors.ActiveCaptionText;
            label3.Location = new Point(292, 295);
            label3.Name = "label3";
            label3.Size = new Size(61, 15);
            label3.TabIndex = 12;
            label3.Text = "Password";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = SystemColors.ActiveBorder;
            label4.Font = new Font("Modern No. 20", 17.9999981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.ForeColor = SystemColors.ActiveCaptionText;
            label4.Location = new Point(346, 122);
            label4.Name = "label4";
            label4.Size = new Size(115, 25);
            label4.TabIndex = 13;
            label4.Text = "SIGN UP";
            // 
            // btnEnterSignUp
            // 
            btnEnterSignUp.Location = new Point(429, 359);
            btnEnterSignUp.Name = "btnEnterSignUp";
            btnEnterSignUp.Size = new Size(72, 25);
            btnEnterSignUp.TabIndex = 14;
            btnEnterSignUp.Text = "Enter";
            btnEnterSignUp.UseVisualStyleBackColor = true;
            btnEnterSignUp.Click += btnEnterSignUp_Click;
            // 
            // btnback
            // 
            btnback.Location = new Point(285, 359);
            btnback.Name = "btnback";
            btnback.Size = new Size(72, 25);
            btnback.TabIndex = 15;
            btnback.Text = "Back";
            btnback.UseVisualStyleBackColor = true;
            btnback.Click += btnback_Click;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(800, 450);
            Controls.Add(btnback);
            Controls.Add(btnEnterSignUp);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(textBox3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtfullname);
            Controls.Add(txtusername);
            Name = "Form2";
            Text = "Form2";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtusername;
        private TextBox txtfullname;
        private Label label1;
        private Label label2;
        private TextBox textBox3;
        private Label label3;
        private Label label4;
        private Button btnEnterSignUp;
        private Button btnback;
    }
}