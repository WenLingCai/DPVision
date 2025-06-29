
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DPVFlowMarkView.ViewModels
{
    public class DPVFlowMarkViewModel : BindableBase
    {
       
        public DPVFlowMarkViewModel()
        {
         
        }
        // 你自己要声明这个事件！
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            // 事件判空，触发通知
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private int _threshold;
        public int Threshold
        {
            get => _threshold;
            set
            {
                if (_threshold != value)
                {
                    _threshold = value;
                    OnPropertyChanged();
                  
                }
            }
        }
        public void Load()
        {
            string v = "";
            
        }

        public void Save()
        {
            string v = "";
            
        }
    }
}
