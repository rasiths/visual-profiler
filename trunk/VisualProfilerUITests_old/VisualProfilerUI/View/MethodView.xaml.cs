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
        public static readonly Color MethodColor = Colors.Red;
        public static readonly Color CalledMethodColor = Colors.Blue;
        public static readonly Color CallingMethodColor = Colors.Green;
        public static readonly Color MethodBorderColor = Colors.Transparent;
        public static readonly Color ActiveMethodBorderColor = Colors.Black;

        #region MouseEnteredCommand

        public ICommand MouseEnteredCommand
        {
            get { return (ICommand)GetValue(MouseEnteredCommandProperty); }
            set { SetValue(MouseEnteredCommandProperty, value); }
        }

        public static readonly DependencyProperty MouseEnteredCommandProperty =
            DependencyProperty.Register("MouseEnteredCommand", typeof(ICommand), typeof(MethodView), new PropertyMetadata(default(ICommand)));

        #endregion

        public static readonly DependencyProperty MouseLeftProperty =
            DependencyProperty.Register("MouseLeft", typeof(ICommand), typeof(MethodView), new PropertyMetadata(default(ICommand)));

        public ICommand MouseLeft
        {
            get { return (ICommand)GetValue(MouseLeftProperty); }
            set { SetValue(MouseLeftProperty, value); }
        }

        #region MouseUpCommand

        public static readonly DependencyProperty MouseUpCommandProperty =
            DependencyProperty.Register("MouseUpCommand", typeof(ICommand), typeof(MethodView), new PropertyMetadata(default(ICommand)));

        public ICommand MouseUpCommand
        {
            get { return (ICommand)GetValue(MouseUpCommandProperty); }
            set { SetValue(MouseUpCommandProperty, value); }
        }

        #endregion

        #region FillProperty

        //public static readonly DependencyProperty FillProperty =
        //    DependencyProperty.Register("Fill", typeof (Brush), typeof (MethodView), new PropertyMetadata(default(Brush)));

        //public Brush Fill
        //{
        //    get { return (Brush) GetValue(FillProperty); }
        //    set { SetValue(FillProperty, value); }
        //}

        #endregion

        public MethodView()
        {
            InitializeComponent();
        }


        private void MethodViewMouseEnter(object sender, MouseEventArgs e)
        {

            if (MouseEnteredCommand != null && MouseEnteredCommand.CanExecute(null))
                MouseEnteredCommand.Execute(null);
        }


        private void MethodViewOnMouseUp(object sender, MouseButtonEventArgs arg)
        {
            if (MouseUpCommand != null && MouseUpCommand.CanExecute(null))
            {
                MouseUpCommand.Execute(null);
            }
        }

        private void MethodViewOnMouseLeave(object sender, MouseEventArgs e)
        {
            if (MouseLeft != null && MouseLeft.CanExecute(null))
            {
                MouseLeft.Execute(null);
            }
        }
    }

}
