using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Saint.TestSetting
{
    [Serializable]
    public class TestSetting : BindableBase
    {
        private ObservableCollection<ColorfulAssay> assays = new ObservableCollection<ColorfulAssay>();
        private string protocolFileName;
        private string settingName;

        public string Name
        {
            get
            {
                return settingName;
            }
            set
            {
                SetProperty(ref settingName, value);
            }
        }

        public override string ToString()
        {
            return settingName;
        }

        public ObservableCollection<ColorfulAssay> Assays
        {
            get
            {
                return assays;
            }
            set
            {
                SetProperty(ref assays, value);
            }
        }

        public string ProtocolFileName
        {
            get
            {
                return protocolFileName;
            }
            set
            {
                SetProperty(ref protocolFileName, value);
            }
        }

    }

   
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value))

                return false; storage = value; this.OnPropertyChanged(propertyName); return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;

            if (eventHandler != null)

            { eventHandler(this, new PropertyChangedEventArgs(propertyName)); }
        }
    }
}
