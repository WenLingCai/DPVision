using System.ComponentModel;
using System.Windows.Input;
using ToolContracts;

namespace ToolAUI
{
    // 包装逻辑，提供WPF数据绑定/命令
    public class ToolAViewModelAdapter : INotifyPropertyChanged
    {
        public ITool Logic { get; }
        public ICommand ExecuteCommand { get; }
        public object Parameters => Logic.Parameters;

        public ToolAViewModelAdapter(ITool logic)
        {
            Logic = logic;
            ExecuteCommand = new RelayCommand(() => Logic.Run());
            Logic.ParametersChanged += (s, e) => OnPropertyChanged(nameof(Parameters));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

 
     public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

}