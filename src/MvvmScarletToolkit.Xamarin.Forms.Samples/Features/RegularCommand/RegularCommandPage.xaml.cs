using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MvvmScarletToolkit.Xamarin.Forms.Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegularCommandPage : ContentPage
    {
        public RegularCommandPage()
        {
            InitializeComponent();
        }
    }
}