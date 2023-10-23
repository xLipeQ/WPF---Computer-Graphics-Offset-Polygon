using Project1.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Project1
{
    public class Polygon : INotifyPropertyChanged
    {
        #region Property Changed
        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties
        
        private static Canvas DrawingCanvas;

        private static int index = 0;

        private int MyIndex = 0;

        private string _Name;
        
        public string Name 
        { 
            get => _Name; 
            set 
            {
                _Name = value;
                OnPropertyChanged(nameof(Name));
            } 
        }

        private List<VertexControl> _Vertices = new List<VertexControl>();

        public List<VertexControl> Vertices { get { return _Vertices; } }

        private bool _Closed;

        private bool _Selected;

        public bool Selected
        {
            get => _Selected;
            set
            {
                if (Selected == value)
                    return;

                _Selected = value;
                foreach (var v in Vertices)
                    v.Selected = value;

                DrawingCanvas.Children.Clear();
            }
        }

        private OffsetPolygon OffSetPolygon;

        public float MaxOffset = 0;

        #endregion

        public void AddVertex(VertexControl vertex, Canvas cv)
        {
            if (DrawingCanvas == null)
                DrawingCanvas = cv;

            Vertices.Add(vertex);
            Name = $"Polgon nr. {MyIndex} has {Vertices.Count} vertex";
        }

        public void InsertVertex(VertexControl VertexToInsertFront, VertexControl VertexToInsert)
        {
            int index = Vertices.FindIndex(v => v.Equals(VertexToInsertFront));

            // Next vertex has no option on edge
            Vertices[index].Vertex.VerticalNext = Vertices[index].Vertex.HorizontalNext = false;
            
            // we are out of array
            if(index + 1 == Vertices.Count)
                Vertices[0].Vertex.VerticalPrevious = Vertices[0].Vertex.HorizontalPrevious = false;
            // we are in array
            else
                Vertices[index + 1].Vertex.VerticalPrevious = Vertices[index + 1].Vertex.HorizontalPrevious = false;

            Vertices.Insert(index + 1, VertexToInsert);
            MainWindow.Instance.DrawAll();
            Name = $"Polgon nr. {MyIndex} has {Vertices.Count} vertex";
        }

        public void DeleteVertex(VertexControl vertex)
        {
            int index = Vertices.FindIndex(t => t.Equals(vertex));

            if(vertex.Vertex.HorizontalPrevious || vertex.Vertex.VerticalPrevious)
            {
                Vertices[index - 1 < 0 ? Vertices.Count - 1 : index - 1].Vertex.HorizontalNext = false;
                Vertices[index - 1 < 0 ? Vertices.Count - 1 : index - 1].Vertex.VerticalNext = false;
            }

            if (vertex.Vertex.HorizontalNext || vertex.Vertex.VerticalNext)
            {
                Vertices[index + 1 >= Vertices.Count ? 0 : index + 1].Vertex.HorizontalPrevious = false;
                Vertices[index + 1 >= Vertices.Count ? 0 : index + 1].Vertex.VerticalPrevious = false;
            }

            Vertices.Remove(vertex);

            if (Vertices.Count == 2)
            {
                _Closed = false;
                OffSetPolygon = null;
            }

            MainWindow.Instance.DrawAll();
            Name = $"Polgon nr. {MyIndex} has {Vertices.Count} vertex";
        }

        public void ClosePolygon()
        {
            if (Vertices.Count <= 2)
                return;
            _Closed = true;
            MainWindow.Instance.DrawAll();
        }

        public bool IsClosed() => _Closed;

        public void CreateOffsetPolygon(float offset)
        {
            OffSetPolygon = new OffsetPolygon(offset, this);
            OffSetPolygon.CreateOffsetPolygon();
        }

        public void DeleteOffsetPolygon()
        {
            OffSetPolygon = null;
        }

        #region Point Inside Helpers

        public bool PointInPolygon(Point p)
        {
            // cannot be inside not closed
            if (!_Closed)
                return false;

            int intersection = 0;
            // check every edge
            for (int i=1; i<Vertices.Count;i++)
            {
                if (IntersectionHelpers.DoIntersect(Vertices[i].Vertex.Point, Vertices[i-1].Vertex.Point,
                    p, new Point(p.X, 0)))
                    intersection++;
            }

            if (IntersectionHelpers.DoIntersect(Vertices[0].Vertex.Point, Vertices[Vertices.Count-1].Vertex.Point,
                    p, new Point(p.X, 0)))
                intersection++;

            MainWindow.Instance.LogText = $"Intersection: {intersection}";
            return intersection % 2 == 1;
        }

        #endregion

        #region Moving Semantic
        public void MoveVertex(VertexControl vertex, Point PreviosMousePosition, Point CurrentPosition, 
            bool AffectsPrev = true, bool AffectsNext = true)
        {
            // find vertex in array
            int begin = Vertices.FindIndex(v => v.Equals(vertex));

            // set vertex position
            vertex.Vertex.Point = CurrentPosition;

            // go for next
            int iter = begin + 1;
            
            // we are at back, we need a cycle
            if (iter >= Vertices.Count)
                iter = 0;

            MainWindow.Instance.LogText = $"HN:{vertex.Vertex.HorizontalNext}, HP:{vertex.Vertex.HorizontalPrevious},VN:{vertex.Vertex.VerticalNext}, VP:{vertex.Vertex.VerticalPrevious}";

            if(AffectsNext && Vertices[iter].Vertex.HorizontalPrevious)
            {
                Vertices[iter].Vertex.Point = new Point(Vertices[iter].Vertex.Point.X, 
                    Vertices[iter].Vertex.Point.Y + CurrentPosition.Y - PreviosMousePosition.Y);
            }

            else if (AffectsNext && Vertices[iter].Vertex.VerticalPrevious)
            {
                Vertices[iter].Vertex.Point = new Point(Vertices[iter].Vertex.Point.X + CurrentPosition.X - PreviosMousePosition.X, 
                    Vertices[iter].Vertex.Point.Y);
            }

            iter = begin - 1;
            if (iter < 0 && _Closed)
                iter = Vertices.Count - 1;

            if (AffectsPrev && Vertices[iter].Vertex.HorizontalNext)
            {
                Vertices[iter].Vertex.Point = new Point(Vertices[iter].Vertex.Point.X,
                    Vertices[iter].Vertex.Point.Y + CurrentPosition.Y - PreviosMousePosition.Y);
            }

            else if (AffectsPrev && Vertices[iter].Vertex.VerticalNext)
            {
                Vertices[iter].Vertex.Point = new Point(Vertices[iter].Vertex.Point.X + CurrentPosition.X - PreviosMousePosition.X,
                    Vertices[iter].Vertex.Point.Y);
            }

            // look forward
            //while (iter != begin && ( Vertices[iter].Vertex.HorizontalPrevious || Vertices[iter].Vertex.VerticalPrevious))
            //{
            //    // we havent closed
            //    if (iter == 0 && !_Closed)
            //        break;

            //    // set new value so as no to make two cycles
            //    previter = iter;

            //    // set new position
            //    Vertices[iter].Vertex.Point = 
            //        new Point(Vertices[iter].Vertex.Point.X + CurrentPosition.X - PreviosMousePosition.X,
            //        Vertices[iter].Vertex.Point.Y + CurrentPosition.Y - PreviosMousePosition.Y);

            //    iter++;

            //    // we are at back, we need a cycle
            //    if (iter >= Vertices.Count)
            //        iter = 0;

            //}

            // set up once again
            //iter = begin - 1;

            //if(iter < 0)
            //    iter = Vertices.Count - 1;


            //// look backward
            //while (iter != previter && ( Vertices[iter].Vertex.HorizontalNext || Vertices[iter].Vertex.VerticalNext))
            //{
            //    // we havent closed
            //    if (iter == Vertices.Count - 1 && !_Closed)
            //        break;

            //    // set point
            //    Vertices[iter].Vertex.Point =
            //        new Point(Vertices[iter].Vertex.Point.X + CurrentPosition.X - PreviosMousePosition.X,
            //        Vertices[iter].Vertex.Point.Y + CurrentPosition.Y - PreviosMousePosition.Y);

            //    // go to previous vertex
            //    iter--;

            //    // make cycle
            //    if (iter < 0)
            //        iter = Vertices.Count - 1;
            //}
        
        }

        public void MoveEdge(Point p, Point previous)
        {
            // previous is vertical or horizontal Edge
            if (CustomLine.VB.Vertex.HorizontalPrevious || CustomLine.VB.Vertex.VerticalPrevious)
            {
                MoveVertex(CustomLine.VB, CustomLine.VB.Vertex.Point,
                    new Point(CustomLine.VB.Vertex.Point.X + p.X - previous.X, CustomLine.VB.Vertex.Point.Y + p.Y - previous.Y),
                    true, false);

                // Subtract what MoveVertex adds
                CustomLine.VB.Vertex.Point = new Point(CustomLine.VB.Vertex.Point.X - p.X + previous.X,
                CustomLine.VB.Vertex.Point.Y - p.Y + previous.Y);
            }

            // next is vertical or horizontal Edge
            if (CustomLine.VE.Vertex.HorizontalNext || CustomLine.VE.Vertex.VerticalNext)
            {
                MoveVertex(CustomLine.VE, CustomLine.VE.Vertex.Point,
                    new Point(CustomLine.VE.Vertex.Point.X + p.X - previous.X, CustomLine.VE.Vertex.Point.Y + p.Y - previous.Y),
                    false, true);

                // Subtract what MoveVertex adds
                CustomLine.VE.Vertex.Point = new Point(CustomLine.VE.Vertex.Point.X - p.X + previous.X,
                CustomLine.VE.Vertex.Point.Y - p.Y + previous.Y);
            }

            CustomLine.VB.Vertex.Point = new Point(CustomLine.VB.Vertex.Point.X + p.X - previous.X,
                CustomLine.VB.Vertex.Point.Y + p.Y - previous.Y);

            CustomLine.VE.Vertex.Point = new Point(CustomLine.VE.Vertex.Point.X + p.X - previous.X,
                CustomLine.VE.Vertex.Point.Y + p.Y - previous.Y);

            CustomLine.MousePositionClicked = p;
        }

        public void MovePolygon(Point p)
        {
            foreach(var VertexControl in Vertices)            
                VertexControl.Vertex.Point = new Point(VertexControl.Vertex.Point.X + p.X - MainWindow.Instance.LastPoint.X,
                    VertexControl.Vertex.Point.Y + p.Y - MainWindow.Instance.LastPoint.Y);
        }

        #endregion

        #region Draw Section

        private void CreateLine(VertexControl begin, VertexControl end, bool OffsetPolygon = false)
        {
            if (MainWindow.Instance.Bresenham)
            {
                var line = new BresenhamLine(begin.Vertex, end.Vertex, Selected);

                if (!OffsetPolygon)
                    new CustomLine(begin, end, this, DrawingCanvas, null, line);

                DrawingCanvas.Children.Add(line);
            }
            else
            {
                var line = new Line
                {
                    Style = Selected ? Application.Current.Resources["LineSelected"] as Style : Application.Current.Resources["LineBase"] as Style,
                    StrokeThickness = 2.5,
                    X1 = begin.Vertex.Point.X,
                    X2 = end.Vertex.Point.X,
                    Y1 = begin.Vertex.Point.Y,
                    Y2 = end.Vertex.Point.Y,
                };

                if (!OffsetPolygon)
                    new CustomLine(begin, end, this, DrawingCanvas, line, null);

                // add line to canvas
                DrawingCanvas.Children.Add(line);
            }

        }

        private void SetVertex(VertexControl VC)
        {
            // add on canvas
            Canvas.SetTop(VC, VC.Vertex.Point.Y);
            Canvas.SetLeft(VC, VC.Vertex.Point.X);
            DrawingCanvas.Children.Add(VC);
        }

        public void DrawPolygon()
        {
            OffSetPolygon?.CreateOffsetPolygon();

            // means that no vertex was added - canvas is null
            if (Vertices.Count == 0)
                return;

            SetVertex(Vertices[0]);
            for (int i=1; i<Vertices.Count; i++)
            {
                // create line between this and previous
                CreateLine(Vertices[i-1], Vertices[i]);

                // add vertex
                SetVertex(Vertices[i]);
            }

            // make sure we can close 
            if(_Closed && Vertices.Count > 2)
            {
                // create line between this and previous
                CreateLine(Vertices[Vertices.Count - 1], Vertices[0]);
            }

            if (OffSetPolygon != null)
                DrawOutline();
        }

        private void DrawOutline()
        {
            SetVertex(OffSetPolygon.Vertices[0]);
            for (int i = 1; i < OffSetPolygon.Vertices.Count; i++)
            {
                // create line between this and previous
                CreateLine(OffSetPolygon.Vertices[i - 1], OffSetPolygon.Vertices[i], true);

                // add vertex
                SetVertex(OffSetPolygon.Vertices[i]);
            }

            CreateLine(OffSetPolygon.Vertices[OffSetPolygon.Vertices.Count - 1], OffSetPolygon.Vertices[0], true);
            
        }

        #endregion

        #region Constructor

        public Polygon()
        {
            MyIndex = index;
            index++;
            Name = $"Polgon nr.{MyIndex} Vertexes:{Vertices.Count}";
        }

        #endregion
    }


    public static class IntersectionHelpers
    {
        // Given three collinear points p, q, r, the function checks if 
        // point q lies on line segment 'pr' 
        public static bool OnSegment(Point p, Point q, Point r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
                return true;

            return false;
        }

        // To find orientation of ordered triplet (p, q, r). 
        // The function returns following values 
        // 0 --> p, q and r are collinear 
        // 1 --> Clockwise 
        // 2 --> Counterclockwise 
        public static int Orientation(Point p, Point q, Point r)
        {
            // See https://www.geeksforgeeks.org/orientation-3-ordered-points/ 
            // for details of below formula. 
            int val = (int)((q.Y - p.Y) * (r.X - q.X) -
                      (q.X - p.X) * (r.Y - q.Y));

            if (val == 0) return 0;  // collinear 

            return (val > 0) ? 1 : 2; // clock or counterclock wise 
        }

        // The main function that returns true if line segment 'p1q1' 
        // and 'p2q2' intersect. 
        public static bool DoIntersect(Point p1, Point q1, Point p2, Point q2)
        {
            // Find the four orientations needed for general and 
            // special cases 
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases 
            // p1, q1 and p2 are collinear and p2 lies on segment p1q1 
            if (o1 == 0 && OnSegment(p1, p2, q1)) return true;

            // p1, q1 and q2 are collinear and q2 lies on segment p1q1 
            if (o2 == 0 && OnSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are collinear and p1 lies on segment p2q2 
            if (o3 == 0 && OnSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are collinear and q1 lies on segment p2q2 
            if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases 
        }

        public static Point GetIntersectionPoint(Point a1, Point a2, Point b1, Point b2)
        {
            double d = (a1.X - a2.X) * (b1.Y - b2.Y) - (a1.Y - a2.Y) * (b1.X - b2.X);

            double px = (a1.X * a2.Y - a1.Y * a2.X) * (b1.X - b2.X) - (a1.X - a2.X) * (b1.X * b2.Y - b1.Y * b2.X);
            double py = (a1.X * a2.Y - a1.Y * a2.X) * (b1.Y - b2.Y) - (a1.Y - a2.Y) * (b1.X * b2.Y - b1.Y * b2.X);

            return new Point(px/d,py/d);
        }
    }
}
