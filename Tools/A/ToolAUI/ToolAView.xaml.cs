using System.Windows.Controls;
using ToolContracts;

namespace ToolAUI
{
    public partial class ToolAView : UserControl, IToolUI
    {
        public ToolAView()
        {
            InitializeComponent();
        }

        public void SetViewModel(object viewModel)
        {
            this.DataContext = viewModel;
        }

        public object GetControl() => this;
    }
}