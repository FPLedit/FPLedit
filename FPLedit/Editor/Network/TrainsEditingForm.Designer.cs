namespace FPLedit.Editor.Network
{
    partial class TrainsEditingForm
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
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.listView = new System.Windows.Forms.ListView();
            this.topActionsPanel = new System.Windows.Forms.Panel();
            this.copyButton = new System.Windows.Forms.Button();
            this.editTimetableButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.editButton = new System.Windows.Forms.Button();
            this.newButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.nameValidator = new FPLedit.Shared.Validators.NotEmptyValidator();
            this.editPathButton = new System.Windows.Forms.Button();
            this.mainTableLayoutPanel.SuspendLayout();
            this.topActionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTableLayoutPanel.ColumnCount = 2;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainTableLayoutPanel.Controls.Add(this.listView, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.topActionsPanel, 1, 0);
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(12, 12);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 1;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(800, 425);
            this.mainTableLayoutPanel.TabIndex = 2;
            // 
            // listView
            // 
            this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView.FullRowSelect = true;
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(3, 3);
            this.listView.MultiSelect = false;
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(674, 419);
            this.listView.TabIndex = 4;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            // 
            // topActionsPanel
            // 
            this.topActionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topActionsPanel.Controls.Add(this.editPathButton);
            this.topActionsPanel.Controls.Add(this.copyButton);
            this.topActionsPanel.Controls.Add(this.editTimetableButton);
            this.topActionsPanel.Controls.Add(this.deleteButton);
            this.topActionsPanel.Controls.Add(this.editButton);
            this.topActionsPanel.Controls.Add(this.newButton);
            this.topActionsPanel.Location = new System.Drawing.Point(683, 3);
            this.topActionsPanel.Name = "topActionsPanel";
            this.topActionsPanel.Size = new System.Drawing.Size(114, 419);
            this.topActionsPanel.TabIndex = 1;
            // 
            // copyButton
            // 
            this.copyButton.Location = new System.Drawing.Point(0, 90);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(108, 23);
            this.copyButton.TabIndex = 4;
            this.copyButton.Text = "Zug kopieren";
            this.copyButton.UseVisualStyleBackColor = true;
            this.copyButton.Click += new System.EventHandler(this.copyButton_Click);
            // 
            // editTimetableButton
            // 
            this.editTimetableButton.Location = new System.Drawing.Point(3, 165);
            this.editTimetableButton.Name = "editTimetableButton";
            this.editTimetableButton.Size = new System.Drawing.Size(108, 41);
            this.editTimetableButton.TabIndex = 3;
            this.editTimetableButton.Text = "Fahrplan bearbeiten";
            this.editTimetableButton.UseVisualStyleBackColor = true;
            this.editTimetableButton.Click += new System.EventHandler(this.editTimetableButton_Click);
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(0, 61);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(108, 23);
            this.deleteButton.TabIndex = 2;
            this.deleteButton.Text = "Zug löschen";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.topDeleteButton_Click);
            // 
            // editButton
            // 
            this.editButton.Location = new System.Drawing.Point(0, 32);
            this.editButton.Name = "editButton";
            this.editButton.Size = new System.Drawing.Size(108, 23);
            this.editButton.TabIndex = 1;
            this.editButton.Text = "Zug bearbeiten";
            this.editButton.UseVisualStyleBackColor = true;
            this.editButton.Click += new System.EventHandler(this.topEditButton_Click);
            // 
            // newButton
            // 
            this.newButton.Location = new System.Drawing.Point(0, 3);
            this.newButton.Name = "newButton";
            this.newButton.Size = new System.Drawing.Size(108, 23);
            this.newButton.TabIndex = 0;
            this.newButton.Text = "Neuer Zug";
            this.newButton.UseVisualStyleBackColor = true;
            this.newButton.Click += new System.EventHandler(this.topNewButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(656, 443);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 14;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(737, 443);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 13;
            this.closeButton.Text = "Schließen";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // nameValidator
            // 
            this.nameValidator.Control = null;
            this.nameValidator.ErrorMessage = "Bitte einen Namen eingeben!";
            // 
            // editPathButton
            // 
            this.editPathButton.Location = new System.Drawing.Point(3, 212);
            this.editPathButton.Name = "editPathButton";
            this.editPathButton.Size = new System.Drawing.Size(108, 41);
            this.editPathButton.TabIndex = 5;
            this.editPathButton.Text = "Laufweg bearbeiten";
            this.editPathButton.UseVisualStyleBackColor = true;
            this.editPathButton.Click += new System.EventHandler(this.editPathButton_Click);
            // 
            // TrainsEditingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 478);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.KeyPreview = true;
            this.Name = "TrainsEditingForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Züge bearbeiten";
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.topActionsPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Shared.Validators.NotEmptyValidator nameValidator;
        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.Panel topActionsPanel;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button editButton;
        private System.Windows.Forms.Button newButton;
        private System.Windows.Forms.Button editTimetableButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.Button editPathButton;
    }
}