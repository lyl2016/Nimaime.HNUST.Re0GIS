using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PolygonCut
{
	/// <summary>
	/// 注释名词规范
	/// 碰撞箱：线段、多边形的矩形范围框
	/// </summary>
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void File_Open_Polygon_Click(object sender, EventArgs e)
		{
			bool is_ascfile_open = false;
			while (is_ascfile_open == false)
			{
				string filename;
				OpenFileDialog dialog = new OpenFileDialog();
				dialog.Multiselect = false;//该值确定是否可以选择多个文件
				dialog.Title = "请选择Shapefile文件";
				dialog.Filter = "Shapefile文件(*.shp)|*.shp";
				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					filename = dialog.FileName;
					is_ascfile_open = true;
					PolyGons polygons = new PolyGons();
					polygons.ReadShapeFile(filename);
					polygons.DrawPolyGons(MainPicBox);
					polygons.CheckLineInPolygonsInse();
					polygons.DrawInsePoints(MainPicBox);
				}
				else
				{
					filename = "";
					is_ascfile_open = false;
				}
			}
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
		//默认构造函数，内含前一顶点ID以及后一顶点ID，以及构造时的初始ID
	}
	//定义类Vertex（顶点）

/*	class BasicLine
	{
	//	public int ID_line;					//线段ID
		public int PolygonID;				//线段所属面ID
		public PointF point1;				//顶点1
		public PointF point2;				//顶点2
		public BasicLine()
		{

		}
	}
	//定义直线用于交点检查
*/
	class PolyGon
	{
		public int id;						//面ID
		public int RecordLength;			//SHP记录长度
		public int ShapeType;				//SHP图形类型
		public double xmin;					//SHP内X最小值
		public double ymin;					//SHP内Y最小值
		public double xmax;					//SHP内X最大值
		public double ymax;					//SHP内Y最大值
		public int NumOfParts;				//SHP内面数量
		public int NumOfPoints;				//SHP内点数量
		public int[] Parts;					//SHP内每个部分索引数组
		public List<Vertex> points;			//由顶点构成的集合
	//	public List<BasicLine> lines;		//由多边形基础直线构成的集合
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
		public List<PointF> inse_points = new List<PointF>();
		ShapeHeader MH;
		int count = 0;
		int loop_time = 0;
		int check_time = 0;
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
					PolyGon polyG = new PolyGon();
					//====
					polyG.id = (Int32)ReverseBytes(br.ReadUInt32());
					polyG.RecordLength = (Int32)ReverseBytes(br.ReadUInt32());
					polyG.ShapeType = br.ReadInt32();
					polyG.xmin = br.ReadDouble();
					polyG.ymin = br.ReadDouble();
					polyG.xmax = br.ReadDouble();
					polyG.ymax = br.ReadDouble();
					polyG.NumOfParts = br.ReadInt32();
					polyG.NumOfPoints = br.ReadInt32();
					//====
					polyG.Parts = new int[polyG.NumOfParts];//长度为NumOfParts的数组，即线的数量
					for (int i = 0; i < polyG.NumOfParts; i++)
					{
						polyG.Parts[i] = br.ReadInt32();
					}
					//面首点数组索引位
					Vertex PreVer = null;
					Vertex VertexFirst = new Vertex();
					//====
					VertexFirst.ID = 1;
					VertexFirst.PolygonID = polyG.id;
					VertexFirst.PreVertex = null;
					VertexFirst.NextVertex = null;
					VertexFirst.X = br.ReadDouble();
					VertexFirst.Y = br.ReadDouble();
					PreVer = VertexFirst;
					//====
					polyG.points.Add(VertexFirst);
					for (int i = 1; i < polyG.NumOfPoints - 1; i++)
					{
						Vertex VertexMid = new Vertex();
						//====
						VertexMid.PreVertex = PreVer;
						VertexMid.PreVertex.NextVertex = VertexMid;
						VertexMid.ID = PreVer.ID + 1;
						VertexMid.PolygonID = polyG.id;
						VertexMid.X = br.ReadDouble();
						VertexMid.Y = br.ReadDouble();
						VertexMid.NextVertex = null;
						PreVer = VertexMid;
						//====
						polyG.points.Add(VertexMid);
					}
					Vertex VetLast = new Vertex();
					//====
					VetLast.PreVertex = PreVer;
					PreVer.NextVertex = VetLast;
					VetLast.ID = VetLast.PreVertex.ID + 1;
					VetLast.PolygonID = polyG.id;
					VetLast.NextVertex = null;
					VetLast.X = br.ReadDouble();
					VetLast.Y = br.ReadDouble();
					//====
					polyG.points.Add(VetLast);
					polygons.Add(polyG);
					count++;
				}
			}
			catch (EndOfStreamException e)
			{
				Console.WriteLine(e.ToString());
				Console.WriteLine("文件读取完毕");				
			}
		}
		//读取SHP文件内容
		public void DrawPolyGons(PictureBox pb)
		{
			Graphics g = pb.CreateGraphics();
			g.Clear(Color.Gray);
			foreach (PolyGon pg in polygons)
			{
				int iSeed = 10;
				Random ro = new Random(iSeed);
				long tick = DateTime.Now.Ticks;
				Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
				int R = ran.Next(255);
				int G = ran.Next(255);
				int B = ran.Next(255);
				B = (R + G > 400) ? R + G - 400 : B;//0 : 380 - R - G;
				B = (B > 255) ? 255 : B;
				//生成随机颜色

				Pen penDrawLine = new Pen(Color.Red, 2);
				Pen penDrawPolygon = new Pen(Color.FromArgb(R, G, B), 2);
				PointF[] pointsF = new PointF[pg.points.Count];
				for (int i = 0; i < pointsF.Length; i++)
				{
					pointsF[i].X = (float)((pg.points[i].X - MH.XMin) / (MH.XMax - MH.XMin) * pb.Width * 3.0 / 4.0 + 1.0 / 8.0 * pb.Width);
					pointsF[i].Y = (float)(pb.Height - (pg.points[i].Y - MH.YMin) / (MH.YMax - MH.YMin) * pb.Height * 3.0 / 4.0 - 1.0 / 8.0 * pb.Height);
				//	Console.WriteLine(pointsF[i].X + " " + pointsF[i].Y);
				}
				g.DrawLines(penDrawLine, pointsF);
				//绘制多边形面

				EdgeInfo[] edgelist = new EdgeInfo[100];
				int j = 0, yu = 0, yd = 1024;
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
				//扫描线填充
			}
		}
		//绘制多边形（边加粗，内部填充）
		public void DrawInsePoints(PictureBox pb)
		{
			Graphics g = pb.CreateGraphics();
			//g.Clear(Color.Gray);
			Pen draw_point = new Pen(Color.Red, 1);
			Brush brush = new SolidBrush(Color.LightGreen);
			PointF[] pointsF = new PointF[inse_points.Count];
			for (int i = 0; i < pointsF.Length; i++)
			{
				pointsF[i].X = (float)((inse_points[i].X - MH.XMin) / (MH.XMax - MH.XMin) * pb.Width * 3.0 / 4.0 + 1.0 / 8.0 * pb.Width);
				pointsF[i].Y = (float)(pb.Height - (inse_points[i].Y - MH.YMin) / (MH.YMax - MH.YMin) * pb.Height * 3.0 / 4.0 - 1.0 / 8.0 * pb.Height);
				g.DrawEllipse(draw_point, pointsF[i].X, pointsF[i].Y, 5, 5);
				g.FillEllipse(brush, pointsF[i].X, pointsF[i].Y, 5, 5);
				//	Console.WriteLine(pointsF[i].X + " " + pointsF[i].Y);
			}
			//g.DrawLines(draw_point, pointsF);
		}
		//绘制交点
		public void CheckLineInPolygonsInse()
		{
			bool Inse;
			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();
			for (int front = 0; front <= polygons.Count - 2; front++)
			{
				for (int rare = front + 1; rare <= polygons.Count - 1; rare++)
				{
					PointF left_up = new PointF((float)polygons[front].xmin, (float)polygons[front].ymax);
					PointF right_up = new PointF((float)polygons[front].xmax, (float)polygons[front].ymax);
					PointF left_down = new PointF((float)polygons[front].xmin, (float)polygons[front].ymin);
					PointF right_down = new PointF((float)polygons[front].xmax, (float)polygons[front].ymin);
					//多边形front的对角线
					if (polygons[front].ymax < polygons[rare].ymin
						|| polygons[front].xmax < polygons[rare].xmin
						|| polygons[rare].ymax < polygons[front].ymin
						|| polygons[rare].xmax < polygons[front].xmin)
					{
						Console.WriteLine("多边形{0}与多边形{1}不可能相交", front, rare);
						loop_time++;
					}
					//多边形碰撞箱检查，不相交
					else
					{
						Console.WriteLine("多边形{0}与多边形{1}可能相交", front, rare);
						string blacklist_j = "";
						for (int i = 0; i < polygons[front].points.Count - 1; i++)
						{
							for (int j = 0; j < polygons[rare].points.Count - 1; j++)
							{
								loop_time++;
								if (blacklist_j.Contains(Convert.ToString(j)))
								{
									Console.WriteLine("边{0}跳过", j);
									continue;
								}
								Console.WriteLine("开始检查多边形{0}的边{1}与多边形{2}的边{3}", front, i, rare, j);
								PointF point1a = new PointF((float)polygons[front].points[i].X, (float)polygons[front].points[i].Y);
								PointF point1b = new PointF((float)polygons[front].points[i + 1].X, (float)polygons[front].points[i + 1].Y);
								PointF point2a = new PointF((float)polygons[rare].points[j].X, (float)polygons[rare].points[j].Y);
								PointF point2b = new PointF((float)polygons[rare].points[j + 1].X, (float)polygons[rare].points[j + 1].Y);
								if ((point2a.X > polygons[front].xmin && point2a.X < polygons[front].xmax &&
									point2a.Y > polygons[front].ymin && point2a.Y < polygons[front].ymax) ||
									//多边形rare的检查边点A在碰撞箱内
									(point2b.X > polygons[front].xmin && point2b.X < polygons[front].xmax &&
									point2b.Y > polygons[front].ymin && point2b.Y < polygons[front].ymax) ||
									//多边形rare的检查边点B在碰撞箱内
									(CheckIfInse(point2a, point2b, left_up, right_down) ||
									//多边形rare的检查边与碰撞箱的＼对角线有交点
									CheckIfInse(point2a, point2b, right_up, left_down))
									//多边形rare的检查边与碰撞箱的／对角线有交点
									)
								{
									Inse = CheckIfInse(point1a, point1b, point2a, point2b);
									if (Inse)
									{
										Console.WriteLine("多边形{0}的边{1}与多边形{2}的边{3}相交", front, i, rare, j);
										inse_points.Add(CalInsePoint(point1a, point1b, point2a, point2b));
									}
									else
									{
										Console.WriteLine("多边形{0}的边{1}与多边形{2}的边{3}不相交", front, i, rare, j);
									}
								}
								else
								{
									Console.WriteLine("多边形{0}的边{1}与多边形{2}的边{3}不相交", front, i, rare, j);
									Console.WriteLine("边{0}已加入多边形{1}的黑名单", j, front);
									blacklist_j += Convert.ToString(j);
								}
							}
						}
						//检查所有其它边（不包括最后一条边）
						for(int j = 0; j < polygons[rare].points.Count - 1; j++)
						{
							loop_time++;
							Console.WriteLine("开始检查多边形{0}的边{1}与多边形{2}的边{3}", front, polygons[front].points.Count - 1, rare, j);
							PointF point1a = new PointF((float)polygons[front].points[polygons[front].points.Count - 1].X, (float)polygons[front].points[0].Y);
							PointF point1b = new PointF((float)polygons[front].points[0].X, (float)polygons[front].points[0].Y);
							PointF point2a = new PointF((float)polygons[rare].points[j].X, (float)polygons[rare].points[j].Y);
							PointF point2b = new PointF((float)polygons[rare].points[j + 1].X, (float)polygons[rare].points[j + 1].Y);
							if ((point2a.X > polygons[front].xmin && point2a.X < polygons[front].xmax &&
								point2a.Y > polygons[front].ymin && point2a.Y < polygons[front].ymax) ||
								//多边形rare的检查边点A在碰撞箱内
								(point2b.X > polygons[front].xmin && point2b.X < polygons[front].xmax &&
								point2b.Y > polygons[front].ymin && point2b.Y < polygons[front].ymax) ||
								//多边形rare的检查边点B在碰撞箱内
								(CheckIfInse(point2a, point2b, left_up, right_down) ||
								//多边形rare的检查边与碰撞箱的＼对角线有交点
								CheckIfInse(point2a, point2b, right_up, left_down))
								//多边形rare的检查边与碰撞箱的／对角线有交点
								)
							{
								Inse = CheckIfInse(point1a, point1b, point2a, point2b);
								if (Inse)
								{
									Console.WriteLine("多边形{0}的边{1}与多边形{2}的边{3}相交", front, polygons[front].points.Count - 1, rare, j);
									inse_points.Add(CalInsePoint(point1a, point1b, point2a, point2b));
								}
								else
								{
									Console.WriteLine("多边形{0}的边{1}与多边形{2}的边{3}不相交", front, polygons[front].points.Count - 1, rare, j);
								}
							}
							else
							{
								Console.WriteLine("多边形{0}的边{1}与多边形{2}的边{3}不相交", front, polygons[front].points.Count - 1, rare, j);
							}
						}
						//检查front的最后一边与rare的其它边
						Console.WriteLine("开始检查多边形{0}的边{1}与多边形{2}的边{3}", front, polygons[front].points.Count - 1, rare, polygons[rare].points.Count - 1);
						PointF p1a = new PointF((float)polygons[front].points[polygons[front].points.Count - 1].X, (float)polygons[front].points[0].Y);
						PointF p1b = new PointF((float)polygons[front].points[0].X, (float)polygons[front].points[0].Y);
						PointF p2a = new PointF((float)polygons[rare].points[polygons[rare].points.Count - 1].X, (float)polygons[rare].points[polygons[rare].points.Count - 1].Y);
						PointF p2b = new PointF((float)polygons[rare].points[0].X, (float)polygons[rare].points[0].Y);
						if (p2a.X > polygons[front].xmin && p2a.X < polygons[front].xmax &&
							p2a.Y > polygons[front].ymin && p2a.Y < polygons[front].ymax ||
							//多边形rare的检查边点A在碰撞箱内
							p2b.X > polygons[front].xmin && p2b.X < polygons[front].xmax &&
							p2b.Y > polygons[front].ymin && p2b.Y < polygons[front].ymax ||
							//多边形rare的检查边点B在碰撞箱内
							CheckIfInse(p2a, p2b, left_up, right_down) ||
							//多边形rare的检查边与碰撞箱的＼对角线有交点
							CheckIfInse(p2a, p2b, right_up, left_down)
							//多边形rare的检查边与碰撞箱的／对角线有交点
							)
						{
							Inse = CheckIfInse(p1a, p1b, p2a, p2b);
							if (Inse)
							{
								Console.WriteLine("多边形{0}的边{1}与多边形{2}的边{3}相交", front, polygons[front].points.Count - 1, rare, polygons[rare].points.Count - 1);
								inse_points.Add(CalInsePoint(p1a, p1b, p2a, p2b));
							}
							else
							{
								Console.WriteLine("多边形{0}的边{1}与多边形{2}的边{3}不相交", front, polygons[front].points.Count - 1, rare, polygons[rare].points.Count - 1);
							}
						}
						else
						{
							Console.WriteLine("多边形{0}的边{1}与多边形{2}的边{3}不相交", front, polygons[front].points.Count - 1, rare, polygons[rare].points.Count - 1);
						}
						//单独检查两个面的最后一条边是否相交
					}
					///碰撞箱检查得交，检查线交点
					///使用碰撞箱检查，减少循环量
				}
			}
			Console.WriteLine("循环次数：{0}，检查次数{1}", loop_time, check_time);
			stopWatch.Stop();
			// Get the elapsed time as a TimeSpan value.
			TimeSpan ts = stopWatch.Elapsed;
			// Format and display the TimeSpan value.
			string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
			Console.WriteLine("RunTime " + elapsedTime);
			///每次检查两个多边形
			///首先检查他们的碰撞箱，假如碰撞箱不交则跳过（节省大部分时间）
			///然后具体检查线段相交
			///当多边形rare的一条直线不在front内的时候跳过行列式运算，并判定不相交，该直线列入front的黑名单
		}
		//检查多边形内直线相交
		private double Determinant(double v1, double v2, double v3, double v4) 
		{
			return (v1 * v3 - v2 * v4);
		}
		//计算行列式
		private bool CheckIfInse(PointF a1, PointF a2, PointF b1, PointF b2)
		{
			check_time++;
			double delta = Determinant(a2.X - a1.X, b2.X - b1.X, b2.Y - b1.Y, a2.Y - a1.Y);
			if (delta <= (1e-6) && delta >= -(1e-6))  // delta = 0，表示两线段重合或平行  
			{
				return false;
			}
			double lamda = Determinant(b2.X - b1.X, a1.X - b1.X, a1.Y - b1.Y, b2.Y - b1.Y) / delta;
			if (lamda > 1 || lamda < 0)
			{
				return false;
			}
			double myu = Determinant(a2.X - a1.X, a1.X - b1.X, a1.Y - b1.Y, a2.Y - a1.Y) / delta;
			if (myu > 1 || myu < 0)
			{
				return false;
			}
			return true;
		}
		//通过两点坐标判断直线是否相交
		private PointF CalInsePoint(PointF a1, PointF a2, PointF b1, PointF b2)
		{
			PointF inse_point = new PointF();
			if (a1.X - a2.X > -(1e-6) && a1.X - a2.X < 1e-6)
			{
				inse_point.X = a1.X;
				inse_point.Y = (a1.X - b1.X) / (b1.X - b2.X) * (b1.Y - b2.Y) + b1.Y;
			}
			//直线A垂直X坐标轴
			else if (b1.X - b2.X > -(1e-6) && b1.X - b2.X < 1e-6)
			{
				inse_point.X = b1.X;
				inse_point.Y = (b1.X - a1.X) / (a1.X - a2.X) * (a1.Y - a2.Y) + a1.Y;
			}
			//直线B垂直X坐标轴
			else if (a1.Y - a2.Y > -(1e-6) && a1.Y - a2.Y < 1e-6)
			{
				inse_point.Y = a1.Y;
				inse_point.X = (a1.Y - b1.Y) / (b1.Y - b2.Y) * (b1.X - b2.X) + b1.Y;
			}
			//直线A平行X坐标轴，大概是废话
			else if (b1.Y - b2.Y > -(1e-6) && b1.Y - b2.Y < 1e-6)
			{
				inse_point.Y = b1.Y;
				inse_point.X = (b1.Y - a1.Y) / (a1.Y - a2.Y) * (a1.X - a2.X) + a1.Y;
			}
			//直线B平行X坐标轴，大概是废话
			else
			{
				inse_point.X = ((a1.Y - b1.Y) + (b1.Y - b2.Y) / (b1.X - b2.X) * b1.X - (a1.Y - a2.Y) / (a1.X - a2.X) * a1.X) / ((b1.Y - b2.Y) / (b1.X - b2.X) - (a1.Y - a2.Y) / (a1.X - a2.X));
				inse_point.Y = (inse_point.X - a1.X) / (a1.X - a2.X) * (a1.Y - a2.Y) + a1.Y;
			}
			//其他一般情况
			return inse_point;
		}
	}
}
