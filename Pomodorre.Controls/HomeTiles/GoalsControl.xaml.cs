using Microsoft.UI.Xaml.Controls;

namespace Pomodorre.Controls.HomeTiles
{
    public sealed partial class GoalsControl : UserControl
    {
        public GoalsControl()
        {
            this.InitializeComponent();
            try {
                var rl = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();
                TitleText.Text = rl.GetString("GoalsTitle/Text");
                SubtitleText.Text = rl.GetString("GoalsSubtitle/Text");
                ProgressText.Text = rl.GetString("GoalsProgress/Text");
            } catch { }
        }
    }
}
