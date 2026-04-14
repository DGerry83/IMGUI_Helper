using UnityEngine;

namespace IMGUI_Helper.UI.Tabs
{
    /// <summary>
    /// Demonstrates a dense multi-state button grid with right-click handling,
    /// visual state cycling, and aggregate status-driven instructional text.
    /// </summary>
    public class StatesTab
    {
        private const int GRID_ROWS = 4;
        private const int GRID_COLS = 4;
        private const int TOTAL_SLOTS = 16;

        public enum SlotState { Disarmed, ArmedDeactivated, ArmedActivated }

        private readonly SlotState[] _slotStates = new SlotState[TOTAL_SLOTS];
        private readonly GUIStyle[] _slotStyles = new GUIStyle[3];
        private bool _stylesInitialized = false;

        public StatesTab()
        {
            for (int i = 0; i < TOTAL_SLOTS; i++)
                _slotStates[i] = SlotState.Disarmed;
        }

        public void Draw()
        {
            InitStyles();

            // Outer bordered container (demarcated section)
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();

            // LEFT: 4x4 grid column
            GUILayout.BeginVertical(GUILayout.Width(160));
            DrawGrid();
            GUILayout.EndVertical();

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.NORMAL);

            // RIGHT: instructional text column
            GUILayout.BeginVertical(GUILayout.Width(240));
            DrawInstructions();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.NORMAL);

            // Reset button outside the box
            if (GUILayout.Button("Reset All", GUILayout.Width(100)))
            {
                for (int i = 0; i < TOTAL_SLOTS; i++)
                    _slotStates[i] = SlotState.Disarmed;
            }
        }

        private void DrawGrid()
        {
            for (int row = 0; row < GRID_ROWS; row++)
            {
                GUILayout.BeginHorizontal();
                for (int col = 0; col < GRID_COLS; col++)
                {
                    int index = row * GRID_COLS + col;
                    DrawSlotButton(index);
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawSlotButton(int index)
        {
            SlotState state = _slotStates[index];
            GUIStyle style = _slotStyles[(int)state];
            string label = (index + 1).ToString();

            // Get explicit rect so we can detect right-clicks before GUI.Button consumes them
            Rect buttonRect = GUILayoutUtility.GetRect(32, 30, style);

            Event evt = Event.current;
            if (evt.type == EventType.MouseDown && evt.button == 1 && buttonRect.Contains(evt.mousePosition))
            {
                // Right-click: disarm
                if (state != SlotState.Disarmed)
                {
                    _slotStates[index] = SlotState.Disarmed;
                    evt.Use();
                }
            }
            else if (GUI.Button(buttonRect, label, style))
            {
                // Left-click: cycle state
                switch (state)
                {
                    case SlotState.Disarmed:
                        _slotStates[index] = SlotState.ArmedDeactivated;
                        break;
                    case SlotState.ArmedDeactivated:
                        _slotStates[index] = SlotState.ArmedActivated;
                        break;
                    case SlotState.ArmedActivated:
                        _slotStates[index] = SlotState.ArmedDeactivated;
                        break;
                }
            }
        }

        private void DrawInstructions()
        {
            GUIStyle header = IMGUIHelperUIResources.Styles.SectionHeader();
            GUILayout.Label("Instructions", header);
            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.TIGHT);

            // Compute aggregate state
            int armedCount = 0;
            int activatedCount = 0;
            for (int i = 0; i < TOTAL_SLOTS; i++)
            {
                if (_slotStates[i] == SlotState.ArmedDeactivated) armedCount++;
                if (_slotStates[i] == SlotState.ArmedActivated) activatedCount++;
            }

            GUIStyle body = new GUIStyle(HighLogic.Skin.label);
            body.wordWrap = true;

            string message;
            if (activatedCount > 0)
            {
                message = $"A button is activated ({activatedCount}).\n\nClick to deactivate, or right-click to disarm.";
                body.normal.textColor = new Color(1f, 0.8f, 0.4f);
            }
            else if (armedCount > 0)
            {
                message = $"A button is armed ({armedCount}).\n\nClick to activate, or right-click to disarm.";
                body.normal.textColor = new Color(0.9f, 0.5f, 0.1f);
            }
            else
            {
                message = "No buttons are armed.\n\nClick any button to arm it.";
                body.normal.textColor = Color.gray;
            }

            GUILayout.Label(message, body);
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;

            // Disarmed: gray
            _slotStyles[0] = MakeSolidStyle(new Color(0.35f, 0.35f, 0.35f), Color.gray);
            // ArmedDeactivated: dark orange
            _slotStyles[1] = MakeSolidStyle(new Color(0.7f, 0.35f, 0.05f), Color.white);
            // ArmedActivated: light orange
            _slotStyles[2] = MakeSolidStyle(new Color(1.0f, 0.6f, 0.15f), Color.white, FontStyle.Bold);

            _stylesInitialized = true;
        }

        private static GUIStyle MakeSolidStyle(Color background, Color text, FontStyle font = FontStyle.Normal)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, background);
            tex.Apply();

            GUIStyle style = new GUIStyle(HighLogic.Skin.button);
            style.fontStyle = font;
            style.normal.textColor = text;
            style.hover.textColor = text;
            style.active.textColor = text;
            style.normal.background = tex;
            style.hover.background = tex;
            style.active.background = tex;
            return style;
        }
    }
}
