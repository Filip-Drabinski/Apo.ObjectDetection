using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ApoObjectDetection.Gui.UserControls
{
    /// <summary>
    ///     Interaction logic for TextBoxInt.xaml
    /// </summary>
    public partial class TextBoxInt : UserControl
    {
        public event Func<int, bool> ValueChanged;


        public int Value
        {
            get => (int) GetValue(InitialValueProperty);
            set
            {
                if (ValueChanged?.Invoke(value) == true)
                {
                    SetValue(InitialValueProperty, value);
                    Tb.Text = value.ToString(CultureInfo.InvariantCulture);
                    Tb.Foreground = SystemColors.WindowTextBrush;
                }
                else
                {
                    Tb.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                }
            }
        }

        // Using a DependencyProperty as the backing store for InitialValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InitialValueProperty =
            DependencyProperty.Register("TextBoxIntValue", typeof(int), typeof(TextBoxInt), new PropertyMetadata(1));


        public TextBoxInt()
        {
            InitializeComponent();
        }

        private void Tb_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.Tab))
                ((TextBox) sender).SelectAll();
        }

        private void Tb_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(Tb.Text, out var val))
                if (ValueChanged?.Invoke(val) == true)
                {
                    Tb.Foreground = SystemColors.WindowTextBrush;

                    return;
                }

            Tb.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        }
    }
}