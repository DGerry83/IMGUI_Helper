using System;
using UnityEngine;

namespace IMGUI_Helper.UI.Tabs
{
    public class LayoutTab
    {
        private readonly string[] _dropdownOptions = {
            IMGUIHelperUIStrings.Layout.DropdownOptionLow,
            IMGUIHelperUIStrings.Layout.DropdownOptionMedium,
            IMGUIHelperUIStrings.Layout.DropdownOptionHigh,
            IMGUIHelperUIStrings.Layout.DropdownOptionUltra
        };

        private Vector2 _layoutScrollPos;
        private bool _showInfoSection = true;
        private bool _showAdvancedSection = false;
        private bool _showDropdown = false;
        private int _dropdownIndex = 0;

        // Grid helpers
        private int _gridColumns;
        private int _gridIndex;

        public void Draw()
        {
            // Dropdown (drawn inline, inside window callback)
            GUILayout.BeginHorizontal();
            GUILayout.Label(IMGUIHelperUIStrings.Layout.QualityLabel, GUILayout.Width(IMGUIHelperUIResources.Layout.Labels.DEFAULT_WIDTH));
            if (GUILayout.Button(_dropdownOptions[_dropdownIndex] + IMGUIHelperUIStrings.Common.DropdownArrow, GUILayout.Width(100)))
            {
                _showDropdown = !_showDropdown;
            }
            GUILayout.EndHorizontal();

            if (_showDropdown)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                for (int i = 0; i < _dropdownOptions.Length; i++)
                {
                    if (GUILayout.Button(_dropdownOptions[i]))
                    {
                        _dropdownIndex = i;
                        _showDropdown = false;
                    }
                }
                GUILayout.EndVertical();
            }

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.LARGE);

            // ScrollView
            GUILayout.Label(IMGUIHelperUIStrings.Layout.ScrollViewHeader);
            _layoutScrollPos = GUILayout.BeginScrollView(_layoutScrollPos, GUILayout.Height(160));
            for (int i = 0; i < 20; i++)
                GUILayout.Label($"Scroll item {i:D2}");
            GUILayout.EndScrollView();

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.LARGE);

            // Grid Layout
            GUILayout.Label(IMGUIHelperUIStrings.Layout.GridHeader);
            BeginGrid(2);

            GridCell(() =>
            {
                GUILayout.Label("Cell 1");
                if (GUILayout.Button("A")) { }
            });

            GridCell(() =>
            {
                GUILayout.Label("Cell 2");
                _showInfoSection = GUILayout.Toggle(_showInfoSection, "Toggle");
            });

            GridCell(() =>
            {
                GUILayout.Label("Cell 3");
                GUILayout.Label("Value: 42");
            });

            GridCell(() =>
            {
                GUILayout.Label("Cell 4");
                GUILayout.Label("Status: OK");
            });

            EndGrid();

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.LARGE);

            // Expandable sections (instant foldouts, no animation)
            DrawSection(ref _showInfoSection, IMGUIHelperUIStrings.Layout.InfoSection, () =>
            {
                GUILayout.Label("Simple conditional content.");
                GUILayout.Label("No height animation = stable layout.");
            });

            GUILayout.Space(IMGUIHelperUIResources.Layout.Spacing.TIGHT);

            DrawSection(ref _showAdvancedSection, IMGUIHelperUIStrings.Layout.AdvancedSection, () =>
            {
                GUILayout.Label("Nested sections are possible.");
                GUILayout.Label("Keep nesting depth under 5 for performance.");
            });
        }

        private void DrawSection(ref bool expanded, string title, Action content)
        {
            string prefix = expanded ? IMGUIHelperUIStrings.Common.ExpandedPrefix : IMGUIHelperUIStrings.Common.CollapsedPrefix;
            expanded = GUILayout.Toggle(expanded, prefix + title, IMGUIHelperUIResources.Styles.SectionHeader());
            if (expanded)
            {
                GUILayout.Space(4);
                content?.Invoke();
            }
        }

        private void BeginGrid(int columns)
        {
            _gridColumns = columns;
            _gridIndex = 0;
            GUILayout.BeginHorizontal();
        }

        private void GridCell(Action drawContent)
        {
            GUILayout.BeginVertical();
            drawContent?.Invoke();
            GUILayout.EndVertical();

            _gridIndex++;
            if (_gridIndex % _gridColumns == 0)
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
            }
        }

        private void EndGrid()
        {
            GUILayout.EndHorizontal();
            _gridIndex = 0;
        }
    }
}
