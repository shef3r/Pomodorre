using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pomodorre.Styling
{
    public sealed partial class PomodorreXamlResources : ResourceDictionary
    {
        public PomodorreXamlResources()
        {
            AddDictionary("ms-appx:///Pomodorre.Styling/Styles/Button.xaml");
            AddDictionary("ms-appx:///Pomodorre.Styling/Styles/NumberBox.xaml");
            AddDictionary("ms-appx:///Pomodorre.Styling/Styles/ListViewItem.xaml");
            AddDictionary("ms-appx:///Pomodorre.Styling/Styles/AchievementControl.xaml");
            AddDictionary("ms-appx:///Pomodorre.Styling/Styles/RadioButton.xaml");
            AddDictionary("ms-appx:///Pomodorre.Styling/Styles/StreakWeekdays.xaml");
            AddDictionary("ms-appx:///Pomodorre.Styling/Styles/FocusGoal.xaml");
        }

        private void AddDictionary(string dictionaryPath)
        {
            MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(dictionaryPath)
            });
        }
    }
}
