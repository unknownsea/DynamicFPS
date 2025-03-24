using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace DynamicFPS
{
    [BepInPlugin("sea.DynamicFPS", "DynamicFPS", "1.0")]
    public class DynamicFPS : BaseUnityPlugin
    {
        internal static DynamicFPS Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger => Instance._logger;
        private ManualLogSource _logger => base.Logger;
        internal Harmony? Harmony { get; set; }

        private ConfigEntry<int>? _unfocusedFPS;
        private ConfigEntry<float>? _unfocusedVolume;
        private int _defaultFPS;
        private float _defaultVolume;

        private void Awake()
        {
            Instance = this;

            gameObject.transform.parent = null;
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            _unfocusedFPS = Config.Bind("Settings", "UnfocusedFPS", 30, new ConfigDescription("FPS when unfocused", new AcceptableValueRange<int>(30, 240)));
            _unfocusedVolume = Config.Bind("Settings", "UnfocusedVolume", 0f, new ConfigDescription("Volume when unfocused", new AcceptableValueRange<float>(0f, 1f)));

            Patch();

            Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
        }

        internal void Patch()
        {
            Harmony ??= new Harmony(Info.Metadata.GUID);
            Harmony.PatchAll();
        }

        internal void Unpatch()
        {
            Harmony?.UnpatchSelf();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                _defaultFPS = Application.targetFrameRate;
                _defaultVolume = AudioListener.volume;

                if (_unfocusedVolume != null)
                {
                    AudioListener.volume = _unfocusedVolume.Value;
                }
                if (_unfocusedFPS != null)
                {
                    Application.targetFrameRate = _unfocusedFPS.Value;
                }
            }
            else
            {
                if (_defaultVolume != 0f)
                {
                    AudioListener.volume = _defaultVolume;
                }
                else
                {
                    Logger.LogWarning("Default volume is 0. Volume might not have been restored correctly.");
                }

                Application.targetFrameRate = _defaultFPS;
            }
        }
    }
}