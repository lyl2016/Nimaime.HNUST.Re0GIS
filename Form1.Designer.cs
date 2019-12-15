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
			this.Load = new System.Windows.Forms.Button();
			this.MainPicBox = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.MainPicBox)).BeginInit();
			this.SuspendLayout();
			// 
			// Load
			// 
			this.Load.Location = new System.Drawing.Point(14, 14);
			this.Load.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Load.Name = "Load";
			this.Load.Size = new System.Drawing.Size(130, 29);
			this.Load.TabIndex = 0;
			this.Load.Text = "加载图形";
			this.Load.UseVisualStyleBackColor = true;
			this.Load.Click += new System.EventHandler(this.Load_Click);
			// 
			// MainPicBox
			// 
			this.MainPicBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MainPicBox.Location = new System.Drawing.Point(15, 52);
			this.MainPicBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MainPicBox.Name = "MainPicBox";
			this.MainPicBox.Size = new System.Drawing.Size(813, 554);
			this.MainPicBox.TabIndex = 1;
			this.MainPicBox.TabStop = false;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(842, 620);
			this.Controls.Add(this.MainPicBox);
			this.Controls.Add(this.Load);
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "Form1";
			this.Text = "Form1";
			((System.ComponentModel.ISupportInitialize)(this.MainPicBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button Load;
		private System.Windows.Forms.PictureBox MainPicBox;
	}
}

