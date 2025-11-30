using System.Collections.Generic;
using UnityEngine;

namespace LoopLegacy.Loader
{
    /// <summary>
    /// 유물 데이터를 나타내는 클래스
    /// </summary>
    public class RelicData
    {
        /// <summary>
        /// 유물 스프라이트
        /// </summary>
        public Sprite sprite;
        
        /// <summary>
        /// 유물 등급
        /// </summary>
        public RelicGrade grade;
        
        /// <summary>
        /// 효과 타입
        /// </summary>
        public string effectName;
        
        /// <summary>
        /// 레벨별 효과 값들
        /// </summary>
        public string[] values;

        /// <summary>
        /// 레벨별 가격들
        /// </summary>
        public int[] prices;
    }

    /// <summary>
    /// 유물 등급 열거형
    /// </summary>
    public enum RelicGrade
    {
        Common,
        Uncommon,
        Rare,
        Epic,
    }
}

