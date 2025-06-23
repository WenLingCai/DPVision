using System.Windows.Controls;
using ToolContracts;

namespace DPVToolTemplateMatchUI
{
    public partial class DPVToolTemplateMatchView : UserControl, IToolUI
    {
        public DPVToolTemplateMatchView()
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