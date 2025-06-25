using Prism.Mvvm;
using Prism.Commands;

namespace PrismDryIocDemo
{
    public class MainWindowViewModel : BindableBase
    {
        private string _greeting = "你好，Prism MVVM！";
        public string Greeting
        {
            get => _greeting;
            set => SetProperty(ref _greeting, value);
        }

        public DelegateCommand ChangeGreetingCommand { get; }

        public MainWindowViewModel()
        {
            ChangeGreetingCommand = new DelegateCommand(ChangeGreeting);
        }

        private void ChangeGreeting()
        {
            Greeting = Greeting == "你好，Prism MVVM！"
                ? "Hello, Prism with .NET 8!"
                : "你好，Prism MVVM！";
        }
    }
}