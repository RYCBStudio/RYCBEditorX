using Prism.Mvvm;

namespace RYCBEditorX.ViewModels
{
    public class AboutViewModel : BindableBase
    {
        public string BuildTime
        {
            get; set;
        }
        public string Version
        {
            get; set;
        }
    }
}