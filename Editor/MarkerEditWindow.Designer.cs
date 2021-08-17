
namespace BhModule.Community.Pathing.Editor {
    partial class MarkerEditWindow {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MarkerEditWindow));
            this.pgPathingAttributeEditor = new System.Windows.Forms.PropertyGrid();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tsPackToolBar = new System.Windows.Forms.ToolStrip();
            this.cbPackList = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.addCategoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addMarkerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.beginTrailToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.ilEntityTreeIcons = new System.Windows.Forms.ImageList(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tsPackToolBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // pgPathingAttributeEditor
            // 
            this.pgPathingAttributeEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgPathingAttributeEditor.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pgPathingAttributeEditor.Location = new System.Drawing.Point(0, 0);
            this.pgPathingAttributeEditor.Name = "pgPathingAttributeEditor";
            this.pgPathingAttributeEditor.Size = new System.Drawing.Size(317, 280);
            this.pgPathingAttributeEditor.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tsPackToolBar);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.pgPathingAttributeEditor);
            this.splitContainer2.Size = new System.Drawing.Size(564, 280);
            this.splitContainer2.SplitterDistance = 243;
            this.splitContainer2.TabIndex = 1;
            // 
            // tsPackToolBar
            // 
            this.tsPackToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cbPackList,
            this.toolStripSplitButton1,
            this.toolStripButton1});
            this.tsPackToolBar.Location = new System.Drawing.Point(0, 0);
            this.tsPackToolBar.Name = "tsPackToolBar";
            this.tsPackToolBar.Size = new System.Drawing.Size(243, 25);
            this.tsPackToolBar.TabIndex = 1;
            this.tsPackToolBar.Text = "toolStrip1";
            // 
            // cbPackList
            // 
            this.cbPackList.Name = "cbPackList";
            this.cbPackList.Size = new System.Drawing.Size(121, 25);
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addCategoryToolStripMenuItem,
            this.addMarkerToolStripMenuItem,
            this.beginTrailToolStripMenuItem});
            this.toolStripSplitButton1.Image = global::BhModule.Community.Pathing.Properties.Resources.add;
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(32, 22);
            this.toolStripSplitButton1.Text = "toolStripSplitButton1";
            // 
            // addCategoryToolStripMenuItem
            // 
            this.addCategoryToolStripMenuItem.Image = global::BhModule.Community.Pathing.Properties.Resources.box;
            this.addCategoryToolStripMenuItem.Name = "addCategoryToolStripMenuItem";
            this.addCategoryToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.addCategoryToolStripMenuItem.Text = "Add Category";
            // 
            // addMarkerToolStripMenuItem
            // 
            this.addMarkerToolStripMenuItem.Image = global::BhModule.Community.Pathing.Properties.Resources.shape_square;
            this.addMarkerToolStripMenuItem.Name = "addMarkerToolStripMenuItem";
            this.addMarkerToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.addMarkerToolStripMenuItem.Text = "Add Marker";
            // 
            // beginTrailToolStripMenuItem
            // 
            this.beginTrailToolStripMenuItem.Image = global::BhModule.Community.Pathing.Properties.Resources.arrow_merge;
            this.beginTrailToolStripMenuItem.Name = "beginTrailToolStripMenuItem";
            this.beginTrailToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.beginTrailToolStripMenuItem.Text = "Record Trail";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::BhModule.Community.Pathing.Properties.Resources.bin_closed;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // ilEntityTreeIcons
            // 
            this.ilEntityTreeIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilEntityTreeIcons.ImageStream")));
            this.ilEntityTreeIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.ilEntityTreeIcons.Images.SetKeyName(0, "category");
            this.ilEntityTreeIcons.Images.SetKeyName(1, "marker");
            this.ilEntityTreeIcons.Images.SetKeyName(2, "trail");
            // 
            // MarkerEditWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(564, 280);
            this.Controls.Add(this.splitContainer2);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "MarkerEditWindow";
            this.ShowInTaskbar = false;
            this.Text = "Edit Marker";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.MarkerEditWindow_Shown);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tsPackToolBar.ResumeLayout(false);
            this.tsPackToolBar.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid pgPathingAttributeEditor;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ToolStrip tsPackToolBar;
        private System.Windows.Forms.ToolStripComboBox cbPackList;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
        private System.Windows.Forms.ToolStripMenuItem addCategoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addMarkerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem beginTrailToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ImageList ilEntityTreeIcons;
    }
}