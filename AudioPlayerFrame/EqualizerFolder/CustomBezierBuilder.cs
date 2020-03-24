using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Diagnostics;

namespace AudioPlayer
{
    public class CustomBezierBuilder
    {
		public Point[] points { get; set; }

		public Canvas canvas { get; set; } 

		public void Draw()
		{
			try
			{
				Point[] points = this.points;
				if (points.Length < 2)
					return;

				Canvas canvas = this.canvas;
				canvas.Children.Clear();

				const double markerSize = 5;
				//Draw Curve points(Black)
				//for (int i = 0; i < points.Length; ++i)
				//{
				//	Rectangle rect = new Rectangle()
				//	{
				//		Stroke = Brushes.Black,
				//		Fill = Brushes.Black,
				//		Height = markerSize,
				//		Width = markerSize
				//	};
				//	Canvas.SetLeft(rect, points[i].X - markerSize / 2);
				//	Canvas.SetTop(rect, points[i].Y - markerSize / 2);
				//	canvas.Children.Add(rect);
				//}

				// Get Bezier Spline Control Points.
				Point[] cp1, cp2;
				BezierSpline.GetCurveControlPoints(points, out cp1, out cp2);

				// Draw curve by Bezier.
				PathSegmentCollection lines = new PathSegmentCollection();
				for (int i = 0; i < cp1.Length; ++i)
				{
					lines.Add(new BezierSegment(cp1[i], cp2[i], points[i + 1], true));
				}
				PathFigure f = new PathFigure(points[0], lines, false);
				PathGeometry g = new PathGeometry(new PathFigure[] { f });
				Path path = new Path() { Stroke = Brushes.Black, StrokeThickness = 1, Data = g };
				canvas.Children.Add(path);

				// Draw Bezier control points markers
				//for (int i = 0; i < cp1.Length; ++i)
				//{
				//	// First control point (Blue)
				//	Ellipse marker = new Ellipse()
				//	{
				//		Stroke = Brushes.Blue,
				//		Fill = Brushes.Blue,
				//		Height = markerSize,
				//		Width = markerSize
				//	};
				//	Canvas.SetLeft(marker, cp1[i].X - markerSize / 2);
				//	Canvas.SetTop(marker, cp1[i].Y - markerSize / 2);
				//	canvas.Children.Add(marker);

				//	//	// Second control point (Green)
				//	marker = new Ellipse()
				//	{
				//		Stroke = Brushes.Green,
				//		Fill = Brushes.Green,
				//		Height = markerSize,
				//		Width = markerSize
				//	};
				//	Canvas.SetLeft(marker, cp2[i].X - markerSize / 2);
				//	Canvas.SetTop(marker, cp2[i].Y - markerSize / 2);
				//	canvas.Children.Add(marker);
				//}

				// Print points
				//Trace.WriteLine(string.Format("Start=({0})", points[0]));
				//for (int i = 0; i < cp1.Length; ++i)
				//{
				//    Trace.WriteLine(string.Format("CP1=({0}) CP2=({1}) Stop=({2})"
				//        , cp1[i], cp2[i], points[i + 1]));
				//}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("\n" + ex + "\n");
			}
		}

		public CustomBezierBuilder(Point[] incomingPoints, Canvas canvas)
		{
			this.points = incomingPoints;
			this.canvas = canvas;
		}
	}
	
}
