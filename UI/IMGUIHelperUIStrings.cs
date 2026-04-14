namespace IMGUI_Helper.UI
{
    public static class IMGUIHelperUIStrings
    {
        public static class Common
        {
            public const string WindowTitle = "IMGUI Showcase";
            public const string CollapsedPrefix = "▶ ";
            public const string ExpandedPrefix = "▼ ";
            public const string DropdownArrow = " ▼";
        }

        public static class Tabs
        {
            public const string Basics = "Basics";
            public const string Sliders = "Sliders";
            public const string Layout = "Layout";
            public const string Performance = "Perf";
            public const string Windows = "Windows";
        }

        public static class Basics
        {
            public const string HoverOverMe = "Hover over me";
            public const string BuiltInTooltip = "This is Unity's built-in tooltip system";
            public const string ToggleButtonGroup = "Toggle Button Group:";
            public const string Classic = "Classic";
            public const string SoftHdr = "Soft HDR";
            public const string ColorButton = "Color Button";
            public const string StateRestoration = "State Restoration Demo:";
            public const string DisabledButton = "I am disabled (restored after)";
            public const string CustomTooltipLabel = "Hover here for custom tooltip";
            public const string CustomTooltipText = "Custom tooltip drawn manually during Repaint!";
        }

        public static class Sliders
        {
            public const string Linear = "Linear: {0:F2}";
            public const string Integer = "Integer: {0}";
            public const string Exponential = "Exponential: {0:F2}";
            public const string DisplayMapped = "Display-Mapped: {0:F1}%";
            public const string Throttled = "Throttled: {0:F2}  |  Committed: {1:F2}";
        }

        public static class Layout
        {
            public const string QualityLabel = "Quality:";
            public const string ScrollViewHeader = "ScrollView (20 items):";
            public const string GridHeader = "Grid Layout (2 columns):";
            public const string InfoSection = "Info Section";
            public const string AdvancedSection = "Advanced Section";
            public const string DropdownOptionLow = "Low";
            public const string DropdownOptionMedium = "Medium";
            public const string DropdownOptionHigh = "High";
            public const string DropdownOptionUltra = "Ultra";
        }

        public static class Performance
        {
            public const string VirtualizedListHeader = "Virtualized List (1000 items, ~12 drawn):";
            public const string ScrollYFormat = "Scroll Y: {0:F0}";
        }

        public static class Windows
        {
            public const string Description = "Demonstrates independent and docked secondary windows.";
            public const string OpenIndependent = "Open Independent Window";
            public const string CloseIndependent = "Close Independent Window";
            public const string OpenDocked = "Open Docked Window";
            public const string CloseDocked = "Close Docked Window";
            public const string IndependentHelp = "Independent window: draggable, persists position.";
            public const string DockedHelp = "Docked window: locked to the right of this window.";
            public const string DockedPanelTitle = "Docked Panel";
            public const string IndependentPanelTitle = "Independent Panel";
            public const string SubControlA = "Sub-control A:";
            public const string SubControlB = "Sub-control B:";
            public const string Close = "Close";
        }
    }
}
