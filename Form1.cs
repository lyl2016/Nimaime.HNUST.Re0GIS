using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PolygonCut
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}
	}

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
		public List<PolyGon> polygons;//由多个面构成的面组
		public static int NumOfInterSects = 0;//设置交点计数，初值为零，设置静态用于计数
		public List<IntersectNode> InterSectPoints;//由多个交点构成的交点组
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
			InterSectPoints = new List<IntersectNode>();
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
		public void CalIntersect(PolyGon l1, PolyGon l2)
		{
			string sortedX = "";
			int[,] CheckedLines = new int[l1.points.Count, l2.points.Count];
			for (int i = 0; i < l1.points.Count; i++)
			{
				for (int j = 0; j < l2.points.Count; j++)
				{
					CheckedLines[i, j] = 0;
				}
			}
			List<Vertex> SortedVert = new List<Vertex>();
			SortedVert.Add(l1.points[0]);
			int PreJ = 0;
			for (int i = 1; i < l1.points.Count; i++)
			{
				int sorted = 0;
				int j = PreJ;
				if (l1.points[i].X <= SortedVert[j].X)
				{
					while (sorted == 0)
					{
						if (j == 0)
						{
							sorted = 1;
							PreJ = j;
						}
						else if (l1.points[i].X > SortedVert[j - 1].X)
						{
							PreJ = j;
							sorted = 1;
						}
						else
						{
							j = j - 1;
						}
					}
					SortedVert.Insert(PreJ, l1.points[i]);
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
						else if (l1.points[i].X < SortedVert[j + 1].X)
						{

							PreJ = j + 1;
							sorted = 1;
						}
						else
						{
							j = j + 1;
						}
					}
					SortedVert.Insert(PreJ, l1.points[i]);
				}
			}
			for (int i = 0; i < l2.points.Count; i++)
			{
				int sorted = 0;
				int j = PreJ;
				if (l2.points[i].X <= SortedVert[j].X)
				{
					while (sorted == 0)
					{
						if (j == 0)
						{
							sorted = 1;
							PreJ = j;
						}
						else if (l2.points[i].X > SortedVert[j - 1].X)
						{
							PreJ = j;
							sorted = 1;
						}
						else
						{
							j = j - 1;
						}
					}
					SortedVert.Insert(PreJ, l2.points[i]);
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
						else if (l2.points[i].X < SortedVert[j + 1].X)
						{

							PreJ = j + 1;
							sorted = 1;
						}
						else
						{
							j = j + 1;
						}
					}
					SortedVert.Insert(PreJ, l2.points[i]);
				}
			}
			sortedX = "";
			for (int ii = 0; ii < SortedVert.Count; ii++)
			{
				sortedX = sortedX + ii.ToString() + ")" + SortedVert[ii].PolygonID.ToString() + "->" + SortedVert[ii].ID + ":" + SortedVert[ii].X.ToString() + "\n";
			}
			//MessageBox.Show(sortedX);
			//MessageBox.Show(l1.points.Count.ToString());
			CheckPoint[] l1Points = new CheckPoint[l1.points.Count];
			CheckPoint[] l2Points = new CheckPoint[l2.points.Count];
			for (int i = 0; i < l1.points.Count; i++)
			{
				l1Points[i].Pre = 0;
				l1Points[i].Next = 0;
			}
			for (int i = 0; i < l2.points.Count; i++)
			{
				l2Points[i].Pre = 0;
				l2Points[i].Next = 0;
			}
			for (int i = 0; i < SortedVert.Count; i++)
			{
				if (SortedVert[i].PolygonID == l1.id)
				{
					if (SortedVert[i].PreVertex != null & l1Points[SortedVert[i].ID - 1].Pre != 1)
					{
						int PointLoc = 0;
						for (int jj = i; jj < SortedVert.Count; jj++)
						{
							if (SortedVert[i].PreVertex.ID == SortedVert[jj].ID & SortedVert[jj].PolygonID == l1.id)
							{
								PointLoc = jj;
								l1Points[SortedVert[i].ID - 1].Pre = 1;
								l1Points[SortedVert[jj].ID - 1].Next = 1;
								break;
							}
						}
						for (int jj = i; jj <= PointLoc; jj++)
						{
							if (SortedVert[jj].PolygonID == l2.id)
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
							if (SortedVert[i].NextVertex.ID == SortedVert[jj].ID & SortedVert[jj].PolygonID == l1.id)
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
							if (SortedVert[jj].PolygonID == l2.id)
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
				else if (SortedVert[i].PolygonID == l2.id)
				{
					if (SortedVert[i].PreVertex != null & l2Points[SortedVert[i].ID - 1].Pre != 1)
					{
						int PointLoc = 0;
						for (int jj = i; jj < SortedVert.Count; jj++)
						{
							if (SortedVert[i].PreVertex.ID == SortedVert[jj].ID & SortedVert[jj].PolygonID == l2.id)
							{
								PointLoc = jj;
								l2Points[SortedVert[i].ID - 1].Pre = 1;
								l2Points[SortedVert[jj].ID - 1].Next = 1;
								break;
							}
						}
						for (int jj = i; jj <= PointLoc; jj++)
						{
							if (SortedVert[jj].PolygonID == l1.id)
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
							if (SortedVert[i].NextVertex.ID == SortedVert[jj].ID & SortedVert[jj].PolygonID == l2.id)
							{
								PointLoc = jj;
								l2Points[SortedVert[i].ID - 1].Next = 1;
								l2Points[SortedVert[jj].ID - 1].Pre = 1;
								break;
							}
						}
						for (int jj = i; jj <= PointLoc; jj++)
						{
							if (SortedVert[jj].PolygonID == l1.id)
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
		public List<IntersectNode> CheckIntersection(Vertex SortedVert11, Vertex SortedVert12, Vertex SortedVert21, Vertex SortedVert22)
		{
			double InterSectX, InterSectY1, InterSectY2;
			double MinL1_y, MaxL1_y, MinL2_y, MaxL2_y;
			double MinL1_x, MaxL1_x;
			double MinL2_x, MaxL2_x;
			string str = "";
			str = str + SortedVert11.PolygonID.ToString() + "." + SortedVert11.ID.ToString() + "->" + SortedVert12.PolygonID.ToString() + "." + SortedVert12.ID.ToString() + "\n";
			str = str + SortedVert21.PolygonID.ToString() + "." + SortedVert21.ID.ToString() + "->" + SortedVert22.PolygonID.ToString() + "." + SortedVert22.ID.ToString() + "\n";
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

				nodes0.polygon1ID = SortedVert11.PolygonID;
				nodes0.polygon2ID = SortedVert21.PolygonID;

				nodes0.PreVertGon1 = SortedVert11.ID;
				nodes0.NextVertGon1 = SortedVert12.ID;
				nodes0.PreVertGon1 = SortedVert21.ID;
				nodes0.NextVertGon2 = SortedVert22.ID;

				nodes1.PreVertGon1 = SortedVert11.ID;
				nodes1.NextVertGon1 = SortedVert12.ID;
				nodes1.PreVertGon2 = SortedVert21.ID;
				nodes1.NextVertGon2 = SortedVert22.ID;

				nodes1.polygon1ID = SortedVert11.PolygonID;
				nodes1.polygon2ID = SortedVert21.PolygonID;
				ListNodes.Add(nodes0);
				ListNodes.Add(nodes1);
				return ListNodes;
			}
			else
			{
				IntersectNode nodes0 = new IntersectNode();
				nodes0.X = InterSectX;
				nodes0.Y = InterSectY1;
				nodes0.PreVertGon1 = SortedVert11.ID;
				nodes0.NextVertGon1 = SortedVert12.ID;
				nodes0.PreVertGon2 = SortedVert21.ID;
				nodes0.NextVertGon2 = SortedVert22.ID;
				nodes0.polygon1ID = SortedVert11.PolygonID;
				nodes0.polygon2ID = SortedVert21.PolygonID;
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
			
			public int polygon1ID;
			public int polygon2ID;
			
			public int PreVertGon1;
			public int NextVertGon1;
			public int PreVertGon2;
			public int NextVertGon2;
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
			foreach (PolyGon pl in polygons)
			{
				int iSeed = 10;
				Random ro = new Random(10);
				long tick = DateTime.Now.Ticks;
				Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
				int R = ran.Next(255);
				int G = ran.Next(255);
				int B = ran.Next(255);
				B = (R + G > 400) ? R + G - 400 : B;//0 : 380 - R - G;
				B = (B > 255) ? 255 : B;
				Pen pen1 = new Pen(Color.Black, 1);
				//SolidBrush brsh = new SolidBrush(Color.FromArgb(R, G, B));
				SolidBrush brsh = new SolidBrush(Color.Green);
				PointF[] pointsF = new PointF[pl.points.Count];
				for (int i = 0; i < pointsF.Length; i++)
				{
					pointsF[i].X = (float)(pl.points[i].X - (float)MH.XMin) / ((float)MH.XMax - (float)MH.XMin) * pb.Width;
					pointsF[i].Y = (float)(pb.Height - (pl.points[i].Y - (float)MH.YMin) / ((float)MH.YMax - (float)MH.YMin) * pb.Height);
				}
				g.DrawLines(pen1, pointsF);
			}
		}
		//绘制SHP内折线段
		public void DrawInterSectPoints(PictureBox pb)
		{
			string str = "";
			Graphics g = pb.CreateGraphics();
			Pen pen1 = new Pen(Color.Red, 3);
			PointF[] pts = new PointF[InterSectPoints.Count];
			for (int i = 0; i < InterSectPoints.Count; i++)
			{
				pts[i].X = (float)(InterSectPoints[i].X - (float)MH.XMin) / ((float)MH.XMax - (float)MH.XMin) * pb.Width;
				pts[i].Y = (float)(pb.Height - (InterSectPoints[i].Y - (float)MH.YMin) / ((float)MH.YMax - (float)MH.YMin) * pb.Height);
				Console.WriteLine("第{0}个交点：" + InterSectPoints[i].X + " " + InterSectPoints[i].Y, i + 1);
				g.DrawEllipse(new Pen(Color.Red, 1), pts[i].X - 2, pts[i].Y - 2, 4, 4);
				str = str + pts[i].X + "," + pts[i].Y + "\n";
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
					if (polygons[i].xmax < polygons[j].xmin ||
						polygons[i].xmin > polygons[j].xmax ||
						polygons[i].ymax < polygons[j].xmin ||
						polygons[i].ymin < polygons[j].ymax)
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
}
