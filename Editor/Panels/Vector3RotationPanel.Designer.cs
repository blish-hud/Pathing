
namespace BhModule.Community.Pathing.Editor.Panels {
    partial class Vector3RotationPanel {
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
            this.button1 = new System.Windows.Forms.Button();
            this.bttnMoveHere = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Image = global::BhModule.Community.Pathing.Properties.Resources.this_way_up_64px;
            this.button1.Location = new System.Drawing.Point(3, 73);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(64, 64);
            this.button1.TabIndex = 2;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // bttnMoveHere
            // 
            this.bttnMoveHere.Image = global::BhModule.Community.Pathing.Properties.Resources.delete_64px;
            this.bttnMoveHere.Location = new System.Drawing.Point(3, 3);
            this.bttnMoveHere.Name = "bttnMoveHere";
            this.bttnMoveHere.Size = new System.Drawing.Size(64, 64);
            this.bttnMoveHere.TabIndex = 1;
            this.bttnMoveHere.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Image = global::BhModule.Community.Pathing.Properties.Resources.look_64px;
            this.button2.Location = new System.Drawing.Point(3, 143);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(64, 64);
            this.button2.TabIndex = 3;
            this.button2.UseVisualStyleBackColor = true;
            // 
            // Vector3RotationPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.bttnMoveHere);
            this.Name = "Vector3RotationPanel";
            this.Size = new System.Drawing.Size(535, 210);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bttnMoveHere;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}
