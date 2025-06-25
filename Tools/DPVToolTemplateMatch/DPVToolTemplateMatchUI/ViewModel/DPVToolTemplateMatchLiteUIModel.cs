using DPVision.Core;
using DPVision.Model.Tool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DPVToolTemplateMatchUI.ViewModel
{
    public class DPVToolTemplateMatchLiteUIModel : INotifyPropertyChanged
    {
        private readonly ITool _tool; // 只持有接口引用
       
        public DPVToolTemplateMatchLiteUIModel(ITool tool)
        {
            _tool = tool;
           
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
                    if(_tool!=null)
                    {
                        _tool.SetParam("Threshold", value.ToString()); // 变更时同步到底层
                    } 
                }
            }
        }
        public void Load()
        {
            string v = "";
            if (_tool.GetParam("Threshold", ref v))
            {
                Threshold = int.Parse(v);
            }
        }

        public void Save()
        {
            string v = "";
            if (_tool.GetParam("Threshold", ref v))
            {
                Threshold = int.Parse(v);
            }
        }
    }
}
