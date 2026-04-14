using UnityEngine;

namespace IMGUI_Helper.UI
{
    public static class IMGUIHelperUIResources
    {
        public static class Colors
        {
            public static readonly Color TOGGLE_ACTIVE_GREEN = new Color(0.2f, 0.9f, 0.2f);
            public static readonly Color INFO_ORANGE = new Color(1f, 0.5490196f, 0f);
            public static readonly Color TEXT_DIM = Color.gray;
        }

        public static class Layout
        {
            public static class Tabs
            {
                public const float BUTTON_WIDTH = 85f;
                public const float BUTTON_HEIGHT = 28f;
            }

            public static class Labels
            {
                public const float DEFAULT_WIDTH = 80f;
                public const float VALUE_WIDTH = 60f;
                public const float SLIDER_WIDTH = 180f;
            }

            public static class Spacing
            {
                public const float TIGHT = 4f;
                public const float NORMAL = 10f;
                public const float LARGE = 15f;
            }
        }

        public static class Styles
        {
            public static GUIStyle Window()
            {
                return new GUIStyle(HighLogic.Skin.window);
            }

            public static GUIStyle TabButton()
            {
                return new GUIStyle(HighLogic.Skin.button);
            }

            public static GUIStyle TabButtonActive()
            {
                GUIStyle style = new GUIStyle(HighLogic.Skin.button);
                style.normal.textColor = Colors.TOGGLE_ACTIVE_GREEN;
                style.fontStyle = FontStyle.Bold;
                return style;
            }

            public static GUIStyle ToggleActive()
            {
                GUIStyle style = new GUIStyle(HighLogic.Skin.toggle);
                style.normal.textColor = Colors.TOGGLE_ACTIVE_GREEN;
                style.onNormal.textColor = Colors.TOGGLE_ACTIVE_GREEN;
                style.fontStyle = FontStyle.Bold;
                return style;
            }

            public static GUIStyle Help()
            {
                GUIStyle style = new GUIStyle(HighLogic.Skin.label);
                style.normal.textColor = Colors.INFO_ORANGE;
                style.wordWrap = true;
                style.fontSize = 11;
                return style;
            }

            public static GUIStyle SectionHeader()
            {
                GUIStyle style = new GUIStyle(HighLogic.Skin.label);
                style.fontStyle = FontStyle.Bold;
                style.alignment = TextAnchor.MiddleLeft;
                return style;
            }

            public static GUIStyle ColorButton(Color backgroundColor)
            {
                GUIStyle style = new GUIStyle(HighLogic.Skin.button);
                Texture2D tex = MakeColorTexture(backgroundColor);
                style.normal.background = tex;
                style.hover.background = tex;
                style.active.background = tex;
                style.normal.textColor = Color.white;
                return style;
            }

            private static Texture2D MakeColorTexture(Color color)
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, color);
                tex.Apply();
                return tex;
            }
        }
    }
}
