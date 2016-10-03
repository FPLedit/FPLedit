namespace Buchfahrplan.Standard
{
    partial class LineEditForm
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
            this.stationListView = new System.Windows.Forms.ListView();
            this.editStationButton = new System.Windows.Forms.Button();
            this.deleteStationButton = new System.Windows.Forms.Button();
            this.newStationButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // stationListView
            // 
            this.stationListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stationListView.FullRowSelect = true;
            this.stationListView.HideSelection = false;
            this.stationListView.Location = new System.Drawing.Point(12, 12);
            this.stationListView.MultiSelect = false;
            this.stationListView.Name = "stationListView";
            this.stationListView.Size = new System.Drawing.Size(524, 282);
            this.stationListView.TabIndex = 8;
            this.stationListView.UseCompatibleStateImageBehavior = false;
            this.stationListView.View = System.Windows.Forms.View.Details;
            // 
            // editStationButton
            // 
            this.editStationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.editStationButton.Location = new System.Drawing.Point(542, 41);
            this.editStationButton.Name = "editStationButton";
            this.editStationButton.Size = new System.Drawing.Size(102, 23);
            this.editStationButton.TabIndex = 17;
            this.editStationButton.Text = "Station bearbeiten";
            this.editStationButton.UseVisualStyleBackColor = true;
            this.editStationButton.Click += new System.EventHandler(this.editStationButton_Click);
            this.editStationButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.editStationButton_MouseDown);
            // 
            // deleteStationButton
            // 
            this.deleteStationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.deleteStationButton.Location = new System.Drawing.Point(542, 70);
            this.deleteStationButton.Name = "deleteStationButton";
            this.deleteStationButton.Size = new System.Drawing.Size(102, 23);
            this.deleteStationButton.TabIndex = 19;
            this.deleteStationButton.Text = "Station löschen";
            this.deleteStationButton.UseVisualStyleBackColor = true;
            this.deleteStationButton.Click += new System.EventHandler(this.deleteStationButton_Click);
            // 
            // newStationButton
            // 
            this.newStationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.newStationButton.Location = new System.Drawing.Point(542, 12);
            this.newStationButton.Name = "newStationButton";
            this.newStationButton.Size = new System.Drawing.Size(102, 23);
            this.newStationButton.TabIndex = 20;
            this.newStationButton.Text = "Neue Station";
            this.newStationButton.UseVisualStyleBackColor = true;
            this.newStationButton.Click += new System.EventHandler(this.newStationButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(488, 300);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 22;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.Location = new System.Drawing.Point(569, 300);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 21;
            this.closeButton.Text = "Schließen";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // LineEditForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(656, 335);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.newStationButton);
            this.Controls.Add(this.deleteStationButton);
            this.Controls.Add(this.editStationButton);
            this.Controls.Add(this.stationListView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "LineEditForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Strecke bearbeiten";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView stationListView;
        private System.Windows.Forms.Button editStationButton;
        private System.Windows.Forms.Button deleteStationButton;
        private System.Windows.Forms.Button newStationButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button closeButton;
    }
}