using System.Linq;
using LoopLegacy.State;

namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 무기 비례 스탯 효과 (최대로 수집한 무기의 개수에 비례해 각 스탯이 증가)
    /// </summary>
    public class StatBoostCollectEffect : RelicEffect
    {
        private readonly EquipmentType equipmentType;
        private readonly int statIncreasePerEquipment;

        public StatBoostCollectEffect(string effectType, EquipmentType equipmentType, int statIncreasePerEquipment) : base(effectType)
        {
            this.equipmentType = equipmentType;
            this.statIncreasePerEquipment = statIncreasePerEquipment;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {            
            int count = PersistentGameState.Instance.InventoryState.GetOwnedEquipments(equipmentType)
                .Count(c => c == InventoryState.MAX_EQUIPMENT_DUPLICATE_COUNT);
            int amount = count * statIncreasePerEquipment;
            context.HPBoost += amount;
            context.ATKBoost += amount;
            context.DEFBoost += amount;
            context.LUCBoost += amount;
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { statIncreasePerEquipment });

        public EquipmentType GetEquipmentType() => equipmentType;
        public int GetStatIncreasePerEquipment() => statIncreasePerEquipment;
    }
}
