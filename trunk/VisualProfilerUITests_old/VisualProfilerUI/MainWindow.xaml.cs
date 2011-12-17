using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VisualProfilerUI.Model;
using VisualProfilerUI.ViewModel;

namespace VisualProfilerUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
          
          
            UplnaBlbost[] uplnaBlbosts = new[]
                                             {
                                                 new UplnaBlbost() {Height = 10, Top = 150},
                                                 new UplnaBlbost() {Height = 20, Top = 80}
                                             };
            UplnaBlbost[][] blbosts = new[] {uplnaBlbosts, uplnaBlbosts};
            itemControls.ItemsSource = blbosts;
        }
    }
}
