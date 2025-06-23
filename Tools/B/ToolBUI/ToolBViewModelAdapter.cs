using System.ComponentModel;
using System.Windows.Input;
using ToolContracts;

namespace ToolBUI
{
    // 包装逻辑，提供WPF数据绑定/命令
    public class ToolBViewModelAdapter : INotifyPropertyChanged
    {
        public ITool Logic { get; }
        public ICommand ExecuteCommand { get; }
        public object Parameters => Logic.Parameters;

        public ToolBViewModelAdapter(ITool logic)
        {
            Logic = logic;
            ExecuteCommand = new RelayCommand(() => Logic.Execute());
            Logic.ParametersChanged += (s, e) => OnPropertyChanged(nameof(Parameters));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    // 简易命令
    public class RelayCommand : ICommand
    {
        private readonly Action _exe;
        public RelayCommand(Action exe) => _exe = exe;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _exe();
        public event EventHandler CanExecuteChanged;
    }
}