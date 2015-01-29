using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Saint.TestSetting
{
    [Serializable]
    public class ColorfulAssay : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        Color _color;
        string _name;

        public Color Color
        {
            get
            {
                return _color;
            }

            set
            {
                _color = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Color"));
            }
        }

        [XmlAttribute("Name")]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }

        public override string ToString()
        {
            return _name;
        }

        public ColorfulAssay(string name, Color color)
        {
            _name = name;
            _color = color;
        }

        public ColorfulAssay()
        {
            _color = Colors.Green;
            _name = "dummy";
        }
    }
}
