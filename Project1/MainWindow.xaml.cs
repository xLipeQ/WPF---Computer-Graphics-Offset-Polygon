using Project1.Geometry;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Project1
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        #region PropertyChanged
        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        private ObservableCollection<Polygon> _Polygons = new ObservableCollection<Polygon>();

        public ObservableCollection<Polygon> Polygons { get { return _Polygons; } set { _Polygons = value; OnPropertyChanged(nameof(Polygons)); } }

        private Polygon SelectedPolygon = new Polygon();

        public static MainWindow Instance { get; private set; }

        private string _LogText;

        public string LogText
        {
            get => _LogText;
            set
            {
                _LogText = value;
                OnPropertyChanged(nameof(LogText));
            }
        }

        public Point LastPoint;

        public Mode SelectedMode = Mode.Create;

        public bool MouseDownHandled = false;

        private bool _Bresenham = false;

        public bool Bresenham 
        { 
            get => _Bresenham; 
            set 
            { 
                _Bresenham = !Bresenham; 
                OnPropertyChanged(nameof(Bresenham)); 
                DrawAll(); 
            } 
        } 
        #endregion

        #region Constructor
        public MainWindow()
        {
            Instance = this;
            DataContext = this; 
            InitializeComponent();
            SelectedPolygon = new Polygon();
            Polygons.Add(SelectedPolygon);

            DefaultCreation.CreateDefault(PolygonCanvas, Polygons);

            DrawAll();
        }

        #endregion

        public void CloseCurrentPolygon()
        {
            SelectedPolygon.ClosePolygon();

            if (!SelectedPolygon.IsClosed())
                return;

            if (SelectedPolygon == Polygons.Last())
                CreateNewPolygon();
        }

        public void CreateNewPolygon()
        {
            if(Polygons.Count == 0)
            {
                SelectedPolygon = new Polygon();
                Polygons.Add(SelectedPolygon);
                return;
            }

            if (!Polygons.Last().IsClosed())
                SelectedPolygon = Polygons.Last();
            else
            {
                SelectedPolygon = new Polygon();
                Polygons.Add(SelectedPolygon);
            }
        }

        public void DrawAll()
        {
            // clear Canvas
            PolygonCanvas.Children.Clear();

            // draw all polygons
            foreach (var Polygon in _Polygons)
                Polygon.DrawPolygon();
        }

        private void PolygonCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // we already handled this mouse down
            if(MouseDownHandled)
                return;

            var p = e.GetPosition(PolygonCanvas);
           
            // check in which polygon we are
            if(SelectedMode == Mode.Edit)
            {
                // unselect all
                UnSelectAll();
                SelectedPolygon = null;

                // choose polygon
                foreach (var Polygon in Polygons)
                {
                    if (Polygon.PointInPolygon(p))
                    {
                        Polygon.Selected = true;
                        SelectedPolygon = Polygon;
                        break;
                    }
                    else
                        Polygon.Selected = false;
                }

                // close current polygon
                if (SelectedPolygon != null && Keyboard.IsKeyDown(Key.LeftShift) && SelectedPolygon.Vertices.Count >= 3)
                {
                    SelectedPolygon.ClosePolygon();

                    CreateNewPolygon();
                }
            }   
            
            else if(SelectedMode == Mode.Create)
            {
                // create new vertex
                VertexControl VC = new VertexControl(SelectedPolygon.Selected)
                {
                    Vertex = new Vertex(p),
                    Polygon = SelectedPolygon,
                };

                SelectedPolygon.AddVertex(VC, PolygonCanvas);
            }

            DrawAll();

        }

        private void PolygonCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseDownHandled)
            {
                MouseDownHandled = false;
                return;
            }
            var p = e.GetPosition(PolygonCanvas);

            if (SelectedMode != Mode.Edit)
                return;

            // we move mouse 
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // move vertex
                if (VertexControl.IsDragged && VertexControl.CurrentDraggedVertex != null)
                {
                    VertexControl.CurrentDraggedVertex.Polygon.MoveVertex(VertexControl.CurrentDraggedVertex, 
                        VertexControl.CurrentDraggedVertex.Vertex.Point, p);
                }

                if (CustomLine.IsDragged && CustomLine.VB != null)
                {
                    CustomLine.VB.Polygon.MoveEdge(p, LastPoint);
                    LogText = $"Moved Begin Point to {CustomLine.VB.Vertex.Point} \nEnd Point to {CustomLine.VE.Vertex.Point}";
                }

                // we can move polygon
                if (Keyboard.IsKeyDown(Key.LeftAlt) && SelectedPolygon != null)
                    SelectedPolygon.MovePolygon(p);

                // we have it here to draw only when sth changes
                DrawAll();

            }

            else
            {
                VertexControl.IsDragged = false;
                VertexControl.CurrentDraggedVertex = null;

                CustomLine.VB = CustomLine.VE = null;
                CustomLine.IsDragged = true;
            }
            LastPoint = p;
        }

        private void PolygonCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            VertexControl.IsDragged = false;
            VertexControl.CurrentDraggedVertex = null;
        }

        private void UnSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectedPolygon.Selected = false;
            PolygonSelection.SelectedIndex = -1;
            SelectedPolygon = Polygons.Last();
            DrawAll();
        }

        private void PolygonSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PolygonSelection.SelectedIndex == -1)
                return;

            SelectedPolygon.Selected = false;
            SelectedPolygon = (Polygon)PolygonSelection.SelectedItem;
            SelectedPolygon.Selected = true;
            DrawAll();
        }

        private void Mode_Checked(object sender, RoutedEventArgs e)
        {
            SelectedMode = ModeHelpers.StringToWorkState(((RadioButton)sender).Content.ToString());

            if (Polygons.Count == 0)
                return;

            // we want to know if we should create one more
            if(SelectedMode == Mode.Create)
            {
                // unselect all
                UnSelectAll();

                if (Polygons.Last().IsClosed())
                {
                    SelectedPolygon = new Polygon();
                    Polygons.Add(SelectedPolygon);
                }
                else
                    SelectedPolygon = Polygons.Last();
            }

            if (SelectedMode == Mode.Edit)
                UnSelectAll();

            if(SelectedMode == Mode.Otoczka)
            {
                if(SelectedPolygon == null || !SelectedPolygon.IsClosed())
                {
                    MessageBox.Show("Select Closed Polygon first");
                    EditRB.IsChecked = true;
                    return;
                }
            }

            DrawAll();
        }

        private void UnSelectAll()
        {
            foreach (var Polygon in Polygons)
                Polygon.Selected = false;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue < 2)
                SelectedPolygon.DeleteOffsetPolygon();
            SelectedPolygon.CreateOffsetPolygon((float)e.NewValue);
            DrawAll();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Polygons.Clear();
            SelectedPolygon = new Polygon();
            Polygons.Add(SelectedPolygon);

            PolygonCanvas.Children.Clear();
        }
    }

    public static class DefaultCreation
    {
        public static void CreateDefault(Canvas PolygonCanvas, ObservableCollection<Polygon> Polygons)
        {
            var pol = new Polygon();
            // create new vertex
            VertexControl VC = new VertexControl()
            {
                Vertex = new Vertex(new Point(10, 40)),
                Polygon = pol,
            };
            pol.AddVertex(VC, PolygonCanvas);

            VC = new VertexControl()
            {
                Vertex = new Vertex(new Point(100, 50)),
                Polygon = pol,
            };
            pol.AddVertex(VC, PolygonCanvas);

            VC = new VertexControl()
            {
                Vertex = new Vertex(new Point(150, 300)),
                Polygon = pol,
            };

            pol.AddVertex(VC, PolygonCanvas);
            pol.ClosePolygon();
            Polygons.Add(pol);

            // SECOND
            pol = new Polygon();
            // create new vertex
            VC = new VertexControl()
            {
                Vertex = new Vertex(new Point(500, 50)),
                Polygon = pol,
            };
            pol.AddVertex(VC, PolygonCanvas);

            VC = new VertexControl()
            {
                Vertex = new Vertex(new Point(550, 50)),
                Polygon = pol,
            };
            pol.AddVertex(VC, PolygonCanvas);

            VC = new VertexControl()
            {
                Vertex = new Vertex(new Point(525, 300)),
                Polygon = pol,
            };

            pol.AddVertex(VC, PolygonCanvas);
            pol.ClosePolygon();
            Polygons.Add(pol);

        }
    }
}
