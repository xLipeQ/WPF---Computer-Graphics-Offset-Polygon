using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;

namespace Project1
{
    public class OffsetPolygon
    {
        public List<VertexControl> Vertices = new List<VertexControl>(30);

        public List<VertexControl> GetVertexes() => Vertices;

        public Polygon PolygonToOffset { get; set; }

        private float _Offset;
        public float Offset { get => _Offset; set
            {
                if (PolygonToOffset.MaxOffset == 0)
                {
                    _Offset = value;
                    return;
                }

                _Offset = value > PolygonToOffset.MaxOffset ? PolygonToOffset.MaxOffset : value;
            }
        }

        public OffsetPolygon(float off, Polygon p)
        {
            PolygonToOffset = p;
            Offset = off;
        }

        public void CreateOffsetPolygon(int ccw = 1)
        {
            Vertices.Clear();
            bool IsInside = false;
            bool Checked = false;

            // store vertex count for easier access
            int VCount = PolygonToOffset.Vertices.Count;
            for(int i=0; i < VCount ; i++)
            {
                // get current vertex and prev
                VertexControl PrevVC, CurrVC, NextVC;

                if(i == 0)
                {
                    PrevVC = PolygonToOffset.Vertices[VCount - 1];
                    CurrVC = PolygonToOffset.Vertices[i];
                    NextVC = PolygonToOffset.Vertices[i + 1];
                }
                else if(i == VCount - 1)
                {
                    PrevVC = PolygonToOffset.Vertices[i-1];
                    CurrVC = PolygonToOffset.Vertices[i];
                    NextVC = PolygonToOffset.Vertices[0];
                }
                else
                {
                    PrevVC = PolygonToOffset.Vertices[i - 1];
                    CurrVC = PolygonToOffset.Vertices[i];
                    NextVC = PolygonToOffset.Vertices[i + 1];
                }

                // v1 - Vector between CurrVC and NextVC
                double v1X = NextVC.Vertex.Point.X - CurrVC.Vertex.Point.X;
                double v1Y = NextVC.Vertex.Point.Y - CurrVC.Vertex.Point.Y;

                // Vector perpendicular to v1
                Point v1 = new Point(v1X, v1Y);
                Point v1n;

                // normalize it
                v1n = NormalizeVector(v1);
                
                double v1nX = v1n.Y;
                double v1nY = -v1n.X;

                // v2 -  Vector between PrevVC and CurrVC
                double v2X = CurrVC.Vertex.Point.X - PrevVC.Vertex.Point.X;
                double v2Y = CurrVC.Vertex.Point.Y - PrevVC.Vertex.Point.Y;

                // normalize
                Point v2 = new Point(v2X, v2Y);
                Point v2n = NormalizeVector(v2);

                double v2nX = v2n.Y;
                double v2nY = -v2n.X;

                // Vector of bisector
                if (IsInside)
                {
                    ccw *= -1;
                    IsInside = false;
                }

                double bisX = (v1nX + v2nX) * ccw;
                double bisY = (v1nY + v2nY) * ccw;

                Point v = new Point(bisX, bisY);
                Point vn = NormalizeVector(v);

                // length from trignometry len = offset/cos(a/2) = dst/sqrt((1+cosa)/2)
                // cosa/2 = |u,v| = xu*xv + yu*yv
                double len = Offset / Math.Sqrt((double)((1 + v1nX * v2nX + v1nY * v2nY) / 2));

                // create new vertex 
                Vertex V = new Vertex(new Point(CurrVC.Vertex.Point.X + len * vn.X, CurrVC.Vertex.Point.Y + len * vn.Y));
                Vertex VControl = new Vertex(new Point(CurrVC.Vertex.Point.X + vn.X, CurrVC.Vertex.Point.Y + vn.Y));


                if (!Checked && PolygonToOffset.PointInPolygon(VControl.Point))
                {
                    IsInside = true;
                    i = -1;
                    continue;
                }
                else
                    Checked = true;

                VertexControl VC = new VertexControl()
                {
                    Vertex = V
                };

                Vertices.Add(VC);
            }
            if (!SolveIntersection())
            {
                PolygonToOffset.MaxOffset = Offset;
            }
        }

        private Point NormalizeVector(Point ToNormalize)
        {
            var distance = Math.Sqrt(ToNormalize.X * ToNormalize.X + ToNormalize.Y * ToNormalize.Y);
            return new Point(ToNormalize.X / distance, ToNormalize.Y / distance);
        }

        /// <summary>
        /// Checks if last added edge intersects with any different
        /// </summary>
        private bool SolveIntersection()
        {
            // only two edges
            if (Vertices.Count <= 3)
                return true;

            List<VertexControl> NewVertices = new List<VertexControl> (Vertices.Count);

            for(int i = 1; i<Vertices.Count; i++)
            {
                // get edge which we are checking with every other
                var prev = Vertices[i-1];
                var curr = Vertices[i];

                bool intersects = false;
                int jindex = -1;
                Point InterPoint = new Point(0,0);

                for(int j = i + 2; j<Vertices.Count; j++) 
                { 
                    if(IntersectionHelpers.DoIntersect(prev.Vertex.Point, curr.Vertex.Point,
                        Vertices[j-1].Vertex.Point, Vertices[j].Vertex.Point)) 
                    {
                        intersects = true;

                        InterPoint = IntersectionHelpers.GetIntersectionPoint(prev.Vertex.Point, curr.Vertex.Point,
                        Vertices[j - 1].Vertex.Point, Vertices[j].Vertex.Point);

                        jindex = j;
                    }  
                }

                if(!intersects)
                {
                    // add this edge to new list - it doesnt intersect
                    if(!NewVertices.Contains(prev))
                        NewVertices.Add(prev);

                    NewVertices.Add(curr);
                }

                // intersects
                else
                {
                    // add this edge to new list
                    if (!NewVertices.Contains(prev))
                        NewVertices.Add(prev);

                    // add intersection point to new vertices
                    var VC = new VertexControl() { Vertex = new Vertex(InterPoint) };
                    NewVertices.Add(VC);

                    // add end point of intersection edge
                    NewVertices.Add(Vertices[jindex]);

                    // dont analyze those between now
                    i = jindex + 1;

                    //return false;
                }

            }
            Vertices = NewVertices;
            return true;

        }

    }
}
