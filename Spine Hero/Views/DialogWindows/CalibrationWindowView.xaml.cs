using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineHero.Views.DialogWindows
{
    public partial class CalibrationWindowView
    {
        public CalibrationWindowView()
        {
            MouseLeftButtonDown += delegate { DragMove(); };
        }
    }
}
