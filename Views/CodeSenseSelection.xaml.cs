using System.Windows.Controls;
using RYCBEditorX.ViewModels;

namespace RYCBEditorX.Views
{
    /// <summary>
    /// CodeSenseSelection.xaml 的交互逻辑
    /// </summary>
    public partial class CodeSenseSelection : UserControl
    {

        public string Icon
        {
            get; set;
        }

        public string Candidates
        {
            get; set;
        }

        public CodeSenseSelection(string Candidates, string Icon)
        {
            InitializeComponent();
            this.Candidates = Candidates;
            this.Icon = Icon;
            DataContext = new CodeSenseViewModel()
            {
                Candidates = Candidates,
                Icon = Icon
            };
        }
    }
}
