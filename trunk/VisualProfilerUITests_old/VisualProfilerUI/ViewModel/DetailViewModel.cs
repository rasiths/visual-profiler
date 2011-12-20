namespace VisualProfilerUI.ViewModel
{
    public class DetailViewModel : ViewModelBase
    {
        private string _metrics;
        public string Metrics
        {
            get { return _metrics; }
            set
            {
                _metrics = value;
                OnPropertyChanged("Metrics");
            }
        }

        private string _methodName;
        public string MethodName
        {
            get { return _methodName; }
            set
            {
                _methodName = value;
                OnPropertyChanged("MethodName");
            }
        }
    }
}
