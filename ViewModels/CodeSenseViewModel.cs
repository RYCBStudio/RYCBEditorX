using Prism.Mvvm;

namespace RYCBEditorX.ViewModels
{
    public class CodeSenseViewModel : BindableBase
    {
        private string _candidates;
        public string Candidates
        {
            get => _candidates;
            set => SetProperty(ref _candidates, value);
        }

        private string _icon;
        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }
    }
}
