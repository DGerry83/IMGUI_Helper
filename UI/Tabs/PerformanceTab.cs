using System.Collections.Generic;
using UnityEngine;

namespace IMGUI_Helper.UI.Tabs
{
    /// <summary>
    /// Demonstrates the difference between a naive large list and a virtualized one.
    /// 
    /// The "naive" approach uses GUILayout.Label for every item. IMGUI processes layout 
    /// for the entire collection every frame, so 1000 items = 1000 layout passes.
    /// Frame rate tanks.
    /// 
    /// The "virtualized" approach reserves total height with one GUILayoutUtility.GetRect,
    /// then uses GUI.Label with explicit Rects to draw only the ~12 rows actually visible.
    /// Frame rate stays smooth regardless of list size.
    /// </summary>
    public class PerformanceTab
    {
        private const float ROW_HEIGHT = 22f;
        private const float LIST_VIEW_HEIGHT = 200f;

        private readonly List<string> _virtualListData = new List<string>();
        private Vector2 _scrollPos;
        private bool _useVirtualization = true;

        public PerformanceTab()
        {
            for (int i = 0; i < 1000; i++)
                _virtualListData.Add($"Telemetry Entry {i:D4}: {UnityEngine.Random.value:F4}");
        }

        public void Draw()
        {
            GUILayout.Label("Naive vs Virtualized List (1000 items)");
            GUILayout.Label("Toggle to feel the difference:", IMGUIHelperUIResources.Styles.Help());

            GUILayout.BeginHorizontal();
            bool newVirtualization = GUILayout.Toggle(_useVirtualization, "Use Virtualization");
            if (newVirtualization != _useVirtualization)
            {
                _useVirtualization = newVirtualization;
                _scrollPos = Vector2.zero; // reset scroll when switching modes
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.TIGHT);

            if (_useVirtualization)
            {
                GUILayout.Label("Virtualized: only ~12 items are actually drawn.");
                GUILayout.Label(string.Format(IMGUIHelperUIStrings.Performance.ScrollYFormat, _scrollPos.y));
                DrawVirtualizedList();
            }
            else
            {
                GUILayout.Label("Naive: all 1000 GUILayout items processed every frame.");
                GUILayout.Label("Try scrolling — notice the stutter.");
                DrawNaiveList();
            }
        }

        private void DrawVirtualizedList()
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(LIST_VIEW_HEIGHT));

            float totalHeight = _virtualListData.Count * ROW_HEIGHT;
            Rect viewRect = GUILayoutUtility.GetRect(0, totalHeight);

            int firstVisible = Mathf.FloorToInt(_scrollPos.y / ROW_HEIGHT);
            int visibleCount = Mathf.CeilToInt(LIST_VIEW_HEIGHT / ROW_HEIGHT) + 2;
            int lastVisible = Mathf.Min(_virtualListData.Count, firstVisible + visibleCount);

            for (int i = firstVisible; i < lastVisible; i++)
            {
                Rect rowRect = new Rect(viewRect.x, viewRect.y + (i * ROW_HEIGHT), viewRect.width, ROW_HEIGHT);
                GUI.Label(rowRect, _virtualListData[i]);

                if (Event.current.type == EventType.Repaint && rowRect.Contains(Event.current.mousePosition))
                {
                    GUI.Box(rowRect, "", HighLogic.Skin.textArea);
                    GUI.Label(rowRect, _virtualListData[i]);
                }
            }

            GUILayout.EndScrollView();
        }

        private void DrawNaiveList()
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(LIST_VIEW_HEIGHT));
            for (int i = 0; i < _virtualListData.Count; i++)
            {
                GUILayout.Label(_virtualListData[i]);
            }
            GUILayout.EndScrollView();
        }
    }
}
