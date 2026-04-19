using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace SimpleDPI;

public class LanguageItem : INotifyPropertyChanged
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    private double _opacity = 1.0;
    public double Opacity { get { return _opacity; } set { _opacity = value; OnPropertyChanged(nameof(Opacity)); } }
    private double _size = 14.0;
    public double Size { get { return _size; } set { _size = value; OnPropertyChanged(nameof(Size)); } }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public partial class MainWindow : Window
{
    private System.Windows.Forms.NotifyIcon? _notifyIcon;
    private ProcessManager _processManager;
    private AppSettings _settings;
    
    private bool _isClosing = false;
    private bool _isSyncing = false;
    private bool _isModified = false;
    private bool _isOnboarding = false;
    private bool _isOperating = false;
    private DispatcherTimer? _watchdogTimer;
    
    private UIElement? _currentView;

    private DispatcherTimer? _fadeTimer;
    private int _fadeWordIndex = 0;
    private string[] _helloWords = { "Merhaba", "Hello", "Hola", "Bonjour", "Hallo", "Ciao", "Привет", "你好", "こんにちは", "مرحباً", "Olá", "नमस्ते" };
    
    private DispatcherTimer? _snapTimer;
    private int _lastTickIndex = -1;
    private bool _isSnapping = false;
    private int _currentMatchCount = 0;
    private bool _isInfiniteActive = false;

    private ObservableCollection<LanguageItem> _languages = new ObservableCollection<LanguageItem>();
    private LanguageItem[] _baseLanguages;

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern bool SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);

    private DispatcherTimer? _memoryTimer;

    private void OptimizeMemory()
    {
        try 
        {
            // Triggers aggressive garbage collection and releases physical memory to OS
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
            GC.WaitForPendingFinalizers();
            SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
        } catch { }
    }

    public MainWindow()
    {
        InitializeComponent();
        _processManager = new ProcessManager();
        _settings = AppSettings.Load();
        
        TxtManual.Text = _settings.Arguments;
        ParseArgumentsToUI();
        
        _processManager.ProcessExited += OnProcessExited;
        _currentView = MainView;
        
        _baseLanguages = new LanguageItem[]
        {
            new LanguageItem { Code = "tr", Name = "Türkçe" },
            new LanguageItem { Code = "en", Name = "English" },
            new LanguageItem { Code = "az", Name = "Azərbaycan" },
            new LanguageItem { Code = "ru", Name = "Pусский" },
            new LanguageItem { Code = "uk", Name = "Українська" },
            new LanguageItem { Code = "es", Name = "Español" },
            new LanguageItem { Code = "fr", Name = "Français" },
            new LanguageItem { Code = "de", Name = "Deutsch" },
            new LanguageItem { Code = "it", Name = "Italiano" },
            new LanguageItem { Code = "pt", Name = "Português" },
            new LanguageItem { Code = "pl", Name = "Polski" },
            new LanguageItem { Code = "nl", Name = "Nederlands" },
            new LanguageItem { Code = "ko", Name = "한국어" },
            new LanguageItem { Code = "ja", Name = "日本語" },
            new LanguageItem { Code = "zh", Name = "中文" },
            new LanguageItem { Code = "ar", Name = "العربية" },
            new LanguageItem { Code = "hi", Name = "हिन्दी" },
            new LanguageItem { Code = "vi", Name = "Tiếng Việt" },
            new LanguageItem { Code = "th", Name = "ไทย" },
            new LanguageItem { Code = "id", Name = "Bahasa Indonesia" },
            new LanguageItem { Code = "el", Name = "Ελληνικά" },
            new LanguageItem { Code = "ro", Name = "Română" },
            new LanguageItem { Code = "hu", Name = "Magyar" },
            new LanguageItem { Code = "cs", Name = "Čeština" },
            new LanguageItem { Code = "sv", Name = "Svenska" },
            new LanguageItem { Code = "no", Name = "Norsk" },
            new LanguageItem { Code = "da", Name = "Dansk" },
            new LanguageItem { Code = "fi", Name = "Suomi" },
            new LanguageItem { Code = "bg", Name = "Български" },
            new LanguageItem { Code = "sr", Name = "Српски" },
            new LanguageItem { Code = "hr", Name = "Hrvatski" },
            new LanguageItem { Code = "sk", Name = "Slovenčina" },
            new LanguageItem { Code = "he", Name = "עברית" },
            new LanguageItem { Code = "fa", Name = "فارسی" },
            new LanguageItem { Code = "ms", Name = "Bahasa Melayu" },
            new LanguageItem { Code = "bn", Name = "বাংলা" },
            new LanguageItem { Code = "ur", Name = "اردو" },
            new LanguageItem { Code = "ta", Name = "தமிழ்" },
            new LanguageItem { Code = "te", Name = "తెలుగు" },
            new LanguageItem { Code = "mr", Name = "मराठी" },
            new LanguageItem { Code = "gu", Name = "ગુજરાતી" },
            new LanguageItem { Code = "kn", Name = "ಕನ್ನಡ" },
            new LanguageItem { Code = "ml", Name = "മലയാളം" },
            new LanguageItem { Code = "pa", Name = "ਪੰਜਾਬੀ" },
            new LanguageItem { Code = "tl", Name = "Filipino" },
            new LanguageItem { Code = "kk", Name = "Қазақша" },
            new LanguageItem { Code = "uz", Name = "O'zbekcha" },
            new LanguageItem { Code = "ka", Name = "ქართული" },
            new LanguageItem { Code = "hy", Name = "Հայերեն" },
            new LanguageItem { Code = "sq", Name = "Shqip" },
            new LanguageItem { Code = "et", Name = "Eesti" },
            new LanguageItem { Code = "lv", Name = "Latviešu" },
            new LanguageItem { Code = "lt", Name = "Lietuvių" }
        };
        PopulateLanguages("");
        ListOnboardLang.ItemsSource = _languages;

    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _notifyIcon = new System.Windows.Forms.NotifyIcon();
        _notifyIcon.Visible = true;
        RefreshTrayMenu();
        
        _memoryTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(2) };
        _memoryTimer.Tick += (s, ev) => { if (WindowState == WindowState.Minimized || !IsVisible) OptimizeMemory(); };
        _memoryTimer.Start();
        
        StateChanged += (s, ev) => { if (WindowState == WindowState.Minimized) OptimizeMemory(); };
        Deactivated += (s, ev) => { if (!IsVisible) OptimizeMemory(); };
        
        UpdateBlurState(_settings.EnableBlur);
        CheckOnboarding();
        CalculateWheelEffect();
        Dispatcher.BeginInvoke(new Action(() => OptimizeMemory()), DispatcherPriority.ContextIdle);

        _watchdogTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _watchdogTimer.Tick += (s, ev) => WatchdogCheck();
        _watchdogTimer.Start();

        // Command line args for autostart
        string[] args = Environment.GetCommandLineArgs();
        if (args.Contains("--autostart"))
        {
            if (_settings.AutoStartService) _ = StartProcessAsync();
            WindowState = WindowState.Minimized;
            Hide();
        }
    }

    private void RefreshTrayMenu()
    {
        if (_notifyIcon == null) return;
        
        try
        {
            if (System.IO.File.Exists("appicon.ico"))
                _notifyIcon.Icon = new System.Drawing.Icon("appicon.ico");
            else
            {
                var uri = new Uri("pack://application:,,,/appicon.ico");
                var streamInfo = System.Windows.Application.GetResourceStream(uri);
                if (streamInfo != null)
                {
                    using (var stream = streamInfo.Stream)
                    {
                        _notifyIcon.Icon = new System.Drawing.Icon(stream);
                    }
                }
                else { _notifyIcon.Icon = System.Drawing.SystemIcons.Shield; }
            }
        }
        catch { _notifyIcon.Icon = System.Drawing.SystemIcons.Shield; }

        _notifyIcon.Text = "SimpleDPI (" + (_processManager.IsRunning ? Localization.Get("Status_Running") : Localization.Get("Status_Off")) + ")";
        _notifyIcon.DoubleClick -= NotifyIcon_DoubleClick; 
        _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

        var menu = new System.Windows.Forms.ContextMenuStrip();
        
        // 1. Show
        menu.Items.Add(Localization.Get("Tray_Show"), null, (s, ev) => ShowWindow());
        
        // 2. Start/Stop Toggle
        menu.Items.Add(Localization.Get("Tray_Toggle"), null, async (s, ev) => {
            if (_processManager.IsRunning) await StopProcessAsync();
            else await StartProcessAsync();
        });

        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        // 3. Exit
        menu.Items.Add(Localization.Get("Tray_Exit"), null, (s, ev) => ExitApplication());

        _notifyIcon.ContextMenuStrip = menu;
    }

    private void FadeTransition(UIElement? outView, UIElement? inView, Action? onComplete = null)
    {
        TimeSpan duration = TimeSpan.FromMilliseconds(200);

        // Optimization: Clean memory after view change
        Dispatcher.BeginInvoke(new Action(() => OptimizeMemory()), DispatcherPriority.Background);
        
        if (outView != null && outView.Visibility == Visibility.Visible)
        {
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, duration);
            fadeOut.Completed += (s, e) =>
            {
                outView.Visibility = Visibility.Collapsed;
                if (inView != null) StartFadeIn(inView, duration, onComplete);
                else onComplete?.Invoke();
            };
            outView.BeginAnimation(OpacityProperty, fadeOut);
        }
        else if (inView != null)
        {
            StartFadeIn(inView, duration, onComplete);
        }
        
        // Only primary navigation views update the state
        if (inView == MainView || inView == SettingsView || inView == AboutView || inView == LanguageOverlay || inView == StartupSetupOverlay)
            _currentView = inView;
        
        // Force evaluation of back button visibility based on the underlying primary view
        BtnBack.Visibility = (_currentView == SettingsView || _currentView == AboutView || (_currentView == LanguageOverlay && !string.IsNullOrEmpty(_settings.Language))) ? Visibility.Visible : Visibility.Collapsed;
        
        // Hide navigation if a modal overlay is opening
        if (inView == ExitOverlay) BtnBack.Visibility = Visibility.Collapsed;

        if (BtnOpenAbout != null)
        {
            BtnOpenAbout.Visibility = (_currentView == SettingsView && inView != ExitOverlay) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void StartFadeIn(UIElement inView, TimeSpan duration, Action? onComplete)
    {
        inView.Visibility = Visibility.Visible;
        DoubleAnimation fadeIn = new DoubleAnimation(0, 1, duration);
        fadeIn.Completed += (s, e) => onComplete?.Invoke();
        inView.BeginAnimation(OpacityProperty, fadeIn);
    }

    private void ToggleHeight(UIElement target, bool expand, double toHeight)
    {
        DoubleAnimation anim = new DoubleAnimation(expand ? toHeight : 0, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut } };
        target.BeginAnimation(HeightProperty, anim);
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            try { this.DragMove(); } catch { }
        }
    }

    // --- INFINITE SCROLL / WHEEL EFFECT ---
    private void PopulateLanguages(string query)
    {
        _languages.Clear();
        var matches = string.IsNullOrEmpty(query) 
            ? _baseLanguages.ToList() 
            : _baseLanguages.Where(bl => bl.Name.ToLower().Contains(query) || bl.Code.ToLower().Contains(query)).ToList();

        _currentMatchCount = matches.Count;
        if (_currentMatchCount == 0) return;

        _isInfiniteActive = _currentMatchCount > 6;

        if (_isInfiniteActive)
        {
            for (int i = 0; i < 3; i++)
                foreach (var m in matches) _languages.Add(new LanguageItem { Code = m.Code, Name = m.Name });
        }
        else
        {
            _languages.Add(new LanguageItem { Code = "", Name = "" });
            _languages.Add(new LanguageItem { Code = "", Name = "" });
            foreach (var m in matches) _languages.Add(new LanguageItem { Code = m.Code, Name = m.Name });
            _languages.Add(new LanguageItem { Code = "", Name = "" });
            _languages.Add(new LanguageItem { Code = "", Name = "" });
        }
    }

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        string q = TxtSearch.Text.ToLower().Trim();
        PopulateLanguages(q);
        Dispatcher.BeginInvoke(new Action(() => CalculateWheelEffect()), DispatcherPriority.Render);
    }
    
    private void LangScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        CalculateWheelEffect();
        if (_isSyncing || _isSnapping) return;

        if (_snapTimer == null)
        {
            _snapTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _snapTimer.Tick += (s, ev) => { _snapTimer.Stop(); SnapWheel(); };
        }
        _snapTimer.Stop();
        _snapTimer.Start();

        // Dynamic Infinite Jump (Only if active)
        if (_isInfiniteActive)
        {
            double blockHeight = _currentMatchCount * 35;
            if (LangScroll.VerticalOffset < blockHeight)
            {
                LangScroll.ScrollToVerticalOffset(LangScroll.VerticalOffset + blockHeight);
                return;
            }
            else if (LangScroll.VerticalOffset > 2 * blockHeight)
            {
                LangScroll.ScrollToVerticalOffset(LangScroll.VerticalOffset - blockHeight);
                return;
            }
        }
    }

    private void CalculateWheelEffect()
    {
        if (LangScroll.ViewportHeight == 0) return;
        double centerOffset = LangScroll.VerticalOffset + (LangScroll.ViewportHeight / 2);

        ICollectionView view = System.Windows.Data.CollectionViewSource.GetDefaultView(_languages);
        int index = 0;
        foreach (LanguageItem item in view)
        {
            double pos = (index * 35) + 17.5;
            double dist = Math.Abs(centerOffset - pos);
            
            // Continuous math for smooth Apple-like cylindrical wheel effect
            double normalizedDist = Math.Min(dist / 90.0, 1.0); 
            
            // Opacity: Center is 1.0, drops to 0.1
            item.Opacity = 1.0 - (0.9 * Math.Pow(normalizedDist, 1.5));
            
            // Font Size: Center is 20, drops to 12
            item.Size = 20.0 - (8.0 * normalizedDist);
            
            index++;
        }
    }

    private void SnapWheel()
    {
        double offset = LangScroll.VerticalOffset;
        // Perfect center for 175px (5 items) viewport: target = i * 35 - 70
        double target = (Math.Round((offset + 70.0) / 35.0) * 35.0) - 70.0;
        
        if (Math.Abs(offset - target) > 0.5)
        {
            _isSnapping = true;
            LangScroll.ScrollToVerticalOffset(target);
            _isSnapping = false;
        }
    }

    private void OnLanguageClicked(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true; // Prevent overlapping with DragMove
        if (sender is Border b && b.Tag != null)
        {
            string code = b.Tag.ToString()!;
            int foundIdx = -1;
            
            // Find closest match to current offset to avoid huge jumps
            double currentCenter = LangScroll.VerticalOffset + (LangScroll.ViewportHeight / 2);
            double minDistance = double.MaxValue;

            int idx = 0;
            foreach (LanguageItem li in _languages) 
            { 
                if (li.Code == code) 
                { 
                    double pos = (idx * 35) + 17.5;
                    double dist = Math.Abs(currentCenter - pos);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        foundIdx = idx;
                    }
                } 
                idx++; 
            }
            
            if (foundIdx != -1)
            {
                LangScroll.ScrollToVerticalOffset(foundIdx * 35 - (LangScroll.ViewportHeight / 2) + 17.5);
                CalculateWheelEffect();
            }
        }
    }

    private void SelectLanguage(string code)
    {
        if (_fadeTimer != null) _fadeTimer.Stop();
        
        _settings.Language = code;
        Localization.CurrentLanguage = code;
        ApplyLanguage();
        UpdateUIState();

        _isModified = false;
        
        if (_isOnboarding)
        {
            // Proceed to Step 2
            FadeTransition(LanguageOverlay, StartupSetupOverlay);
        }
        else
        {
            _settings.Save();
            BtnBack.Visibility = Visibility.Visible;
            FadeTransition(LanguageOverlay, SettingsView, () => OptimizeMemory());
        }
    }

    private void BtnFinishOnboarding_Click(object sender, RoutedEventArgs e)
    {
        ICollectionView view = System.Windows.Data.CollectionViewSource.GetDefaultView(_languages);
        LanguageItem? target = view.Cast<LanguageItem>().OrderByDescending(x => x.Opacity).FirstOrDefault();
        if (target != null) SelectLanguage(target.Code);
        
        _settings.Language = Localization.CurrentLanguage;
        FadeTransition(LanguageOverlay, StartupSetupOverlay);
    }


    // --- ONBOARDING & LANGUAGE LOGIC ---

    private void CheckOnboarding()
    {
        if (string.IsNullOrEmpty(_settings.Language))
        {
            _isOnboarding = true;
            string sysLang = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            
            int targetIdx = 0;
            for(int i = 0; i < _baseLanguages.Length; i++) { if (_baseLanguages[i].Code == sysLang) targetIdx = i; }
            
            Dispatcher.BeginInvoke(new Action(() => {
                // Focus on the middle block (10th copy out of 20)
                int middleOffsetIndex = (10 * 12) + targetIdx;
                LangScroll.ScrollToVerticalOffset(middleOffsetIndex * 35 - (LangScroll.ViewportHeight / 2) + 17.5);
                CalculateWheelEffect();
            }), DispatcherPriority.Loaded);

            FadeTransition(MainView, LanguageOverlay);
            StartFadeLoop();
        }
        else
        {
            Localization.CurrentLanguage = _settings.Language;
            ApplyLanguage();
            UpdateUIState();
            SyncSettingsToUI();
        }
    }

    private void SyncSettingsToUI()
    {
        _isSyncing = true;
        TxtManual.Text = _settings.Arguments;
        ParseArgumentsToUI();
        ChkBlur.IsChecked = _settings.EnableBlur;
        ChkBoot.IsChecked = _settings.StartOnBoot;
        ChkAutoStart.IsChecked = _settings.AutoStartService;
        _isSyncing = false;
    }

    private void StartFadeLoop()
    {
        _fadeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.0) };
        _fadeTimer.Tick += (s, e) =>
        {
            _fadeWordIndex = (_fadeWordIndex + 1) % _helloWords.Length;
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
            fadeOut.Completed += (s2, e2) => 
            {
                TxtLangFade.Text = _helloWords[_fadeWordIndex];
                DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                TxtLangFade.BeginAnimation(OpacityProperty, fadeIn);
            };
            TxtLangFade.BeginAnimation(OpacityProperty, fadeOut);
        };
        _fadeTimer.Start();
    }

    private void ApplyLanguage()
    {
        _isSyncing = true;
        LblSettings.Text = Localization.Get("Settings_Title");
        LblProfile.Text = Localization.Get("Profile_Title");
        TipProfile.ToolTip = Localization.Get("Profile_Tip");
        LblDns.Text = Localization.Get("Dns_Title");
        TipDns.ToolTip = Localization.Get("Dns_Tip");
        LblAdvanced.Text = Localization.Get("Advanced_Title");
        if (TxtQuic != null) TxtQuic.Text = Localization.Get("Quic_Label");
        TipQuic.ToolTip = Localization.Get("Quic_Tip");
        if (TxtReverse != null) TxtReverse.Text = Localization.Get("Reverse_Label");
        TipReverse.ToolTip = Localization.Get("Reverse_Tip");
        BtnToggleManual.Content = Localization.Get("Manual_Btn");
        
        BtnResetSettings.Content = Localization.Get("Reset_Btn");
        BtnSaveSettings.Content = Localization.Get("Apply_Btn");
        
        LblOverlayMsg.Text = Localization.Get("Ask_Exit");
        BtnExitTrue.Content = Localization.Get("Exit_Exit");
        BtnExitCancel.Content = Localization.Get("Exit_Cancel");
        BtnExitHide.Content = Localization.Get("Exit_Hide");
        LblToast.Text = Localization.Get("Toast_Applied");
        TxtStatus.Text = Localization.Get("Status_" + (_processManager.IsRunning ? "Running" : "Off"));
        
        ChkBlur.Content = Localization.Get("Blur_Label");
        ChkBoot.Content = Localization.Get("Boot_Label");
        ChkAutoStart.Content = Localization.Get("AutoStart_Label");
        BtnUnsavedDiscard.Content = Localization.Get("Discard_Btn");
        BtnResetTrue.Content = Localization.Get("Reset_Confirm");
        BtnResetCancel.Content = Localization.Get("Exit_Cancel");
        LblAbout.Text = Localization.Get("About_Title");
        
        if (LblOnboardBoot != null)
        {
            LblOnboardBoot.Text = Localization.Get("Settings_Title");
            LblOnboardBootText.Text = Localization.Get("Boot_Label");
            LblOnboardAutoText.Text = Localization.Get("AutoStart_Label");
        }

        UpdateUIState();
        RefreshTrayMenu();
        _isSyncing = false;
    }

    // --- SYSTEM & TRAY LOGIC ---

    private void NotifyIcon_DoubleClick(object? sender, EventArgs e) => ShowWindow();

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        SystemUtils.SetEfficiencyMode(false); // Disable Efficiency Mode
        if (_notifyIcon != null) _notifyIcon.Visible = false;
        Activate();
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
        OptimizeMemory();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        if (_currentView == SettingsView && _isModified)
        {
            LblOverlayMsg.Text = Localization.Get("Unsaved_Msg");
            ExitButtons.Visibility = Visibility.Collapsed;
            UnsavedButtons.Visibility = Visibility.Visible;
            ResetButtons.Visibility = Visibility.Collapsed;
            FadeTransition(null, ExitOverlay);
        }
        else
        {
            LblOverlayMsg.Text = Localization.Get("Ask_Exit");
            ExitButtons.Visibility = Visibility.Visible;
            UnsavedButtons.Visibility = Visibility.Collapsed;
            ResetButtons.Visibility = Visibility.Collapsed;
            FadeTransition(null, ExitOverlay);
        }
    }

    private void BtnExitTrue_Click(object sender, RoutedEventArgs e) => ExitApplication();

    private void BtnOverlayCancel_Click(object sender, RoutedEventArgs e) => FadeTransition(ExitOverlay, null);

    private void BtnHide_Click(object sender, RoutedEventArgs e)
    {
        Hide();
        SystemUtils.SetEfficiencyMode(true); // Enable Efficiency Mode
        if (_notifyIcon != null) _notifyIcon.Visible = true;
        FadeTransition(ExitOverlay, null);
    }

    private void Window_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            SystemUtils.SetEfficiencyMode(true);
            OptimizeMemory();
        }
        else if (WindowState == WindowState.Normal)
        {
            SystemUtils.SetEfficiencyMode(false);
        }
    }

    private void BtnUnsavedDiscard_Click(object sender, RoutedEventArgs e)
    {
        FadeTransition(ExitOverlay, null);
        _isModified = false;
        ParseArgumentsToUI(); // Revert
        FadeTransition(_currentView, MainView);
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (AboutView.Visibility == Visibility.Visible) 
        {
            FadeTransition(AboutView, null, () => {
                _currentView = SettingsView;
                BtnOpenAbout.Visibility = Visibility.Visible;
                BtnBack.Visibility = Visibility.Visible;
            });
            return;
        }

        if (_currentView == LanguageOverlay)
        {
            FadeTransition(LanguageOverlay, SettingsView);
            return;
        }

        if (_currentView == SettingsView) 
        {
            if (_isModified)
            {
                LblOverlayMsg.Text = Localization.Get("Unsaved_Msg");
                ExitButtons.Visibility = Visibility.Collapsed;
                UnsavedButtons.Visibility = Visibility.Visible;
                ResetButtons.Visibility = Visibility.Collapsed;
                FadeTransition(null, ExitOverlay);
                return;
            }

            BtnOpenAbout.Visibility = Visibility.Collapsed;
            FadeTransition(SettingsView, MainView);
        }
    }

    private void ExitApplication()
    {
        _isClosing = true;
        _processManager.Stop();
        if (_notifyIcon != null) { _notifyIcon.Visible = false; _notifyIcon.Dispose(); }
        System.Windows.Application.Current.Shutdown();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        if (!_isClosing) _processManager.Stop();
    }

    // --- ABOUT PANEL ---
    private void BtnOpenAbout_Click(object sender, RoutedEventArgs e) 
    {
        _currentView = AboutView;
        BtnBack.Visibility = Visibility.Visible;
        BtnOpenAbout.Visibility = Visibility.Collapsed;
        FadeTransition(null, AboutView);
    }
    private void BtnSteam_Click(object sender, RoutedEventArgs e) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://steamcommunity.com/id/Hadixcanim/") { UseShellExecute = true });
    private void BtnDiscord_Click(object sender, RoutedEventArgs e) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://discord.com/users/327464543288688640") { UseShellExecute = true });


    // --- PROCESS START / STOP ---

    private async void BtnToggle_Click(object sender, RoutedEventArgs e)
    {
        if (_processManager.IsRunning) await StopProcessAsync();
        else await StartProcessAsync();
    }

    private async Task StartProcessAsync()
    {
        if (_isOperating) return;
        _isOperating = true;
        try
        {
            string args = TxtManual.Text;
            await Task.Run(() => _processManager.Start(args));
            UpdateUIState();
            RefreshTrayMenu();
        }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message, "Error"); }
        finally { _isOperating = false; }
    }

    private async Task StopProcessAsync()
    {
        if (_isOperating) return;
        _isOperating = true;
        try
        {
            await Task.Run(() => _processManager.Stop());
            UpdateUIState();
            RefreshTrayMenu();
        }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message, "Error"); }
        finally { _isOperating = false; }
    }

    private void OnProcessExited() => Dispatcher.BeginInvoke(new Action(() => { if (!_isOperating) { UpdateUIState(); RefreshTrayMenu(); } }));

    private void UpdateUIState()
    {
        var pulse = (System.Windows.Media.Animation.Storyboard)FindResource("PulseAnimation");
        pulse.Stop(BtnToggle);
        BtnToggle.Opacity = 1.0;

        if (_processManager.IsRunning)
        {
            BtnToggle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#0066CC")!; // Theme Blue
            BtnToggle.Foreground = System.Windows.Media.Brushes.White;
            TxtStatus.Text = Localization.Get("Status_Running");
            TxtStatus.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 199, 89)); // iOS Green
        }
        else
        {
            BtnToggle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#222222")!;
            BtnToggle.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(85, 85, 85)); 
            TxtStatus.Text = Localization.Get("Status_Off");
            TxtStatus.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(119, 119, 119));
        }
    }

    private void WatchdogCheck()
    {
        if (!IsVisible || _isOperating || _isOnboarding) return;

        bool actualRunning = _processManager.IsRunning;
        bool uiThinksRunning = TxtStatus.Text == Localization.Get("Status_Running");

        if (uiThinksRunning && !actualRunning)
        {
            // Crashed / Stopped externally
            BtnToggle.Foreground = System.Windows.Media.Brushes.Red;
            TxtStatus.Text = Localization.Get("Status_Crashed");
            TxtStatus.Foreground = System.Windows.Media.Brushes.Red;

            var pulse = (System.Windows.Media.Animation.Storyboard)FindResource("PulseAnimation");
            pulse.Begin(BtnToggle);
        }
    }

    // --- SETTINGS VIEW ---
    private void BtnOpenSettings_Click(object sender, RoutedEventArgs e)
    {
        SyncSettingsToUI();
        _isModified = false;
        BtnOpenAbout.Visibility = Visibility.Visible;
        FadeTransition(MainView, SettingsView);
    }

    private void BtnOpenLanguage_Click(object sender, RoutedEventArgs e)
    {
        _currentView = LanguageOverlay;
        BtnOpenAbout.Visibility = Visibility.Collapsed;
        FadeTransition(SettingsView, LanguageOverlay);
    }

    private void Toggle_Blur(object sender, RoutedEventArgs e)
    {
        if (_isSyncing) return;
        _isModified = true;
        UpdateBlurState(ChkBlur.IsChecked == true);
    }

    private void UpdateBlurState(bool isEnabled)
    {
        SystemUtils.EnableBlur(this, isEnabled);
        if (isEnabled)
            RootBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x40, 0x15, 0x15, 0x15)); // Lighter Mica feel
        else
            RootBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x15, 0x15, 0x15)); // Opaque Matte
    }

    private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
    {
        _settings.Arguments = TxtManual.Text;
        _settings.EnableBlur = ChkBlur.IsChecked == true;
        _settings.StartOnBoot = ChkBoot.IsChecked == true;
        _settings.AutoStartService = ChkAutoStart.IsChecked == true;
        _settings.Save();
        
        SystemUtils.SetStartOnBoot(_settings.StartOnBoot);

        _isModified = false;
        BtnOpenAbout.Visibility = Visibility.Collapsed;
        FadeTransition(SettingsView, MainView, async () => 
        {
            if (_processManager.IsRunning) 
            { 
                await StopProcessAsync(); 
                await StartProcessAsync(); 
            }
            FadeTransition(null, ToastNotification, async () => { await Task.Delay(2000); FadeTransition(ToastNotification, null); });
            OptimizeMemory();
        });
    }

    private void BtnResetSettings_Click(object sender, RoutedEventArgs e)
    {
        LblOverlayMsg.Text = Localization.Get("Reset_Msg");
        ExitButtons.Visibility = Visibility.Collapsed;
        UnsavedButtons.Visibility = Visibility.Collapsed;
        ResetButtons.Visibility = Visibility.Visible;
        FadeTransition(null, ExitOverlay);
    }

    private async void BtnResetTrue_Click(object sender, RoutedEventArgs e)
    {
        FadeTransition(ExitOverlay, null);
        
        await StopProcessAsync();
        
        _settings = new AppSettings();
        _settings.Save(); // Persist the reset state to AppData folder
        
        SystemUtils.SetStartOnBoot(false);
        UpdateBlurState(true);
        
        ApplyLanguage();
        SyncSettingsToUI();
        _isModified = false;
        BtnBack.Visibility = Visibility.Collapsed;
        FadeTransition(SettingsView, null, () => CheckOnboarding());
    }

    private void BtnToggleManual_Click(object sender, RoutedEventArgs e)
    {
        ToggleHeight(BrdManualBox, BrdManualBox.Height == 0, 70);
    }

    private void Setting_Changed(object sender, RoutedEventArgs e)
    {
        if (_isSyncing || !IsLoaded) return;
        _isModified = true;
        GenerateArgumentsFromUI();
    }

    private void TxtManual_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isSyncing) return;
        _isModified = true;
        ParseArgumentsToUI();
    }

    private void ParseArgumentsToUI()
    {
        _isSyncing = true;
        string args = TxtManual.Text ?? "";
        if (args.Contains("-5")) CmbMode.SelectedIndex = 0;
        else if (args.Contains("-7")) CmbMode.SelectedIndex = 1;
        else if (args.Contains("-9")) CmbMode.SelectedIndex = 2;
        else CmbMode.SelectedIndex = 0;

        if (args.Contains("77.88.8.8")) CmbDns.SelectedIndex = 0;
        else if (args.Contains("8.8.8.8")) CmbDns.SelectedIndex = 1;
        else if (args.Contains("1.1.1.1")) CmbDns.SelectedIndex = 2;
        else CmbDns.SelectedIndex = 3;

        ChkQuic.IsChecked = args.Contains("-q");
        ChkReverse.IsChecked = args.Contains("--reverse-frag");
        
        // Argument sync should not override UI-only logic toggles
        // These are handled by their own events or Save button
        _isSyncing = false;
    }

    private void GenerateArgumentsFromUI()
    {
        _isSyncing = true;
        string args = "";
        if (CmbMode.SelectedIndex == 0) args += "-5 ";
        else if (CmbMode.SelectedIndex == 1) args += "-7 ";
        else if (CmbMode.SelectedIndex == 2) args += "-9 ";

        if (CmbDns.SelectedIndex == 0) args += "--dns-addr 77.88.8.8 --dns-port 1253 --dnsv6-addr 2a02:6b8::feed:0ff --dnsv6-port 1253 ";
        else if (CmbDns.SelectedIndex == 1) args += "--dns-addr 8.8.8.8 --dns-port 53 ";
        else if (CmbDns.SelectedIndex == 2) args += "--dns-addr 1.1.1.1 --dns-port 53 ";
        
        if (ChkQuic.IsChecked == true) args += "-q ";
        if (ChkReverse.IsChecked == true) args += "--reverse-frag ";

        TxtManual.Text = args.Trim();
        _isSyncing = false;
    }
    
    // STARTUP OVERLAY LOGIC
    private void OnboardSetting_Changed(object sender, RoutedEventArgs e) 
    {
        if (!IsLoaded) return;
        _isModified = true;
    }
    
    private void BtnFinishStartupSetup_Click(object sender, RoutedEventArgs e)
    {
        _settings.StartOnBoot = ChkOnboardBoot.IsChecked == true;
        _settings.AutoStartService = ChkOnboardAuto.IsChecked == true;
        _settings.Save();
        
        SystemUtils.SetStartOnBoot(_settings.StartOnBoot);

        _isOnboarding = false;
        _isModified = false;
        FadeTransition(StartupSetupOverlay, MainView, async () => {
            if (_settings.AutoStartService) await StartProcessAsync();
            OptimizeMemory();
        });
    }
}