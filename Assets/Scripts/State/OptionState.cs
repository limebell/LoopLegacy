using R3;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace LoopLegacy.State
{
    public class OptionState
    {
        public static OptionState Instance
        {
            get {
                if (_instance == null)
                {
                    if (File.Exists(Application.persistentDataPath + "/option.json"))
                    {
                        var json = File.ReadAllText(Application.persistentDataPath + "/option.json");
                        var data = JsonUtility.FromJson<OptionData>(json);
                        _instance = new OptionState(data);
                    }
                    else
                    {
                        _instance = new OptionState();
                        _instance.SaveData();
                    }
                }

                return _instance;
            }
        }

        private static OptionState _instance;

        public ReactiveProperty<string> Language { get; private set; }
        public ReactiveProperty<int> CombatSpeed { get; private set; }
        public ReactiveProperty<ControlType> Control { get; private set; }
        public ReactiveProperty<int> MasterVolume { get; private set; }
        public ReactiveProperty<int> BgmVolume { get; private set; }
        public ReactiveProperty<int> SfxVolume { get; private set; }
        public ReactiveProperty<int> FrameRate { get; private set; }
        public ReactiveProperty<int> Quality { get; private set; }

        private OptionState()
        {
            Language = new ReactiveProperty<string>("");
            CombatSpeed = new ReactiveProperty<int>(0);
            Control = new ReactiveProperty<ControlType>(ControlType.Touch);
            MasterVolume = new ReactiveProperty<int>(0);
            BgmVolume = new ReactiveProperty<int>(0);
            SfxVolume = new ReactiveProperty<int>(0);
            FrameRate = new ReactiveProperty<int>(0);
            Quality = new ReactiveProperty<int>(0);

            Reset();
            SaveData();
        }

        private OptionState(OptionData data)
        {
            Language = new ReactiveProperty<string>(data.language);
            CombatSpeed = new ReactiveProperty<int>(data.combatSpeed);
            Control = new ReactiveProperty<ControlType>(data.control);
            MasterVolume = new ReactiveProperty<int>(data.masterVolume);
            BgmVolume = new ReactiveProperty<int>(data.bgmVolume);
            SfxVolume = new ReactiveProperty<int>(data.sfxVolume);
            FrameRate = new ReactiveProperty<int>(data.frameRate);
            Quality = new ReactiveProperty<int>(data.quality);
        }

        public void Reset()
        {
            Language.Value = LocalizationSettings.SelectedLocale.Identifier.Code;
            CombatSpeed.Value = 0;
            Control.Value = ControlType.Touch;
            MasterVolume.Value = 10;
            BgmVolume.Value = 10;
            SfxVolume.Value = 10;
            FrameRate.Value = 2;
            Quality.Value = 2;
        }

        public void SaveData()
        {
            var json = JsonUtility.ToJson(new OptionData
            {
                language = Language.Value,
                combatSpeed = CombatSpeed.Value,
                control = Control.Value,
                masterVolume = MasterVolume.Value,
                bgmVolume = BgmVolume.Value,
                sfxVolume = SfxVolume.Value,
                frameRate = FrameRate.Value,
                quality = Quality.Value,
            });
            File.WriteAllText(Application.persistentDataPath + "/option.json", json);
        }
    }

    [Serializable]
    internal class OptionData
    {
        public string language;
        public int combatSpeed;
        public ControlType control;
        public int masterVolume;
        public int bgmVolume;
        public int sfxVolume;
        public int frameRate;
        public int quality;
    }
}