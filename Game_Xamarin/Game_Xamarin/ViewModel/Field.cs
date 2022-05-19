using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Game_Xamarin.ViewModel
{
    //Egy mező osztálya
    public class Field : ViewModelBase
    {
        #region fields
        //mező háttérszíne
        private Color _color;
        #endregion

        #region properties
        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged("Color");
            }
        }

        public Int32 X { get; set; }

        public Int32 Y { get; set; }

        public Int32 Number { get; set; }

        #endregion
    }
}
