using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml.Serialization;

namespace SaintX.Data
{
    [Serializable]
    class ColorfulAssay
    {
        Color _color;
        string _name;
        [XmlAttribute("AssayColor")]
        public Color Color
        {
            get
            {
                return _color;
            }

            set
            {
                _color = value;
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
            }
        }

        public ColorfulAssay(string name, Color color)
        {
            _name = name;
            _color = color;
        }

        public ColorfulAssay()
        {
        }

    }
}
