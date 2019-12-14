﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PolygonCut
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void Load_Click(object sender, EventArgs e)
		{
			string filename = @"D:\LYLUs\Desktop\ShortestPath\road2.shp";
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Multiselect = false;//该值确定是否可以选择多个文件
			dialog.Title = "请选择Shapefile文件";
			dialog.Filter = "Shapefile文件(*.shp)|*.shp";
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				filename = dialog.FileName;
				//is_ascfile_open = true;
			}
			else
			{
				filename = "";
				//is_ascfile_open = false;
			}
			PolyGons polygons = new PolyGons();
			polygons.ReadShapeFile(filename);
			polygons.DrawPolyGons(MainPicBox);
		}
	}

	public struct EdgeInfo
	{
		int ymax, ymin;//Y的上下端点
		float k, xmin;//斜率倒数和X的下端点
		public int YMax { get { return ymax; } set { ymax = value; } }
		public int YMin { get { return ymin; } set { ymin = value; } }
		public float XMin { get { return xmin; } set { xmin = value; } }
		public float K { get { return k; } set { k = value; } }
		public EdgeInfo(int x1, int y1, int x2, int y2)
		{
			ymax = y2; ymin = y1; xmin = (float)x1; k = (float)(x1 - x2) / (float)(y1 - y2);
		}
	}
	//扫描线填充算法预定义结构

	struct ShapeHeader
	{
		//高字节序
		public int FileCode;    //value: 9994
		public int Unused1;
		public int Unused2;
		public int Unused3;
		public int Unused4;
		public int Unused5;
		public int FileLength;
		//低字节序
		public int Version;     //value: 1000
		public int ShapeType;
		public double XMin;
		public double YMin;
		public double XMax;
		public double YMax;
		public double ZMin;
		public double ZMax;
		public double MMin;
		public double MMax;
	}
	//Shapefile文件标头结构

	struct CheckPoint
	{
		public int Pre;
		public int Next;
	}
	//前后检查点结构

	class Vertex
	{
		public int ID;//顶点ID
		public Vertex PreVertex;//前顶点
		public Vertex NextVertex;//后顶点
		public double X;//点坐标X
		public double Y;//点坐标Y
		public int PolygonID;//所在面ID
		public Vertex()
		{
			PreVertex = null;
			NextVertex = null;
			ID = 0;
		}
		//默认构造函数，内含前一顶点ID以及后一顶点ID，以及其所属的折线段的ID
	}
	//定义类Vertex（顶点）

	class PolyGon
	{
		public int id;//面ID
		public int RecordLength;//SHP记录长度
		public int ShapeType;//SHP图形类型
		public double xmin;//SHP内X最小值
		public double ymin;//SHP内Y最小值
		public double xmax;//SHP内X最大值
		public double ymax;//SHP内Y最大值
		public int NumOfParts;//SHP内面数量
		public int NumOfPoints;//SHP内面数量
		public int[] Parts;//SHP内每个部分所占长度
		public List<Vertex> points;//由顶点构成的集合
		public PolyGon()
		{
			points = new List<Vertex>();
		}
		//默认构造函数，将上面的points实例化
	}
	//定义类PolyGon

	class PolyGons
	{
		public int FirstX;
		public List<PolyGon> polygons;//由多个面构成的面组
		ShapeHeader MH;
		int count = 0;
		public int Count
		{
			get { return count; }
		}
		//属性Count，操作元素count，只读
		public PolyGons()
		{
			polygons = new List<PolyGon>();
			//InterSectPoints = new List<IntersectNode>();
		}
		//默认构造函数，生成由面类以及交点类构成的泛型集合
		public static UInt32 ReverseBytes(UInt32 value)
		{
			return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 | (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
		}
		//高低字节转序
		public void ReadShapeFile(string filePath)
		{
			FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			BinaryReader br = new BinaryReader(fs);
			//高字节序
			MH.FileCode = (Int32)ReverseBytes(br.ReadUInt32());
			MH.Unused1 = (Int32)ReverseBytes(br.ReadUInt32());
			MH.Unused2 = (Int32)ReverseBytes(br.ReadUInt32());
			MH.Unused3 = (Int32)ReverseBytes(br.ReadUInt32());
			MH.Unused4 = (Int32)ReverseBytes(br.ReadUInt32());
			MH.Unused5 = (Int32)ReverseBytes(br.ReadUInt32());
			MH.FileLength = (Int32)ReverseBytes(br.ReadUInt32());
			//低字节序
			MH.Version = br.ReadInt32();
			MH.ShapeType = br.ReadInt32();
			MH.XMin = br.ReadDouble();
			MH.YMin = br.ReadDouble();
			MH.XMax = br.ReadDouble();
			MH.YMax = br.ReadDouble();
			MH.ZMin = br.ReadDouble();
			MH.ZMax = br.ReadDouble();
			MH.MMin = br.ReadDouble();
			MH.MMax = br.ReadDouble();
			try
			{
				while (true)
				{
					PolyGon polyL = new PolyGon();
					//====
					polyL.id = (Int32)ReverseBytes(br.ReadUInt32());
					polyL.RecordLength = (Int32)ReverseBytes(br.ReadUInt32());
					polyL.ShapeType = br.ReadInt32();
					polyL.xmin = br.ReadDouble();
					polyL.ymin = br.ReadDouble();
					polyL.xmax = br.ReadDouble();
					polyL.ymax = br.ReadDouble();
					polyL.NumOfParts = br.ReadInt32();
					polyL.NumOfPoints = br.ReadInt32();
					//====
					polyL.Parts = new int[polyL.NumOfParts];//长度为NumOfParts的数组，即线的数量
					for (int i = 0; i < polyL.NumOfParts; i++)
					{
						polyL.Parts[i] = br.ReadInt32();
					}
					//面首点数组索引位
					Vertex PreVer = null;
					Vertex VertexFirst = new Vertex();
					//====
					VertexFirst.ID = 1;
					VertexFirst.PolygonID = polyL.id;
					VertexFirst.PreVertex = null;
					VertexFirst.NextVertex = null;
					VertexFirst.X = br.ReadDouble();
					VertexFirst.Y = br.ReadDouble();
					PreVer = VertexFirst;
					//====
					polyL.points.Add(VertexFirst);
					for (int i = 1; i < polyL.NumOfPoints - 1; i++)
					{
						Vertex VertexMid = new Vertex();
						//====
						VertexMid.PreVertex = PreVer;
						VertexMid.PreVertex.NextVertex = VertexMid;
						VertexMid.ID = PreVer.ID + 1;
						VertexMid.PolygonID = polyL.id;
						VertexMid.X = br.ReadDouble();
						VertexMid.Y = br.ReadDouble();
						VertexMid.NextVertex = null;
						PreVer = VertexMid;
						//====
						polyL.points.Add(VertexMid);
					}
					Vertex VetLast = new Vertex();
					//====
					VetLast.PreVertex = PreVer;
					PreVer.NextVertex = VetLast;
					VetLast.ID = VetLast.PreVertex.ID + 1;
					VetLast.PolygonID = polyL.id;
					VetLast.NextVertex = null;
					VetLast.X = br.ReadDouble();
					VetLast.Y = br.ReadDouble();
					//====
					polyL.points.Add(VetLast);
					polygons.Add(polyL);
					count++;
				}
			}
			catch (EndOfStreamException e)
			{
				//MessageBox.Show(e.ToString());
				//MessageBox.Show("Ending of File");				
			}
		}
		//读取SHP文件内容
		public void DrawPolyGons(PictureBox pb)
		{
			Graphics g = pb.CreateGraphics();
			g.Clear(Color.Gray);
			foreach (PolyGon pl in polygons)
			{
				Pen penDrawLine = new Pen(Color.Red, 2);
				Pen penDrawPolygon = new Pen(Color.Green, 2);
				PointF[] pointsF = new PointF[pl.points.Count];
				for (int i = 0; i < pointsF.Length; i++)
				{
					pointsF[i].X = (float)(pl.points[i].X - (float)MH.XMin) / ((float)MH.XMax - (float)MH.XMin) * pb.Width;
					pointsF[i].Y = (float)(pb.Height - (pl.points[i].Y - (float)MH.YMin) / ((float)MH.YMax - (float)MH.YMin) * pb.Height);
				}
				g.DrawLines(penDrawLine, pointsF);

				
				EdgeInfo[] edgelist = new EdgeInfo[100];
				int j = 0, yu = 0, yd = 1024;//需修改为图框上部
				for (int i = 0; i < pointsF.Length - 1; i++)
				{
					if (pointsF[i].Y > yu) yu = (int)pointsF[i].Y;
					if (pointsF[i].Y < yd) yd = (int)pointsF[i].Y;
					if (pointsF[i].Y != pointsF[i + 1].Y)
					{
						if (pointsF[i].Y > pointsF[i + 1].Y)
						{
							edgelist[j++] = new EdgeInfo((int)pointsF[i + 1].X, (int)pointsF[i + 1].Y, (int)pointsF[i].X, (int)pointsF[i].Y);
						}
						else
						{
							edgelist[j++] = new EdgeInfo((int)pointsF[i].X, (int)pointsF[i].Y, (int)pointsF[i + 1].X, (int)pointsF[i + 1].Y);
						}
					}
				}
				for (int y = yd; y < yu; y++)
				{
					//AEL
					var sorted = from item in edgelist
								 where y < item.YMax && y >= item.YMin
								 orderby item.XMin, item.K
								 select item;
					int flag = 0;
					foreach (var item in sorted)
					{
						//FirstX = 0;
						if (flag == 0)
						{
							FirstX = (int)(item.XMin + 0.5);
							flag++;
						}
						else
						{
							g.DrawLine(penDrawPolygon, (int)(item.XMin + 0.5), y, FirstX + 2, y);
							//g.DrawLine(penDrawPolygon, (int)(item.XMin + 0.5), y, FirstX - 1, y);
							flag = 0;
						}
					}
					for (int i = 0; i < j; i++)
					{
						if (y < edgelist[i].YMax - 1 && y > edgelist[i].YMin)
						{
							edgelist[i].XMin += edgelist[i].K;
						}
					}
				}
			}		
		}
	}
}