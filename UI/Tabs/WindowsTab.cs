using UnityEngine;

namespace IMGUI_Helper.UI.Tabs
{
    public class WindowsTab
    {
        private const int INDEPENDENT_WINDOW_ID = 9438674;
        private const int DOCKED_WINDOW_ID = 9438675;

        private bool _showIndependent = false;
        private bool _showDocked = false;
        private Rect _independentRect = new Rect(640, 100, 280, 180);
        private Rect _dockedRect = new Rect(0, 0, 260, 180);
        private bool _dockedInitialized = false;

        public bool ShowIndependent => _showIndependent;
        public bool ShowDocked => _showDocked;

        public void OnMainWindowRectChanged(Rect mainRect)
        {
            if (_showDocked)
            {
                _dockedRect.x = mainRect.x + mainRect.width + 5f;
                _dockedRect.y = mainRect.y;
            }
        }

        public void Draw()
        {
            GUILayout.Label(IMGUIHelperUIStrings.Windows.Description);
            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.NORMAL);

            // Independent window button
            string indieText = _showIndependent ? IMGUIHelperUIStrings.Windows.CloseIndependent : IMGUIHelperUIStrings.Windows.OpenIndependent;
            if (GUILayout.Button(indieText, GUILayout.Height(30)))
            {
                _showIndependent = !_showIndependent;
            }

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.TIGHT);
            GUILayout.Label(IMGUIHelperUIStrings.Windows.IndependentHelp, IMGUIHelperUIResources.Styles.Help());

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.NORMAL);

            // Docked window button
            string dockedText = _showDocked ? IMGUIHelperUIStrings.Windows.CloseDocked : IMGUIHelperUIStrings.Windows.OpenDocked;
            if (GUILayout.Button(dockedText, GUILayout.Height(30)))
            {
                _showDocked = !_showDocked;
                _dockedInitialized = false;
            }

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.TIGHT);
            GUILayout.Label(IMGUIHelperUIStrings.Windows.DockedHelp, IMGUIHelperUIResources.Styles.Help());
        }

        public void DrawSecondaryWindows(Rect mainRect)
        {
            if (_showIndependent)
            {
                _independentRect = GUILayout.Window(INDEPENDENT_WINDOW_ID, _independentRect, DrawIndependentContents,
                    IMGUIHelperUIStrings.Windows.IndependentPanelTitle);
                _independentRect.x = Mathf.Clamp(_independentRect.x, 0, Screen.width - Mathf.Max(_independentRect.width, 80));
                _independentRect.y = Mathf.Clamp(_independentRect.y, 0, Screen.height - Mathf.Max(_independentRect.height, 40));
            }

            if (_showDocked)
            {
                if (!_dockedInitialized)
                {
                    _dockedRect.x = mainRect.x + mainRect.width + 5f;
                    _dockedRect.y = mainRect.y;
                    _dockedInitialized = true;
                }

                _dockedRect = GUILayout.Window(DOCKED_WINDOW_ID, _dockedRect, DrawDockedContents,
                    IMGUIHelperUIStrings.Windows.DockedPanelTitle);
            }
        }

        private void DrawIndependentContents(int windowId)
        {
            GUILayout.BeginVertical(GUILayout.Width(240));
            GUILayout.Label(IMGUIHelperUIStrings.Windows.IndependentPanelTitle);
            GUILayout.Space(10);
            GUILayout.Label(IMGUIHelperUIStrings.Windows.SubControlA);
            GUILayout.HorizontalSlider(0.5f, 0f, 1f);
            GUILayout.Space(5);
            GUILayout.Label(IMGUIHelperUIStrings.Windows.SubControlB);
            GUILayout.TextField("Editable text");
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(IMGUIHelperUIStrings.Windows.Close))
                _showIndependent = false;

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void DrawDockedContents(int windowId)
        {
            GUILayout.BeginVertical(GUILayout.Width(220));
            GUILayout.Label(IMGUIHelperUIStrings.Windows.DockedPanelTitle);
            GUILayout.Space(10);
            GUILayout.Label("Locked to main window.");
            GUILayout.Space(5);
            GUILayout.Label(IMGUIHelperUIStrings.Windows.SubControlA);
            GUILayout.HorizontalSlider(0.5f, 0f, 1f);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(IMGUIHelperUIStrings.Windows.Close))
                _showDocked = false;

            GUILayout.EndVertical();
            // No GUI.DragWindow() = not draggable
        }

        public void OnMainWindowClosed()
        {
            _showDocked = false;
            _dockedInitialized = false;
        }
    }
}
