using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pomodorre.Controls
{
    public sealed partial class AchievementControl : Control
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(AchievementControl),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
                nameof(Description),
                typeof(string),
                typeof(AchievementControl),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(AchievementControl),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(
                nameof(MaxValue),
                typeof(double),
                typeof(AchievementControl),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty StarValueProperty =
            DependencyProperty.Register(
                nameof(StarValue),
                typeof(int),
                typeof(AchievementControl),
                new PropertyMetadata(0));

        public AchievementControl() {
            this.DefaultStyleKey = typeof(AchievementControl);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public double PercentageValue => MaxValue == 0 ? 0 : (Value / MaxValue) * 100;

        public int StarValue
        {
            get => (int)GetValue(StarValueProperty);
            set => SetValue(StarValueProperty, value);
        }
    }
}
