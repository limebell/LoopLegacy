using LoopLegacy.Battle.RelicEffects;
using LoopLegacy.Loader;
using LoopLegacy.Manager;
using UnityEngine;

namespace LoopLegacy.Battle
{
    public class Weapon
    {
        public int Id { get; private set; }
        public Sprite Sprite { get; private set; }
        public int EnchantmentLevel { get; private set; }
        private int BaseDamage;
        private int DamageMultiplier;
        private float BaseDamagePerLv;
        private float DamageMultiplierPerLv;

        public Weapon(EquipmentData equipment, int count)
        {
            Id = equipment.id;
            Sprite = equipment.sprite;
            EnchantmentLevel = count;
            BaseDamage = equipment.baseValue;
            DamageMultiplier = equipment.multiplier;
            BaseDamagePerLv = equipment.basePerLv;
            DamageMultiplierPerLv = equipment.multPerLv;
        }

        public float GetBaseDamage()
        {
            if (GameManager.Instance == null)
            {
                return BaseDamage;
            }

            float baseDamageSpecMultiplier = 1.0f;
            if (GameManager.Instance.TryGetRelic<EquipmentBaseSpecBoostEffect>(out Relic relic))
            {
                baseDamageSpecMultiplier = (relic.Effect as EquipmentBaseSpecBoostEffect).GetEquipmentBaseSpecBoostPercentage() / 100f;
            }

            return BaseDamage * baseDamageSpecMultiplier;
        }

        public float GetEnchantedBaseDamage()
        {
            var enchantmentLevel = EnchantmentLevel < 10 ? EnchantmentLevel - 1 : 10;
            if (GameManager.Instance == null)
            {
                return BaseDamagePerLv * enchantmentLevel;
            }

            float baseDamageSpecMultiplier = 1.0f;
            if (GameManager.Instance.TryGetRelic<EquipmentBaseSpecBoostEffect>(out Relic relic))
            {
                baseDamageSpecMultiplier = (relic.Effect as EquipmentBaseSpecBoostEffect).GetEquipmentBaseSpecBoostPercentage() / 100f;
            }
            return BaseDamagePerLv * enchantmentLevel * baseDamageSpecMultiplier;
        }

        public float GetCalculatedBaseDamage()
        {
            if (GameManager.Instance == null)
            {
                return BaseDamage + GetEnchantedBaseDamage();
            }

            return GetBaseDamage() + GetEnchantedBaseDamage();
        }

        public float GetDamageMultiplier()
        {
            return DamageMultiplier;
        }

        public float GetEnchantedDamageMultiplier()
        {
            var enchantmentLevel = EnchantmentLevel < 10 ? EnchantmentLevel - 1 : 10;
            return DamageMultiplierPerLv * enchantmentLevel;
        }

        public float GetCalculatedDamageMultiplier()
        {
            return DamageMultiplier + GetEnchantedDamageMultiplier();
        }

        public float GetFinalDamage(int stat)
        {
            return GetCalculatedBaseDamage() + (GetCalculatedDamageMultiplier() * 0.01f * stat);
        }
    }
} 