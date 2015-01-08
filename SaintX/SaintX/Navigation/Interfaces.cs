using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SaintX.Interfaces
{
    public interface IStageControl
    {
        event EventHandler onFinished;
        void NotifyFinished();
        Stage CurStage { get; }
    }

     public interface IHost
    {
        event EventHandler onStageChanged;
    }
}
