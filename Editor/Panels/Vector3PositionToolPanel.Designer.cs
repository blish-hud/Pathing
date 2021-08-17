
namespace BhModule.Community.Pathing.Editor.Panels {
    partial class Vector3PositionToolPanel {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.bttnTranslateTool = new System.Windows.Forms.Button();
            this.bttnMoveHere = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // bttnTranslateTool
            // 
            this.bttnTranslateTool.BackgroundImage = global::BhModule.Community.Pathing.Properties.Resources.arrow_up_left_right_64px;
            this.bttnTranslateTool.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.bttnTranslateTool.Location = new System.Drawing.Point(106, 2);
            this.bttnTranslateTool.Margin = new System.Windows.Forms.Padding(2);
            this.bttnTranslateTool.Name = "bttnTranslateTool";
            this.bttnTranslateTool.Size = new System.Drawing.Size(48, 48);
            this.bttnTranslateTool.TabIndex = 1;
            this.bttnTranslateTool.UseVisualStyleBackColor = true;
            // 
            // bttnMoveHere
            // 
            this.bttnMoveHere.BackgroundImage = global::BhModule.Community.Pathing.Properties.Resources.here_64px;
            this.bttnMoveHere.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.bttnMoveHere.Location = new System.Drawing.Point(2, 2);
            this.bttnMoveHere.Margin = new System.Windows.Forms.Padding(2);
            this.bttnMoveHere.Name = "bttnMoveHere";
            this.bttnMoveHere.Size = new System.Drawing.Size(48, 48);
            this.bttnMoveHere.TabIndex = 0;
            this.bttnMoveHere.UseVisualStyleBackColor = true;
            this.bttnMoveHere.Click += new System.EventHandler(this.bttnMoveHere_Click);
            // 
            // button1
            // 
            this.button1.BackgroundImage = global::BhModule.Community.Pathing.Properties.Resources.move_64px;
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button1.Location = new System.Drawing.Point(54, 2);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(48, 48);
            this.button1.TabIndex = 2;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // Vector3PositionToolPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.bttnTranslateTool);
            this.Controls.Add(this.bttnMoveHere);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Vector3PositionToolPanel";
            this.Size = new System.Drawing.Size(401, 171);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bttnMoveHere;
        private System.Windows.Forms.Button bttnTranslateTool;
        private System.Windows.Forms.Button button1;
    }
}
