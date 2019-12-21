using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Re0GIS
{
	/// <summary>
	/// 注释名词统一
	/// 碰撞箱				线段、多边形的矩形范围框
	/// 
	/// 英文译名统一
	/// vertex				顶点
	/// points				点集
	/// polyline			折线
	/// polygon				多边形
	/// rectangle			矩形
	/// header				文件标头
	/// inse				intersect 相交
	/// inname				mark the IDs of some polygons that the mouse point is in
	/// </summary>
	public partial class Form1 : Form
	{
		//====
		// 版本显示相关
		//====
		AssemblyName assName = Assembly.GetExecutingAssembly().GetName();
		public string ver;
		///标记版本号
		///版本号格式：主版本号.子版本号[.编译版本号[.修正版本号]]
		///this string may be using in various ways
		const string channel = "A";
		///预计会使用到的通道
		///Channel			Cname
		///Early Preview	E		早期测试版
		///Alpha			A		内部测试版
		///Beta				B		外部测试版
		///Demo				D		演示版
		///Release			R		发行版
		///

		string filepath_polyline;//represent a path of a polyline shapefile
		string filepath_polygon;//represent a path of a polygon shapefile
		private int FirstX;//用于扫描线填充【待实现】
		public bool is_polylineshp_open = false;//mark the status that a polyline file open
		public bool is_polygonshp_open = false;//mark the status that a polygon file open
		PolyLines polylines;//用于Form1内调用
		PolyGons polygons;//用于Form1内调用
		public bool inname_form = true;///mark the method to represent of coordinate system
		///true for screen coordinate, false for the coordinate of shapefile

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			ver = assName.Version.ToString() + "_" + channel;
			VerString.Text = "版本号：" + ver;
			///load the version string while form load
		}

		private void File_Open_PolyLine_Click(object sender, EventArgs e)
		{
			is_polylineshp_open = false;
			Stopwatch stopWatch = new Stopwatch();
			if (is_polylineshp_open == false)
			{
				OpenFileDialog dialog = new OpenFileDialog
				{
					Multiselect = false,//该值确定是否可以选择多个文件
					Title = "请选择Shapefile文件",
					Filter = "Shapefile文件(*.shp)|*.shp"
				};
				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					filepath_polyline = dialog.FileName;
					is_polylineshp_open = true;
					polylines = new PolyLines();
					polylines.ReadShapeFile(filepath_polyline);
					if (polylines.MH.ShapeType == 3)
					{
						polylines.DrawPolyLines(MainPicBox);
						stopWatch.Start();
						for (int i = 0; i < polylines.polylines.Count; i++)
						{
							for (int j = i + 1; j < polylines.polylines.Count; j++)
							{
								polylines.CalIntersect(polylines.polylines[i], polylines.polylines[j]);
							}
						}
						stopWatch.Stop();
						polylines.DrawInterSectPoints(MainPicBox);
						// Get the elapsed time as a TimeSpan value.
						TimeSpan ts = stopWatch.Elapsed;
						// Format and display the TimeSpan value.
						string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
						Console.WriteLine("运行用时：" + elapsedTime);
						VerString.Text = "运行用时：" + elapsedTime + "，共绘制了" + polylines.InterSectPoints.Count + "个交点";
					}
					else
					{
						DialogResult retry_open_polyline = MessageBox.Show("你选择的文件不是折线文件", "请重试", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
					}
				}
				else
				{
					filepath_polygon = "";
					is_polygonshp_open = false;
				}
			}
			is_polygonshp_open = false;
		}

		private void File_Open_Polygon_Click(object sender, EventArgs e)
		{
			is_polygonshp_open = false;
			Stopwatch stopWatch = new Stopwatch();
			if (is_polygonshp_open == false)
			{
				OpenFileDialog dialog = new OpenFileDialog
				{
					Multiselect = false,//该值确定是否可以选择多个文件
					Title = "请选择Shapefile文件",
					Filter = "Shapefile文件(*.shp)|*.shp"
				};
				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					filepath_polygon = dialog.FileName;
					is_polygonshp_open = true;
					polygons = new PolyGons();
					polygons.ReadShapeFile(filepath_polygon);
					if(polygons.MH.ShapeType == 5)
					{
						polygons.DrawPolyGons(MainPicBox);
						stopWatch.Start();
						polygons.CheckLineInPolygonsInse();
						stopWatch.Stop();
						polygons.DrawInsePoints(MainPicBox);
						// Get the elapsed time as a TimeSpan value.
						TimeSpan ts = stopWatch.Elapsed;
						// Format and display the TimeSpan value.
						string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
						//Console.WriteLine("运行用时：" + elapsedTime);
						VerString.Text = "运行用时：" + elapsedTime + "，共绘制了" + polygons.inse_points.Count + "个交点";
					}
					else
					{
						DialogResult retry_open_polygon = MessageBox.Show("你选择的文件不是多边形文件", "请重试", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
					}
				}
				else
				{
					filepath_polygon = "";
					is_polygonshp_open = false;
				}
			}
			is_polylineshp_open = false;
		}

		private void MainPicBox_MouseMove(object sender, MouseEventArgs e)
		{
			if (is_polylineshp_open)
			{
				PointF position = new PointF(e.X, e.Y);
				position.X = (float)((position.X / MainPicBox.Width) * (polylines.MH.XMax - polylines.MH.XMin) + polylines.MH.XMin);
				position.Y = (float)((position.Y - MainPicBox.Height) / (-MainPicBox.Height) * (polylines.MH.YMax - polylines.MH.YMin) + polylines.MH.YMin);
				if (inname_form)
					inname.Text = "屏幕坐标(" + e.X.ToString() + "," + e.Y.ToString() + ")，点击文字切换坐标";
				else
					inname.Text = "地图坐标(" + position.X.ToString() + "," + position.Y.ToString() + ")，点击文字切换坐标";
			}

			else if (is_polygonshp_open)
			{
				PointF position = new PointF(e.X, e.Y);
				position.X = (float)((position.X / MainPicBox.Width) * (polygons.MH.XMax - polygons.MH.XMin) + polygons.MH.XMin);
				position.Y = (float)((position.Y - MainPicBox.Height) / (-MainPicBox.Height) * (polygons.MH.YMax - polygons.MH.YMin) + polygons.MH.YMin);
				polygons.IsPointInPolygon(position);
				if (inname_form)
					inname.Text = "屏幕坐标(" + e.X.ToString() + "," + e.Y.ToString() + ")在多边形ID：" + polygons.inname + "内，点击文字切换坐标";
				else
					inname.Text = "地图坐标(" + position.X.ToString() + "," + position.Y.ToString() + ")在多边形ID：" + polygons.inname + "内，点击文字切换坐标";
			}

			else
			{
				inname.Text = "屏幕坐标(" + e.X.ToString() + "," + e.Y.ToString() + ")";
			}
		}

		private void inname_Click(object sender, EventArgs e)
		{
			inname_form = !inname_form;
		}
	}

	struct EdgeInfo
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
		/// 0	Null Shape
		/// 1	Point
		/// 3	PolyLine
		/// 5	Polygon
		/// 8	MultiPoint
		/// 11	PointZ
		/// 13	PolyLineZ
		/// 15	PolygonZ
		/// 18	MultiPointZ
		/// 21	PointM
		/// 23	PolyLineM
		/// 25	PolygonM
		/// 28	MultiPointM
		/// 31	MultiPatch
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

	class VertexPolyLine
	{
		public int ID;//顶点ID
		public VertexPolyLine PreVertex;//前顶点
		public VertexPolyLine NextVertex;//后顶点
		public double X;//点坐标X
		public double Y;//点坐标Y
		public int LineID;//所在折线段ID
		public VertexPolyLine()
		{
			PreVertex = null;
			NextVertex = null;
			ID = 0;
		}
		//默认构造函数，内含前一顶点ID以及后一顶点ID，以及其所属的折线段的ID
	}
	//定义类Vertex（顶点）

	class PolyLine
	{
		public int id;//折线段ID
		public int RecordLength;//SHP记录长度
		public int ShapeType;//SHP图形类型
		public double xmin;//SHP内X最小值
		public double ymin;//SHP内Y最小值
		public double xmax;//SHP内X最大值
		public double ymax;//SHP内Y最大值
		public int NumOfParts;//SHP内线数量
		public int NumOfPoints;//SHP内点数量
		public int[] Parts;//SHP内每个部分所占长度
		public List<VertexPolyLine> points;//由顶点构成的集合
		public PolyLine()
		{
			points = new List<VertexPolyLine>();
		}
		//默认构造函数，将上面的points实例化
	}
	//定义类PolyLine

	class PolyLines
	{
		public List<PolyLine> polylines;//由多条折线段构成的折线组
		public static int NumOfInterSects = 0;//设置交点计数，初值为零，设置静态用于计数
		public List<IntersectNode> InterSectPoints;//由多个交点构成的交点组
		//public List<PointF> inse_points = new List<PointF>();
		public ShapeHeader MH;
		int count = 0;
		public int Count
		{
			get { return count; }
		}
		//属性Count，操作元素count，只读
		public PolyLines()
		{
			polylines = new List<PolyLine>();
			InterSectPoints = new List<IntersectNode>();
		}
		//默认构造函数，生成由折线类以及交点类构成的泛型集合
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
					PolyLine polyL = new PolyLine();
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
					//折线段首点数组索引位
					VertexPolyLine PreVer = null;
					VertexPolyLine VertexFirst = new VertexPolyLine();
					//====
					VertexFirst.ID = 1;
					VertexFirst.LineID = polyL.id;
					VertexFirst.PreVertex = null;
					VertexFirst.NextVertex = null;
					VertexFirst.X = br.ReadDouble();
					VertexFirst.Y = br.ReadDouble();
					PreVer = VertexFirst;
					//====
					polyL.points.Add(VertexFirst);
					for (int i = 1; i < polyL.NumOfPoints - 1; i++)
					{
						VertexPolyLine VertexMid = new VertexPolyLine();
						//====
						VertexMid.PreVertex = PreVer;
						VertexMid.PreVertex.NextVertex = VertexMid;
						VertexMid.ID = PreVer.ID + 1;
						VertexMid.LineID = polyL.id;
						VertexMid.X = br.ReadDouble();
						VertexMid.Y = br.ReadDouble();
						VertexMid.NextVertex = null;
						PreVer = VertexMid;
						//====
						polyL.points.Add(VertexMid);
					}
					VertexPolyLine VetLast = new VertexPolyLine();
					//====
					VetLast.PreVertex = PreVer;
					PreVer.NextVertex = VetLast;
					VetLast.ID = VetLast.PreVertex.ID + 1;
					VetLast.LineID = polyL.id;
					VetLast.NextVertex = null;
					VetLast.X = br.ReadDouble();
					VetLast.Y = br.ReadDouble();
					//====
					polyL.points.Add(VetLast);
					polylines.Add(polyL);
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
		public void CalIntersect(PolyLine line1, PolyLine line2)
		{
			string sortedX = "";
			int[,] CheckedLines = new int[line1.points.Count, line2.points.Count];
			for (int i = 0; i < line1.points.Count; i++)
			{
				for (int j = 0; j < line2.points.Count; j++)
				{
					CheckedLines[i, j] = 0;
				}
			}
			List<VertexPolyLine> SortedVert = new List<VertexPolyLine>
			{
				line1.points[0]
			};
			int PreJ = 0;
			for (int i = 1; i < line1.points.Count; i++)
			{
				int sorted = 0;
				int j = PreJ;
				if (line1.points[i].X <= SortedVert[j].X)
				{
					while (sorted == 0)
					{
						if (j == 0)
						{
							sorted = 1;
							PreJ = j;
						}
						else if (line1.points[i].X > SortedVert[j - 1].X)
						{
							PreJ = j;
							sorted = 1;
						}
						else
						{
							j -= 1;
						}
					}
					SortedVert.Insert(PreJ, line1.points[i]);
				}
				else
				{
					while (sorted == 0)
					{
						if (j == SortedVert.Count - 1)
						{
							sorted = 1;
							PreJ = j + 1;
						}
						else if (line1.points[i].X < SortedVert[j + 1].X)
						{

							PreJ = j + 1;
							sorted = 1;
						}
						else
						{
							j += 1;
						}
					}
					SortedVert.Insert(PreJ, line1.points[i]);
				}
			}
			for (int i = 0; i < line2.points.Count; i++)
			{
				int sorted = 0;
				int j = PreJ;
				if (line2.points[i].X <= SortedVert[j].X)
				{
					while (sorted == 0)
					{
						if (j == 0)
						{
							sorted = 1;
							PreJ = j;
						}
						else if (line2.points[i].X > SortedVert[j - 1].X)
						{
							PreJ = j;
							sorted = 1;
						}
						else
						{
							j -= 1;
						}
					}
					SortedVert.Insert(PreJ, line2.points[i]);
				}
				else
				{
					while (sorted == 0)
					{
						if (j == SortedVert.Count - 1)
						{
							sorted = 1;
							PreJ = j + 1;
						}
						else if (line2.points[i].X < SortedVert[j + 1].X)
						{

							PreJ = j + 1;
							sorted = 1;
						}
						else
						{
							j += 1;
						}
					}
					SortedVert.Insert(PreJ, line2.points[i]);
				}
			}
			sortedX = "";
			for (int ii = 0; ii < SortedVert.Count; ii++)
			{
				sortedX = sortedX + ii.ToString() + ")" + SortedVert[ii].LineID.ToString() + "->" + SortedVert[ii].ID + ":" + SortedVert[ii].X.ToString() + "\n";
			}
			//MessageBox.Show(sortedX);
			//MessageBox.Show(l1.points.Count.ToString());
			CheckPoint[] l1Points = new CheckPoint[line1.points.Count];
			CheckPoint[] l2Points = new CheckPoint[line2.points.Count];
			for (int i = 0; i < line1.points.Count; i++)
			{
				l1Points[i].Pre = 0;
				l1Points[i].Next = 0;
			}
			for (int i = 0; i < line2.points.Count; i++)
			{
				l2Points[i].Pre = 0;
				l2Points[i].Next = 0;
			}
			for (int i = 0; i < SortedVert.Count; i++)
			{
				if (SortedVert[i].LineID == line1.id)
				{
					if (SortedVert[i].PreVertex != null & l1Points[SortedVert[i].ID - 1].Pre != 1)
					{
						int PointLoc = 0;
						for (int jj = i; jj < SortedVert.Count; jj++)
						{
							if (SortedVert[i].PreVertex.ID == SortedVert[jj].ID & SortedVert[jj].LineID == line1.id)
							{
								PointLoc = jj;
								l1Points[SortedVert[i].ID - 1].Pre = 1;
								l1Points[SortedVert[jj].ID - 1].Next = 1;
								break;
							}
						}
						for (int jj = i; jj <= PointLoc; jj++)
						{
							if (SortedVert[jj].LineID == line2.id)
							{
								if (SortedVert[jj].PreVertex != null)
								{
									//MessageBox.Show("L1:"+SortedVert[PointLoc].ID.ToString()+"->"+ SortedVert[i].ID.ToString()+"\nL2:"+ SortedVert[jj].PreVertex.ID.ToString() + "->" + SortedVert[jj].ID.ToString());
									//MessageBox.Show(SortedVert[PointLoc].X.ToString() + "," + SortedVert[PointLoc].Y.ToString()+";"+ SortedVert[i].X.ToString() + "," + SortedVert[i].Y.ToString() + "\n" + SortedVert[jj].PreVertex.X.ToString() + "," + SortedVert[jj].PreVertex.Y.ToString() + ";" + SortedVert[jj].X.ToString() + "," + SortedVert[jj].Y.ToString());
									if (CheckedLines[SortedVert[PointLoc].ID - 1, SortedVert[jj].PreVertex.ID - 1] != 1)
									{
										List<IntersectNode> nodes = CheckIntersection(SortedVert[PointLoc], SortedVert[i], SortedVert[jj].PreVertex, SortedVert[jj]);
										CheckedLines[SortedVert[PointLoc].ID - 1, SortedVert[jj].PreVertex.ID - 1] = 1;
										if (nodes != null)
										{
											InterSectPoints.Add(nodes[0]);
											if (nodes.Count > 1) InterSectPoints.Add(nodes[0]);
										}
									}
								}
								if (SortedVert[jj].NextVertex != null)
								{
									//MessageBox.Show("L1:" + SortedVert[PointLoc].ID.ToString() + "->" + SortedVert[i].ID.ToString() + "\nL2:" + SortedVert[jj].ID.ToString() + "->" + SortedVert[jj].NextVertex.ID.ToString());
									if (CheckedLines[SortedVert[PointLoc].ID - 1, SortedVert[jj].ID - 1] != 1)
									{
										List<IntersectNode> nodes = CheckIntersection(SortedVert[PointLoc], SortedVert[i], SortedVert[jj], SortedVert[jj].NextVertex);
										CheckedLines[SortedVert[PointLoc].ID - 1, SortedVert[jj].ID - 1] = 1;
										if (nodes != null)
										{
											InterSectPoints.Add(nodes[0]);
											if (nodes.Count > 1) InterSectPoints.Add(nodes[0]);
										}
									}

								}

							}
						}
					}

					if (SortedVert[i].NextVertex != null & l1Points[SortedVert[i].ID - 1].Next != 1)
					{
						int PointLoc = 0;
						for (int jj = i; jj < SortedVert.Count; jj++)
						{
							if (SortedVert[i].NextVertex.ID == SortedVert[jj].ID & SortedVert[jj].LineID == line1.id)
							{
								PointLoc = jj;
								l1Points[SortedVert[i].ID - 1].Next = 1;
								l1Points[SortedVert[jj].ID - 1].Pre = 1;
								break;
							}
						}
						//MessageBox.Show("--"+SortedVert[i].ID.ToString() + "," + SortedVert[PointLoc].ID.ToString());



						for (int jj = i; jj <= PointLoc; jj++)
						{
							if (SortedVert[jj].LineID == line2.id)
							{
								if (SortedVert[jj].PreVertex != null)
								{
									//MessageBox.Show("L1 Next:" + SortedVert[i].ID.ToString() + "->" + SortedVert[PointLoc].ID.ToString() + "\nL2 Next:" + SortedVert[jj].PreVertex.ID.ToString() + "->" + SortedVert[jj].ID.ToString());
									//MessageBox.Show(SortedVert[i].X.ToString() + "," + SortedVert[i].Y.ToString() + ";" + SortedVert[PointLoc].X.ToString() + "," + SortedVert[PointLoc].Y.ToString() + "\n" + SortedVert[jj].PreVertex.X.ToString() + "," + SortedVert[jj].PreVertex.Y.ToString() + ";" + SortedVert[jj].X.ToString() + "," + SortedVert[jj].Y.ToString());
									if (CheckedLines[SortedVert[i].ID - 1, SortedVert[jj].PreVertex.ID - 1] != 1)
									{
										List<IntersectNode> nodes = CheckIntersection(SortedVert[i], SortedVert[PointLoc], SortedVert[jj].PreVertex, SortedVert[jj]);
										CheckedLines[SortedVert[i].ID - 1, SortedVert[jj].PreVertex.ID - 1] = 1;
										if (nodes != null)
										{
											InterSectPoints.Add(nodes[0]);
											if (nodes.Count > 1) InterSectPoints.Add(nodes[0]);
										}
									}

								}
								if (SortedVert[jj].NextVertex != null)
								{
									if (CheckedLines[SortedVert[i].ID - 1, SortedVert[jj].ID - 1] != 1)
									{
										//MessageBox.Show("L1 Next:" + SortedVert[i].ID.ToString() + "->" + SortedVert[PointLoc].ID.ToString() + "\nL2 Next:" + SortedVert[jj].ID.ToString() + "->" + SortedVert[jj].NextVertex.ID.ToString());
										List<IntersectNode> nodes = CheckIntersection(SortedVert[i], SortedVert[PointLoc], SortedVert[jj], SortedVert[jj].NextVertex);
										CheckedLines[SortedVert[i].ID - 1, SortedVert[jj].ID - 1] = 1;
										if (nodes != null)
										{
											InterSectPoints.Add(nodes[0]);
											if (nodes.Count > 1) InterSectPoints.Add(nodes[0]);
										}
									}

								}

							}
						}
					}
				}
				else if (SortedVert[i].LineID == line2.id)
				{
					if (SortedVert[i].PreVertex != null & l2Points[SortedVert[i].ID - 1].Pre != 1)
					{
						int PointLoc = 0;
						for (int jj = i; jj < SortedVert.Count; jj++)
						{
							if (SortedVert[i].PreVertex.ID == SortedVert[jj].ID & SortedVert[jj].LineID == line2.id)
							{
								PointLoc = jj;
								l2Points[SortedVert[i].ID - 1].Pre = 1;
								l2Points[SortedVert[jj].ID - 1].Next = 1;
								break;
							}
						}
						for (int jj = i; jj <= PointLoc; jj++)
						{
							if (SortedVert[jj].LineID == line1.id)
							{
								if (SortedVert[jj].PreVertex != null)
								{
									//MessageBox.Show("LL1:" + SortedVert[PointLoc].ID.ToString() + "->" + SortedVert[i].ID.ToString() + "\nLL2:" + SortedVert[jj].PreVertex.ID.ToString() + "->" + SortedVert[jj].ID.ToString());
									//MessageBox.Show(SortedVert[PointLoc].X.ToString() + "," + SortedVert[PointLoc].Y.ToString() + ";" + SortedVert[i].X.ToString() + "," + SortedVert[i].Y.ToString() + "\n" + SortedVert[jj].PreVertex.X.ToString() + "," + SortedVert[jj].PreVertex.Y.ToString() + ";" + SortedVert[jj].X.ToString() + "," + SortedVert[jj].Y.ToString());
									if (CheckedLines[SortedVert[jj].PreVertex.ID - 1, SortedVert[i].PreVertex.ID - 1] != 1)
									{
										List<IntersectNode> nodes = CheckIntersection(SortedVert[jj].PreVertex, SortedVert[jj], SortedVert[PointLoc], SortedVert[i]);
										CheckedLines[SortedVert[jj].PreVertex.ID - 1, SortedVert[i].PreVertex.ID - 1] = 1;
										if (nodes != null)
										{
											InterSectPoints.Add(nodes[0]);
											if (nodes.Count > 1) InterSectPoints.Add(nodes[0]);
										}
									}

								}
								if (SortedVert[jj].NextVertex != null)
								{
									if (CheckedLines[SortedVert[jj].ID - 1, SortedVert[i].PreVertex.ID - 1] != 1)
									{
										//MessageBox.Show("LL1:" + SortedVert[PointLoc].ID.ToString() + "->" + SortedVert[i].ID.ToString() + "\nLL2:" + SortedVert[jj].ID.ToString() + "->" + SortedVert[jj].NextVertex.ID.ToString());
										List<IntersectNode> nodes = CheckIntersection(SortedVert[jj], SortedVert[jj].NextVertex, SortedVert[PointLoc], SortedVert[i]);
										CheckedLines[SortedVert[jj].ID - 1, SortedVert[i].PreVertex.ID - 1] = 1;
										if (nodes != null)
										{
											InterSectPoints.Add(nodes[0]);
											if (nodes.Count > 1) InterSectPoints.Add(nodes[0]);
										}
									}
								}

							}
						}
					}

					if (SortedVert[i].NextVertex != null & l2Points[SortedVert[i].ID - 1].Next != 1)
					{
						int PointLoc = 0;
						for (int jj = i; jj < SortedVert.Count; jj++)
						{
							if (SortedVert[i].NextVertex.ID == SortedVert[jj].ID & SortedVert[jj].LineID == line2.id)
							{
								PointLoc = jj;
								l2Points[SortedVert[i].ID - 1].Next = 1;
								l2Points[SortedVert[jj].ID - 1].Pre = 1;
								break;
							}
						}
						for (int jj = i; jj <= PointLoc; jj++)
						{
							if (SortedVert[jj].LineID == line1.id)
							{
								if (SortedVert[jj].PreVertex != null)
								{
									if (CheckedLines[SortedVert[jj].PreVertex.ID - 1, SortedVert[i].ID - 1] != 1)
									{
										//MessageBox.Show("LL1 Next:" + SortedVert[i].ID.ToString() + "->" + SortedVert[PointLoc].ID.ToString() + "\nLL2 Next:" + SortedVert[jj].PreVertex.ID.ToString() + "->" + SortedVert[jj].ID.ToString());
										//MessageBox.Show(SortedVert[i].X.ToString() + "," + SortedVert[i].Y.ToString() + ";" + SortedVert[PointLoc].X.ToString() + "," + SortedVert[PointLoc].Y.ToString() + "\n" + SortedVert[jj].PreVertex.X.ToString() + "," + SortedVert[jj].PreVertex.Y.ToString() + ";" + SortedVert[jj].X.ToString() + "," + SortedVert[jj].Y.ToString());
										List<IntersectNode> nodes = CheckIntersection(SortedVert[jj].PreVertex, SortedVert[jj], SortedVert[i], SortedVert[PointLoc]);
										CheckedLines[SortedVert[jj].PreVertex.ID - 1, SortedVert[i].ID - 1] = 1;
										if (nodes != null)
										{
											InterSectPoints.Add(nodes[0]);
											if (nodes.Count > 1) InterSectPoints.Add(nodes[0]);
										}
									}
								}
								if (SortedVert[jj].NextVertex != null)
								{
									if (CheckedLines[SortedVert[jj].ID - 1, SortedVert[i].ID - 1] != 1)
									{
										//MessageBox.Show("LL1 Next:" + SortedVert[i].ID.ToString() + "->" + SortedVert[PointLoc].ID.ToString() + "\nLL2 Next:" + SortedVert[jj].ID.ToString() + "->" + SortedVert[jj].NextVertex.ID.ToString());
										List<IntersectNode> nodes = CheckIntersection(SortedVert[jj], SortedVert[jj].NextVertex, SortedVert[i], SortedVert[PointLoc]);
										CheckedLines[SortedVert[jj].ID - 1, SortedVert[i].ID - 1] = 1;
										if (nodes != null)
										{
											InterSectPoints.Add(nodes[0]);
											if (nodes.Count > 1) InterSectPoints.Add(nodes[0]);
										}
									}
								}

							}
						}
					}
				}
			}
		}
		//计算交点
		public List<IntersectNode> CheckIntersection(VertexPolyLine SortedVert11, VertexPolyLine SortedVert12, VertexPolyLine SortedVert21, VertexPolyLine SortedVert22)
		{
			double InterSectX, InterSectY1, InterSectY2;
			double MinL1_y, MaxL1_y, MinL2_y, MaxL2_y;
			double MinL1_x, MaxL1_x;
			double MinL2_x, MaxL2_x;
			string str = "";
			str = str + SortedVert11.LineID.ToString() + "." + SortedVert11.ID.ToString() + "->" + SortedVert12.LineID.ToString() + "." + SortedVert12.ID.ToString() + "\n";
			str = str + SortedVert21.LineID.ToString() + "." + SortedVert21.ID.ToString() + "->" + SortedVert22.LineID.ToString() + "." + SortedVert22.ID.ToString() + "\n";
			//str = str + "The" + NumOfInterSects.ToString() + "th:" + InterSectX.ToString() + ";" + InterSectY1.ToString() + ";" + SortedVert11.ID.ToString() + ";" + SortedVert12.ID.ToString();
			//MessageBox.Show(str);
			List<IntersectNode> ListNodes = new List<IntersectNode>();
			InterSectY2 = -9999;
			if (SortedVert11.Y > SortedVert12.Y)
			{
				MinL1_y = SortedVert12.Y;
				MaxL1_y = SortedVert11.Y;
			}
			else
			{
				MinL1_y = SortedVert11.Y;
				MaxL1_y = SortedVert12.Y;
			}
			if (SortedVert21.Y > SortedVert22.Y)
			{
				MinL2_y = SortedVert22.Y;
				MaxL2_y = SortedVert21.Y;
			}
			else
			{
				MinL2_y = SortedVert21.Y;
				MaxL2_y = SortedVert22.Y;
			}
			if (MinL1_y > MaxL2_y | MinL2_y > MaxL1_y)
			{
				return null;
			}
			if (SortedVert11.X > SortedVert12.X)
			{
				MinL1_x = SortedVert12.X;
				MaxL1_x = SortedVert11.X;
			}
			else
			{
				MinL1_x = SortedVert11.X;
				MaxL1_x = SortedVert12.X;
			}
			if (SortedVert21.X > SortedVert22.X)
			{
				MinL2_x = SortedVert22.X;
				MaxL2_x = SortedVert21.X;
			}
			else
			{
				MinL2_x = SortedVert21.X;
				MaxL2_x = SortedVert22.X;
			}
			if ((SortedVert11.X == SortedVert12.X) | SortedVert21.X == SortedVert22.X)
			{
				if ((SortedVert11.X == SortedVert12.X) & (SortedVert21.X == SortedVert22.X))
				{
					if ((SortedVert11.X == SortedVert22.X))
					{
						InterSectX = SortedVert11.X;
						if (MinL1_y > MinL2_y)
						{
							InterSectY1 = MinL1_y;
						}
						else
						{
							InterSectY1 = MinL2_y;
						}
						if (MaxL1_y > MaxL2_y)
						{
							InterSectY2 = MaxL2_y;
						}
						else
						{
							InterSectY2 = MaxL1_y;
						}
					}
					else
					{
						return null;
					}
				}
				else if (SortedVert11.X == SortedVert12.X)
				{
					InterSectX = SortedVert11.X;
					InterSectY1 = SortedVert21.Y - (SortedVert21.Y - SortedVert22.Y) / (SortedVert21.X - SortedVert22.X) * (SortedVert21.X - SortedVert11.X);
					if (InterSectY1 > MaxL1_y | InterSectY1 < MinL1_y)
					{
						return null;
					}
				}
				else
				{
					InterSectX = SortedVert21.X;
					InterSectY1 = SortedVert11.Y - (SortedVert11.Y - SortedVert12.Y) / (SortedVert11.X - SortedVert12.X) * (SortedVert11.X - SortedVert21.X);
					if (InterSectY1 > MaxL1_y | InterSectY1 < MinL1_y)
					{
						return null;
					}
				}
			}
			else
			{
				//MessageBox.Show("("+SortedVert11.X.ToString()+","+ SortedVert11.Y.ToString()+"),("+ SortedVert12.X.ToString()+","+ SortedVert12.Y.ToString()+")"+"\n" +"("+ SortedVert21.X.ToString()+","+ SortedVert21.Y.ToString()+"),("+ SortedVert22.X.ToString()+","+ SortedVert22.Y.ToString()+")");
				double tmp1 = SortedVert21.Y - SortedVert11.Y + (SortedVert11.Y - SortedVert12.Y) / (SortedVert11.X - SortedVert12.X) * SortedVert11.X - (SortedVert21.Y - SortedVert22.Y) / (SortedVert21.X - SortedVert22.X) * SortedVert21.X;
				double tmp2 = (SortedVert11.Y - SortedVert12.Y) / (SortedVert11.X - SortedVert12.X) - (SortedVert21.Y - SortedVert22.Y) / (SortedVert21.X - SortedVert22.X);
				InterSectX = tmp1 / tmp2;
				InterSectY1 = 0;
				//MessageBox.Show("InterSectX:" + InterSectX.ToString() + "\n" + "InterSectY:" + InterSectY1.ToString());
				//MessageBox.Show(SortedVert11.X.ToString()+";" + InterSectX.ToString() +","+ SortedVert12.X.ToString());

				if ((InterSectX < MinL1_x) | (InterSectX > MaxL1_x) | (InterSectX < MinL2_x) | (InterSectX > MaxL2_x))
				{
					return null;
				}
				else
				{
					InterSectY1 = SortedVert11.Y - (SortedVert11.Y - SortedVert12.Y) / (SortedVert11.X - SortedVert12.X) * (SortedVert11.X - InterSectX);
				}
				NumOfInterSects = NumOfInterSects + 1;

				//MessageBox.Show(NumOfInterSects.ToString()+"\nInterSectX:" + InterSectX.ToString()+"\n"+ "InterSectY:" + InterSectY1.ToString());

			}
			if (InterSectY2 != -9999)
			{
				IntersectNode nodes0 = new IntersectNode();
				IntersectNode nodes1 = new IntersectNode();

				nodes0.X = InterSectX;
				nodes0.Y = InterSectY1;
				nodes1.X = InterSectX;
				nodes1.Y = InterSectY2;

				nodes0.line1ID = SortedVert11.LineID;
				nodes0.line2ID = SortedVert21.LineID;

				nodes0.PreVertLine1 = SortedVert11.ID;
				nodes0.NextVertLine1 = SortedVert12.ID;
				nodes0.PreVertLine1 = SortedVert21.ID;
				nodes0.NextVertLine2 = SortedVert22.ID;

				nodes1.PreVertLine1 = SortedVert11.ID;
				nodes1.NextVertLine1 = SortedVert12.ID;
				nodes1.PreVertLine2 = SortedVert21.ID;
				nodes1.NextVertLine2 = SortedVert22.ID;

				nodes1.line1ID = SortedVert11.LineID;
				nodes1.line2ID = SortedVert21.LineID;
				ListNodes.Add(nodes0);
				ListNodes.Add(nodes1);
				return ListNodes;
			}
			else
			{
				IntersectNode nodes0 = new IntersectNode();
				nodes0.X = InterSectX;
				nodes0.Y = InterSectY1;
				nodes0.PreVertLine1 = SortedVert11.ID;
				nodes0.NextVertLine1 = SortedVert12.ID;
				nodes0.PreVertLine2 = SortedVert21.ID;
				nodes0.NextVertLine2 = SortedVert22.ID;
				nodes0.line1ID = SortedVert11.LineID;
				nodes0.line2ID = SortedVert21.LineID;
				ListNodes.Add(nodes0);
				//str = str + SortedVert11.LineID.ToString()+"."+ SortedVert11.ID.ToString()+"->"+ SortedVert12.LineID.ToString() + "." + SortedVert12.ID.ToString()+"\n";
				//str = str + SortedVert21.LineID.ToString() + "." + SortedVert21.ID.ToString() + "->" + SortedVert22.LineID.ToString() + "." + SortedVert22.ID.ToString() + "\n";

				//str = str + "The" + NumOfInterSects.ToString() + "th:" + InterSectX.ToString() + ";" + InterSectY1.ToString() + ";" + SortedVert11.ID.ToString() + ";" + SortedVert12.ID.ToString();
				//MessageBox.Show(str);
				return ListNodes;
			}
		}
		//检查交点
		public class IntersectNode
		{
			public int ID;
			//Line1，2线段ID
			public int line1ID;
			public int line2ID;
			//Line1，2前后顶点
			public int PreVertLine1;
			public int NextVertLine1;
			public int PreVertLine2;
			public int NextVertLine2;
			public double X
			{
				get { return x; }
				set { x = value; }
			}
			public double Y
			{
				get { return y; }
				set { y = value; }
			}
			double x;
			double y;
		}
		//定义交点类
		public void DrawPolyLines(PictureBox pb)
		{
			Graphics g = pb.CreateGraphics();
			g.Clear(Color.Gray);
			foreach (PolyLine pl in polylines)
			{
				Pen penDrawLine = new Pen(Color.Red, 3);
				PointF[] pointsF = new PointF[pl.points.Count];
				for (int i = 0; i < pointsF.Length; i++)
				{
					pointsF[i].X = (float)(pl.points[i].X - (float)MH.XMin) / ((float)MH.XMax - (float)MH.XMin) * pb.Width;
					pointsF[i].Y = (float)(pb.Height - (pl.points[i].Y - (float)MH.YMin) / ((float)MH.YMax - (float)MH.YMin) * pb.Height);
				}
				g.DrawLines(penDrawLine, pointsF);
			}
		}
		//绘制SHP内折线段
		public void DrawInterSectPoints(PictureBox pb)
		{
			//string str = "";
			Graphics g = pb.CreateGraphics();
			//g.Clear(Color.Gray);
			Pen draw_point = new Pen(Color.Red, 1);
			Brush brush = new SolidBrush(Color.LightGreen);
			//Pen pen1 = new Pen(Color.Red, 3);
			PointF[] pts = new PointF[InterSectPoints.Count];
			for (int i = 0; i < InterSectPoints.Count; i++)
			{
				pts[i].X = (float)(InterSectPoints[i].X - (float)MH.XMin) / ((float)MH.XMax - (float)MH.XMin) * pb.Width;
				pts[i].Y = (float)(pb.Height - (InterSectPoints[i].Y - (float)MH.YMin) / ((float)MH.YMax - (float)MH.YMin) * pb.Height);
				Console.WriteLine("第{0}个交点：" + InterSectPoints[i].X + " " + InterSectPoints[i].Y, i + 1);
				g.DrawEllipse(draw_point, pts[i].X - 5, pts[i].Y - 5, 9, 9);
				g.FillEllipse(brush, pts[i].X - 5, pts[i].Y - 5, 9, 9);
				//str = str + pts[i].X + "," + pts[i].Y + "\n";
			}
			//MessageBox.Show(str);
		}
		//绘制交点
		public void Network()
		{
			for (int i = 0; i < count; i++)
			{
				for (int j = i + 1; j < count; j++)
				{
					bool c = false;
					if (polylines[i].xmax < polylines[j].xmin ||
						polylines[i].xmin > polylines[j].xmax ||
						polylines[i].ymax < polylines[j].xmin ||
						polylines[i].ymin < polylines[j].ymax)
					{
						c = false;// We're outside the polygon!
					}
					else
					{
						//
					}
				}
			}
		}
		//网格分析，待实现
	}
	//定义类PolyLines，由折线构造的线表

	class VertexPolygon
	{
		public int ID;//顶点ID
		public VertexPolygon PreVertex;//前顶点
		public VertexPolygon NextVertex;//后顶点
		public double X;//点坐标X
		public double Y;//点坐标Y
		public int PolygonID;//所在面ID
		public VertexPolygon()
		{
			PreVertex = null;
			NextVertex = null;
			ID = 0;
		}
		//默认构造函数，内含前一顶点ID以及后一顶点ID，以及构造时的初始ID
	}
	//定义类Vertex(顶点)

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
	//定义线段用于交点检查
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
		public List<VertexPolygon> points;			//由顶点构成的集合
	//	public List<BasicLine> lines;		//由多边形基础线段构成的集合
		public PolyGon()
		{
			points = new List<VertexPolygon>();
		}
		//默认构造函数，将上面的points实例化
	}
	//定义类PolyGon

	class PolyGons
	{
		private int FirstX;
		public List<PolyGon> polygons;//由多个面构成的面组
		public List<PointF> inse_points = new List<PointF>();
		public ShapeHeader MH;
		public string inname = "";
		//用来存放IsPointInPolygon函数的数据
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
		///高低字节转序
		///该函数用于在读取SHP文件时对高字节序部分进行转序
		///参数为一个无符号的32位整形值，假设字节内容为ABCD
		///返回值即为DCBA
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
					for(int i = 0; i < polyG.NumOfParts; i++)
					{
						polyG.Parts[i] = br.ReadInt32();
					}
					//面首点数组索引位
					VertexPolygon PreVer = null;
					VertexPolygon VertexFirst = new VertexPolygon();
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
					for(int i = 1; i < polyG.NumOfPoints - 1; i++)
					{
						VertexPolygon VertexMid = new VertexPolygon();
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
					VertexPolygon VetLast = new VertexPolygon();
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
				//Console.WriteLine(e.ToString());
				//Console.WriteLine("文件读取完毕");				
			}
		}
		//读取SHP文件内容
		public void DrawPolyGons(PictureBox pb)
		{
			Graphics g = pb.CreateGraphics();
			g.Clear(Color.Gray);
			foreach(PolyGon pg in polygons)
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

				Pen penDrawLine = new Pen(Color.Red, 3);
				Pen penDrawPolygon = new Pen(Color.FromArgb(R, G, B), 1);
				PointF[] pointsF = new PointF[pg.points.Count];
				for(int i = 0; i < pointsF.Length; i++)
				{
					pointsF[i].X = (float)((pg.points[i].X - MH.XMin) / (MH.XMax - MH.XMin) * pb.Width);
					pointsF[i].Y = (float)(pb.Height - (pg.points[i].Y - MH.YMin) / (MH.YMax - MH.YMin) * pb.Height);
				}
				g.DrawLines(penDrawLine, pointsF);
				//绘制多边形面

				EdgeInfo[] edgelist = new EdgeInfo[100];
				int j = 0, yu = 0, yd = 1024;
				for(int i = 0; i < pointsF.Length - 1; i++)
				{
					if(pointsF[i].Y > yu) yu = (int)pointsF[i].Y;
					if(pointsF[i].Y < yd) yd = (int)pointsF[i].Y;
					if(pointsF[i].Y != pointsF[i + 1].Y)
					{
						if(pointsF[i].Y > pointsF[i + 1].Y)
						{
							edgelist[j++] = new EdgeInfo((int)pointsF[i + 1].X, (int)pointsF[i + 1].Y, (int)pointsF[i].X, (int)pointsF[i].Y);
						}
						else
						{
							edgelist[j++] = new EdgeInfo((int)pointsF[i].X, (int)pointsF[i].Y, (int)pointsF[i + 1].X, (int)pointsF[i + 1].Y);
						}
					}
				}
				for(int y = yd; y < yu; y++)
				{
					//AEL
					var sorted = from item in edgelist
								 where y < item.YMax && y >= item.YMin
								 orderby item.XMin, item.K
								 select item;
					int flag = 0;
					foreach(var item in sorted)
					{
						//FirstX = 0;
						if(flag == 0)
						{
							FirstX = (int)(item.XMin + 0.5);
							flag++;
						}
						else
						{
							g.DrawLine(penDrawPolygon, (int)(item.XMin + 0.5), y, FirstX - 1, y);
							g.DrawLine(penDrawPolygon, (int)(item.XMin + 0.5), y, FirstX - 1, y);
							flag = 0;
						}
					}
					for(int i = 0; i < j; i++)
					{
						if(y < edgelist[i].YMax - 1 && y > edgelist[i].YMin)
						{
							edgelist[i].XMin += edgelist[i].K;
						}
					}
				}
				
				//扫描线填充
			}
		}
		//绘制多边形(边加粗，内部填充)
		public void DrawInsePoints(PictureBox pb)
		{
			Graphics g = pb.CreateGraphics();
			Pen draw_point = new Pen(Color.Red, 1);
			Brush brush = new SolidBrush(Color.LightGreen);
			PointF[] pointsF = new PointF[inse_points.Count];
			for(int i = 0; i < pointsF.Length; i++)
			{
				pointsF[i].X = (float)((inse_points[i].X - MH.XMin) / (MH.XMax - MH.XMin) * pb.Width);
				pointsF[i].Y = (float)(pb.Height - (inse_points[i].Y - MH.YMin) / (MH.YMax - MH.YMin) * pb.Height);
				g.DrawEllipse(draw_point, pointsF[i].X - 5, pointsF[i].Y - 5, 9, 9);
				g.FillEllipse(brush, pointsF[i].X - 5, pointsF[i].Y - 5, 9, 9);
				//Console.WriteLine(pointsF[i].X + " " + pointsF[i].Y);
			}
			//Console.WriteLine("共绘制了{0}个交点", inse_points.Count);
		}
		//绘制交点
		public void CheckLineInPolygonsInse()
		{
			bool Inse;
			for(int front = 0; front <= polygons.Count - 2; front++)
			{
				for(int rear = front + 1; rear <= polygons.Count - 1; rear++)
				{
					PointF left_up = new PointF((float)polygons[front].xmin, (float)polygons[front].ymax);
					PointF right_up = new PointF((float)polygons[front].xmax, (float)polygons[front].ymax);
					PointF left_down = new PointF((float)polygons[front].xmin, (float)polygons[front].ymin);
					PointF right_down = new PointF((float)polygons[front].xmax, (float)polygons[front].ymin);
					//多边形front的对角线
					if(polygons[front].ymax < polygons[rear].ymin
						|| polygons[front].xmax < polygons[rear].xmin
						|| polygons[rear].ymax < polygons[front].ymin
						|| polygons[rear].xmax < polygons[front].xmin)
					{
						//Console.WriteLine("多边形{0}与多边形{1}不可能相交", front, rear);
						loop_time++;
					}
					//多边形碰撞箱检查，不相交
					else
					{
						//Console.WriteLine("多边形{0}与多边形{1}可能相交", front, rear);
						string blacklist_j = "";
						for(int i = 0; i < polygons[front].points.Count - 1; i++)
						{
							for(int j = 0; j < polygons[rear].points.Count - 1; j++)
							{
								loop_time++;
								if(blacklist_j.Contains(Convert.ToString(j)))
								{
									//Console.WriteLine("边{0}跳过", j);
									continue;
								}
								//Console.WriteLine("开始检查多边形{0}的边{1}与多边形{2}的边{3}", front, i, rear, j);
								PointF point1a = new PointF((float)polygons[front].points[i].X, (float)polygons[front].points[i].Y);
								PointF point1b = new PointF((float)polygons[front].points[i + 1].X, (float)polygons[front].points[i + 1].Y);
								PointF point2a = new PointF((float)polygons[rear].points[j].X, (float)polygons[rear].points[j].Y);
								PointF point2b = new PointF((float)polygons[rear].points[j + 1].X, (float)polygons[rear].points[j + 1].Y);
								if((point2a.X > polygons[front].xmin && point2a.X < polygons[front].xmax &&
									point2a.Y > polygons[front].ymin && point2a.Y < polygons[front].ymax) ||
									//多边形rear的检查边点A在碰撞箱内
									(point2b.X > polygons[front].xmin && point2b.X < polygons[front].xmax &&
									point2b.Y > polygons[front].ymin && point2b.Y < polygons[front].ymax) ||
									//多边形rear的检查边点B在碰撞箱内
									CheckIfInse(point2a, point2b, left_up, right_down) ||
									//多边形rear的检查边与碰撞箱的＼对角线有交点
									CheckIfInse(point2a, point2b, right_up, left_down)
									//多边形rear的检查边与碰撞箱的／对角线有交点
									)
								{
									Inse = CheckIfInse(point1a, point1b, point2a, point2b);
									if(Inse)
									{
										//Console.WriteLine("多边形{0}的边{1}与多边形{2}的边{3}相交", front, i, rear, j);
										CalInsePoint(point1a, point1b, point2a, point2b);
									}
									else
									{
										//Console.WriteLine("多边形{0}的边{1}与多边形{2}的边{3}不相交", front, i, rear, j);
									}
								}
								else
								{
									//Console.WriteLine("多边形{0}的边{1}与多边形{2}的边{3}不相交", front, i, rear, j);
									//Console.WriteLine("边{0}已加入多边形{1}的黑名单", j, front);
									blacklist_j += Convert.ToString(j);
								}
							}
						}
						//检查所有边
					}
					//碰撞箱检查得交，检查线交点
				}
			}
			//多边形之间检查
			//Console.WriteLine("循环次数：{0}，检查次数：{1}", loop_time, check_time);
			///每次检查两个多边形
			///首先检查他们的碰撞箱，假如碰撞箱不交则跳过(节省大部分时间)
			///然后具体检查线段相交
			///当多边形rear的一条线段不在front内的时候跳过行列式运算，并判定不相交，该线段列入front的黑名单
		}
		///检查多边形内线段相交
		///1.每次检查两个多边形，分别标记为front和rear。
		///第一层循环front从第一个多边形遍历到倒数第二个多边形，
		///第二层循环rear从front+1开始遍历到最后一个多边形。
		///2.第二层循环体内，首先对两个多边形的外接矩形框（下称碰撞箱）进行检查，
		///假如二者外接矩形框相交（下称碰撞箱发生碰撞），则初步判断二者可能存在相交点，
		///而碰撞箱未发生碰撞的情况直接跳过检查，并判定不相交。
		///3.在两多边形碰撞箱碰撞的前提下，进行逐边检查。第三层循环遍历多边形front的每一条边，
		///检查边记为i，第四层循环遍历多边形rear的每一条边，检查边记为j。
		///a)首先检查rear的边j是否与多边形front的碰撞箱发生碰撞，检查方法：分别判断边j的两点坐
		///标是否在碰撞箱内，假如有一点在碰撞箱内则该边j与多边形front的碰撞箱发生了碰撞。
		///b)而当两点均不在碰撞箱内时再判断该边j是否与碰撞箱任意一条对角线有交点，如果有交点则
		///边j与碰撞箱有碰撞，具体检查边i与边j是否相交。而当边j两点均不在碰撞箱内且边j与碰撞箱
		///对角线无交点时，判定边j与多边形front任意边均无交点，后续检查跳过。
		private bool CheckIfInse(PointF a1, PointF a2, PointF b1, PointF b2)
		{
			check_time++;
			double delta = Determinant(a2.X - a1.X, b2.X - b1.X, b2.Y - b1.Y, a2.Y - a1.Y);
			if(Math.Abs(delta) <= (1e-6))
			{
				return false;
			}
			///1.将两线段转换为向量(a2-a1),(b2-b1)，两向量叉乘结果记为delta。
			///若delta为0，说明线段平行或者共直线。具体判别共直线的情况，两线段
			///分别任取一点构成一条新的直线，若新直线的斜率等于旧直线斜率则共线，
			///特别的当两条直线都垂直X坐标轴斜率不存在时则判别两直线的X值是否相等
			///即可。delta大于零，向量2在向量1的逆时针方向，小于零在顺时针方向。
			double lamda = Determinant(b2.X - b1.X, a1.X - b1.X, a1.Y - b1.Y, b2.Y - b1.Y) / delta;
			if(lamda > 1 || lamda < 0)
			{
				return false;
			}
			///2.再求向量(b2 - b1)和(a1 - b1)的叉乘以及向量(a2 - a1)和(a1 - b1)
			///的叉乘，所得结果分别除delta得(a1 - b1)/(a2 - a1)和(a1 - b1)/(a2 - b1)，
			///分别记为lamda和myu。若lamda小于0则说明向量1在向量2的某时针方向，而向量(a1 - b1)也在向量2的某时针方向，
			///即向量1与向量(a1 - b1)在向量2同侧，此时两线段不可能有交点，判false。
			///当lamda大于0时说明向量1与向量(a1 - b1)在向量2异侧，线段可能有交点。
			///当lamda大于1时，(a1 - b1)长度大于(a2 - a1)长度乘向量2与向量1夹角正弦值，
			///这时线段也无交点。当lamda小于等于1大于0时才可能有交点。
			double myu = Determinant(a2.X - a1.X, a1.X - b1.X, a1.Y - b1.Y, a2.Y - a1.Y) / delta;
			if(myu > 1 || myu < 0)
			{
				return false;
			}
			///3.当myu小于零时说明向量(a1-b1)与(b2-b1)在向量1的不同侧，此时不可能有交点。
			///当大于0时，如果myu大于1，情况与上面类似，无交点。myu大于0小于等于1时可能有交点。
			return true;
			///综上所述，两线段有交点（不考虑共直线情况）需同时满足以下条件，delta不等于0，lamda和myu都在区间(0,1]内
		}
		///跨立实验：通过两点坐标判断线段是否相交
		///参数a1, a2, b1, b2分别为线段A, B的两端点
		///返回值false不相交，true相交
		private static double Determinant(double v1, double v2, double v3, double v4)
		{
			return (v1 * v3 - v2 * v4);
		}
		///计算2x2矩阵行列式
		///2x2矩阵格式：
		///v1	v2
		///v4	v3
		private void CalInsePoint(PointF a1, PointF a2, PointF b1, PointF b2)
		{
			PointF inse_point = new PointF();
			if(Math.Abs(a1.X - a2.X) <= 1e-6)
				{
					inse_point.X = a1.X;
					inse_point.Y = (a1.X - b1.X) / (b1.X - b2.X) * (b1.Y - b2.Y) + b1.Y;
				}
			//线段A垂直X坐标轴(加法 4，乘法 2)
			else if(Math.Abs(b1.X - b2.X) <= 1e-6)
				{
					inse_point.X = b1.X;
					inse_point.Y = (b1.X - a1.X) / (a1.X - a2.X) * (a1.Y - a2.Y) + a1.Y;
				}
			//线段B垂直X坐标轴(加法 4，乘法 2)
			else if(Math.Abs(a1.Y - a2.Y) <= 1e-6)
				{
					inse_point.Y = a1.Y;
					inse_point.X = (a1.Y - b1.Y) / (b2.Y - b1.Y) * (b2.X - b1.X) + b1.X;
				}
			//线段A平行X坐标轴(加法 4，乘法 2)
			else if(Math.Abs(b1.Y - b2.Y) <= 1e-6)
				{
					inse_point.Y = b1.Y;
					inse_point.X = (b1.Y - a1.Y) / (a2.Y - a1.Y) * (a2.X - a1.X) + a1.X;
				}
			//线段B平行X坐标轴(加法 4，乘法 2)
			else
				{
					inse_point.X = ((a1.Y - b1.Y) + (b1.Y - b2.Y) / (b1.X - b2.X) * b1.X - (a1.Y - a2.Y) / (a1.X - a2.X) * a1.X) / ((b1.Y - b2.Y) / (b1.X - b2.X) - (a1.Y - a2.Y) / (a1.X - a2.X));
					inse_point.Y = (inse_point.X - a1.X) / (a1.X - a2.X) * (a1.Y - a2.Y) + a1.Y;
				}
			//其他一般情况(加法 16，乘法 8)
			if(Math.Abs(inse_point.X - a1.X) <= 1e-6 && Math.Abs(inse_point.Y - a1.Y) <= 1e-6 ||
				Math.Abs(inse_point.X - a2.X) <= 1e-6 && Math.Abs(inse_point.Y - a2.Y) <= 1e-6 ||
				Math.Abs(inse_point.X - b1.X) <= 1e-6 && Math.Abs(inse_point.Y - b1.Y) <= 1e-6 ||
				Math.Abs(inse_point.X - b2.X) <= 1e-6 && Math.Abs(inse_point.Y - b2.Y) <= 1e-6
			)
			{
				inse_points.Add(inse_point);
				//Console.WriteLine("直线端点，不取");
			}
			else
			{
				inse_points.Add(inse_point);
				//非端点时加一点
			}
			//Console.WriteLine(inse_point.ToString());
		}
		///计算交点坐标
		///通过线段两点式方程联立确定交点坐标
		///对平行或垂直X坐标轴的线段进行特殊检查
		public void IsPointInPolygon(PointF pt)
		{
			inname = "";
			foreach(PolyGon poly in polygons)
			{
				bool c = false;//初始置否
				if (pt.X < poly.xmin || pt.X > poly.xmax || pt.Y < poly.ymin || pt.Y > poly.ymax)
				{
					c = false;
				}
				//碰撞箱判别
				else
				{
					int i, j, nvert;
					nvert = poly.points.Count;
					for (i = 0, j = nvert - 2; i < nvert - 1; j = i++)
					{
						if (((poly.points[i].Y > pt.Y) != (poly.points[j].Y > pt.Y)) &&
							(pt.X < (poly.points[j].X - poly.points[i].X) * (pt.Y - poly.points[i].Y) / (poly.points[j].Y - poly.points[i].Y) + poly.points[i].X))
							c = !c;
					}
				}
				//扫描线交多边形偶数次则点在多边形内，否则点在多边形外
				if (c)
				{
					inname += poly.id.ToString() + " ";
					//Console.WriteLine("该点在多边形{0}内", poly.id);
				}
			}
		}
		///判断点是否在多边形内
		///参数PointF pt指定点，PolyGon poly指定一个多边形
	}
}
