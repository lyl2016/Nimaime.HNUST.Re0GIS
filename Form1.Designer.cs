namespace PolygonCut
{
	partial class Form1
	{
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows 窗体设计器生成的代码

		/// <summary>
		/// 设计器支持所需的方法 - 不要修改
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			this.MainPicBox = new System.Windows.Forms.PictureBox();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.File = new System.Windows.Forms.ToolStripMenuItem();
			this.Open = new System.Windows.Forms.ToolStripMenuItem();
			this.File_Open_Polygon = new System.Windows.Forms.ToolStripMenuItem();
			this.File_Save = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.MainPicBox)).BeginInit();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainPicBox
			// 
			this.MainPicBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MainPicBox.Location = new System.Drawing.Point(12, 31);
			this.MainPicBox.Name = "MainPicBox";
			this.MainPicBox.Size = new System.Drawing.Size(1238, 630);
			this.MainPicBox.TabIndex = 1;
			this.MainPicBox.TabStop = false;
			// 
			// menuStrip1
			// 
			this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.File});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1262, 28);
			this.menuStrip1.TabIndex = 2;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// File
			// 
			this.File.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Open,
            this.File_Save});
			this.File.Name = "File";
			this.File.Size = new System.Drawing.Size(53, 24);
			this.File.Text = "文件";
			// 
			// Open
			// 
			this.Open.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.File_Open_Polygon});
			this.Open.Name = "Open";
			this.Open.Size = new System.Drawing.Size(224, 26);
			this.Open.Text = "打开";
			// 
			// File_Open_Polygon
			// 
			this.File_Open_Polygon.Name = "File_Open_Polygon";
			this.File_Open_Polygon.Size = new System.Drawing.Size(224, 26);
			this.File_Open_Polygon.Text = "多边形";
			this.File_Open_Polygon.Click += new System.EventHandler(this.File_Open_Polygon_Click);
			// 
			// File_Save
			// 
			this.File_Save.Name = "File_Save";
			this.File_Save.Size = new System.Drawing.Size(224, 26);
			this.File_Save.Text = "保存";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1262, 673);
			this.Controls.Add(this.MainPicBox);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Form1";
			this.Text = "从零开始的GIS生活（BY：二枚目）";
			((System.ComponentModel.ISupportInitialize)(this.MainPicBox)).EndInit();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.PictureBox MainPicBox;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem File;
		private System.Windows.Forms.ToolStripMenuItem Open;
		private System.Windows.Forms.ToolStripMenuItem File_Open_Polygon;
		private System.Windows.Forms.ToolStripMenuItem File_Save;
	}
}

