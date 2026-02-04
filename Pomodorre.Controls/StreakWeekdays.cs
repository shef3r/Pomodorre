using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pomodorre.Controls
{
    public class StreakWeekdays : Control
    {
        public static readonly DependencyProperty CurrentStreakWeekdayProperty =
            DependencyProperty.Register(
                nameof(StreakWeekday),
                typeof(int),
                typeof(StreakWeekdays),
                new PropertyMetadata(0, OnCurrentStreakWeekdayChanged));
        public int StreakWeekday
        {
            get => (int)GetValue(CurrentStreakWeekdayProperty);
            set => SetValue(CurrentStreakWeekdayProperty, value);
        }

        // IsChecked dependency properties
        public static readonly DependencyProperty Is1CheckedProperty =
            DependencyProperty.Register(nameof(Is1Checked), typeof(bool), typeof(StreakWeekdays), new PropertyMetadata(false));
        public bool Is1Checked { get => (bool)GetValue(Is1CheckedProperty); set => SetValue(Is1CheckedProperty, value); }

        public static readonly DependencyProperty Is2CheckedProperty =
            DependencyProperty.Register(nameof(Is2Checked), typeof(bool), typeof(StreakWeekdays), new PropertyMetadata(false));
        public bool Is2Checked { get => (bool)GetValue(Is2CheckedProperty); set => SetValue(Is2CheckedProperty, value); }

        public static readonly DependencyProperty Is3CheckedProperty =
            DependencyProperty.Register(nameof(Is3Checked), typeof(bool), typeof(StreakWeekdays), new PropertyMetadata(false));
        public bool Is3Checked { get => (bool)GetValue(Is3CheckedProperty); set => SetValue(Is3CheckedProperty, value); }

        public static readonly DependencyProperty Is4CheckedProperty =
            DependencyProperty.Register(nameof(Is4Checked), typeof(bool), typeof(StreakWeekdays), new PropertyMetadata(false));
        public bool Is4Checked { get => (bool)GetValue(Is4CheckedProperty); set => SetValue(Is4CheckedProperty, value); }

        public static readonly DependencyProperty Is5CheckedProperty =
            DependencyProperty.Register(nameof(Is5Checked), typeof(bool), typeof(StreakWeekdays), new PropertyMetadata(false));
        public bool Is5Checked { get => (bool)GetValue(Is5CheckedProperty); set => SetValue(Is5CheckedProperty, value); }

        public static readonly DependencyProperty Is6CheckedProperty =
            DependencyProperty.Register(nameof(Is6Checked), typeof(bool), typeof(StreakWeekdays), new PropertyMetadata(false));
        public bool Is6Checked { get => (bool)GetValue(Is6CheckedProperty); set => SetValue(Is6CheckedProperty, value); }

        public static readonly DependencyProperty Is7CheckedProperty =
            DependencyProperty.Register(nameof(Is7Checked), typeof(bool), typeof(StreakWeekdays), new PropertyMetadata(false));
        public bool Is7Checked { get => (bool)GetValue(Is7CheckedProperty); set => SetValue(Is7CheckedProperty, value); }

        // Radio name dependency properties
        public static readonly DependencyProperty Radio1NameProperty =
            DependencyProperty.Register(nameof(Radio1Name), typeof(string), typeof(StreakWeekdays), new PropertyMetadata("Mon"));
        public string Radio1Name { get => (string)GetValue(Radio1NameProperty); set => SetValue(Radio1NameProperty, value); }

        public static readonly DependencyProperty Radio2NameProperty =
            DependencyProperty.Register(nameof(Radio2Name), typeof(string), typeof(StreakWeekdays), new PropertyMetadata("Tue"));
        public string Radio2Name { get => (string)GetValue(Radio2NameProperty); set => SetValue(Radio2NameProperty, value); }

        public static readonly DependencyProperty Radio3NameProperty =
            DependencyProperty.Register(nameof(Radio3Name), typeof(string), typeof(StreakWeekdays), new PropertyMetadata("Wed"));
        public string Radio3Name { get => (string)GetValue(Radio3NameProperty); set => SetValue(Radio3NameProperty, value); }

        public static readonly DependencyProperty Radio4NameProperty =
            DependencyProperty.Register(nameof(Radio4Name), typeof(string), typeof(StreakWeekdays), new PropertyMetadata("Thu"));
        public string Radio4Name { get => (string)GetValue(Radio4NameProperty); set => SetValue(Radio4NameProperty, value); }

        public static readonly DependencyProperty Radio5NameProperty =
            DependencyProperty.Register(nameof(Radio5Name), typeof(string), typeof(StreakWeekdays), new PropertyMetadata("Fri"));
        public string Radio5Name { get => (string)GetValue(Radio5NameProperty); set => SetValue(Radio5NameProperty, value); }

        public static readonly DependencyProperty Radio6NameProperty =
            DependencyProperty.Register(nameof(Radio6Name), typeof(string), typeof(StreakWeekdays), new PropertyMetadata("Sat"));
        public string Radio6Name { get => (string)GetValue(Radio6NameProperty); set => SetValue(Radio6NameProperty, value); }

        public static readonly DependencyProperty Radio7NameProperty =
            DependencyProperty.Register(nameof(Radio7Name), typeof(string), typeof(StreakWeekdays), new PropertyMetadata("Sun"));
        public string Radio7Name { get => (string)GetValue(Radio7NameProperty); set => SetValue(Radio7NameProperty, value); }

        public StreakWeekdays()
        {
            this.DefaultStyleKey = typeof(StreakWeekdays);
            UpdateChecks(StreakWeekday);
        }

        private static void OnCurrentStreakWeekdayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StreakWeekdays control && e.NewValue is int newVal)
            {
                control.UpdateChecks(newVal);
            }
        }

        private void UpdateChecks(int day)
        {
            if (!(day == 0 || day == -1))
            {
                int clamped = Math.Max(0, Math.Min(7, day));

                SetValue(Is1CheckedProperty, clamped >= 1);
                SetValue(Is2CheckedProperty, clamped >= 2);
                SetValue(Is3CheckedProperty, clamped >= 3);
                SetValue(Is4CheckedProperty, clamped >= 4);
                SetValue(Is5CheckedProperty, clamped >= 5);
                SetValue(Is6CheckedProperty, clamped >= 6);
                SetValue(Is7CheckedProperty, clamped >= 7);
            }
        }
    }
}
