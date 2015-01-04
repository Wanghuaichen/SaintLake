using SaintX.Utility;
using SaintX.viewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SaintX
{
    class StepViewModel : ViewModelBase
    {
        ObservableCollection<StepDesc> stepDescs = new ObservableCollection<StepDesc>();
        public StepViewModel()
        {
            string sDataFolder =  FolderHelper.GetImageFolder();
            BitmapImage scan = new BitmapImage(new Uri(sDataFolder + "scan.jpg"));
            BitmapImage dissolve = new BitmapImage(new Uri(sDataFolder + "dissolve.png"));
            BitmapImage tick = new BitmapImage(new Uri(sDataFolder + "tick.png"));
            BitmapImage scanPlate = new BitmapImage(new Uri(sDataFolder + "plate.jpg"));
            BitmapImage package = new BitmapImage(new Uri(sDataFolder + "pipetting.png"));
            stepDescs.Add(new StepDesc("扫描试管", scan, Stage.AssayDef));
            stepDescs.Add(new StepDesc("添加DMSO", dissolve, Stage.BarcodeDef));
            stepDescs.Add(new StepDesc("标识溶解", tick, Stage.GenerateScript));
        }

      

        public ObservableCollection<StepDesc> StepsModel
        {
            get
            {
                return stepDescs;
            }
            set
            {
                stepDescs = value;
            }
        }
    }


    public enum Stage
    {
        AssayDef = 0,
        BarcodeDef = 1,
        GenerateScript
    };

    class StepDesc
    {
        string name;
        Stage correspondingStage;
        BitmapImage image;

        public StepDesc(string name, BitmapImage bmp, Stage stage)
        {
            this.name = name;
            this.image = bmp;
            correspondingStage = stage;
        }
        public Stage CorrespondingStage
        {
            get { return correspondingStage; }
            set { correspondingStage = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public BitmapImage Image
        {
            get { return image; }
            set { image = value; }
        }
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            StepDesc anotherDesc = obj as StepDesc;
            if ((System.Object)anotherDesc == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (name == anotherDesc.name);
        }

        public static bool operator ==(StepDesc a, StepDesc b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(StepDesc a, StepDesc b)
        {
            return !(a == b);
        }

    }
}
