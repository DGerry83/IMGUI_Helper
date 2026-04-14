# Unity IMGUI Engineering Reference

**Context:** Immediate Mode GUI patterns for Kerbal Space Program mod development.  
**Core Constraint:** IMGUI rebuilds the entire UI description every frame—no persistent widget state exists between frames.

---

## 1. Event System Architecture

IMGUI executes multiple passes per frame. Code must be pass-aware.

### 1.1 Event Pass Types
| Pass | Purpose |
|------|---------|
| `Layout` | Calculate element geometry and positions |
| `Repaint` | Execute actual rendering |
| Input Events | `MouseMove`, `MouseDown`, `ScrollWheel`, etc. |

### 1.2 Critical Rule: Layout Query Timing
**Only query layout rectangles during `Repaint`.**

```csharp
// CORRECT
if (Event.current.type == EventType.Repaint)
{
    Rect r = GUILayoutUtility.GetLastRect();
    // Use r for hover detection, tooltip positioning, etc.
}
```

**Anti-pattern:** Querying `GetLastRect()` during `Layout` pass yields undefined/cached values.

---

## 2. Rect Lifecycle Constraints

### 2.1 Frame-Local Validity
Rects returned by `GUILayoutUtility.GetLastRect()` are **valid for the current frame only**. Layout recalculates every frame.

**Forbidden:**
```csharp
// WRONG: Do not cache rects in member variables
_lastSliderRect = GUILayoutUtility.GetLastRect();
```

**Correct Pattern:** Detect hover/tooltips within the same `OnGUI` call where the control is drawn.

### 2.2 Invalid Rect Filtering
Always guard against degenerate rectangles:

```csharp
if (rect.width <= 1 || rect.height <= 1)
    return; // Skip processing this frame
```

---

## 3. State Management

### 3.1 Global State Stack
IMGUI uses immediate-mode global state. Any modification affects subsequent draw calls.

**Critical State Variables:**
- `GUI.enabled`
- `GUI.color`
- `GUI.backgroundColor`
- `GUI.matrix`

**Mandatory Restoration Pattern:**
```csharp
bool oldEnabled = GUI.enabled;
try
{
    GUI.enabled = false;
    GUILayout.Button("Disabled Control");
}
finally
{
    GUI.enabled = oldEnabled; // Always restore
}
```

---

## 4. Layout Patterns

### 4.1 Nesting Depth Limits
**Constraint:** Keep nesting depth under 5 levels (`Vertical > Horizontal > Vertical...`).  
Each layer triggers recursive layout calculations—deep nesting causes frame time spikes.

### 4.2 Constant Centralization
**Anti-pattern:** Magic numbers scattered in layout code.  
**Pattern:** Static layout configuration classes.

```csharp
// CinematicShadersUIResources.Layout.LabelWidth example
GUILayout.Label(label, GUILayout.Width(CinematicShadersUIResources.Layout.LabelWidth));

// Or local constants
const float LABEL_WIDTH = 150f;
```

**Spacing Constants:**
- `NORMAL = 10`
- `TIGHT = 4`
- `SECTION = 15`
- `LARGE = 15`

### 4.3 Horizontal Grouping for Label+Control
**Stable Pattern:**
```csharp
GUILayout.BeginHorizontal();
GUILayout.Label(label, GUILayout.Width(LABEL_WIDTH));
value = GUILayout.HorizontalSlider(value, min, max);
GUILayout.EndHorizontal();
```

### 4.4 Window Size Stabilization
Prevent `GUILayout.Window` from stretching based on content by constraining the inner layout:

```csharp
// Outer window call
_windowRect = GUILayout.Window(WINDOW_ID, _windowRect, DrawWindow, "Title");

// Inner content
void DrawWindow(int id)
{
    GUILayout.BeginVertical(GUILayout.Width(300));
    // ... content ...
    GUILayout.EndVertical();
    GUI.DragWindow();
}
```

---

## 5. Control Implementation Patterns

### 5.1 Slider Patterns

#### 5.1.1 Linear Slider
```csharp
float newValue = GUILayout.HorizontalSlider(value, min, max);
if (!Mathf.Approximately(newValue, value))
{
    value = newValue;
    PushSettingsToNative();
}
```

#### 5.1.2 Integer Slider
Map a float slider to an integer value:
```csharp
float floatValue = value;
float newValue = GUILayout.HorizontalSlider(floatValue, min, max);
int newIntValue = Mathf.RoundToInt(newValue);

if (newIntValue != value)
{
    value = newIntValue;
    PushSettingsToNative();
}
```

#### 5.1.3 Exponential Slider
Useful for values that need fine control at low end and coarse control at high end:
```csharp
float normalized = Mathf.InverseLerp(min, max, value);
float sliderT = Mathf.Pow(normalized, 1.0f / exponent);
float newSliderT = GUILayout.HorizontalSlider(sliderT, 0f, 1f);
float newNormalized = Mathf.Pow(newSliderT, exponent);
float newValue = Mathf.Lerp(min, max, newNormalized);
```

#### 5.1.4 Display-Mapped Slider
When the internal value differs from the user-facing display value:
```csharp
float displayValue = value * 100.0f;
float newDisplayValue = GUILayout.HorizontalSlider(displayValue, 0f, 10f);
value = newDisplayValue / 100.0f;
```

#### 5.1.5 Throttled Slider
Prevent expensive updates (e.g., catalog regeneration) from firing every frame during drag:
```csharp
private const float THROTTLE_SECONDS = 0.05f;
private float _lastPushTime = -999f;
private bool _pushPending = false;

void DrawThrottledSlider(ref float value, float min, float max)
{
    float newValue = GUILayout.HorizontalSlider(value, min, max);
    if (!Mathf.Approximately(newValue, value))
    {
        value = newValue;
        _pushPending = true;

        if (Time.time - _lastPushTime >= THROTTLE_SECONDS)
        {
            PushSettingsToNative();
            _lastPushTime = Time.time;
            _pushPending = false;
        }
    }
}

// In Draw() entry point, catch any pending final update:
if (_pushPending && Time.time - _lastPushTime >= THROTTLE_SECONDS)
{
    PushSettingsToNative();
    _lastPushTime = Time.time;
    _pushPending = false;
}
```

### 5.2 ScrollView Persistence
**Critical Bug Source:** Scroll position must be stored in a persistent member variable.

```csharp
// WRONG: Missing assignment causes reset every frame
GUILayout.BeginScrollView(_scrollPos);

// CORRECT
_scrollPos = GUILayout.BeginScrollView(_scrollPos);
GUILayout.EndScrollView();
```

### 5.3 Collapsible Sections
Use foldouts to prevent layout cost for hidden UI sections:

```csharp
_showSection = GUILayout.Toggle(
    _showSection,
    (_showSection ? " ▼ " : " ▶ ") + "Advanced Generation",
    HighLogic.Skin.label
);

if (_showSection)
{
    DrawAdvancedSection();
}
```

---

## 6. Widget Patterns

### 6.1 Tabs
Horizontal tab bar with active/inactive visual states:

```csharp
public enum ShaderTab { GTAO, Starfield, Kartographer }
private ShaderTab currentTab = ShaderTab.GTAO;

private void DrawTabs()
{
    GUILayout.BeginHorizontal();

    GUIStyle gtaoStyle = (currentTab == ShaderTab.GTAO) ? tabButtonActiveStyle : tabButtonStyle;
    if (GUILayout.Button("GTAO", gtaoStyle, GUILayout.Height(30), GUILayout.Width(100)))
        currentTab = ShaderTab.GTAO;

    GUIStyle starfieldStyle = (currentTab == ShaderTab.Starfield) ? tabButtonActiveStyle : tabButtonStyle;
    if (GUILayout.Button("Starfield", starfieldStyle, GUILayout.Height(30), GUILayout.Width(100)))
        currentTab = ShaderTab.Starfield;

    GUILayout.EndHorizontal();
}
```

**Active Style Pattern:**
```csharp
public static GUIStyle TabButtonActive()
{
    GUIStyle style = new GUIStyle(HighLogic.Skin.button);
    style.normal.textColor = new Color(0.2f, 0.9f, 0.2f);
    style.fontStyle = FontStyle.Bold;
    return style;
}
```

### 6.2 Dropdown Menus
Custom dropdown with mutual exclusivity (opening one closes the other):

```csharp
private bool _showQualityDropdown = false;
private bool _showDebugDropdown = false;
private int _qualityPresetIndex = 0;
private string[] _qualityNames = { "Low", "Medium", "High", "Ultra" };

private void DrawQualityDropdown()
{
    GUILayout.BeginHorizontal();
    GUILayout.Label("Quality", GUILayout.Width(80));

    if (GUILayout.Button(_qualityNames[_qualityPresetIndex], GUILayout.Width(100)))
    {
        _showQualityDropdown = !_showQualityDropdown;
        _showDebugDropdown = false; // Close other dropdown
    }
    GUILayout.EndHorizontal();

    if (_showQualityDropdown)
    {
        GUILayout.BeginVertical(GUI.skin.box);
        for (int i = 0; i < _qualityNames.Length; i++)
        {
            if (GUILayout.Button(_qualityNames[i]))
            {
                _qualityPresetIndex = i;
                _showQualityDropdown = false;
            }
        }
        GUILayout.EndVertical();
    }
}
```

### 6.3 Toggle Buttons (Button-Style Toggles)
Useful for mode selection where only one option can be active:

```csharp
bool useClassic = !_useSoftBloom;
bool useSoft = _useSoftBloom;

bool newClassic = GUILayout.Toggle(useClassic, "Classic", "button");
bool newSoft = GUILayout.Toggle(useSoft, "Soft HDR", "button");

if (newClassic && !useClassic)
    _useSoftBloom = false;
else if (newSoft && !useSoft)
    _useSoftBloom = true;
```

### 6.4 SelectionGrid
Vertical or horizontal radio-button-like selection:

```csharp
string[] options = { "KSP", "Retro" };
int currentStyle = (int)_selectedStyle;
int newStyle = GUILayout.SelectionGrid(currentStyle, options, 1, HighLogic.Skin.toggle);
if (newStyle != currentStyle)
    _selectedStyle = (MyEnum)newStyle;
```

### 6.5 Color Buttons
Generate a colored button background texture on the fly:

```csharp
public static GUIStyle ColorButton(Color backgroundColor)
{
    GUIStyle style = new GUIStyle(HighLogic.Skin.button);
    Texture2D tex = new Texture2D(1, 1);
    tex.SetPixel(0, 0, backgroundColor);
    tex.Apply();
    style.normal.background = tex;
    style.normal.textColor = Color.black;
    style.hover.background = tex;
    style.active.background = tex;
    return style;
}
```

*Note: In a wrapper API, cache the texture to avoid per-frame allocation.*

### 6.6 Active/Inactive Visual States
Consistent pattern for toggles and buttons that need to show "on" vs "off" state:

```csharp
GUIStyle toggleStyle = _settingEnabled ?
    CinematicShadersUIResources.Styles.ToggleActive() : HighLogic.Skin.toggle;

bool newValue = GUILayout.Toggle(_settingEnabled, " Enable Feature", toggleStyle);
if (newValue != _settingEnabled)
    _settingEnabled = newValue;
```

---

## 7. Window Management

### 7.1 Window ID Uniqueness
**Constraint:** Each window requires a unique `int` ID. Conflicts cause input fighting.

**Pattern:** Use large random constants.
```csharp
const int WINDOW_ID = 9438672; // Unique per window class
```

### 7.2 Drag Implementation
Standard KSP draggable window pattern:

```csharp
_windowRect = GUILayout.Window(
    WINDOW_ID,
    _windowRect,
    DrawWindowContents,
    "Window Title"
);

void DrawWindowContents(int windowId)
{
    GUI.DragWindow(); // Entire window draggable
    // Content here
}
```

For title-bar-only dragging:
```csharp
GUI.DragWindow(new Rect(0, 0, 10000, 20)); // Top drag handle only
```

### 7.3 Viewport Clamping (Critical)
Windows must be constrained to screen bounds to prevent loss:

```csharp
_windowRect.x = Mathf.Clamp(_windowRect.x, 0, Screen.width - _windowRect.width);
_windowRect.y = Mathf.Clamp(_windowRect.y, 0, Screen.height - _windowRect.height);
```

### 7.4 Docking Windows
Position a secondary window relative to a primary window:

```csharp
Rect mainRect = CinematicShadersWindow.Instance.WindowRect;
_windowRect.x = mainRect.x + mainRect.width + 5f;
_windowRect.y = mainRect.y;
```

Reset dock state when hidden so it re-docks correctly on next show:
```csharp
public void Hide()
{
    _isVisible = false;
    _positionInitialized = false;
}
```

---

## 8. Tooltip System

Unity's built-in `GUI.tooltip` is reliable for standard `GUILayout` controls when the `tooltip` parameter of `GUIContent` is set.

**Working Pattern:**
```csharp
private void DrawTooltip()
{
    if (string.IsNullOrEmpty(GUI.tooltip))
        return;

    Vector2 mousePos = Event.current.mousePosition;
    GUIStyle tooltipStyle = HighLogic.Skin.box;
    float tooltipWidth = Mathf.Min(250f, tooltipStyle.CalcSize(new GUIContent(GUI.tooltip)).x + 20f);
    float tooltipHeight = tooltipStyle.CalcHeight(new GUIContent(GUI.tooltip), tooltipWidth) + 10f;

    float x = mousePos.x + 15f;
    float y = mousePos.y + 15f;

    // Clamp to window bounds
    Rect windowRect = CinematicShadersWindow.Instance.WindowRect;
    x = Mathf.Min(x, windowRect.width - tooltipWidth - 5f);
    y = Mathf.Min(y, windowRect.height - tooltipHeight - 5f);

    GUI.Box(new Rect(x, y, tooltipWidth, tooltipHeight), GUI.tooltip, tooltipStyle);
}
```

**Usage with controls:**
```csharp
GUIContent labelContent = new GUIContent("Exposure", "EV Stops");
GUILayout.Label(labelContent, GUILayout.Width(80));
```

**Alternative:** For complex custom-drawn controls, custom hover detection using `GUILayoutUtility.GetLastRect()` during `Repaint` is still available as a fallback.

---

## 9. Performance Constraints

### 9.1 Allocation Best Practices
`OnGUI` executes 60+ times per second. While KSP/Unity can tolerate some allocation, minimizing it improves smoothness.

**Best practice:** Cache reusable objects as class members where practical:

```csharp
// Initialization
private GUIContent _tempContent = new GUIContent();
private GUIStyle _cachedLabelStyle;

void InitStyles()
{
    _cachedLabelStyle = new GUIStyle(GUI.skin.label);
    // Configure style...
}

// Usage
_tempContent.text = dynamicString;
_tempContent.tooltip = dynamicTooltip;
GUILayout.Label(_tempContent, _cachedLabelStyle);
```

**Acceptable in practice (but cache if called frequently):**
- `new GUIContent(...)`
- `string.Format(...)`
- `new GUIStyle(...)`

**Strongly recommended to cache:**
- `GUIStyle` instances that are reused every frame
- `Texture2D` assets used as backgrounds
- `GUIContent` objects in tight loops

### 9.2 Hot Path Optimization
For lists with thousands of elements, bypass `GUILayout` entirely:

```csharp
// Fast immediate mode
GUI.Label(rect, content);
GUI.Button(rect, content);
```

Use pre-calculated rectangles, not layout.

---

## 10. Debugging Utilities

### 10.1 Visual Layout Debugger
Temporary visual feedback for control boundaries:

```csharp
// Draw immediately after control
GUILayout.Button("Test");
GUI.Box(GUILayoutUtility.GetLastRect(), GUIContent.none);
```

---

## 11. Common Failure Modes

| Symptom | Cause | Solution |
|---------|-------|----------|
| Tooltip stuck | Wrong coordinate space (GUI vs screen) | Use `Event.current.mousePosition` with rect checks |
| Control overlap | Mismatched Begin/End calls | Audit layout group pairing |
| UI flicker | Code executing during `Layout` pass | Guard with `if (Event.current.type == EventType.Repaint)` |
| ScrollView reset | Scroll position not stored | Assign back to member: `_scrollPos = GUILayout.BeginScrollView(_scrollPos)` |
| Window lost offscreen | Missing clamping | Apply `Mathf.Clamp` to `_windowRect.x/y` |
| Dropdown stays open | No mutual exclusion logic | Close other dropdowns when opening one |
| Slider "loses" final value | No pending-update catch | Check pending flag at start of `Draw()` |

---

## 12. Virtualized Scroll Lists (Large Dataset Optimization)

**Constraint:** IMGUI iterates entire collections during layout regardless of visibility. A 1000-item `foreach` loop creates layout overhead for every element every frame.

**Optimization Threshold:** Virtualization becomes mandatory at >500 visible controls.

### 12.1 Implementation Pattern
Replace `GUILayout` iteration with manual `GUI` positioning based on scroll position:

```csharp
Vector2 _scrollPosition;
const float ROW_HEIGHT = 22f;

void DrawVirtualList(List<string> items, float viewHeight)
{
    // Begin scroll view
    _scrollPosition = GUILayout.BeginScrollView(
        _scrollPosition,
        GUILayout.Height(viewHeight)
    );

    // Reserve total height for the scrollbar
    float totalContentHeight = items.Count * ROW_HEIGHT;
    Rect viewRect = GUILayoutUtility.GetRect(0, totalContentHeight);

    // Calculate visible range
    int firstVisible = Mathf.FloorToInt(_scrollPosition.y / ROW_HEIGHT);
    int visibleCount = Mathf.CeilToInt(viewHeight / ROW_HEIGHT) + 2; // Buffer
    int lastVisible = Mathf.Min(items.Count, firstVisible + visibleCount);

    // Draw only visible items using immediate mode
    for (int i = firstVisible; i < lastVisible; i++)
    {
        Rect rowRect = new Rect(
            viewRect.x,
            viewRect.y + (i * ROW_HEIGHT),
            viewRect.width,
            ROW_HEIGHT
        );

        GUI.Label(rowRect, items[i]);
    }

    GUILayout.EndScrollView();
}
```

### 12.2 Performance Characteristics

| List Size | Controls Drawn | Layout Cost |
|-----------|---------------|-------------|
| 100 | ~12 | Constant |
| 1,000 | ~12 | Constant |
| 10,000 | ~12 | Constant |

**Critical Rule:** Do not use `GUILayout` methods for virtualized items. Use `GUI.Label()`, `GUI.Button()` with explicit `Rect` parameters.

### 12.3 Applications
- Part catalogs
- Mod lists
- Asset browsers
- Large telemetry logs

---

## 13. Grid Layout System

**Constraint:** IMGUI provides no native grid layout. Complex settings panels devolve into excessive vertical nesting.

**Solution:** Manual column tracking with automatic row breaking.

### 13.1 Grid Helper Implementation

```csharp
private int _gridColumns;
private int _gridIndex;

void BeginGrid(int columns)
{
    _gridColumns = columns;
    _gridIndex = 0;
    GUILayout.BeginHorizontal();
}

void GridCell(Action drawContent)
{
    GUILayout.BeginVertical();

    drawContent();

    GUILayout.EndVertical();

    _gridIndex++;

    // Auto-break rows
    if (_gridIndex % _gridColumns == 0)
    {
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
    }
}

void EndGrid()
{
    GUILayout.EndHorizontal();
}
```

### 13.2 Usage Pattern

```csharp
BeginGrid(2);

GridCell(() => DrawSlider("Exposure", ref _exposure, -2f, 8f));
GridCell(() => DrawSlider("Bloom", ref _bloom, 0f, 2f));
GridCell(() => DrawSlider("Threshold", ref _threshold, 0f, 1f));
GridCell(() => DrawSlider("Blur", ref _blur, 0f, 1f));

EndGrid();
```

### 13.3 Layout Benefits
- **Density:** Reduces vertical scroll requirements
- **Scanability:** Paired labels/controls align visually
- **Consistency:** Standard two-column inspector pattern familiar to users

---

## 14. Immediate-Mode Animation

**Constraint:** IMGUI has no retained-state animation system. All motion must be driven procedurally in `OnGUI()` using `Time.deltaTime`.

**Pattern:** Store animation state in `float` member variables; lerp toward targets each frame.

### 14.1 Height Animation (Collapsible Sections)

Anti-pattern: Instant state changes cause UI popping.

Correct implementation:

```csharp
private float _sectionHeightAnim; // 0 = closed, 1 = open

void DrawAnimatedSection(bool isExpanded)
{
    float target = isExpanded ? 1f : 0f;

    // Smooth interpolation
    _sectionHeightAnim = Mathf.Lerp(
        _sectionHeightAnim,
        target,
        Time.deltaTime * 8f
    );

    // Calculate current height
    float currentHeight = Mathf.Lerp(0f, 300f, _sectionHeightAnim);

    // Apply constrained height
    GUILayout.BeginVertical(GUILayout.Height(currentHeight));

    // Only draw content when visible (optimization)
    if (_sectionHeightAnim > 0.01f)
    {
        DrawSectionControls();
    }

    GUILayout.EndVertical();
}
```

### 14.2 Hover State Animation

```csharp
private float _hoverState; // 0 = normal, 1 = highlighted

void DrawAnimatedButton(string label, bool isHovered)
{
    // Update hover state
    float target = isHovered ? 1f : 0f;
    _hoverState = Mathf.Lerp(_hoverState, target, Time.deltaTime * 12f);

    // Apply color interpolation
    Color normalColor = Color.white;
    Color hoverColor = Color.cyan;
    GUI.color = Color.Lerp(normalColor, hoverColor, _hoverState);

    GUILayout.Button(label);

    GUI.color = Color.white; // Restore
}
```

### 14.3 Alpha Fade Transitions

```csharp
// Cross-fade between views
GUI.color = new Color(1f, 1f, 1f, _fadeAlpha);
DrawViewContent();
GUI.color = Color.white;
```

### 14.4 Animation Threshold Guard
Always gate content drawing behind animation completion to avoid layout cost for invisible elements:

```csharp
if (_expandAnim > 0.001f) // Not fully closed
{
    DrawExpensiveContent();
}
```

---

## 15. Architectural Patterns for Large UIs

**Constraint:** Complex KSP mods risk "thousand-line Draw() functions" that become unmaintainable.

**Two additional patterns address this:**

### 15.1 Inspector-Style Layout Framework
Auto-generate labeled controls via reflection or configuration objects:

```csharp
// Instead of manual GUILayout calls:
InspectorField.DrawRange("Exposure", ref settings.exposure, -2f, 8f);
InspectorField.DrawToggle("Enable Bloom", ref settings.bloomEnabled);
```

**Benefits:**
- Consistent labeling alignment across entire mod
- Automatic tooltip binding
- Validation integration
- Reduced code duplication

### 15.2 Stateful UI Models
Separate UI presentation state from settings/persistence state:

```csharp
public class UIStateModel
{
    public Vector2 ScrollPosition;
    public bool ShowAdvanced;
    public float SectionAnimation;
    public int SelectedTab;
    // Transient UI state only
}

public class SettingsModel
{
    public float Exposure;
    public bool EnableEffects;
    // Persisted configuration only
}
```

**Critical Separation:** UI state (scroll positions, animations, foldouts) must never mix with persistence models that get serialized to config files.

---

## 16. Performance Scaling Guide

| UI Complexity | Control Count | Required Optimization |
|--------------|---------------|---------------------|
| Simple | < 50 | Standard GUILayout acceptable |
| Medium | 50-200 | Centralized helper functions, style caching |
| Large | 200-500 | Strict layout discipline, grid systems |
| Massive | 500+ | Virtualization mandatory, hot path GUI.Label |

**GC Pressure Rule:** `OnGUI()` executes at render framerate (60-144 Hz). Cache `GUIContent`, `Rect`, and style objects as class members where practical, especially in frequently-called helpers.
