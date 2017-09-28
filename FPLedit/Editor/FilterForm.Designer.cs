namespace FPLedit.Editor
{
    partial class FilterForm
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
            this.trainPattGroupBox = new System.Windows.Forms.GroupBox();
            this.editTrainPattButton = new System.Windows.Forms.Button();
            this.deleteTrainPattButton = new System.Windows.Forms.Button();
            this.addTrainPattButton = new System.Windows.Forms.Button();
            this.trainPattListView = new System.Windows.Forms.ListView();
            this.stationPattGroupBox2 = new System.Windows.Forms.GroupBox();
            this.editStationPattButton = new System.Windows.Forms.Button();
            this.deleteStationPattButton = new System.Windows.Forms.Button();
            this.addStationPattButton = new System.Windows.Forms.Button();
            this.stationPattListView = new System.Windows.Forms.ListView();
            this.closeButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.filterTypeGroupBox = new System.Windows.Forms.GroupBox();
            this.typeListBox = new System.Windows.Forms.ListBox();
            this.trainPattGroupBox.SuspendLayout();
            this.stationPattGroupBox2.SuspendLayout();
            this.filterTypeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // trainPattGroupBox
            // 
            this.trainPattGroupBox.Controls.Add(this.editTrainPattButton);
            this.trainPattGroupBox.Controls.Add(this.deleteTrainPattButton);
            this.trainPattGroupBox.Controls.Add(this.addTrainPattButton);
            this.trainPattGroupBox.Controls.Add(this.trainPattListView);
            this.trainPattGroupBox.Location = new System.Drawing.Point(148, 12);
            this.trainPattGroupBox.Name = "trainPattGroupBox";
            this.trainPattGroupBox.Size = new System.Drawing.Size(238, 207);
            this.trainPattGroupBox.TabIndex = 0;
            this.trainPattGroupBox.TabStop = false;
            this.trainPattGroupBox.Text = "Züge ausblenden";
            // 
            // editTrainPattButton
            // 
            this.editTrainPattButton.Location = new System.Drawing.Point(82, 178);
            this.editTrainPattButton.Name = "editTrainPattButton";
            this.editTrainPattButton.Size = new System.Drawing.Size(70, 23);
            this.editTrainPattButton.TabIndex = 2;
            this.editTrainPattButton.Text = "Bearbeiten";
            this.editTrainPattButton.UseVisualStyleBackColor = true;
            this.editTrainPattButton.Click += new System.EventHandler(this.editTrainPattButton_Click);
            // 
            // deleteTrainPattButton
            // 
            this.deleteTrainPattButton.Location = new System.Drawing.Point(158, 178);
            this.deleteTrainPattButton.Name = "deleteTrainPattButton";
            this.deleteTrainPattButton.Size = new System.Drawing.Size(70, 23);
            this.deleteTrainPattButton.TabIndex = 3;
            this.deleteTrainPattButton.Text = "Löschen";
            this.deleteTrainPattButton.UseVisualStyleBackColor = true;
            this.deleteTrainPattButton.Click += new System.EventHandler(this.deleteTrainPattButton_Click);
            // 
            // addTrainPattButton
            // 
            this.addTrainPattButton.Location = new System.Drawing.Point(6, 178);
            this.addTrainPattButton.Name = "addTrainPattButton";
            this.addTrainPattButton.Size = new System.Drawing.Size(70, 23);
            this.addTrainPattButton.TabIndex = 1;
            this.addTrainPattButton.Text = "Hinzufügen";
            this.addTrainPattButton.UseVisualStyleBackColor = true;
            this.addTrainPattButton.Click += new System.EventHandler(this.addTrainPattButton_Click);
            // 
            // trainPattListView
            // 
            this.trainPattListView.FullRowSelect = true;
            this.trainPattListView.Location = new System.Drawing.Point(6, 14);
            this.trainPattListView.Name = "trainPattListView";
            this.trainPattListView.Size = new System.Drawing.Size(222, 158);
            this.trainPattListView.TabIndex = 0;
            this.trainPattListView.UseCompatibleStateImageBehavior = false;
            this.trainPattListView.View = System.Windows.Forms.View.Details;
            // 
            // stationPattGroupBox2
            // 
            this.stationPattGroupBox2.Controls.Add(this.editStationPattButton);
            this.stationPattGroupBox2.Controls.Add(this.deleteStationPattButton);
            this.stationPattGroupBox2.Controls.Add(this.addStationPattButton);
            this.stationPattGroupBox2.Controls.Add(this.stationPattListView);
            this.stationPattGroupBox2.Location = new System.Drawing.Point(392, 12);
            this.stationPattGroupBox2.Name = "stationPattGroupBox2";
            this.stationPattGroupBox2.Size = new System.Drawing.Size(237, 207);
            this.stationPattGroupBox2.TabIndex = 1;
            this.stationPattGroupBox2.TabStop = false;
            this.stationPattGroupBox2.Text = "Bahnhöfe ausblenden";
            // 
            // editStationPattButton
            // 
            this.editStationPattButton.Location = new System.Drawing.Point(82, 178);
            this.editStationPattButton.Name = "editStationPattButton";
            this.editStationPattButton.Size = new System.Drawing.Size(70, 23);
            this.editStationPattButton.TabIndex = 2;
            this.editStationPattButton.Text = "Bearbeiten";
            this.editStationPattButton.UseVisualStyleBackColor = true;
            this.editStationPattButton.Click += new System.EventHandler(this.editStationPattButton_Click);
            // 
            // deleteStationPattButton
            // 
            this.deleteStationPattButton.Location = new System.Drawing.Point(158, 178);
            this.deleteStationPattButton.Name = "deleteStationPattButton";
            this.deleteStationPattButton.Size = new System.Drawing.Size(70, 23);
            this.deleteStationPattButton.TabIndex = 3;
            this.deleteStationPattButton.Text = "Löschen";
            this.deleteStationPattButton.UseVisualStyleBackColor = true;
            this.deleteStationPattButton.Click += new System.EventHandler(this.deleteStationPattButton_Click);
            // 
            // addStationPattButton
            // 
            this.addStationPattButton.Location = new System.Drawing.Point(6, 178);
            this.addStationPattButton.Name = "addStationPattButton";
            this.addStationPattButton.Size = new System.Drawing.Size(70, 23);
            this.addStationPattButton.TabIndex = 1;
            this.addStationPattButton.Text = "Hinzufügen";
            this.addStationPattButton.UseVisualStyleBackColor = true;
            this.addStationPattButton.Click += new System.EventHandler(this.addStationPattButton_Click);
            // 
            // stationPattListView
            // 
            this.stationPattListView.FullRowSelect = true;
            this.stationPattListView.Location = new System.Drawing.Point(6, 14);
            this.stationPattListView.Name = "stationPattListView";
            this.stationPattListView.Size = new System.Drawing.Size(222, 158);
            this.stationPattListView.TabIndex = 0;
            this.stationPattListView.UseCompatibleStateImageBehavior = false;
            this.stationPattListView.View = System.Windows.Forms.View.Details;
            // 
            // closeButton
            // 
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeButton.Location = new System.Drawing.Point(554, 225);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 2;
            this.closeButton.Text = "Schließen";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(473, 225);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // filterTypeGroupBox
            // 
            this.filterTypeGroupBox.Controls.Add(this.typeListBox);
            this.filterTypeGroupBox.Location = new System.Drawing.Point(12, 12);
            this.filterTypeGroupBox.Name = "filterTypeGroupBox";
            this.filterTypeGroupBox.Size = new System.Drawing.Size(130, 207);
            this.filterTypeGroupBox.TabIndex = 4;
            this.filterTypeGroupBox.TabStop = false;
            this.filterTypeGroupBox.Text = "Filter für";
            // 
            // typeListBox
            // 
            this.typeListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.typeListBox.FormattingEnabled = true;
            this.typeListBox.Location = new System.Drawing.Point(3, 16);
            this.typeListBox.Name = "typeListBox";
            this.typeListBox.Size = new System.Drawing.Size(124, 188);
            this.typeListBox.TabIndex = 0;
            this.typeListBox.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // FilterForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(641, 260);
            this.Controls.Add(this.filterTypeGroupBox);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.stationPattGroupBox2);
            this.Controls.Add(this.trainPattGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FilterForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Filterregeln";
            this.trainPattGroupBox.ResumeLayout(false);
            this.stationPattGroupBox2.ResumeLayout(false);
            this.filterTypeGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox trainPattGroupBox;
        private System.Windows.Forms.GroupBox stationPattGroupBox2;
        private System.Windows.Forms.Button deleteTrainPattButton;
        private System.Windows.Forms.Button addTrainPattButton;
        private System.Windows.Forms.ListView trainPattListView;
        private System.Windows.Forms.Button deleteStationPattButton;
        private System.Windows.Forms.Button addStationPattButton;
        private System.Windows.Forms.ListView stationPattListView;
        private System.Windows.Forms.Button editTrainPattButton;
        private System.Windows.Forms.Button editStationPattButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.GroupBox filterTypeGroupBox;
        private System.Windows.Forms.ListBox typeListBox;
    }
}