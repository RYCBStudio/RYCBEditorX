using System.Linq;
using System.Windows.Media;
using System.Windows.Controls;
using static RYCBEditorX.Utils.Language;
using System;

namespace RYCBEditorX.Views.SettingsPages;
/// <summary>
/// Appearance.xaml 的交互逻辑
/// </summary>
public partial class Appearance : UserControl
{
    public Appearance()
    {
        InitializeComponent();
        FontBox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
        FontBox.SelectedValue = new FontFamily(GlobalConfig.Resources["Main"].ToString());
        for (var i = 0; i < SkinBox.Items.Count; i++)
        {
            if ((SkinBox.Items[i] as ComboBoxItem).Content.ToString().Equals(GlobalConfig.Skin, StringComparison.CurrentCultureIgnoreCase))
            {
                SkinBox.SelectedIndex = i;
                break;
            }
        }
        for (var i = 0; i < LangBox.Items.Count; i++)
        {
            if ((LangBox.Items[i] as ComboBoxItem).Content.ToString().Equals(Dictionaries.LangDict[GlobalConfig.LocalizationString], StringComparison.CurrentCultureIgnoreCase))
            {
                LangBox.SelectedIndex = i;
                break;
            }
        }
    }
}
