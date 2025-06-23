using System.Windows.Controls;
using ToolContracts;

namespace ToolBUI
{
    public partial class ToolBView : UserControl, IToolUI
    {
        public ToolBView()
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