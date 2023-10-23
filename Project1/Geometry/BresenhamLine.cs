using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Project1.Geometry
{
    public class BresenhamLine : UIElement
    {
        public Vertex P1 { get; set; }
        public Vertex P2 { get; set; }

        private List<Rect> Pixels;

        private bool _Selected = false;

        public BresenhamLine(Vertex p1, Vertex p2, bool Selected)
        {
            P1 = p1;
            P2 = p2;
            _Selected = Selected;
            CreateLine();
        }

        private void CreateLine()
        {
            Pixels = new List<Rect>();

            int x2 = (int)P2.Point.X;
            int y2 = (int)P2.Point.Y;
            int x = (int)P1.Point.X;
            int y = (int)P1.Point.Y;

            int w = x2 - x; // width
            int h = y2 - y; // height
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;

            if (w < 0) 
                dx1 = -1; 
            else if (w > 0) 
                dx1 = 1;

            if (h < 0) 
                dy1 = -1; 
            else if (h > 0) 
                dy1 = 1;

            if (w < 0) 
                dx2 = -1; 
            else if (w > 0) 
                dx2 = 1;

            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);

            if (shortest >= longest)
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) 
                    dy2 = -1; 
                else if (h > 0) 
                    dy2 = 1;
                dx2 = 0;
            }

            int numerator = longest / 2;
            
            for (int i = 0; i <= longest; i++)
            {
                Pixels.Add(new Rect(new Point(x, y), new Point(x + 1, y + 1)));
                numerator += shortest;
                if (longest <= numerator)
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if(_Selected)
                foreach(var pixel in Pixels)
                {
                    drawingContext.DrawRectangle(Brushes.Black, new Pen(Brushes.Blue, 1), pixel);
                }

            else
                foreach (var pixel in Pixels)
                {
                    drawingContext.DrawRectangle(Brushes.Black, new Pen(Brushes.Black, 1), pixel);
                }
            base.OnRender(drawingContext);
        }

    }
}
