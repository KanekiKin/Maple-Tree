namespace MapleSeed
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.status = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.storeEncCont = new System.Windows.Forms.CheckBox();
            this.updateBtn = new System.Windows.Forms.Button();
            this.fullScreen = new System.Windows.Forms.CheckBox();
            this.userList = new System.Windows.Forms.ListBox();
            this.chatInput = new System.Windows.Forms.TextBox();
            this.username = new System.Windows.Forms.TextBox();
            this.playBtn = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.chatbox = new System.Windows.Forms.RichTextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.discordPass = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.discordEmail = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkUpdateBtn = new System.Windows.Forms.Button();
            this.serverHub = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cemuDir = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.titleDir = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cemu173Patch = new System.Windows.Forms.CheckBox();
            this.sendChat = new System.Windows.Forms.Button();
            this.progressOverlay = new System.Windows.Forms.Label();
            this.discordConnect = new System.Windows.Forms.Button();
            this.dlcBtn = new System.Windows.Forms.Button();
            this.cleanTitleBtn = new System.Windows.Forms.Button();
            this.titleVersion = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.titleList = new System.Windows.Forms.CheckedListBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.titleIdTextBox = new System.Windows.Forms.TextBox();
            this.newdlbtn = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 638);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1125, 23);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 0;
            // 
            // status
            // 
            this.status.AutoSize = true;
            this.status.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.status.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.status.Location = new System.Drawing.Point(9, 664);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(112, 13);
            this.status.TabIndex = 2;
            this.status.Text = "GitHub.com/Tsumes";
            this.status.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox2
            // 
            this.groupBox2.Location = new System.Drawing.Point(12, 622);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1125, 10);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.SystemColors.HighlightText;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.DetectUrls = false;
            this.richTextBox1.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.richTextBox1.Location = new System.Drawing.Point(370, 6);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Horizontal;
            this.richTextBox1.Size = new System.Drawing.Size(504, 480);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // storeEncCont
            // 
            this.storeEncCont.AutoSize = true;
            this.storeEncCont.Location = new System.Drawing.Point(133, 592);
            this.storeEncCont.Name = "storeEncCont";
            this.storeEncCont.Size = new System.Drawing.Size(152, 17);
            this.storeEncCont.TabIndex = 11;
            this.storeEncCont.Text = "Store Encrypted Content";
            this.storeEncCont.UseVisualStyleBackColor = true;
            this.storeEncCont.CheckedChanged += new System.EventHandler(this.storeEncCont_CheckedChanged);
            // 
            // updateBtn
            // 
            this.updateBtn.Location = new System.Drawing.Point(253, 7);
            this.updateBtn.Name = "updateBtn";
            this.updateBtn.Size = new System.Drawing.Size(74, 23);
            this.updateBtn.TabIndex = 12;
            this.updateBtn.Text = "Update";
            this.updateBtn.UseVisualStyleBackColor = true;
            this.updateBtn.Click += new System.EventHandler(this.updateBtn_Click);
            // 
            // fullScreen
            // 
            this.fullScreen.AutoSize = true;
            this.fullScreen.Location = new System.Drawing.Point(12, 592);
            this.fullScreen.Name = "fullScreen";
            this.fullScreen.Size = new System.Drawing.Size(115, 17);
            this.fullScreen.TabIndex = 13;
            this.fullScreen.Text = "Full Screen Mode";
            this.fullScreen.UseVisualStyleBackColor = true;
            this.fullScreen.CheckedChanged += new System.EventHandler(this.fullScreen_CheckedChanged);
            // 
            // userList
            // 
            this.userList.FormattingEnabled = true;
            this.userList.Location = new System.Drawing.Point(1120, 34);
            this.userList.Name = "userList";
            this.userList.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.userList.Size = new System.Drawing.Size(109, 433);
            this.userList.Sorted = true;
            this.userList.TabIndex = 14;
            // 
            // chatInput
            // 
            this.chatInput.Location = new System.Drawing.Point(12, 564);
            this.chatInput.Name = "chatInput";
            this.chatInput.Size = new System.Drawing.Size(1121, 22);
            this.chatInput.TabIndex = 0;
            // 
            // username
            // 
            this.username.Location = new System.Drawing.Point(1120, 6);
            this.username.MaxLength = 12;
            this.username.Name = "username";
            this.username.ReadOnly = true;
            this.username.Size = new System.Drawing.Size(109, 22);
            this.username.TabIndex = 15;
            this.username.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.username.TextChanged += new System.EventHandler(this.username_TextChanged);
            // 
            // playBtn
            // 
            this.playBtn.Location = new System.Drawing.Point(12, 7);
            this.playBtn.Name = "playBtn";
            this.playBtn.Size = new System.Drawing.Size(75, 23);
            this.playBtn.TabIndex = 17;
            this.playBtn.Text = "Play!";
            this.playBtn.UseVisualStyleBackColor = true;
            this.playBtn.Click += new System.EventHandler(this.playBtn_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(12, 36);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1240, 522);
            this.tabControl1.TabIndex = 18;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.pictureBox1);
            this.tabPage1.Controls.Add(this.titleList);
            this.tabPage1.Controls.Add(this.richTextBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1232, 496);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Dashboard";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.chatbox);
            this.tabPage2.Controls.Add(this.username);
            this.tabPage2.Controls.Add(this.userList);
            this.tabPage2.Controls.Add(this.discordConnect);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(1232, 496);
            this.tabPage2.TabIndex = 2;
            this.tabPage2.Text = "Community";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // chatbox
            // 
            this.chatbox.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.chatbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chatbox.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.chatbox.Location = new System.Drawing.Point(3, 3);
            this.chatbox.Name = "chatbox";
            this.chatbox.ReadOnly = true;
            this.chatbox.Size = new System.Drawing.Size(1111, 490);
            this.chatbox.TabIndex = 0;
            this.chatbox.Text = "";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.discordPass);
            this.tabPage3.Controls.Add(this.label6);
            this.tabPage3.Controls.Add(this.discordEmail);
            this.tabPage3.Controls.Add(this.label5);
            this.tabPage3.Controls.Add(this.checkUpdateBtn);
            this.tabPage3.Controls.Add(this.serverHub);
            this.tabPage3.Controls.Add(this.label4);
            this.tabPage3.Controls.Add(this.cemuDir);
            this.tabPage3.Controls.Add(this.label3);
            this.tabPage3.Controls.Add(this.titleDir);
            this.tabPage3.Controls.Add(this.label2);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1232, 496);
            this.tabPage3.TabIndex = 3;
            this.tabPage3.Text = "Settings";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // discordPass
            // 
            this.discordPass.Location = new System.Drawing.Point(435, 102);
            this.discordPass.Name = "discordPass";
            this.discordPass.PasswordChar = '*';
            this.discordPass.Size = new System.Drawing.Size(174, 22);
            this.discordPass.TabIndex = 11;
            this.discordPass.UseSystemPasswordChar = true;
            this.discordPass.TextChanged += new System.EventHandler(this.discordPass_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(432, 85);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(98, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Discord Password";
            // 
            // discordEmail
            // 
            this.discordEmail.Location = new System.Drawing.Point(435, 43);
            this.discordEmail.Name = "discordEmail";
            this.discordEmail.Size = new System.Drawing.Size(174, 22);
            this.discordEmail.TabIndex = 9;
            this.discordEmail.TextChanged += new System.EventHandler(this.discordEmail_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(432, 26);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Discord E-Mail";
            // 
            // checkUpdateBtn
            // 
            this.checkUpdateBtn.Location = new System.Drawing.Point(9, 467);
            this.checkUpdateBtn.Name = "checkUpdateBtn";
            this.checkUpdateBtn.Size = new System.Drawing.Size(75, 23);
            this.checkUpdateBtn.TabIndex = 6;
            this.checkUpdateBtn.Text = "Update";
            this.checkUpdateBtn.UseVisualStyleBackColor = true;
            this.checkUpdateBtn.Click += new System.EventHandler(this.checkUpdateBtn_Click);
            // 
            // serverHub
            // 
            this.serverHub.Location = new System.Drawing.Point(10, 158);
            this.serverHub.Name = "serverHub";
            this.serverHub.Size = new System.Drawing.Size(174, 22);
            this.serverHub.TabIndex = 5;
            this.serverHub.TextChanged += new System.EventHandler(this.serverHub_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 141);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Server Hub";
            // 
            // cemuDir
            // 
            this.cemuDir.Location = new System.Drawing.Point(10, 102);
            this.cemuDir.Name = "cemuDir";
            this.cemuDir.Size = new System.Drawing.Size(174, 22);
            this.cemuDir.TabIndex = 3;
            this.cemuDir.TextChanged += new System.EventHandler(this.cemuDir_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Cemu Directory";
            // 
            // titleDir
            // 
            this.titleDir.Location = new System.Drawing.Point(10, 45);
            this.titleDir.Name = "titleDir";
            this.titleDir.Size = new System.Drawing.Size(174, 22);
            this.titleDir.TabIndex = 1;
            this.titleDir.TextChanged += new System.EventHandler(this.titleDir_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Title Directory";
            // 
            // cemu173Patch
            // 
            this.cemu173Patch.AutoSize = true;
            this.cemu173Patch.Checked = true;
            this.cemu173Patch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cemu173Patch.Location = new System.Drawing.Point(291, 592);
            this.cemu173Patch.Name = "cemu173Patch";
            this.cemu173Patch.Size = new System.Drawing.Size(158, 17);
            this.cemu173Patch.TabIndex = 7;
            this.cemu173Patch.Text = "Cemu 1.7.3 Patch Support";
            this.cemu173Patch.UseVisualStyleBackColor = true;
            this.cemu173Patch.CheckedChanged += new System.EventHandler(this.cemu173Patch_CheckedChanged);
            // 
            // sendChat
            // 
            this.sendChat.Location = new System.Drawing.Point(1143, 564);
            this.sendChat.Name = "sendChat";
            this.sendChat.Size = new System.Drawing.Size(109, 23);
            this.sendChat.TabIndex = 21;
            this.sendChat.Text = "Send";
            this.sendChat.UseVisualStyleBackColor = true;
            this.sendChat.Click += new System.EventHandler(this.sendChat_Click);
            // 
            // progressOverlay
            // 
            this.progressOverlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.progressOverlay.AutoSize = true;
            this.progressOverlay.BackColor = System.Drawing.Color.Transparent;
            this.progressOverlay.Location = new System.Drawing.Point(12, 621);
            this.progressOverlay.Name = "progressOverlay";
            this.progressOverlay.Size = new System.Drawing.Size(89, 13);
            this.progressOverlay.TabIndex = 23;
            this.progressOverlay.Text = "0 bytes / 0 bytes";
            // 
            // discordConnect
            // 
            this.discordConnect.Location = new System.Drawing.Point(1120, 470);
            this.discordConnect.Name = "discordConnect";
            this.discordConnect.Size = new System.Drawing.Size(109, 23);
            this.discordConnect.TabIndex = 24;
            this.discordConnect.Text = "Connect To Chat";
            this.discordConnect.UseVisualStyleBackColor = true;
            this.discordConnect.Click += new System.EventHandler(this.discordConnect_Click);
            // 
            // dlcBtn
            // 
            this.dlcBtn.Location = new System.Drawing.Point(173, 7);
            this.dlcBtn.Name = "dlcBtn";
            this.dlcBtn.Size = new System.Drawing.Size(74, 23);
            this.dlcBtn.TabIndex = 25;
            this.dlcBtn.Text = "DLC";
            this.dlcBtn.UseVisualStyleBackColor = true;
            this.dlcBtn.Click += new System.EventHandler(this.dlcBtn_Click);
            // 
            // cleanTitleBtn
            // 
            this.cleanTitleBtn.Location = new System.Drawing.Point(93, 7);
            this.cleanTitleBtn.Name = "cleanTitleBtn";
            this.cleanTitleBtn.Size = new System.Drawing.Size(74, 23);
            this.cleanTitleBtn.TabIndex = 26;
            this.cleanTitleBtn.Text = "Clean Title";
            this.cleanTitleBtn.UseVisualStyleBackColor = true;
            this.cleanTitleBtn.Click += new System.EventHandler(this.cleanTitleBtn_Click);
            // 
            // titleVersion
            // 
            this.titleVersion.Location = new System.Drawing.Point(351, 8);
            this.titleVersion.MaxLength = 3;
            this.titleVersion.Name = "titleVersion";
            this.titleVersion.Size = new System.Drawing.Size(39, 22);
            this.titleVersion.TabIndex = 27;
            this.titleVersion.Text = "0";
            this.titleVersion.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(333, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(16, 19);
            this.label1.TabIndex = 28;
            this.label1.Text = "v";
            // 
            // titleList
            // 
            this.titleList.FormattingEnabled = true;
            this.titleList.Location = new System.Drawing.Point(3, 6);
            this.titleList.Name = "titleList";
            this.titleList.Size = new System.Drawing.Size(361, 480);
            this.titleList.TabIndex = 29;
            this.titleList.SelectedValueChanged += new System.EventHandler(this.titleList_SelectedValueChanged);
            this.titleList.DoubleClick += new System.EventHandler(this.titleList_DoubleClick);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(880, 6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(346, 484);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 30;
            this.pictureBox1.TabStop = false;
            // 
            // titleIdTextBox
            // 
            this.titleIdTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.titleIdTextBox.Location = new System.Drawing.Point(1112, 7);
            this.titleIdTextBox.MaxLength = 16;
            this.titleIdTextBox.Name = "titleIdTextBox";
            this.titleIdTextBox.Size = new System.Drawing.Size(130, 22);
            this.titleIdTextBox.TabIndex = 30;
            this.titleIdTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.titleIdTextBox.TextChanged += new System.EventHandler(this.titleIdTextBox_TextChanged);
            // 
            // newdlbtn
            // 
            this.newdlbtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.newdlbtn.Location = new System.Drawing.Point(1032, 7);
            this.newdlbtn.Name = "newdlbtn";
            this.newdlbtn.Size = new System.Drawing.Size(74, 23);
            this.newdlbtn.TabIndex = 31;
            this.newdlbtn.Text = "[+] Library";
            this.newdlbtn.UseVisualStyleBackColor = true;
            this.newdlbtn.Click += new System.EventHandler(this.newdlbtn_Click);
            // 
            // Form1
            // 
            this.AcceptButton = this.sendChat;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 681);
            this.Controls.Add(this.newdlbtn);
            this.Controls.Add(this.titleIdTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.titleVersion);
            this.Controls.Add(this.cleanTitleBtn);
            this.Controls.Add(this.dlcBtn);
            this.Controls.Add(this.progressOverlay);
            this.Controls.Add(this.sendChat);
            this.Controls.Add(this.cemu173Patch);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.playBtn);
            this.Controls.Add(this.chatInput);
            this.Controls.Add(this.fullScreen);
            this.Controls.Add(this.updateBtn);
            this.Controls.Add(this.storeEncCont);
            this.Controls.Add(this.status);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.groupBox2);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Maple Seed";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label status;
        private System.Windows.Forms.GroupBox groupBox2;
        internal System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        internal System.Windows.Forms.CheckBox storeEncCont;
        private System.Windows.Forms.Button updateBtn;
        private System.Windows.Forms.CheckBox fullScreen;
        private System.Windows.Forms.ListBox userList;
        private System.Windows.Forms.TextBox chatInput;
        private System.Windows.Forms.TextBox username;
        private System.Windows.Forms.Button playBtn;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button sendChat;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.RichTextBox chatbox;
        private System.Windows.Forms.Label progressOverlay;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TextBox cemuDir;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox titleDir;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox serverHub;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button checkUpdateBtn;
        private System.Windows.Forms.CheckBox cemu173Patch;
        private System.Windows.Forms.TextBox discordPass;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox discordEmail;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button discordConnect;
        private System.Windows.Forms.Button dlcBtn;
        private System.Windows.Forms.Button cleanTitleBtn;
        private System.Windows.Forms.TextBox titleVersion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox titleList;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox titleIdTextBox;
        private System.Windows.Forms.Button newdlbtn;
    }
}

