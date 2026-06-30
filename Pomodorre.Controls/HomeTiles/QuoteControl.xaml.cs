using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using Microsoft.Windows.ApplicationModel.Resources;

namespace Pomodorre.Controls.HomeTiles
{
    public sealed partial class QuoteControl : UserControl
    {
        private readonly DispatcherTimer _timer;
        private readonly ResourceLoader _resourceLoader;
        private const int MaxQuotes = 5;
        private readonly System.Random _random = new System.Random();

        public QuoteControl()
        {
            InitializeComponent();
            _resourceLoader = new ResourceLoader();
            try { TitleText.Text = _resourceLoader.GetString("QuoteTitle/Text"); } catch { }
            LoadRandomQuote();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(10);
            _timer.Tick += (s, e) => LoadRandomQuote();
            _timer.Start();
        }

        private void LoadRandomQuote()
        {
            var index = _random.Next(1, MaxQuotes + 1);
            
            try
            {
                var quoteText = _resourceLoader.GetString($"Quote{index}_Text");
                var quoteAuthor = _resourceLoader.GetString($"Quote{index}_Author");
                
                if (!string.IsNullOrEmpty(quoteText))
                {
                    QuoteText.Text = $"\"{quoteText}\"";
                    AuthorText.Text = $"- {quoteAuthor}";
                }
            }
            catch { }
        }
    }
}
