﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HandyControl.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls;

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
    }
}
