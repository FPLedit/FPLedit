namespace FPLedit.NewEditor
{
    partial class LineEditingControl
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.newButton = new System.Windows.Forms.Button();
            this.lineRenderer = new FPLedit.LineRenderer();
            this.SuspendLayout();
            // 
            // newButton
            // 
            this.newButton.Enabled = false;
            this.newButton.Location = new System.Drawing.Point(3, 3);
            this.newButton.Name = "newButton";
            this.newButton.Size = new System.Drawing.Size(86, 23);
            this.newButton.TabIndex = 1;
            this.newButton.Text = "Neue Station";
            this.newButton.UseVisualStyleBackColor = true;
            // 
            // lineRenderer
            // 
            this.lineRenderer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lineRenderer.Location = new System.Drawing.Point(3, 55);
            this.lineRenderer.Name = "lineRenderer";
            this.lineRenderer.Size = new System.Drawing.Size(618, 269);
            this.lineRenderer.TabIndex = 0;
            this.lineRenderer.Text = "lineRenderer1";
            // 
            // LineEditingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.newButton);
            this.Controls.Add(this.lineRenderer);
            this.Name = "LineEditingControl";
            this.Size = new System.Drawing.Size(618, 324);
            this.ResumeLayout(false);

        }

        #endregion

        private LineRenderer lineRenderer;
        private System.Windows.Forms.Button newButton;
    }
}
