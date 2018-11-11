namespace FPLedit
{
    partial class InfoForm
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
            this.websiteLinkLabel = new System.Windows.Forms.LinkLabel();
            this.versionLabel = new System.Windows.Forms.Label();
            this.checkButton = new System.Windows.Forms.Button();
            this.copyrightLabel = new System.Windows.Forms.Label();
            this.licenseGroupBox = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.dokuLabel = new System.Windows.Forms.Label();
            this.updateCheckBox = new System.Windows.Forms.CheckBox();
            this.licenseGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // websiteLinkLabel
            // 
            this.websiteLinkLabel.AutoSize = true;
            this.websiteLinkLabel.Location = new System.Drawing.Point(94, 62);
            this.websiteLinkLabel.Name = "websiteLinkLabel";
            this.websiteLinkLabel.Size = new System.Drawing.Size(145, 13);
            this.websiteLinkLabel.TabIndex = 4;
            this.websiteLinkLabel.TabStop = true;
            this.websiteLinkLabel.Text = "https://fahrplan.manuelhu.de";
            this.websiteLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.websiteLinkLabel_LinkClicked);
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(15, 17);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(87, 13);
            this.versionLabel.TabIndex = 0;
            this.versionLabel.Text = "Version {version}";
            // 
            // checkButton
            // 
            this.checkButton.Location = new System.Drawing.Point(207, 12);
            this.checkButton.Name = "checkButton";
            this.checkButton.Size = new System.Drawing.Size(139, 23);
            this.checkButton.TabIndex = 1;
            this.checkButton.Text = "Auf neue Version prüfen";
            this.checkButton.UseVisualStyleBackColor = true;
            this.checkButton.Click += new System.EventHandler(this.checkButton_Click);
            // 
            // copyrightLabel
            // 
            this.copyrightLabel.AutoSize = true;
            this.copyrightLabel.Location = new System.Drawing.Point(15, 47);
            this.copyrightLabel.Name = "copyrightLabel";
            this.copyrightLabel.Size = new System.Drawing.Size(140, 13);
            this.copyrightLabel.TabIndex = 2;
            this.copyrightLabel.Text = "© 2015-2017 Manuel Huber";
            // 
            // licenseGroupBox
            // 
            this.licenseGroupBox.Controls.Add(this.textBox1);
            this.licenseGroupBox.Location = new System.Drawing.Point(12, 94);
            this.licenseGroupBox.Name = "licenseGroupBox";
            this.licenseGroupBox.Padding = new System.Windows.Forms.Padding(7);
            this.licenseGroupBox.Size = new System.Drawing.Size(334, 179);
            this.licenseGroupBox.TabIndex = 5;
            this.licenseGroupBox.TabStop = false;
            this.licenseGroupBox.Text = "Lizenz / Hinweise";
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(7, 20);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(320, 152);
            this.textBox1.TabIndex = 0;
            // 
            // closeButton
            // 
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(271, 319);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 7;
            this.closeButton.Text = "Schließen";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // dokuLabel
            // 
            this.dokuLabel.AutoSize = true;
            this.dokuLabel.Location = new System.Drawing.Point(15, 62);
            this.dokuLabel.Name = "dokuLabel";
            this.dokuLabel.Size = new System.Drawing.Size(82, 13);
            this.dokuLabel.TabIndex = 3;
            this.dokuLabel.Text = "Dokumentation:";
            // 
            // updateCheckBox
            // 
            this.updateCheckBox.Location = new System.Drawing.Point(12, 279);
            this.updateCheckBox.Name = "updateCheckBox";
            this.updateCheckBox.Size = new System.Drawing.Size(334, 32);
            this.updateCheckBox.TabIndex = 6;
            this.updateCheckBox.Text = "Automatische Überprüfung auf Updates beim Programmstart aktivieren. (Dabei wird I" +
    "hre IP-Adresse an den Server übermittelt)";
            this.updateCheckBox.UseVisualStyleBackColor = true;
            // 
            // InfoForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(358, 354);
            this.Controls.Add(this.updateCheckBox);
            this.Controls.Add(this.dokuLabel);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.licenseGroupBox);
            this.Controls.Add(this.copyrightLabel);
            this.Controls.Add(this.checkButton);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.websiteLinkLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "InfoForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FPLedit Info";
            this.licenseGroupBox.ResumeLayout(false);
            this.licenseGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.LinkLabel websiteLinkLabel;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Button checkButton;
        private System.Windows.Forms.Label copyrightLabel;
        private System.Windows.Forms.GroupBox licenseGroupBox;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Label dokuLabel;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.CheckBox updateCheckBox;
    }
}