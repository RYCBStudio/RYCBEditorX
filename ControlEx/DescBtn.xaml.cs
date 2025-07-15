using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RYCBEditorX.ControlEx
{
    /// <summary>
    /// DescBtn.xaml 的交互逻辑
    /// </summary>
    public partial class DescBtn : UserControl
    {
        // 暴露Command属性
        public static readonly DependencyProperty ButtonCommandProperty =
            DependencyProperty.Register("ButtonCommand", typeof(ICommand), typeof(DescBtn));

        public ICommand ButtonCommand
        {
            get => (ICommand)GetValue(ButtonCommandProperty);
            set => SetValue(ButtonCommandProperty, value);
        }

        // 暴露Content属性
        public static readonly DependencyProperty ButtonContentProperty =
            DependencyProperty.Register("ButtonContent", typeof(object), typeof(DescBtn));

        public object ButtonContent
        {
            get => GetValue(ButtonContentProperty);
            set => SetValue(ButtonContentProperty, value);
        }

        // 暴露Text属性
        public static readonly DependencyProperty DescProperty =
            DependencyProperty.Register("Desc", typeof(object), typeof(DescBtn), new PropertyMetadata(string.Empty));

        public object Desc
        {
            get => GetValue(DescProperty);
            set => SetValue(DescProperty, value);
        }

        // 暴露ButtonStyle属性
        public static readonly DependencyProperty ButtonStyleProperty =
            DependencyProperty.Register("ButtonStyleProperty", typeof(object), typeof(DescBtn));

        public object ButtonStyle
        {
            get => GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }

        // 暴露ButtonFontFamily属性
        public static readonly DependencyProperty ButtonFontFamilyProperty =
            DependencyProperty.Register("ButtonFontFamilyProperty", typeof(object), typeof(DescBtn));

        public object ButtonFontFamily
        {
            get => GetValue(ButtonFontFamilyProperty);
            set => SetValue(ButtonFontFamilyProperty, value);
        }

        // 暴露ButtonFontSize属性
        public static readonly DependencyProperty ButtonFontSizeProperty =
            DependencyProperty.Register("ButtonFontSizeProperty", typeof(object), typeof(DescBtn));

        public object ButtonFontSize
        {
            get => GetValue(ButtonFontSizeProperty);
            set => SetValue(ButtonFontSizeProperty, value);
        }

        public static readonly RoutedEvent ButtonClickEvent =
            EventManager.RegisterRoutedEvent("ButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DescBtn));

        public event RoutedEventHandler ButtonClick
        {
            add => AddHandler(ButtonClickEvent, value);
            remove => RemoveHandler(ButtonClickEvent, value);
        }
        // 其他需要暴露的属性类似

        // 定义描述位置枚举
        public enum DescPositions
        {
            Right,  // 描述在右边
            Bottom  // 描述在下边
        }

        // 依赖属性：描述位置
        public static readonly DependencyProperty DescPositionProperty =
            DependencyProperty.Register(
                "DescPosition",
                typeof(DescPositions),
                typeof(DescBtn),
                new PropertyMetadata(DescPositions.Right, OnDescPositionChanged));

        // 属性包装器
        public DescPositions DescPosition
        {
            get => (DescPositions)GetValue(DescPositionProperty);
            set => SetValue(DescPositionProperty, value);
        }

        // 依赖属性变化回调
        private static void OnDescPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (DescBtn)d;
            control.UpdateDescVisibility();
        }

        // 更新描述文本的可见性
        private void UpdateDescVisibility()
        {
            if (DescPosition == DescPositions.Right)
            {
                DescR.Visibility = Visibility.Visible;
                DescB.Visibility = Visibility.Collapsed;
            }
            else
            {
                DescR.Visibility = Visibility.Collapsed;
                DescB.Visibility = Visibility.Visible;
            }
        }

        public DescBtn()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                // 将依赖属性绑定到内部按钮
                //Btn.SetBinding(Button.CommandProperty, new Binding("ButtonCommand") { Source = this });
                //Btn.SetBinding(Button.ContentProperty, new Binding("ButtonContent") { Source = this });
                //Btn.SetBinding(Button.StyleProperty, new Binding("ButtonStyle") { Source = this });
                //Btn.SetBinding(Button.FontFamilyProperty, new Binding("ButtonFontFamily") { Source = this });
                //Btn.SetBinding(Button.FontSizeProperty, new Binding("ButtonFontSize") { Source = this });
                Btn.Click += (sender, e) =>
                {
                    RaiseEvent(new RoutedEventArgs(ButtonClickEvent, this));
                };
                //DescR.SetBinding(TextBlock.TextProperty, new Binding("DescProperty") { Source = this });
                // 其他属性绑定...
            };
        }
    }
}
