namespace FPLedit.jTrainGraphStarter
{
    partial class SettingsForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.javaPathTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.jtgPathTextBox = new System.Windows.Forms.TextBox();
            this.generalDocLabel = new System.Windows.Forms.Label();
            this.closeButton = new System.Windows.Forms.Button();
            this.downloadLinkLabel = new System.Windows.Forms.LinkLabel();
            this.docLinkLabel = new System.Windows.Forms.LinkLabel();
            this.jTGDocLabel = new System.Windows.Forms.Label();
            this.javaDocLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Java-Pfad";
            // 
            // javaPathTextBox
            // 
            this.javaPathTextBox.Location = new System.Drawing.Point(157, 12);
            this.javaPathTextBox.Name = "javaPathTextBox";
            this.javaPathTextBox.Size = new System.Drawing.Size(242, 20);
            this.javaPathTextBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Pfad zur jTrainGraph-Datei";
            // 
            // jtgPathTextBox
            // 
            this.jtgPathTextBox.Location = new System.Drawing.Point(157, 58);
            this.jtgPathTextBox.Name = "jtgPathTextBox";
            this.jtgPathTextBox.Size = new System.Drawing.Size(240, 20);
            this.jtgPathTextBox.TabIndex = 4;
            // 
            // generalDocLabel
            // 
            this.generalDocLabel.Location = new System.Drawing.Point(10, 112);
            this.generalDocLabel.Name = "generalDocLabel";
            this.generalDocLabel.Size = new System.Drawing.Size(387, 50);
            this.generalDocLabel.TabIndex = 6;
            this.generalDocLabel.Text = "Weitere Hinweise:\r\n\r\nWenn Sie jTrainGraph noch gar nicht installiert haben:\r\n\r\n";
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(324, 158);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 9;
            this.closeButton.Text = "Schließen";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // downloadLinkLabel
            // 
            this.downloadLinkLabel.AutoSize = true;
            this.downloadLinkLabel.Location = new System.Drawing.Point(267, 138);
            this.downloadLinkLabel.Name = "downloadLinkLabel";
            this.downloadLinkLabel.Size = new System.Drawing.Size(130, 13);
            this.downloadLinkLabel.TabIndex = 8;
            this.downloadLinkLabel.TabStop = true;
            this.downloadLinkLabel.Text = "jTrainGraph herunterladen";
            this.downloadLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.downloadLinkLabel_LinkClicked);
            // 
            // docLinkLabel
            // 
            this.docLinkLabel.AutoSize = true;
            this.docLinkLabel.Location = new System.Drawing.Point(99, 112);
            this.docLinkLabel.Name = "docLinkLabel";
            this.docLinkLabel.Size = new System.Drawing.Size(161, 13);
            this.docLinkLabel.TabIndex = 7;
            this.docLinkLabel.TabStop = true;
            this.docLinkLabel.Text = "Dokumentation zu diesem Plugin";
            this.docLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.docLinkLabel_LinkClicked);
            // 
            // jTGDocLabel
            // 
            this.jTGDocLabel.Location = new System.Drawing.Point(156, 81);
            this.jTGDocLabel.Name = "jTGDocLabel";
            this.jTGDocLabel.Size = new System.Drawing.Size(243, 31);
            this.jTGDocLabel.TabIndex = 5;
            this.jTGDocLabel.Text = "Anwendungsdatei von jTrainGraph (jTrainGraph_xxx.jar, wobei xxx >= 202)";
            // 
            // javaDocLabel
            // 
            this.javaDocLabel.Location = new System.Drawing.Point(156, 35);
            this.javaDocLabel.Name = "javaDocLabel";
            this.javaDocLabel.Size = new System.Drawing.Size(243, 20);
            this.javaDocLabel.TabIndex = 2;
            this.javaDocLabel.Text = "i.d.R: Windows: javaw.exe, Linux: java";
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(411, 193);
            this.Controls.Add(this.javaDocLabel);
            this.Controls.Add(this.jTGDocLabel);
            this.Controls.Add(this.docLinkLabel);
            this.Controls.Add(this.downloadLinkLabel);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.generalDocLabel);
            this.Controls.Add(this.jtgPathTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.javaPathTextBox);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "jTrainGraphStarter Einstellungen";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox javaPathTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox jtgPathTextBox;
        private System.Windows.Forms.Label generalDocLabel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.LinkLabel downloadLinkLabel;
        private System.Windows.Forms.LinkLabel docLinkLabel;
        private System.Windows.Forms.Label jTGDocLabel;
        private System.Windows.Forms.Label javaDocLabel;
    }
}