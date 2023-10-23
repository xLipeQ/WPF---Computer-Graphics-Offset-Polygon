using Project1.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Project1
{
    public class CustomLine
    {
        #region Static For dragging

        public static VertexControl VB = null;

        public static VertexControl VE = null;

        public static bool IsDragged = false;

        public static bool JustClicked = false;

        public static Point MousePositionClicked;

        #endregion

        private Line _Line;
        private BresenhamLine _BLine;

        /// <summary>
        /// Vertex from which line is drawn
        /// </summary>
        private VertexControl _VertexBegin;

        public VertexControl VertexBegin { get => _VertexBegin; set => _VertexBegin = value; }

        private VertexControl _VertexEnd;
        public VertexControl VertexEnd { get => _VertexEnd; set => _VertexEnd = value; }


        private Rectangle Rectangle;

        private Polygon _Polygon;

        private Canvas Canvas;

        public CustomLine(VertexControl vertexBegin, VertexControl vertexEnd, Polygon polygon, Canvas CV, Line line = null, BresenhamLine BLine = null)
        {
            _Line = line;
            _BLine = BLine;
            _VertexBegin = vertexBegin;
            _VertexEnd = vertexEnd;

            if(_Line != null)
            {
                _Line.MouseLeftButtonDown += Line_MouseLeftButtonDown;
                _Line.MouseLeftButtonUp += Line_MouseLeftButtonUp;
            }
            else
            {
                _BLine.MouseLeftButtonDown += Line_MouseLeftButtonDown;
                _BLine.MouseLeftButtonUp += Line_MouseLeftButtonUp;
            }

            _Polygon = polygon;
            Canvas = CV;

            CreateRectangle();
        }

        private void CreateRectangle()
        {
            float DesiredWidth = 0;
            float DesiredHeight = 0;
            float CanvasTop = 0 ;
            float CanvasLeft = 0;

            if(_VertexBegin.Vertex.HorizontalNext)
            {
                DesiredWidth = 50;
                DesiredHeight = 10;

                CanvasTop = (float)_VertexBegin.Vertex.Point.Y - DesiredHeight/2;
                CanvasLeft = _VertexBegin.Vertex.Point.X > _VertexEnd.Vertex.Point.X ?
                (float)(_VertexBegin.Vertex.Point.X - _VertexEnd.Vertex.Point.X) / 2 + (float)_VertexEnd.Vertex.Point.X - DesiredWidth / 2:
                (float)(_VertexEnd.Vertex.Point.X - _VertexBegin.Vertex.Point.X) / 2 + (float)_VertexBegin.Vertex.Point.X - DesiredWidth / 2;

            }
            else if (_VertexBegin.Vertex.VerticalNext)
            {
                DesiredWidth = 10;
                DesiredHeight = 50;

                CanvasLeft = (float)_VertexBegin.Vertex.Point.X - DesiredWidth/2;
                CanvasTop = _VertexBegin.Vertex.Point.Y > _VertexEnd.Vertex.Point.Y ?
                (float)(_VertexBegin.Vertex.Point.Y - _VertexEnd.Vertex.Point.Y) / 2 + (float)_VertexEnd.Vertex.Point.Y - DesiredHeight / 2:
                (float)(_VertexEnd.Vertex.Point.Y - _VertexBegin.Vertex.Point.Y) / 2 + (float)_VertexBegin.Vertex.Point.Y - DesiredHeight / 2;
            }

            Rectangle = new Rectangle()
            {
                Width = DesiredWidth,
                Height = DesiredHeight,
                Style = _Polygon.Selected ? Application.Current.Resources["RectangleSelected"] as Style : Application.Current.Resources["RectangleBase"] as Style,
            };

            // we have edit mode on
            if(MainWindow.Instance.SelectedMode == Mode.Edit && DesiredWidth == 0)
            {
                DesiredWidth = 10;
                DesiredHeight = 10;
                Rectangle = new Rectangle()
                {
                    Width = DesiredHeight,
                    Height = DesiredWidth,
                    Style = _Polygon.Selected ? Application.Current.Resources["RectangleSelected"] as Style : Application.Current.Resources["RectangleBase"] as Style,
                };

                CanvasLeft = _VertexBegin.Vertex.Point.X > _VertexEnd.Vertex.Point.X ?
                (float)(_VertexBegin.Vertex.Point.X - _VertexEnd.Vertex.Point.X) / 2 + (float)_VertexEnd.Vertex.Point.X - DesiredWidth / 2:
                (float)(_VertexEnd.Vertex.Point.X - _VertexBegin.Vertex.Point.X) / 2 + (float)_VertexBegin.Vertex.Point.X - DesiredWidth / 2;


                CanvasTop = _VertexBegin.Vertex.Point.Y > _VertexEnd.Vertex.Point.Y ?
                (float)(_VertexBegin.Vertex.Point.Y - _VertexEnd.Vertex.Point.Y) / 2 + (float)_VertexEnd.Vertex.Point.Y - DesiredHeight / 2:
                (float)(_VertexEnd.Vertex.Point.Y - _VertexBegin.Vertex.Point.Y) / 2 + (float)_VertexBegin.Vertex.Point.Y - DesiredHeight / 2;
            }

            Rectangle.MouseDown += Line_MouseLeftButtonDown;
            Rectangle.MouseUp += Line_MouseLeftButtonUp;

            Canvas.SetTop(Rectangle, CanvasTop);
            Canvas.SetLeft(Rectangle, CanvasLeft);

            this.Canvas.Children.Add(Rectangle);

        }

        private void Line_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsDragged = false;
            VE = VB = null;
        }

        private void Line_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (MainWindow.Instance.SelectedMode != Mode.Edit)
                return;

            if(Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                var p = e.GetPosition(Canvas);
                VertexControl VC = new VertexControl(_Polygon.Selected)
                {
                    Vertex = new Vertex(p),
                    Polygon = _Polygon,
                };
                _Polygon.InsertVertex(_VertexBegin, VC);
                return;
            }
            
            // make this Line Vertical
            if(Keyboard.IsKeyDown(Key.LeftAlt))
            {
                // we want to disable vertical
                if(VertexEnd.Vertex.VerticalPrevious)
                {
                    VertexBegin.Vertex.VerticalNext = VertexEnd.Vertex.VerticalPrevious = false;
                    goto End;
                }

                // we cannot have two vertical lines next to each other
                if (VertexEnd.Vertex.VerticalNext || VertexBegin.Vertex.VerticalPrevious)
                    goto End;

                // set vertical
                VertexBegin.Vertex.VerticalNext = VertexEnd.Vertex.VerticalPrevious = true;
                VertexEnd.Vertex.Point = new Point(VertexBegin.Vertex.Point.X, VertexEnd.Vertex.Point.Y);
            }

            // make this line horizontal
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                // we want to disable vertical
                if (VertexEnd.Vertex.HorizontalPrevious)
                {
                    VertexBegin.Vertex.HorizontalNext = VertexEnd.Vertex.HorizontalPrevious = false;
                    goto End;
                }
                // we cannot have two vertical lines next to each other
                if (VertexEnd.Vertex.HorizontalNext || VertexBegin.Vertex.HorizontalPrevious)
                    goto End;

                // set vertical
                VertexBegin.Vertex.HorizontalNext = VertexEnd.Vertex.HorizontalPrevious = true;
                VertexEnd.Vertex.Point = new Point(VertexEnd.Vertex.Point.X, VertexBegin.Vertex.Point.Y);
            }

        End:
            MainWindow.Instance.DrawAll();
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsDragged = true;
                JustClicked = true;
                VE = VertexEnd;
                VB = VertexBegin;
                MousePositionClicked = e.GetPosition(Canvas);
            }
        }


    }
}
