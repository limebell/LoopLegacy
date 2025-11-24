using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LoopLegacy.Loader
{
    public static class CutsceneLoader
    {
        public static List<CutsceneDialogue> Load(string cutsceneName)
        {
            var list = new List<CutsceneDialogue>();
            var handle = Addressables.LoadAssetAsync<TextAsset>("Data/Cutscenes/" + cutsceneName);
            TextAsset csv = handle.WaitForCompletion();
            if (csv == null)
            {
                throw new Exception("Data/Cutscenes/" + cutsceneName + "을 로드할 수 없습니다.");
            }

            var lines = csv.text.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var tokens = line.Split(',');

                Sprite sprite = null;
                if (tokens[0] != "")
                {
                    try
                    {
                        sprite = Addressables.LoadAssetAsync<Sprite>(
                            $"Images/Cutscenes/{tokens[0]}").WaitForCompletion();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to load sprite for {tokens[0]}: {e.Message}");
                    }
                }

                var data = new CutsceneDialogue
                {
                    text = tokens[1],
                    image = sprite,
                    name = tokens[2],
                    pause = tokens[3] == "pause"
                };

                list.Add(data);
            }

            return list;
        }
    }

    [System.Serializable]
    public class CutsceneDialogue
    {
        public string text;
        public Sprite image;
        public string name;
        public bool pause;
    }
}