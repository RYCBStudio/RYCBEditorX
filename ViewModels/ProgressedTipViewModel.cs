using Prism.Mvvm;

namespace RYCBEditorX.ViewModels
{
    public class ProgressedTipViewModel : BindableBase
    {
        public int _now;
        public int Now
        {
            get => _now;
            set => SetProperty(ref _now, value);
        }

        public int _total;
        public int Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }
    }
}