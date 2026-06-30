using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using Microsoft.Windows.ApplicationModel.Resources;

namespace Pomodorre.Controls.HomeTiles
{
    public sealed partial class TipsControl : UserControl
    {
        private readonly DispatcherTimer _timer;
        private readonly ResourceLoader _resourceLoader;
        private const int MaxTips = 7;
        private readonly System.Random _random = new System.Random();

        public TipsControl()
        {
            InitializeComponent();
            _resourceLoader = new ResourceLoader();
            try { TitleText.Text = _resourceLoader.GetString("TipsTitle/Text"); } catch { }
            LoadRandomTip();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(10);
            _timer.Tick += (s, e) => LoadRandomTip();
            _timer.Start();
        }

        private void LoadRandomTip()
        {
            var index = _random.Next(1, MaxTips + 1);
            
            try
            {
                var tipText = _resourceLoader.GetString($"Tip{index}_Text");
                if (!string.IsNullOrEmpty(tipText))
                {
                    TipText.Text = tipText;
                }
            }
            catch { }
        }
    }
}
