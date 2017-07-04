namespace FPLedit.AushangfahrplanExport.Forms
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
            this.trainPattGroupBox.SuspendLayout();
            this.stationPattGroupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // trainPattGroupBox
            // 
            this.trainPattGroupBox.Controls.Add(this.editTrainPattButton);
            this.trainPattGroupBox.Controls.Add(this.deleteTrainPattButton);
            this.trainPattGroupBox.Controls.Add(this.addTrainPattButton);
            this.trainPattGroupBox.Controls.Add(this.trainPattListView);
            this.trainPattGroupBox.Location = new System.Drawing.Point(12, 12);
            this.trainPattGroupBox.Name = "trainPattGroupBox";
            this.trainPattGroupBox.Size = new System.Drawing.Size(269, 186);
            this.trainPattGroupBox.TabIndex = 0;
            this.trainPattGroupBox.TabStop = false;
            this.trainPattGroupBox.Text = "Züge ausblenden";
            // 
            // editTrainPattButton
            // 
            this.editTrainPattButton.Location = new System.Drawing.Point(87, 150);
            this.editTrainPattButton.Name = "editTrainPattButton";
            this.editTrainPattButton.Size = new System.Drawing.Size(75, 23);
            this.editTrainPattButton.TabIndex = 2;
            this.editTrainPattButton.Text = "Bearbeiten";
            this.editTrainPattButton.UseVisualStyleBackColor = true;
            this.editTrainPattButton.Click += new System.EventHandler(this.editTrainPattButton_Click);
            // 
            // deleteTrainPattButton
            // 
            this.deleteTrainPattButton.Location = new System.Drawing.Point(168, 150);
            this.deleteTrainPattButton.Name = "deleteTrainPattButton";
            this.deleteTrainPattButton.Size = new System.Drawing.Size(75, 23);
            this.deleteTrainPattButton.TabIndex = 3;
            this.deleteTrainPattButton.Text = "Löschen";
            this.deleteTrainPattButton.UseVisualStyleBackColor = true;
            this.deleteTrainPattButton.Click += new System.EventHandler(this.deleteTrainPattButton_Click);
            // 
            // addTrainPattButton
            // 
            this.addTrainPattButton.Location = new System.Drawing.Point(6, 150);
            this.addTrainPattButton.Name = "addTrainPattButton";
            this.addTrainPattButton.Size = new System.Drawing.Size(75, 23);
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
            this.trainPattListView.Size = new System.Drawing.Size(257, 130);
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
            this.stationPattGroupBox2.Location = new System.Drawing.Point(286, 12);
            this.stationPattGroupBox2.Name = "stationPattGroupBox2";
            this.stationPattGroupBox2.Size = new System.Drawing.Size(269, 186);
            this.stationPattGroupBox2.TabIndex = 1;
            this.stationPattGroupBox2.TabStop = false;
            this.stationPattGroupBox2.Text = "Bahnhöfe ausblenden";
            // 
            // editStationPattButton
            // 
            this.editStationPattButton.Location = new System.Drawing.Point(87, 150);
            this.editStationPattButton.Name = "editStationPattButton";
            this.editStationPattButton.Size = new System.Drawing.Size(75, 23);
            this.editStationPattButton.TabIndex = 2;
            this.editStationPattButton.Text = "Bearbeiten";
            this.editStationPattButton.UseVisualStyleBackColor = true;
            this.editStationPattButton.Click += new System.EventHandler(this.editStationPattButton_Click);
            // 
            // deleteStationPattButton
            // 
            this.deleteStationPattButton.Location = new System.Drawing.Point(168, 150);
            this.deleteStationPattButton.Name = "deleteStationPattButton";
            this.deleteStationPattButton.Size = new System.Drawing.Size(75, 23);
            this.deleteStationPattButton.TabIndex = 3;
            this.deleteStationPattButton.Text = "Löschen";
            this.deleteStationPattButton.UseVisualStyleBackColor = true;
            this.deleteStationPattButton.Click += new System.EventHandler(this.deleteStationPattButton_Click);
            // 
            // addStationPattButton
            // 
            this.addStationPattButton.Location = new System.Drawing.Point(6, 150);
            this.addStationPattButton.Name = "addStationPattButton";
            this.addStationPattButton.Size = new System.Drawing.Size(75, 23);
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
            this.stationPattListView.Size = new System.Drawing.Size(257, 130);
            this.stationPattListView.TabIndex = 0;
            this.stationPattListView.UseCompatibleStateImageBehavior = false;
            this.stationPattListView.View = System.Windows.Forms.View.Details;
            // 
            // closeButton
            // 
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeButton.Location = new System.Drawing.Point(480, 204);
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
            this.cancelButton.Location = new System.Drawing.Point(399, 204);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // FilterForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(567, 243);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.stationPattGroupBox2);
            this.Controls.Add(this.trainPattGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FilterForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Aushangfahrplan-Filterregeln";
            this.trainPattGroupBox.ResumeLayout(false);
            this.stationPattGroupBox2.ResumeLayout(false);
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
    }
}