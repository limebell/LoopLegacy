using LoopLegacy.Battle.RelicEffects;
using LoopLegacy.Loader;
using LoopLegacy.Manager;
using UnityEngine;

namespace LoopLegacy.Battle
{
    public class Armor
    {
        public int Id { get; private set; }
        public Sprite Sprite { get; private set; }
        public int EnchantmentLevel { get; private set; }
        private int BaseDefense;
        private int DefenseMultiplier;
        private float BaseDefensePerLv;
        private float DefenseMultiplierPerLv;

        public Armor(EquipmentData equipment, int count)
        {
            Id = equipment.id;
            Sprite = equipment.sprite;
            EnchantmentLevel = count;
            BaseDefense = equipment.baseValue;
            DefenseMultiplier = equipment.multiplier;
            BaseDefensePerLv = equipment.basePerLv;
            DefenseMultiplierPerLv = equipment.multPerLv;
        }

        public float GetBaseDefense()
        {
            if (GameManager.Instance == null)
            {
                return BaseDefense;
            }

            float baseDefenseSpecMultiplier = 1.0f;
            if (GameManager.Instance.TryGetRelic<EquipmentBaseSpecBoostEffect>(out Relic relic))
            {
                baseDefenseSpecMultiplier = (relic.Effect as EquipmentBaseSpecBoostEffect).GetEquipmentBaseSpecBoostPercentage() / 100f;
            }

            return BaseDefense * baseDefenseSpecMultiplier;
        }

        public float GetEnchantedBaseDefense()
        {
            var enchantmentLevel = EnchantmentLevel < 10 ? EnchantmentLevel - 1 : 10;
            if (GameManager.Instance == null)
            {
                return BaseDefensePerLv * enchantmentLevel;
            }

            float baseDefenseSpecMultiplier = 1.0f;
            if (GameManager.Instance.TryGetRelic<EquipmentBaseSpecBoostEffect>(out Relic relic))
            {
                baseDefenseSpecMultiplier = (relic.Effect as EquipmentBaseSpecBoostEffect).GetEquipmentBaseSpecBoostPercentage() / 100f;
            }
            return BaseDefensePerLv * enchantmentLevel * baseDefenseSpecMultiplier;
        }

        public float GetCalculatedBaseDefense()
        {
            return GetBaseDefense() + GetEnchantedBaseDefense();
        }

        public float GetDefenseMultiplier()
        {
            return DefenseMultiplier;
        }

        public float GetEnchantedDefenseMultiplier()
        {
            var enchantmentLevel = EnchantmentLevel < 10 ? EnchantmentLevel - 1 : 10;
            return DefenseMultiplierPerLv * enchantmentLevel;
        }

        public float GetCalculatedDefenseMultiplier()
        {
            return DefenseMultiplier + GetEnchantedDefenseMultiplier();
        }

        public float GetFinalDefense(int stat)
        {
            return GetCalculatedBaseDefense() + (GetCalculatedDefenseMultiplier() * 0.01f * stat);
        }
    }
} 