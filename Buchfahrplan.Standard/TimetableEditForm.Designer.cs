namespace Buchfahrplan.Standard
{
    partial class TimetableEditForm
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
            this.topDataGridView = new System.Windows.Forms.DataGridView();
            this.cancelButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.bottomDataGridView = new System.Windows.Forms.DataGridView();
            this.topFromToLabel = new System.Windows.Forms.Label();
            this.bottomFromToLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.topDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bottomDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // topDataGridView
            // 
            this.topDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllHeaders;
            this.topDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.topDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.topDataGridView.Location = new System.Drawing.Point(12, 33);
            this.topDataGridView.MultiSelect = false;
            this.topDataGridView.Name = "topDataGridView";
            this.topDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.topDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.topDataGridView.Size = new System.Drawing.Size(956, 271);
            this.topDataGridView.TabIndex = 0;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(810, 627);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 24;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(891, 627);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 23;
            this.closeButton.Text = "Schließen";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // bottomDataGridView
            // 
            this.bottomDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllHeaders;
            this.bottomDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.bottomDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.bottomDataGridView.Location = new System.Drawing.Point(12, 339);
            this.bottomDataGridView.MultiSelect = false;
            this.bottomDataGridView.Name = "bottomDataGridView";
            this.bottomDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.bottomDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.bottomDataGridView.Size = new System.Drawing.Size(956, 271);
            this.bottomDataGridView.TabIndex = 25;
            // 
            // topFromToLabel
            // 
            this.topFromToLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topFromToLabel.Location = new System.Drawing.Point(9, 9);
            this.topFromToLabel.Name = "topFromToLabel";
            this.topFromToLabel.Size = new System.Drawing.Size(955, 15);
            this.topFromToLabel.TabIndex = 27;
            this.topFromToLabel.Text = "Züge von ... nach ...";
            this.topFromToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // bottomFromToLabel
            // 
            this.bottomFromToLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomFromToLabel.Location = new System.Drawing.Point(9, 321);
            this.bottomFromToLabel.Name = "bottomFromToLabel";
            this.bottomFromToLabel.Size = new System.Drawing.Size(955, 15);
            this.bottomFromToLabel.TabIndex = 28;
            this.bottomFromToLabel.Text = "Züge von ... nach ...";
            this.bottomFromToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TimetableEditForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(985, 662);
            this.Controls.Add(this.bottomFromToLabel);
            this.Controls.Add(this.topFromToLabel);
            this.Controls.Add(this.bottomDataGridView);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.topDataGridView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "TimetableEditForm";
            this.Text = "Fahrplan bearbeiten";
            ((System.ComponentModel.ISupportInitialize)(this.topDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bottomDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView topDataGridView;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.DataGridView bottomDataGridView;
        private System.Windows.Forms.Label topFromToLabel;
        private System.Windows.Forms.Label bottomFromToLabel;
    }
}