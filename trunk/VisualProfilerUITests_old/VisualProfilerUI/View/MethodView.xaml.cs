using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace VisualProfilerUI.View
{
    /// <summary>
    /// Interaction logic for MethodControl.xaml
    /// </summary>
    public partial class MethodView : UserControl
    {
        public static readonly DependencyProperty MouseEnteredCommandProperty =
            DependencyProperty.Register("MouseEnteredCommand", typeof(ICommand), typeof(MethodView), new PropertyMetadata(default(ICommand)));

        public ICommand MouseEnteredCommand
        {
            get { return (ICommand)GetValue(MouseEnteredCommandProperty); }
            set { SetValue(MouseEnteredCommandProperty, value); }
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof (Brush), typeof (MethodView), new PropertyMetadata(default(Brush)));

        public Brush Fill
        {
            get { return (Brush) GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }


        public MethodView()
        {
            InitializeComponent();
        }

       
        private void RectangleMouseEnter(object sender, MouseEventArgs e)
        {
            
            if(MouseEnteredCommand != null && MouseEnteredCommand.CanExecute(null))
                MouseEnteredCommand.Execute(null);
        }
     
    
}

}
