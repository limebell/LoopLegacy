using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace LoopLegacy.Loader
{
    /// <summary>
    /// 몬스터 데이터를 나타내는 클래스
    /// </summary>
    public class MonsterData
    {
        /// <summary>
        /// 몬스터 코드
        /// </summary>
        public string code;
        
        /// <summary>
        /// 몬스터 타입
        /// </summary>
        public MonsterType type;
        
        /// <summary>
        /// 몬스터 스프라이트
        /// </summary>
        public Sprite sprite;
        
        /// <summary>
        /// 몬스터 레벨
        /// </summary>
        public int level;
        
        /// <summary>
        /// 체력
        /// </summary>
        public int hp;
        
        /// <summary>
        /// 공격력
        /// </summary>
        public int atk;
        
        /// <summary>
        /// 골드 보상
        /// </summary>
        public int gold;
        
        /// <summary>
        /// 경험치 보상
        /// </summary>
        public BigInteger exp;
        
        /// <summary>
        /// BP 보상
        /// </summary>
        public int bp;
        
        /// <summary>
        /// 드롭 아이템 목록
        /// </summary>
        public IEnumerable<DropEntry> drops;
        
        /// <summary>
        /// 몬스터 액션 목록
        /// </summary>
        public IEnumerable<MonsterActionEntry> actions;
    }

    /// <summary>
    /// 드롭 엔트리를 나타내는 클래스
    /// </summary>
    public class DropEntry
    {
        /// <summary>
        /// 아이템 타입
        /// </summary>
        public DropType itemType;
        
        /// <summary>
        /// 아이템 ID
        /// </summary>
        public int itemId;

        public string relicEffectName;

        public int relicLevel;
        
        /// <summary>
        /// 드롭 확률
        /// </summary>
        public float dropRate;
    }

    /// <summary>
    /// 몬스터 액션 엔트리를 나타내는 클래스
    /// </summary>
    public class MonsterActionEntry
    {
        /// <summary>
        /// 액션 타입
        /// </summary>
        public MonsterActionType actionType;
        
        /// <summary>
        /// 액션 값들
        /// </summary>
        public string[] values;
    }
}

