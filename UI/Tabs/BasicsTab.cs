using UnityEngine;

namespace IMGUI_Helper.UI.Tabs
{
    public class BasicsTab
    {
        private bool _useSoftBloom = false;
        private bool _showColorDropdown = false;
        private int _selectedColorIndex = 0;
        private readonly string[] _colorNames = { "Crimson", "Forest", "Ocean", "Amethyst" };
        private readonly Color[] _colors = {
            new Color(0.9f, 0.2f, 0.2f),
            new Color(0.2f, 0.7f, 0.2f),
            new Color(0.2f, 0.4f, 0.9f),
            new Color(0.6f, 0.2f, 0.8f)
        };
        private GUIStyle[] _colorButtonStyles = null;

        // Custom tooltip state
        private string _pendingTooltip = null;
        private float _tooltipHoverTime = 0f;
        private const float TOOLTIP_DELAY = 0.4f;

        public void Draw()
        {
            // Built-in tooltip via GUIContent
            GUIContent labelContent = new GUIContent(
                IMGUIHelperUIStrings.Basics.HoverOverMe,
                IMGUIHelperUIStrings.Basics.BuiltInTooltip);
            GUILayout.Label(labelContent);

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.NORMAL);

            // Toggle buttons
            GUILayout.Label(IMGUIHelperUIStrings.Basics.ToggleButtonGroup);
            GUILayout.BeginHorizontal();
            bool useClassic = !_useSoftBloom;
            bool useSoft = _useSoftBloom;

            bool newClassic = GUILayout.Toggle(useClassic, IMGUIHelperUIStrings.Basics.Classic, "button");
            bool newSoft = GUILayout.Toggle(useSoft, IMGUIHelperUIStrings.Basics.SoftHdr, "button");

            if (newClassic && !useClassic)
                _useSoftBloom = false;
            else if (newSoft && !useSoft)
                _useSoftBloom = true;
            GUILayout.EndHorizontal();

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.NORMAL);

            // Colored dropdown
            DrawColorDropdown();

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.NORMAL);

            // Color button
            bool parentEnabled = GUI.enabled;
            try
            {
                GUI.enabled = true;
                if (GUILayout.Button(IMGUIHelperUIStrings.Basics.ColorButton,
                    IMGUIHelperUIResources.Styles.ColorButton(IMGUIHelperUIResources.Colors.INFO_ORANGE),
                    GUILayout.Height(28)))
                {
                    ScreenMessages.PostScreenMessage("Color button clicked!", 2f);
                }
            }
            finally
            {
                GUI.enabled = parentEnabled;
            }

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.NORMAL);

            // Disabled control + state restoration demo
            GUILayout.Label(IMGUIHelperUIStrings.Basics.StateRestoration);
            bool oldEnabled = GUI.enabled;
            try
            {
                GUI.enabled = false;
                GUILayout.Button(IMGUIHelperUIStrings.Basics.DisabledButton);
            }
            finally
            {
                GUI.enabled = oldEnabled;
            }

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.NORMAL);

            // Custom hover tooltip using GetLastRect + Repaint guard
            GUILayout.Label(IMGUIHelperUIStrings.Basics.CustomTooltipLabel);
            if (Event.current.type == EventType.Repaint)
            {
                Rect lastRect = GUILayoutUtility.GetLastRect();
                if (lastRect.width > 1 && lastRect.height > 1)
                {
                    bool hovered = lastRect.Contains(Event.current.mousePosition);
                    if (hovered)
                    {
                        if (_pendingTooltip != IMGUIHelperUIStrings.Basics.CustomTooltipText)
                        {
                            _pendingTooltip = IMGUIHelperUIStrings.Basics.CustomTooltipText;
                            _tooltipHoverTime = Time.realtimeSinceStartup;
                        }
                    }
                    else if (_pendingTooltip == IMGUIHelperUIStrings.Basics.CustomTooltipText)
                    {
                        _pendingTooltip = null;
                    }
                }
            }

            // Draw custom tooltip at end of tab (window-local coordinates)
            DrawCustomTooltip();

            // Draw built-in tooltip at end of tab
            DrawBuiltInTooltip();
        }

        private void DrawCustomTooltip()
        {
            if (string.IsNullOrEmpty(_pendingTooltip)) return;
            if (Time.realtimeSinceStartup - _tooltipHoverTime < TOOLTIP_DELAY) return;
            if (Event.current.type != EventType.Repaint) return;

            GUIStyle style = HighLogic.Skin.box;
            GUIContent content = new GUIContent(_pendingTooltip);
            float width = Mathf.Min(style.CalcSize(content).x + 16f, 250f);
            float height = style.CalcHeight(content, width) + 8f;

            Vector2 mouse = Event.current.mousePosition;
            float x = mouse.x + 15f;
            float y = mouse.y + 15f;

            GUI.Box(new Rect(x, y, width, height), _pendingTooltip, style);
        }

        private void DrawBuiltInTooltip()
        {
            if (string.IsNullOrEmpty(GUI.tooltip)) return;

            GUIStyle style = HighLogic.Skin.box;
            GUIContent content = new GUIContent(GUI.tooltip);
            float width = Mathf.Min(250f, style.CalcSize(content).x + 20f);
            float height = style.CalcHeight(content, width) + 10f;

            Vector2 mouse = Event.current.mousePosition;
            float x = mouse.x + 15f;
            float y = mouse.y + 15f;

            GUI.Box(new Rect(x, y, width, height), GUI.tooltip, style);
        }

        private void DrawColorDropdown()
        {
            EnsureColorStyles();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Favorite Color:", GUILayout.Width(100));
            GUIStyle currentStyle = _colorButtonStyles[_selectedColorIndex];
            if (GUILayout.Button(_colorNames[_selectedColorIndex], currentStyle, GUILayout.Width(120)))
            {
                _showColorDropdown = !_showColorDropdown;
            }
            GUILayout.EndHorizontal();

            if (_showColorDropdown)
            {
                GUIStyle boxStyle = IMGUIHelperUIResources.Styles.Window();
                GUILayout.BeginVertical(GUI.skin.box);
                for (int i = 0; i < _colorNames.Length; i++)
                {
                    if (GUILayout.Button(_colorNames[i], _colorButtonStyles[i]))
                    {
                        if (_selectedColorIndex != i)
                            _selectedColorIndex = i;
                        _showColorDropdown = false;
                    }
                }
                GUILayout.EndVertical();
            }
        }

        private void EnsureColorStyles()
        {
            if (_colorButtonStyles != null) return;

            _colorButtonStyles = new GUIStyle[_colorNames.Length];
            for (int i = 0; i < _colorNames.Length; i++)
            {
                _colorButtonStyles[i] = IMGUIHelperUIResources.Styles.ColorButton(_colors[i]);
            }
        }
    }
}
