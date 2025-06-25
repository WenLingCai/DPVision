using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DPVision.Core
{
    public class BindableParamBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// 通用setter，自动通知属性变更
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string prop = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(prop);
            return true;
        }
    }
}
