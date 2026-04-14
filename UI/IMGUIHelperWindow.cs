using IMGUI_Helper.UI.Tabs;
using UnityEngine;

namespace IMGUI_Helper.UI
{
    public class IMGUIHelperWindow
    {
        private const int MAIN_WINDOW_ID = 9438672;

        private Rect _mainRect = new Rect(100, 100, 520, 650);
        private bool _isVisible = false;

        private enum DemoTab { Basics, Sliders, Layout, Performance, States, Windows }
        private DemoTab _currentTab = DemoTab.Basics;

        private GUIStyle _windowStyle;
        private GUIStyle _tabButtonStyle;
        private GUIStyle _tabButtonActiveStyle;
        private bool _stylesInitialized = false;

        private BasicsTab _basicsTab;
        private SlidersTab _slidersTab;
        private LayoutTab _layoutTab;
        private PerformanceTab _performanceTab;
        private StatesTab _statesTab;
        private WindowsTab _windowsTab;

        public IMGUIHelperWindow()
        {
            _basicsTab = new BasicsTab();
            _slidersTab = new SlidersTab();
            _layoutTab = new LayoutTab();
            _performanceTab = new PerformanceTab();
            _statesTab = new StatesTab();
            _windowsTab = new WindowsTab();
        }

        public bool IsVisible => _isVisible;
        public Rect WindowRect => _mainRect;

        public void Toggle() => _isVisible = !_isVisible;

        public void Hide()
        {
            _isVisible = false;
            _windowsTab.OnMainWindowClosed();
        }

        public void OnGUI()
        {
            if (!_isVisible) return;

            if (HighLogic.Skin != null)
                GUI.skin = HighLogic.Skin;

            InitStyles();
            _slidersTab.UpdatePending();

            _mainRect = GUILayout.Window(MAIN_WINDOW_ID, _mainRect, DrawMainWindow,
                IMGUIHelperUIStrings.Common.WindowTitle,
                _windowStyle,
                GUILayout.Width(480),
                GUILayout.Height(600));

            _mainRect.x = Mathf.Clamp(_mainRect.x, 0, Screen.width - Mathf.Max(_mainRect.width, 100));
            _mainRect.y = Mathf.Clamp(_mainRect.y, 0, Screen.height - Mathf.Max(_mainRect.height, 50));

            // Update docked window position
            _windowsTab.OnMainWindowRectChanged(_mainRect);

            // Draw secondary windows
            _windowsTab.DrawSecondaryWindows(_mainRect);
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;

            _windowStyle = IMGUIHelperUIResources.Styles.Window();
            _tabButtonStyle = IMGUIHelperUIResources.Styles.TabButton();
            _tabButtonActiveStyle = IMGUIHelperUIResources.Styles.TabButtonActive();

            _stylesInitialized = true;
        }

        private void DrawMainWindow(int windowId)
        {
            GUILayout.BeginVertical(GUILayout.Width(480));

            DrawTabs();
            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.NORMAL);

            switch (_currentTab)
            {
                case DemoTab.Basics:
                    _basicsTab.Draw();
                    break;
                case DemoTab.Sliders:
                    _slidersTab.Draw();
                    break;
                case DemoTab.Layout:
                    _layoutTab.Draw();
                    break;
                case DemoTab.Performance:
                    _performanceTab.Draw();
                    break;
                case DemoTab.States:
                    _statesTab.Draw();
                    break;
                case DemoTab.Windows:
                    _windowsTab.Draw();
                    break;
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(IMGUIHelperUIStrings.Windows.Close, GUILayout.Height(30)))
                Hide();

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void DrawTabs()
        {
            GUILayout.BeginHorizontal();

            DrawTabButton(IMGUIHelperUIStrings.Tabs.Basics, DemoTab.Basics);
            DrawTabButton(IMGUIHelperUIStrings.Tabs.Sliders, DemoTab.Sliders);
            DrawTabButton(IMGUIHelperUIStrings.Tabs.Layout, DemoTab.Layout);
            DrawTabButton(IMGUIHelperUIStrings.Tabs.Performance, DemoTab.Performance);
            DrawTabButton(IMGUIHelperUIStrings.Tabs.States, DemoTab.States);
            DrawTabButton(IMGUIHelperUIStrings.Tabs.Windows, DemoTab.Windows);

            GUILayout.EndHorizontal();
        }

        private void DrawTabButton(string label, DemoTab tab)
        {
            GUIStyle style = (_currentTab == tab) ? _tabButtonActiveStyle : _tabButtonStyle;
            if (GUILayout.Button(label, style,
                GUILayout.Height(IMGUIHelperUIResources.Layout.Tabs.BUTTON_HEIGHT),
                GUILayout.Width(72)))
            {
                _currentTab = tab;
            }
        }
    }
}
