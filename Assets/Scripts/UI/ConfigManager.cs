using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Decel
{
    public class ConfigManager : MonoBehaviour
    {
        [SerializeField] private Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Toggle vsyncToggle;
        [SerializeField] private Dropdown colorBlindDropdown;
        [SerializeField] private Toggle colorBlindDiffrence;
        [SerializeField] private Slider colorBlindSeveritySlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider effectSlider;
        [SerializeField] private Slider voiceSlider;
        private Resolution[] resolutions;
        private Camera mainCamera;
        private ColorBlindness colorBlindness;

        private string path => $"{Application.dataPath}/Data/Config.json";

        private void Awake()
        {
            ResetResolutionDropdown();
            fullscreenToggle.onValueChanged.AddListener(Fullscreen);
            resolutionDropdown.onValueChanged.AddListener(Resolution);
            vsyncToggle.onValueChanged.AddListener(Vsync);
            colorBlindDropdown.onValueChanged.AddListener(ColorBlind);
            colorBlindDiffrence.onValueChanged.AddListener(ColorBlindDiffrence);
            colorBlindSeveritySlider.onValueChanged.AddListener(ColorBlindSeverity);
            voiceSlider.onValueChanged.AddListener(VoicesVolume);
            musicSlider.onValueChanged.AddListener(MusicVolume);
            effectSlider.onValueChanged.AddListener(EffectsVolume);

            if (!FileManager.Exists(path)) { ResetConfig(); }
            LoadConfig();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            colorBlindness = null;
            mainCamera = Camera.main;
            if (mainCamera == null) { return; }
            mainCamera.TryGetComponent(out colorBlindness);

            if (colorBlindness != null)
            {
                ConfigData configData = FileManager.Load<ConfigData>(path);
                ColorBlind(configData.colorBlind);
                ColorBlindDiffrence(configData.colorBlindDiffrence);
                ColorBlindSeverity(configData.colorBlindSeverity);
            }
        }

        private void OnApplicationQuit()
        {
            SaveConfig();
        }

        private void ResetResolutionDropdown()
        {
            resolutions = Screen.resolutions;
            var options = new List<Dropdown.OptionData>();
            int selected = 0;
            foreach (var res in resolutions)
            {
                string text = res.width + "x" + res.height/* + " @" + res.refreshRate*/;
                options.Add(new Dropdown.OptionData(text));
            }
            resolutionDropdown.options = options;
            resolutionDropdown.value = selected;
        }

        public void Resolution(int value)
        {
            Screen.SetResolution(resolutions[value].width, resolutions[value].height, Screen.fullScreen);
        }

        public void FrameRate(int value)
        {
            Application.targetFrameRate = value;
        }

        public void Fullscreen(bool value)
        {
            Screen.fullScreen = value;
        }

        public void ColorBlind(int value)
        {
            if (colorBlindness == null) { return; }
            if (value == 0) { colorBlindness.enabled = false; }
            else
            {
                colorBlindness.enabled = true;
                colorBlindness.blindType = (ColorBlindness.BlindTypes)(value - 1);
            }
        }

        public void ColorBlindDiffrence(bool value)
        {
            if (colorBlindness == null) { return; }
            colorBlindness.difference = value;
        }

        public void ColorBlindSeverity(float value)
        {
            if (colorBlindness == null) { return; }
            colorBlindness.severity = value;
        }

        public void Vsync(bool value)
        {
            if (value) { QualitySettings.vSyncCount = 1; }
            else { QualitySettings.vSyncCount = 0; }
        }

        public void VoicesVolume(float volume)
        {
            //audioMixer.SetFloat("VoiceVolume", Mathf.Log10(volume) * 10);
        }

        public void MusicVolume(float volume)
        {
            //audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 10);
        }

        public void EffectsVolume(float volume)
        {
            //audioMixer.SetFloat("EffectsVolume", Mathf.Log10(volume) * 10);
        }

        public void ResetConfig()
        {
            int resolutionIndex = 0;
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (Screen.currentResolution.ToString() == resolutions[i].ToString())
                {
                    resolutionIndex = i; break;
                }
            }

            ConfigData configData;
            configData.resolution = resolutionIndex;
            configData.framerate = 6000;
            configData.fullscreen = true;
            configData.vsync = true;
            configData.colorBlind = 0;
            configData.colorBlindDiffrence = false;
            configData.colorBlindSeverity = 0.5f;
            configData.musicVolume = 10;
            configData.effectVolume = 10;
            configData.voiceVolume = 10;
            FileManager.Save(path, configData);
        }

        private void LoadConfig()
        {
            ConfigData configData = FileManager.Load<ConfigData>(path);

            resolutionDropdown.value = configData.resolution;
            Resolution(configData.resolution);

            fullscreenToggle.isOn = configData.fullscreen;
            Fullscreen(configData.fullscreen);

            vsyncToggle.isOn = configData.vsync;
            Vsync(configData.vsync);

            colorBlindDropdown.value = configData.colorBlind;
            ColorBlind(configData.colorBlind);

            colorBlindDiffrence.isOn = configData.colorBlindDiffrence;
            ColorBlindDiffrence(configData.colorBlindDiffrence);

            colorBlindSeveritySlider.value = configData.colorBlindSeverity;
            ColorBlindSeverity(configData.colorBlindSeverity);

            FrameRate(configData.framerate);

            musicSlider.value = configData.musicVolume;
            MusicVolume(configData.musicVolume);

            effectSlider.value = configData.effectVolume;
            EffectsVolume(configData.effectVolume);

            voiceSlider.value = configData.voiceVolume;
            VoicesVolume(configData.voiceVolume);
        }

        public void SaveConfig()
        {
            ConfigData configData;
            configData.resolution = resolutionDropdown.value;
            configData.framerate = 6000;
            configData.fullscreen = fullscreenToggle.isOn;
            configData.vsync = vsyncToggle.isOn;
            configData.colorBlind = colorBlindDropdown.value;
            configData.colorBlindDiffrence = colorBlindDiffrence.isOn;
            configData.colorBlindSeverity = colorBlindSeveritySlider.value;
            configData.musicVolume = musicSlider.value;
            configData.effectVolume = effectSlider.value;
            configData.voiceVolume = voiceSlider.value;
            FileManager.Save(path, configData);
        }

        private struct ConfigData
        {
            public int resolution;
            public int framerate;
            public bool fullscreen;
            public bool vsync;
            public int colorBlind;
            public bool colorBlindDiffrence;
            public float colorBlindSeverity;
            public float musicVolume;
            public float effectVolume;
            public float voiceVolume;
        }
    }
}