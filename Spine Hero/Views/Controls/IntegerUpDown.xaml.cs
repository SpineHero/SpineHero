using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SpineHero.Views.Controls
{
    public partial class IntegerUpDown : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(int), typeof(IntegerUpDown),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    ValueChangedCallback, CoerceValue, false, UpdateSourceTrigger.Explicit ));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(int), typeof(IntegerUpDown), new FrameworkPropertyMetadata(0, (d, e) => { }, CoerceMinMax));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(int), typeof(IntegerUpDown), new FrameworkPropertyMetadata(int.MaxValue, (d, e) => { }, CoerceMinMax));

        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register(nameof(Step), typeof(int), typeof(IntegerUpDown), new FrameworkPropertyMetadata(1));

        public static RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IntegerUpDown));

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        protected virtual void OnValueChanged()
        {
            RaiseEvent(new RoutedEventArgs { RoutedEvent = ValueChangedEvent });
        }

        public IntegerUpDown()
        {
            InitializeComponent();
        }

        public int Value
        {
            get
            {
                return (int)GetValue(ValueProperty);
            }
            set
            {
                var val = Convert.ToInt32(value);
                SetValue(ValueProperty, val);
            }
        }

        public int Minimum
        {
            get
            {
                return (int)GetValue(MinimumProperty);
            }
            set
            {
                SetValue(MinimumProperty, value);
            }
        }

        public int Maximum
        {
            get
            {
                return (int)GetValue(MaximumProperty);
            }
            set
            {
                SetValue(MaximumProperty, value);
            }
        }

        public int Step
        {
            get
            {
                return (int)GetValue(StepProperty);
            }
            set
            {
                SetValue(StepProperty, value);
            }
        }

        public void Increment(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                Value += Step;
            }
            catch (ArgumentException) { }
        }

        public void Decrement(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                Value -= Step;
            }
            catch (ArgumentException) { }
        }

        private static object CoerceMinMax(DependencyObject d, object baseValue)
        {
            CoerceValue(d, baseValue);
            return baseValue;
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            var updown = (IntegerUpDown)d;
            var value = (int)baseValue;
            value = (value < updown.Minimum) ? updown.Minimum : (value > updown.Maximum) ? updown.Maximum : value;
            return value;
        }

        private static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var updown = d as IntegerUpDown;
            if (updown == null) return;
            updown.GetBindingExpression(ValueProperty)?.UpdateSource();
            updown.AdjustButtons((int)e.NewValue);
            updown.OnValueChanged();
        }

        private void AdjustButtons(int newValue)
        {
            ButtonUp.IsEnabled = newValue != Maximum;
            ButtonDown.IsEnabled = newValue != Minimum;
        }
    }
}