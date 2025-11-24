using System.Linq;
using LoopLegacy.Loader;
using LoopLegacy.State;

namespace LoopLegacy.Manager
{
    public class LibraryManager
    {
        public static bool IsLibraryUnlocked()
        {
            return PersistentGameState.Instance.HouseState.GetUpgradeLevel(UpgradeType.LibraryManagement) > 0;
        }

        public static int GetAccumulatedLevelBonus(int accumulatedLevel)
        {
            var bonus = 0;
            if (accumulatedLevel >= 200_000_000)
            {
                bonus = 20;
            }
            else if (accumulatedLevel >= 150_000_000)
            {
                bonus = 19;
            }
            else if (accumulatedLevel >= 120_000_000)
            {
                bonus = 18;
            }
            else if (accumulatedLevel >= 100_000_000)
            {
                bonus = 17;
            }
            else if (accumulatedLevel >= 85_000_000)
            {
                bonus = 16;
            }
            else if (accumulatedLevel >= 65_000_000)
            {
                bonus = 15;
            }
            else if (accumulatedLevel >= 50_000_000)
            {
                bonus = 14;
            }
            else if (accumulatedLevel >= 40_000_000)
            {
                bonus = 13;
            }
            else if (accumulatedLevel >= 32_000_000)
            {
                bonus = 12;
            }
            else if (accumulatedLevel >= 25_000_000)
            {
                bonus = 11;
            }
            else if (accumulatedLevel >= 19_000_000)
            {
                bonus = 10;
            }
            else if (accumulatedLevel >= 14_000_000)
            {
                bonus = 9;
            }
            else if (accumulatedLevel >= 10_000_000)
            {
                bonus = 8;
            }
            else if (accumulatedLevel >= 7_000_000)
            {
                bonus = 7;
            }
            else if (accumulatedLevel >= 4_000_000)
            {
                bonus = 6;
            }
            else if (accumulatedLevel >= 2_000_000)
            {
                bonus = 5;
            }
            else if (accumulatedLevel >= 1_000_000)
            {
                bonus = 4;
            }
            else if (accumulatedLevel >= 500_000)
            {
                bonus = 3;
            }
            else if (accumulatedLevel >= 100_000)
            {
                bonus = 2;
            }
            else if (accumulatedLevel >= 50_000)
            {
                bonus = 1;
            }
            else bonus = 0;

            return bonus;
        }

        public static int GetStatBoost()
        {
            if (!IsLibraryUnlocked())
            {
                return 0;
            }

            int multiplier = PersistentGameState.Instance.HouseState.GetUpgradeValue(UpgradeType.LibraryManagement);
            int weaponCount = FullyCollectionCount(EquipmentType.Weapon);
            int armorCount = FullyCollectionCount(EquipmentType.Armor);
            return multiplier * (weaponCount + armorCount);
        }

        public static float GetStatMultiplier(int accumulatedLevel)
        {
            if (!IsLibraryUnlocked())
            {
                return 1f;
            }

            int bonus = GetAccumulatedLevelBonus(accumulatedLevel);
            return 1f + (bonus / 100f);
        }

        public static int FullyCollectionCount(EquipmentType equipmentType)
        {
            return PersistentGameState.Instance.InventoryState.GetOwnedEquipments(equipmentType).Count(e => e == InventoryState.MAX_EQUIPMENT_DUPLICATE_COUNT);
        }

        public static float GetMonsterDropMultiplier(int killCount)
        {
            if (!IsLibraryUnlocked())
            {
                return 1f;
            }

            if (killCount >= 200)
            {
                return 2f;
            }
            else if (killCount >= 100)
            {
                return 1.5f;
            }
            else if (killCount >= 50)
            {
                return 1.2f;
            }
            
            return 1f;
        }
    }
}