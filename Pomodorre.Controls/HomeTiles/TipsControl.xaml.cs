using Microsoft.UI.Xaml.Controls;

namespace Pomodorre.Controls.HomeTiles
{
    public sealed partial class TipsControl : UserControl
    {
        private static readonly string[] _tips = new[]
        {
            "Take short breaks often to keep your mind fresh and maintain high productivity over time.",
            "Use the 20-20-20 rule: every 20 minutes, look at something 20 feet away for 20 seconds.",
            "Eliminate distractions during focus blocks. Turn on 'Do Not Disturb' on your devices.",
            "Set clear, actionable goals before starting a timer.",
            "Stay hydrated! Keep a glass of water on your desk.",
            "Try planning your most demanding tasks for your peak energy hours.",
            "Review your weekly stats to identify patterns in your focus behavior."
        };

        public TipsControl()
        {
            InitializeComponent();
            LoadRandomTip();
        }

        private void LoadRandomTip()
        {
            var random = new System.Random();
            var index = random.Next(_tips.Length);
            TipText.Text = _tips[index];
        }
    }
}
