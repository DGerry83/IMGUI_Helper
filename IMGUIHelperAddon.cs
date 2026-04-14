using System;
using IMGUI_Helper.UI;
using KSP.UI.Screens;
using UnityEngine;

namespace IMGUI_Helper
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class IMGUIHelperAddon : MonoBehaviour
    {
        private static IMGUIHelperAddon _instance;
        private ApplicationLauncherButton _toolbarButton;
        private IMGUIHelperWindow _window;
        private Texture2D _buttonTexture;

        public static IMGUIHelperAddon Instance => _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _window = new IMGUIHelperWindow();
            _buttonTexture = CreateOrangeTexture();

            GameEvents.onGUIApplicationLauncherReady.Add(OnAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnAppLauncherDestroyed);
        }

        private void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnAppLauncherDestroyed);

            if (_toolbarButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(_toolbarButton);
                _toolbarButton = null;
            }

            if (_instance == this)
                _instance = null;
        }

        private void OnGUI()
        {
            _window?.OnGUI();
        }

        private void OnAppLauncherReady()
        {
            if (_toolbarButton != null)
                return;

            _toolbarButton = ApplicationLauncher.Instance.AddModApplication(
                onTrue: OnToolbarButtonToggle,
                onFalse: OnToolbarButtonToggle,
                onHover: null,
                onHoverOut: null,
                onEnable: null,
                onDisable: null,
                visibleInScenes: ApplicationLauncher.AppScenes.ALWAYS,
                texture: _buttonTexture
            );
        }

        private void OnAppLauncherDestroyed()
        {
            if (_toolbarButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(_toolbarButton);
                _toolbarButton = null;
            }
        }

        private void OnToolbarButtonToggle()
        {
            _window?.Toggle();
        }

        private Texture2D CreateOrangeTexture()
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, new Color(1f, 0.5f, 0f, 1f));
            tex.Apply();
            return tex;
        }
    }
}
