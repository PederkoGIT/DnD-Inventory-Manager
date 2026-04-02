using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnD_InventoryManager.ViewModels;

namespace DnD_InventoryManager.Views;

public partial class AddCharacterPage : ContentPage
{
    public AddCharacterPage(AddCharacterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}