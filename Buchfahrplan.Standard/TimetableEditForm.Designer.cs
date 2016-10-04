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
            this.cancelButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.topPanel = new System.Windows.Forms.Panel();
            this.topLineLabel = new System.Windows.Forms.Label();
            this.topDataGridView = new System.Windows.Forms.DataGridView();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.bottomLineLabel = new System.Windows.Forms.Label();
            this.bottomDataGridView = new System.Windows.Forms.DataGridView();
            this.mainTableLayoutPanel.SuspendLayout();
            this.topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.topDataGridView)).BeginInit();
            this.bottomPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bottomDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(683, 603);
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
            this.closeButton.Location = new System.Drawing.Point(763, 603);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 23;
            this.closeButton.Text = "Schließen";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTableLayoutPanel.ColumnCount = 1;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.Controls.Add(this.topPanel, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.bottomPanel, 0, 1);
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(12, 12);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 2;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(826, 585);
            this.mainTableLayoutPanel.TabIndex = 29;
            // 
            // topPanel
            // 
            this.topPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topPanel.Controls.Add(this.topLineLabel);
            this.topPanel.Controls.Add(this.topDataGridView);
            this.topPanel.Location = new System.Drawing.Point(3, 3);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(820, 286);
            this.topPanel.TabIndex = 0;
            // 
            // topLineLabel
            // 
            this.topLineLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topLineLabel.Location = new System.Drawing.Point(3, 0);
            this.topLineLabel.Name = "topLineLabel";
            this.topLineLabel.Size = new System.Drawing.Size(814, 15);
            this.topLineLabel.TabIndex = 33;
            this.topLineLabel.Text = "Züge von ... nach ...";
            this.topLineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // topDataGridView
            // 
            this.topDataGridView.AllowUserToAddRows = false;
            this.topDataGridView.AllowUserToDeleteRows = false;
            this.topDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllHeaders;
            this.topDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.topDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.topDataGridView.Location = new System.Drawing.Point(3, 18);
            this.topDataGridView.MultiSelect = false;
            this.topDataGridView.Name = "topDataGridView";
            this.topDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.topDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.topDataGridView.Size = new System.Drawing.Size(814, 268);
            this.topDataGridView.TabIndex = 32;
            // 
            // bottomPanel
            // 
            this.bottomPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomPanel.Controls.Add(this.bottomLineLabel);
            this.bottomPanel.Controls.Add(this.bottomDataGridView);
            this.bottomPanel.Location = new System.Drawing.Point(3, 295);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(820, 287);
            this.bottomPanel.TabIndex = 1;
            // 
            // bottomLineLabel
            // 
            this.bottomLineLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomLineLabel.Location = new System.Drawing.Point(3, 0);
            this.bottomLineLabel.Name = "bottomLineLabel";
            this.bottomLineLabel.Size = new System.Drawing.Size(814, 15);
            this.bottomLineLabel.TabIndex = 30;
            this.bottomLineLabel.Text = "Züge von ... nach ...";
            this.bottomLineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // bottomDataGridView
            // 
            this.bottomDataGridView.AllowUserToAddRows = false;
            this.bottomDataGridView.AllowUserToDeleteRows = false;
            this.bottomDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllHeaders;
            this.bottomDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.bottomDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.bottomDataGridView.Location = new System.Drawing.Point(0, 18);
            this.bottomDataGridView.MultiSelect = false;
            this.bottomDataGridView.Name = "bottomDataGridView";
            this.bottomDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.bottomDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.bottomDataGridView.Size = new System.Drawing.Size(820, 266);
            this.bottomDataGridView.TabIndex = 29;
            // 
            // TimetableEditForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(850, 638);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.closeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "TimetableEditForm";
            this.Text = "Fahrplan bearbeiten";
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.topPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.topDataGridView)).EndInit();
            this.bottomPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bottomDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.Label topLineLabel;
        private System.Windows.Forms.DataGridView topDataGridView;
        private System.Windows.Forms.Label bottomLineLabel;
        private System.Windows.Forms.DataGridView bottomDataGridView;
    }
}