using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pomodorre.Controls
{
    public class FocusGoal : Control
    {
        public FocusGoal()
        {
            this.DefaultStyleKey = typeof(FocusGoal);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(AchievementControl),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(
                nameof(MaxValue),
                typeof(double),
                typeof(AchievementControl),
                new PropertyMetadata(string.Empty));

        public double Value = 0;
        public double MaxValue = 0;
    }
}
