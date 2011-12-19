using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using VisualProfilerUI.Model;
using VisualProfilerUI.Model.Methods;

namespace VisualProfilerUI.ViewModel
{
    public class MethodViewModel : ViewModelBase
    {
        private readonly int _top;
        private readonly int _height;
        private readonly int _value;
        private Method _method;
        private uint _id;
        private const int LineHeight = 2;
        private int MaxValue = 100;
        //public MethodViewModel(Method method )
        //{
        //    Contract.Requires(method != null);
        //    _method = method;

        //}
        public MethodViewModel(uint id, int top, int height, int value)
        {
            _id = id;
            _top = top;
            _height = height;
            _value = value;
            ActivateCommand = new RelayCommand(o =>
                                                   {
                                                       Debug.WriteLine(_id);
                                                       Console.Beep();
                                                   });
        }

        public void SetMax(int newMax)
        {
            MaxValue = newMax;
            OnPropertyChanged("Fill");
        }


        private Brush _fill;
        public Brush Fill
        {
            get
            {
                return _fill;
            }
            set
            {
                _fill = value;
                OnPropertyChanged("Fill");
            }
        }

        private Brush _borderBrush;
        public Brush BorderBrush
        {
            get { return _borderBrush; }
            set
            {
                _borderBrush = value;
                OnPropertyChanged("BorderBrush");
            }
        }

        private int _borderThinkness;
        public int BorderThinkness
        {
            get { return _borderThinkness; }
            set { _borderThinkness = value;
            OnPropertyChanged("BorderThinkness");
            }
        }

        public int Top
        {
            get
            {
                //  int top =_method.FirstLineNumber *  LineHeight;
                // OnPropertyChanged();
                return _top;
            }
        }

        public int Height
        {
            get
            {
                //int height = _method.LineExtend*LineHeight;
                return _height;
            }
        }

        public ICommand ActivateCommand { get; set; }

        public Color FillColorNoAlpha { get; set; }
    }
}
