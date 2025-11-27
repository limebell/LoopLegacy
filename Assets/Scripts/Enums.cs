namespace LoopLegacy
{
    public enum ControlType
    {
        Touch,
        Static,
        Dynamic
    }

    public enum StatType : int
    {
        HP,
        ATK,
        DEF,
        LUC
    }

    public enum BattleStep
    {
        Prepare,
        PrepareComplete,
        Simulating,
        End,
        Result,
        None,
    }

    public enum TutorialStep
    {
        Begin,
        Movement,
        Encounter,
        PlayerStats,
        Equipment,
        End,
    }

    public enum DropType
    {
        Weapon,
        Armor,
        Relic
    }

    public enum EquipmentType
    {
        Weapon,
        Armor,
    }

    public enum MonsterType
    {
        Normal,
        Boss
    }

    public enum MonsterActionType
    {
        None,
        RewardRelic,
        Teleport
    }

    public enum DamageTextType
    {
        Normal,
        Critical,
        Evasion,
        Execution,
        Heal,
    }

    public enum TutorialType
    {
        GameStart,
        Territory,
    }

    public enum NPCType
    {
        GameStart,
        Codex,
        Shop_Maid,
        Shop_Equipment,
        Shop_Upgrade,
        Shop_Relic,
        Bard,
        Worker,
        Library,
    }
}