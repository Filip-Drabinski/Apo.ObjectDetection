using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ApoObjectDetection.Gui.UserControls
{
    /// <summary>
    ///     Interaction logic for TextBoxDouble.xaml
    /// </summary>
    public partial class TextBoxDouble : UserControl
    {
        public event Func<double, bool> ValueChanged;


        public double Value
        {
            get => (double) GetValue(InitialValueProperty);
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
            DependencyProperty.Register("TextBoxDoubleValue", typeof(double), typeof(TextBoxDouble),
                new PropertyMetadata(1.0d));


        public TextBoxDouble()
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
            if (double.TryParse(Tb.Text, out var val))
                if (ValueChanged?.Invoke(val) == true)
                {
                    Tb.Foreground = SystemColors.WindowTextBrush;

                    return;
                }

            Tb.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        }
    }
}