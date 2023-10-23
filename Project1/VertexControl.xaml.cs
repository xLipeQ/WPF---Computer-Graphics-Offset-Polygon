using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Project1
{
    /// <summary>
    /// Logika interakcji dla klasy VertexControl.xaml
    /// </summary>
    public partial class VertexControl : UserControl
    {
        public static bool IsDragged = false;
        public static VertexControl CurrentDraggedVertex;

        public Vertex Vertex { get; set; }

        public Polygon Polygon { get; set; }

        private bool _Selected;
        public bool Selected
        {
            get => _Selected;
            set
            {
                VertexElipse.Style = value ? this.Resources["Selected"] as Style : this.Resources["Template"] as Style;
                _Selected = value;
            }
        }

        public VertexControl(bool selected = false)
        {
            InitializeComponent();

            if (selected)
                VertexElipse.Style = this.Resources["Selected"] as Style;

        }

        private void VertexElipse_MouseRightDown(object sender, MouseButtonEventArgs e)
        {
            if (Polygon == null)
                return;

            e.Handled = true;
            Polygon.DeleteVertex(this);

            if(Polygon.Vertices.Count <= 2)
            {
                MainWindow.Instance.Polygons.Remove(Polygon);
                MainWindow.Instance.CreateNewPolygon();
                MainWindow.Instance.DrawAll();
            }
        }

        private void VertexElipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Polygon == null)
                return;

            IsDragged = true;
            CurrentDraggedVertex = this;

            if (MainWindow.Instance.SelectedMode == Mode.Create && Polygon.Vertices[0] == this)
                MainWindow.Instance.CloseCurrentPolygon();

            MainWindow.Instance.MouseDownHandled = true;
        }
    }
}
