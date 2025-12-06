using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoopLegacy.Battle;
using LoopLegacy.Battle.RelicEffects;
using LoopLegacy.Loader;
using LoopLegacy.State;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LoopLegacy
{
    public static class Utils
    {
        public static string Ordinal(int number)
        {
            if (LocalizationSettings.SelectedLocale.Identifier.Code != "en")
                return number.ToString();

            if (number >= 11 && number <= 13)
                return number +"th";
                
            switch (number % 10)
            {
                case 1: return number + "st";
                case 2: return number + "nd";
                case 3: return number + "rd";
                default: return number + "th";
            }
        }

        public static Color HexToColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }

        public static string ColorToHex(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        // Weighted Random Sampling (Efraimidis–Spirakis)
        public static List<Relic> PickRelics(
            IEnumerable<Relic> relics,
            IEnumerable<Relic> excludeRelics,
            IEnumerable<Relic> alreadyRolledRelics,
            IReadOnlyDictionary<RelicGrade, float> rarityWeights,
            int count = 3)
        {
            var candidates = relics
                .Where(r => !excludeRelics.Any(e => e.EffectName == r.EffectName))
                .Where(r => rarityWeights.TryGetValue(r.Grade, out var w) && w > 0f)
                .ToList();

            if (candidates.Count == 0) return new List<Relic>();
            if (candidates.Count <= count) return candidates;

            // 각 후보에 키를 부여
            var keyed = new List<(float key, Relic relic)>(candidates.Count);
            foreach (var c in candidates)
            {
                float w = rarityWeights[c.Grade] * (alreadyRolledRelics.Any(r => r.EffectName == c.EffectName) ? 0.1f : 1f); // 한 번 뽑았던 유물은 확률 1/10으로 감소
                float u = Mathf.Clamp01(Random.value);
                if (u <= 0f) u = Mathf.Epsilon; // 로그 보호
                float key = -Mathf.Log(u) / w;  // 작을수록 선택 우선
                keyed.Add((key, c));
            }

            // 키가 작은 상위 N개 선택
            keyed.Sort((a, b) => a.key.CompareTo(b.key));
            return keyed.Take(count).Select(x => x.relic).ToList();
        }

        public static RegionEffectType PickRegionEffectType(
            IReadOnlyDictionary<RegionEffectType, float> regionEffectWeights
        )
        {
            float totalWeight = regionEffectWeights.Values.Sum();
            if (totalWeight == 0)
                return RegionEffectType.None;
            float randomValue = UnityEngine.Random.value * totalWeight;
            foreach (var entry in regionEffectWeights)
            {
                randomValue -= entry.Value;
                if (randomValue <= 0)
                    return entry.Key;
            }

            return regionEffectWeights.Keys.Last();
        }

        public static string GetStatName(StatType statType)
        {
            return statType switch
            {
                StatType.HP => "HP",
                StatType.ATK => "ATK",
                StatType.DEF => "DEF",
                StatType.LUC => "LUC",
                _ => "Error",
            };
        }

        public static string GetRelicGradeColorHex(RelicGrade grade)
        {
            return grade switch
            {
                RelicGrade.Common => "#FFFFFF",
                RelicGrade.Uncommon => "#41D229",
                RelicGrade.Rare => "#45b0f7",
                RelicGrade.Epic => "#ac2dd6",
                _ => "#9D9D9D",
            };
        }

        public static Color GetRelicGradeColor(RelicGrade grade)
        {
            return HexToColor(GetRelicGradeColorHex(grade));
        }

        public static string GetUIString(string entry, object[] args = null)
        {
            var localizedString = new LocalizedString {
                TableReference = "UI",
                TableEntryReference = entry };

            return args is null
                ? localizedString.GetLocalizedString()
                : localizedString.GetLocalizedString(args);
        }

        public static string GetRegionName(string regionCode)
        {
            return new LocalizedString {
                TableReference = "Region",
                TableEntryReference = regionCode }.GetLocalizedString();
        }

        public static string GetMonsterName(MonsterData monster)
        {
            return new LocalizedString {
                TableReference = "Monster",
                TableEntryReference = monster.code }.GetLocalizedString();
        }

        public static string GetEquipmentName(EquipmentType type, int id)
        {
            return new LocalizedString {
                TableReference = type == EquipmentType.Weapon ? "Weapon" : "Armor",
                TableEntryReference = id.ToString() }.GetLocalizedString();
        }

        public static string GetEquipmentDescription(EquipmentType type, int id)
        {
            return new LocalizedString {
                TableReference = type == EquipmentType.Weapon ? "Weapon" : "Armor",
                TableEntryReference = $"{id}-description" }.GetLocalizedString();
        }

        public static string GetUpgradeName(UpgradeType type)
        {
            return new LocalizedString {
                TableReference = "Upgrade",
                TableEntryReference = type.ToString() }.GetLocalizedString();
        }

        public static string GetUpgradeDescription(UpgradeType type, object[] args = null)
        {
            return new LocalizedString {
                TableReference = "Upgrade",
                TableEntryReference = $"{type}-description",
                Arguments = args
            }.GetLocalizedString();
        }

        public static string GetUpgradeDescription(UpgradeType type, int level, object[] args = null)
        {
            return new LocalizedString {
                TableReference = "Upgrade",
                TableEntryReference = $"{type}-description-{level}",
                Arguments = args
            }.GetLocalizedString();
        }

        public static string GetShortcutName(Shortcut shortcut)
        {
            return new LocalizedString {
                TableReference = "Shortcut",
                TableEntryReference = shortcut.ToString() }.GetLocalizedString();
        }
        public static string GetShortcutDescription(Shortcut shortcut)
        {
            return new LocalizedString {
                TableReference = "Shortcut",
                TableEntryReference = $"{shortcut}-description" }.GetLocalizedString();
        }

        public static string GetRelicName(string effectType)
        {
            return new LocalizedString {
                TableReference = "Relic",
                TableEntryReference = effectType }.GetLocalizedString();
        }

        public static string GetRelicDescription(string effectName, object[] args = null) =>
            new LocalizedString
            {
                TableReference = "Relic",
                TableEntryReference = $"{effectName}-description",
                Arguments = args
            }.GetLocalizedString();

        public static string GetNPCCode(NPCType npcType)
        {
            return npcType switch
            {
                NPCType.GameStart => "game_start",
                NPCType.Codex => "codex",
                NPCType.Shop_Maid => "shop_maid",
                NPCType.Shop_Equipment => "shop_equipment",
                NPCType.Shop_Upgrade => "shop_upgrade",
                NPCType.Shop_Relic => "shop_relic",
                NPCType.Bard => "bard",
                NPCType.Worker => "worker",
                NPCType.Library => "library",
                _ => "",
            };
        }

        public static string GetNPCName(string npcCode)
        {
            return new LocalizedString {
                TableReference = "NPC",
                TableEntryReference = $"{npcCode}-name" }.GetLocalizedString();
        }

        public static string GetNPCName(NPCType npcType)
        {
            string npcCode = GetNPCCode(npcType);
            return GetNPCName(npcCode);
        }

        public static string GetNPCDescription(NPCType npcType)
        {
            string npcCode = GetNPCCode(npcType);
            return new LocalizedString {
                TableReference = "NPC",
                TableEntryReference = $"{npcCode}-description" }.GetLocalizedString();
        }

        public static string GetCutsceneText(string entry)
        {
            return new LocalizedString {
                TableReference = "Cutscene",
                TableEntryReference = entry }.GetLocalizedString();
        }

        public static string GetRomanNumber(int number)
        {
            return number switch
            {
                1 => "Ⅰ",
                2 => "Ⅱ",
                3 => "Ⅲ",
                4 => "Ⅳ",
                5 => "Ⅴ",
                6 => "Ⅵ",
                7 => "Ⅶ",
                8 => "Ⅷ",
                9 => "Ⅸ",
                10 => "Ⅹ",
                _ => number.ToString(),
            };
        }

        public static Sprite LoadFirstSpriteAsync(object key)
        {
            // 1) 싱글 스프라이트 시도
            {
                AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(key);
                Sprite s = null;
                try
                {
                    s = handle.WaitForCompletion();
                    if (s != null)
                        return s;
                }
                catch
                {
                }

                if (s == null)
                {
                    Addressables.Release(handle);
                }
            }

            // 2) 여러 스프라이트(멀티/라벨 등) 시도
            {
                AsyncOperationHandle<IList<Sprite>> handle =
                    Addressables.LoadAssetsAsync<Sprite>(key, null); // ← 여기서만 두 번째 인자 사용
                Sprite s = null;

                try
                {
                    IList<Sprite> list = handle.WaitForCompletion();
                    if (list != null && list.Count > 0)
                        return list[0]; // 첫 번째 스프라이트
                }
                catch
                {
                }

                if (s == null)
                {
                    Addressables.Release(handle);
                }
            }

            return null;
        }

        public static string GetRegionEffectText(RegionEffectType regionEffectType)
        {
            return regionEffectType switch
            {
                RegionEffectType.BoostExpSmall => $"Exp x1.5",
                RegionEffectType.BoostExpLarge => $"Exp x2.0",
                RegionEffectType.BoostGoldSmall => $"Gold x1.5",
                RegionEffectType.BoostGoldLarge => $"Gold x2.0",
                RegionEffectType.ReduceEnemyHPSmall => $"Enemy HP x0.9",
                RegionEffectType.ReduceEnemyHPLarge => $"Enemy HP x0.7",
                RegionEffectType.ReduceEnemyATKSmall => $"Enemy ATK x0.9",
                RegionEffectType.ReduceEnemyATKLarge => $"Enemy ATK x0.7",
                RegionEffectType.SpecialA => $"Special A",
                RegionEffectType.SpecialB => $"Special B",
                RegionEffectType.SpecialC => $"Special C",
                _ => "",
            };
        }
    }
}