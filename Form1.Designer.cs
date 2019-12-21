namespace Re0GIS
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
			this.File_Open = new System.Windows.Forms.ToolStripMenuItem();
			this.File_Open_Polygon = new System.Windows.Forms.ToolStripMenuItem();
			this.File_Save = new System.Windows.Forms.ToolStripMenuItem();
			this.VerString = new System.Windows.Forms.Label();
			this.inname = new System.Windows.Forms.Label();
			this.File_Open_PolyLine = new System.Windows.Forms.ToolStripMenuItem();
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
			this.MainPicBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MainPicBox_MouseMove);
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
            this.File_Open,
            this.File_Save});
			this.File.Name = "File";
			this.File.Size = new System.Drawing.Size(53, 24);
			this.File.Text = "文件";
			// 
			// File_Open
			// 
			this.File_Open.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.File_Open_PolyLine,
            this.File_Open_Polygon});
			this.File_Open.Name = "File_Open";
			this.File_Open.Size = new System.Drawing.Size(224, 26);
			this.File_Open.Text = "打开";
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
			// VerString
			// 
			this.VerString.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.VerString.AutoSize = true;
			this.VerString.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Bold);
			this.VerString.Location = new System.Drawing.Point(12, 634);
			this.VerString.Margin = new System.Windows.Forms.Padding(3);
			this.VerString.Name = "VerString";
			this.VerString.Size = new System.Drawing.Size(394, 27);
			this.VerString.TabIndex = 3;
			this.VerString.Text = "版本号：AA.BB.CCCC.DDDDD E";
			// 
			// inname
			// 
			this.inname.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.inname.AutoSize = true;
			this.inname.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Bold);
			this.inname.Location = new System.Drawing.Point(12, 601);
			this.inname.Margin = new System.Windows.Forms.Padding(3);
			this.inname.Name = "inname";
			this.inname.Size = new System.Drawing.Size(0, 27);
			this.inname.TabIndex = 4;
			this.inname.Click += new System.EventHandler(this.inname_Click);
			// 
			// File_Open_PolyLine
			// 
			this.File_Open_PolyLine.Name = "File_Open_PolyLine";
			this.File_Open_PolyLine.Size = new System.Drawing.Size(224, 26);
			this.File_Open_PolyLine.Text = "折线";
			this.File_Open_PolyLine.Click += new System.EventHandler(this.File_Open_PolyLine_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1262, 673);
			this.Controls.Add(this.inname);
			this.Controls.Add(this.VerString);
			this.Controls.Add(this.MainPicBox);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Form1";
			this.Text = "从零开始的GIS生活（BY：二枚目）";
			this.Load += new System.EventHandler(this.Form1_Load);
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
		private System.Windows.Forms.ToolStripMenuItem File_Open;
		private System.Windows.Forms.ToolStripMenuItem File_Open_Polygon;
		private System.Windows.Forms.ToolStripMenuItem File_Save;
		private System.Windows.Forms.Label VerString;
		private System.Windows.Forms.Label inname;
		private System.Windows.Forms.ToolStripMenuItem File_Open_PolyLine;
	}
}

