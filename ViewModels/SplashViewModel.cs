using Prism.Mvvm;

namespace RYCBEditorX.ViewModels
{
    public class SplashViewModel : BindableBase
    {
        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
    }
}