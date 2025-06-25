using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

using AutoMapper;


namespace DPVision.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public ObservableCollection<ItemModel> Items { get; } = new ObservableCollection<ItemModel>();
        public DelegateCommand<ItemModel> SelectionChangedCommand { get; }
        public ObservableCollection<string> FlowItems { get; } = new ObservableCollection<string>()
        {
            "标定流程", "Mark流程1", "Mark流程2"
        };
        private string _selectedItem;
        public string SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public MainWindowViewModel()
        {
            // 初始化数据
            Items.Add(new ItemModel(1, "标定流程"));
            Items.Add(new ItemModel(2, "Mark流程1"));
            Items.Add(new ItemModel(3, "Mark流程2"));


            // 初始化命令
            SelectionChangedCommand = new DelegateCommand<ItemModel>(OnSelectionChanged);
        }
        private void OnSelectionChanged(ItemModel selectedItem)
        {
            if (selectedItem != null)
            {
                MessageBox.Show($"命令执行: 选中了 {selectedItem.Name} (ID: {selectedItem.Id})");
            }
        }


        public class ItemModel
        {
            public int Id { get; }
            public string Name { get; }

            public ItemModel(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }
    }
}
