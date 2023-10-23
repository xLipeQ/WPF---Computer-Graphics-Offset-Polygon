using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Project1
{
    public class Vertex
    {
        private Point _point;

        public Point Point { get { return _point; } set { _point = value; } }

        #region HorizontalPrevious
        private bool _HorizontalPrevious = false;

        /// <summary>
        /// Indicates if should have horizontal edge with previos one
        /// </summary>
        public bool HorizontalPrevious
        {
            get => _HorizontalPrevious;
            set => _HorizontalPrevious = value;
        }
        #endregion

        #region VerticalPrevious
        private bool _VerticalPrevious = false;

        /// <summary>
        /// Indicates if should have Vertical edge with previos one
        /// </summary>
        public bool VerticalPrevious
        {
            get => _VerticalPrevious;
            set => _VerticalPrevious = value;
        }

        #endregion

        #region HorizontalNext
        private bool _HorizontalNext = false;

        /// <summary>
        /// Indicates if should have horizontal edge with next one
        /// </summary>
        public bool HorizontalNext
        {
            get => _HorizontalNext;
            set => _HorizontalNext = value;
        }
        #endregion

        #region VerticalNext
        private bool _VerticalNext;
        /// <summary>
        /// Indicates if should have horizontal edge with next one
        /// </summary>
        public bool VerticalNext
        {
            get => _VerticalNext;
            set => _VerticalNext = value;
        }
        #endregion

        public Vertex(Point point)
        {
            _point = point;
        }

    }
}
