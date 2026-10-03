namespace Checkers
{
    partial class Form3
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form3));
            btnplay2players = new Button();
            btnvscomputer = new Button();
            btnleaderboards = new Button();
            btnsettings = new Button();
            SuspendLayout();
            // 
            // btnplay2players
            // 
            btnplay2players.Location = new Point(303, 154);
            btnplay2players.Name = "btnplay2players";
            btnplay2players.Size = new Size(153, 42);
            btnplay2players.TabIndex = 0;
            btnplay2players.Text = "2 Players";
            btnplay2players.UseVisualStyleBackColor = true;
            btnplay2players.Click += button1_Click;
            // 
            // btnvscomputer
            // 
            btnvscomputer.Location = new Point(303, 219);
            btnvscomputer.Name = "btnvscomputer";
            btnvscomputer.Size = new Size(153, 42);
            btnvscomputer.TabIndex = 1;
            btnvscomputer.Text = "VS Computer";
            btnvscomputer.UseVisualStyleBackColor = true;
            btnvscomputer.Click += btnvscomputer_Click;
            // 
            // btnleaderboards
            // 
            btnleaderboards.Location = new Point(317, 355);
            btnleaderboards.Name = "btnleaderboards";
            btnleaderboards.Size = new Size(86, 23);
            btnleaderboards.TabIndex = 2;
            btnleaderboards.Text = "Leaderboards";
            btnleaderboards.UseVisualStyleBackColor = true;
            btnleaderboards.Click += btnleaderboards_Click;
            // 
            // btnsettings
            // 
            btnsettings.Location = new Point(317, 396);
            btnsettings.Name = "btnsettings";
            btnsettings.Size = new Size(86, 23);
            btnsettings.TabIndex = 3;
            btnsettings.Text = "Settings";
            btnsettings.UseVisualStyleBackColor = true;
            btnsettings.Click += btnsettings_Click;
            // 
            // Form3
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(800, 450);
            Controls.Add(btnsettings);
            Controls.Add(btnleaderboards);
            Controls.Add(btnvscomputer);
            Controls.Add(btnplay2players);
            Name = "Form3";
            Text = "Form3";
            ResumeLayout(false);
        }

        #endregion

        private Button btnplay2players;
        private Button btnvscomputer;
        private Button btnleaderboards;
        private Button btnsettings;
    }
}