namespace Buchfahrplan
{
    partial class NewEditForm
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.listView1 = new System.Windows.Forms.ListView();
            this.cancelButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.saveTrainButton = new System.Windows.Forms.Button();
            this.lineLabel = new System.Windows.Forms.Label();
            this.lineTextBox = new System.Windows.Forms.TextBox();
            this.negativeCheckBox = new System.Windows.Forms.CheckBox();
            this.locomotiveLabel = new System.Windows.Forms.Label();
            this.locomotiveTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.locomotiveLabel);
            this.splitContainer1.Panel2.Controls.Add(this.locomotiveTextBox);
            this.splitContainer1.Panel2.Controls.Add(this.cancelButton);
            this.splitContainer1.Panel2.Controls.Add(this.closeButton);
            this.splitContainer1.Panel2.Controls.Add(this.saveTrainButton);
            this.splitContainer1.Panel2.Controls.Add(this.lineLabel);
            this.splitContainer1.Panel2.Controls.Add(this.lineTextBox);
            this.splitContainer1.Panel2.Controls.Add(this.negativeCheckBox);
            this.splitContainer1.Size = new System.Drawing.Size(507, 331);
            this.splitContainer1.SplitterDistance = 196;
            this.splitContainer1.TabIndex = 3;
            // 
            // listView1
            // 
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(507, 196);
            this.listView1.TabIndex = 2;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(339, 96);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(420, 96);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 4;
            this.closeButton.Text = "Schließen";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // saveTrainButton
            // 
            this.saveTrainButton.Location = new System.Drawing.Point(12, 96);
            this.saveTrainButton.Name = "saveTrainButton";
            this.saveTrainButton.Size = new System.Drawing.Size(75, 23);
            this.saveTrainButton.TabIndex = 3;
            this.saveTrainButton.Text = "Speichern";
            this.saveTrainButton.UseVisualStyleBackColor = true;
            this.saveTrainButton.Click += new System.EventHandler(this.saveTrainButton_Click);
            // 
            // lineLabel
            // 
            this.lineLabel.AutoSize = true;
            this.lineLabel.Location = new System.Drawing.Point(12, 23);
            this.lineLabel.Name = "lineLabel";
            this.lineLabel.Size = new System.Drawing.Size(44, 13);
            this.lineLabel.TabIndex = 2;
            this.lineLabel.Text = "Strecke";
            // 
            // lineTextBox
            // 
            this.lineTextBox.Location = new System.Drawing.Point(78, 20);
            this.lineTextBox.Name = "lineTextBox";
            this.lineTextBox.Size = new System.Drawing.Size(283, 20);
            this.lineTextBox.TabIndex = 1;
            // 
            // negativeCheckBox
            // 
            this.negativeCheckBox.AutoSize = true;
            this.negativeCheckBox.Location = new System.Drawing.Point(15, 73);
            this.negativeCheckBox.Name = "negativeCheckBox";
            this.negativeCheckBox.Size = new System.Drawing.Size(124, 17);
            this.negativeCheckBox.TabIndex = 0;
            this.negativeCheckBox.Text = "Umgekerte Richtung";
            this.negativeCheckBox.UseVisualStyleBackColor = true;
            // 
            // locomotiveLabel
            // 
            this.locomotiveLabel.AutoSize = true;
            this.locomotiveLabel.Location = new System.Drawing.Point(12, 50);
            this.locomotiveLabel.Name = "locomotiveLabel";
            this.locomotiveLabel.Size = new System.Drawing.Size(22, 13);
            this.locomotiveLabel.TabIndex = 7;
            this.locomotiveLabel.Text = "Tfz";
            // 
            // locomotiveTextBox
            // 
            this.locomotiveTextBox.Location = new System.Drawing.Point(78, 47);
            this.locomotiveTextBox.Name = "locomotiveTextBox";
            this.locomotiveTextBox.Size = new System.Drawing.Size(283, 20);
            this.locomotiveTextBox.TabIndex = 6;
            // 
            // NewEditForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(507, 331);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "NewEditForm";
            this.Text = "Zusätliche Informationen zum Zug hinzfügen...";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NewEditForm_FormClosing);
            this.Load += new System.EventHandler(this.NewEditForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label lineLabel;
        private System.Windows.Forms.TextBox lineTextBox;
        private System.Windows.Forms.CheckBox negativeCheckBox;
        private System.Windows.Forms.Button saveTrainButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label locomotiveLabel;
        private System.Windows.Forms.TextBox locomotiveTextBox;

    }
}