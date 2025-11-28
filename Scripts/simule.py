#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
LoopLegacy 몬스터 스탯 계산기
레벨에 따른 몬스터 스탯을 계산하고 시뮬레이션하는 프로그램
"""

import math
import random
import csv
import argparse
from typing import Dict, List, Tuple, Optional
from dataclasses import dataclass
from enum import IntEnum


# 플레이어 레벨업 시스템 상수
PLAYER_LEVEL_EXPONENT = 1.4


def calculate_required_exp_for_level(level: int) -> int:
    """특정 레벨에서 다음 레벨로 가는데 필요한 경험치 계산"""
    return int(level ** PLAYER_LEVEL_EXPONENT)


def calculate_total_exp_for_levelups(start_level: int, level_ups: int) -> int:
    """start_level에서 level_ups번 레벨업하는데 필요한 총 경험치 계산"""
    total = 0
    for i in range(start_level, start_level + level_ups):
        total += calculate_required_exp_for_level(i)
    return total


def calculate_levelups_from_exp(start_level: int, exp_amount: int) -> Tuple[int, int]:
    """start_level에서 exp_amount 경험치로 몇 번 레벨업하는지 계산
    
    Returns:
        (최종_레벨, 남은_경험치) 튜플
    """
    current_level = start_level
    remaining_exp = exp_amount
    
    while remaining_exp > 0:
        needed = calculate_required_exp_for_level(current_level)
        if remaining_exp >= needed:
            remaining_exp -= needed
            current_level += 1
        else:
            break
    
    return current_level, remaining_exp


class StatType(IntEnum):
    """스탯 타입 열거형"""
    HP = 0
    ATK = 1
    DEF = 2
    LUC = 3


@dataclass
class MonsterStats:
    """몬스터 스탯 데이터 클래스"""
    level: int
    hp: int
    atk: int
    gold: int
    exp: int
    bp: int = -1
    
    def __str__(self):
        return f"Lv.{self.level} - HP:{self.hp}, ATK:{self.atk}, Gold:{self.gold}, EXP:{self.exp}"


class Weapon:
    """무기 클래스"""
    def __init__(self, base_damage: int, damage_multiplier: int):
        self.base_damage = base_damage
        self.damage_multiplier = damage_multiplier
    
    def get_calculated_base_damage(self) -> int:
        """계산된 기본 대미지 반환"""
        return self.base_damage
    
    def get_calculated_damage_multiplier(self) -> int:
        """계산된 대미지 배율 반환"""
        return self.damage_multiplier


class Armor:
    """방어구 클래스"""
    def __init__(self, base_defense: int, defense_multiplier: int):
        self.base_defense = base_defense
        self.defense_multiplier = defense_multiplier
    
    def get_calculated_base_defense(self) -> int:
        """계산된 기본 방어력 반환"""
        return self.base_defense
    
    def get_calculated_defense_multiplier(self) -> int:
        """계산된 방어력 배율 반환"""
        return self.defense_multiplier


class PlayerStats:
    """플레이어 스탯 클래스"""
    
    def __init__(self, stat_points_per_level: int = 4, initial_stats: dict = None, level: int = 1, stat_distribution: dict = None):
        self.level = level
        self.exp = 0
        self.stat_points = 0
        self.battle_point = 30
        self.stats = [0] * len(StatType)
        self.stat_points_per_level = stat_points_per_level
    
        # 초기 스탯 설정
        if initial_stats:
            self.stats[StatType.HP] = initial_stats.get('hp', 100)
            self.stats[StatType.ATK] = initial_stats.get('atk', 0)
            self.stats[StatType.DEF] = initial_stats.get('def', 0)
            self.stats[StatType.LUC] = initial_stats.get('luc', 0)
        else:
            self.stats[StatType.HP] = 100  # 기본 HP
        
        # 스탯 투자
        if level > 1 and stat_distribution:
            # 레벨업에 따른 스탯 포인트 계산
            total_points = (level - 1) * self.stat_points_per_level
            for stat_type, ratio in stat_distribution.items():
                points_to_add = int(total_points * ratio)
                if points_to_add > 0:
                    self.add_stat(stat_type, points_to_add)
    
    
    def add_stat(self, stat_type: StatType, amount: int):
        """스탯 포인트 사용 (스탯 타입별 배수 적용)"""
        if amount <= 0:
            return False
        
        # 스탯 타입별 배수
        stat_multiplier = 1
        if stat_type == StatType.HP:
            stat_multiplier = 4
        elif stat_type == StatType.ATK:
            stat_multiplier = 2
        elif stat_type == StatType.DEF:
            stat_multiplier = 2
        elif stat_type == StatType.LUC:
            stat_multiplier = 1
        
        self.stats[stat_type] += amount * stat_multiplier
        return True
    
    def get_stat(self, stat_type: StatType) -> int:
        """스탯 값 반환"""
        return self.stats[stat_type]
    


class MonsterStatCalculator:
    """몬스터 스탯 계산기"""
    
    def __init__(self):
        # 기본 스탯 계수들
        self.base_gold = 120
        self.base_exp = 20
        
        # 레벨별 스탯 증가 계수 (선형 증가량)
        self.gold_growth = 3  # 골드는 레벨당 3 증가
        
    def calculate_stats(self, level: int) -> MonsterStats:
        """레벨에 따른 몬스터 스탯 계산 (단계별 성장률 조정)"""
        if level <= 0:
            raise ValueError("레벨은 1 이상이어야 합니다.")
        
        # 몬스터 스탯 계산 - 구간별 공식 (HP 70%)
        if level <= 300:
            # 초반부: 플레이어 유리
            hp = int(23.2 * level * 0.7)
            atk = int(1.8 * level)
        elif level <= 800:
            # 초중반부: 플레이어 약간 유리
            hp = int((6960 + 30.0 * (level - 300)) * 0.7)
            atk = int(540 + 2.1 * (level - 300))
        elif level <= 1800:
            # 중반부: 균형잡힌 전투
            hp = int((21960 + 36.0 * (level - 800)) * 0.7)
            atk = int(1590 + 2.5 * (level - 800))
        elif level <= 3333:
            # 중후반부: 몬스터 강화
            hp = int((57960 + 46.0 * (level - 1800)) * 0.7)
            atk = int(4090 + 3.2 * (level - 1800))
        elif level <= 5000:
            # 후반부: 몬스터 압도적
            hp = int((128478 + 48.0 * (level - 3333)) * 0.7)
            atk = int(8995.6 + 3.2 * (level - 3333))
        elif level <= 20000:
            # 극후반부: 매우 어려움
            hp = int((208494 + 64.0 * (level - 5000)) * 0.7)
            atk = int(14330.0 + 4.3 * (level - 5000))
        elif level <= 30000:
            # 초극후반부: 극한 난이도 (소폭 강화)
            hp = int((1168494 + 90.0 * (level - 20000)) * 0.7)
            atk = int(78830.0 + 6.2 * (level - 20000))
        elif level <= 50000:
            # 초극후반부2: 극한 난이도 (추가 강화)
            hp = int((2068494 + 122.0 * (level - 30000)) * 0.7)
            atk = int(140830.0 + 8.5 * (level - 30000))
        elif level <= 60000:
            # 초극후반부3: 극한 난이도 (5만 이후 추가 강화)
            hp = int((4508494 + 160.0 * (level - 50000)) * 0.7)
            atk = int(310830.0 + 11.3 * (level - 50000))
        elif level <= 70000:
            # 초극후반부4: 극한 난이도 (5만 이후 추가 강화)
            hp = int((6108494 + 182.0 * (level - 60000)) * 0.7)
            atk = int(423830.0 + 12.8 * (level - 60000))
        elif level <= 80000:
            # 초극후반부5: 극한 난이도 (7만 이후 추가 강화)
            hp = int((7928494 + 205.0 * (level - 70000)) * 0.7)
            atk = int(551830.0 + 14.5 * (level - 70000))
        elif level <= 90000:
            # 초극후반부6: 극한 난이도 (7만 이후 추가 강화)
            hp = int((9978494 + 205.0 * (level - 80000)) * 0.7)
            atk = int(696830.0 + 14.5 * (level - 80000))
        else:
            # 최종구간: 최종 난이도 (최종 구간 추가 강화)
            hp = int((12028494 + 260.0 * (level - 90000)) * 0.7)
            atk = int(841830.0 + 18.4 * (level - 90000))
        
        # 지수함수 근사 (참고용 주석)
        # hp = int(8.9 * (level ** 1.19))
        # atk = int(0.7 * (level ** 1.18))
        
        gold = int(self.base_gold + self.gold_growth * (level ** 0.8))
        # 경험치 공식: level^1.7 * log(level + 1) * level * 2 / (10000 + level)
        # n^1.4 필요 경험치 시스템에 맞춰 조정됨
        log_val = math.log(level + 1)
        level_factor = level * 2 / (10000 + level)  # 저레벨 억제, 고레벨 성장
        exp = int(self.base_exp * (level ** 1.6) * log_val * level_factor)
        
        return MonsterStats(
            level=level,
            hp=hp,
            atk=atk,
            gold=gold,
            exp=exp
        )


class BattleSimulator:
    """전투 시뮬레이터 (Unity와 완전히 일치)"""
    
    def __init__(self, weapon: Weapon, armor: Armor, monster: MonsterStats, player: PlayerStats):
        self.weapon = weapon
        self.armor = armor
        self.monster = monster
        self.player_stats = player
        
        # 몬스터 체력만 초기화
        self.max_monster_hp = monster.hp
        self.current_monster_hp = self.max_monster_hp
        self.max_player_hp = player.get_stat(StatType.HP)
        self.current_player_hp = self.max_player_hp
        
        # 전투 결과
        self.battle_result = {
            "is_victory": False,
            "total_damage_dealt": 0,
            "total_damage_taken": 0,
            "turns": 0,
            "monster_hp_remaining": 0,
            "player_attack_count": 0,
            "monster_attack_count": 0,
            "total_healed": 0  # 흡혈로 회복한 총 체력
        }
    
    def is_monster_dead(self) -> bool:
        """몬스터가 죽었는지 확인"""
        return self.current_monster_hp <= 0
    
    def is_player_dead(self) -> bool:
        """플레이어가 죽었는지 확인"""
        return self.current_player_hp <= 0
    
    def is_battle_ended(self) -> bool:
        """전투가 끝났는지 확인"""
        return self.is_monster_dead() or self.is_player_dead()
    
    def process_hit(self, base_damage: int, hit_count: int):
        """타격 처리 (단순화된 로직)"""
        # 대미지 계산 (랜덤 변동 80%~120%)
        damage_multiplier = 0.8 + random.uniform(0, 1) * 0.4
        damage = int(math.ceil(base_damage * damage_multiplier))
        
        # 몬스터 ATK 기반 대미지 감소 (실험적 기능 - ATK * 0.1 사용)
        monster_atk = self.monster.atk
        if damage > 0 and monster_atk > 0:
            atk_tenth = monster_atk * 0.1
            reduction_rate = atk_tenth / (damage + atk_tenth)
            damage = int(damage * (1 - reduction_rate))
        
        # 대미지 적용
        self.current_monster_hp = max(0, self.current_monster_hp - damage)
        self.battle_result["total_damage_dealt"] += damage
        
        return damage, False  # 치명타는 제거됨
    
    def simulate_player_attack(self, lifesteal_rate: float = 0.0, damage_multiplier: float = 1.0):
        """플레이어 공격 시뮬레이션 (단순화된 로직)
        
        Args:
            lifesteal_rate: 입힌 피해의 몇 %만큼 회복할지 (기본값: 0.0 = 0%)
            damage_multiplier: 플레이어 대미지 배수 (기본값: 1.0)
        """
        # 기본 대미지 계산
        base_damage = (self.weapon.get_calculated_base_damage() + 
                      int(self.player_stats.get_stat(StatType.ATK) * 
                          (self.weapon.get_calculated_damage_multiplier() * 0.01)))
        
        # 대미지 배수 적용
        base_damage = int(base_damage * damage_multiplier)
        
        # 공격 횟수 카운트
        self.battle_result["player_attack_count"] += 1
        
        # 단일 타격만 처리 (멀티히트 제거)
        damage_dealt, _ = self.process_hit(base_damage, 1)
        
        # 흡혈: 입힌 피해의 일정 비율만큼 체력 회복
        if damage_dealt > 0 and lifesteal_rate > 0:
            heal_amount = int(math.ceil(damage_dealt * lifesteal_rate))
            old_hp = self.current_player_hp
            self.current_player_hp = min(self.max_player_hp, self.current_player_hp + heal_amount)
            actual_heal = self.current_player_hp - old_hp
            self.battle_result["total_healed"] += actual_heal
    
    def simulate_monster_attack(self):
        """몬스터 공격 시뮬레이션 (단순화된 로직)"""
        # 공격 횟수 카운트
        self.battle_result["monster_attack_count"] += 1
        
        # 방어력 계산
        total_defense = (self.armor.get_calculated_base_defense() + 
                        int(self.player_stats.get_stat(StatType.DEF) * 
                            (self.armor.get_calculated_defense_multiplier() * 0.01)))
        
        # 받는 피해 계산 (몬스터 ATK가 0인 경우 처리)
        if self.monster.atk == 0:
            base_damage = 0
        else:
            damage_reduction = total_defense / (total_defense + self.monster.atk)
            base_damage = self.monster.atk * (1 - damage_reduction)
        
        # 랜덤 대미지 변동 (80%~120%)
        damage_multiplier = 0.8 + random.uniform(0, 1) * 0.4
        damage = int(math.ceil(base_damage * damage_multiplier))
        
        self.current_player_hp = max(0, self.current_player_hp - damage)
        self.battle_result["total_damage_taken"] += damage
    
    def simulate_battle(self, lifesteal_rate: float = 0.0, damage_multiplier: float = 1.0) -> Dict:
        """전투 시뮬레이션 실행
        
        Args:
            lifesteal_rate: 입힌 피해의 몇 %만큼 회복할지 (기본값: 0.0 = 0%)
            damage_multiplier: 플레이어 대미지 배수 (기본값: 1.0)
        """
        self.current_player_hp = self.max_player_hp
        self.current_monster_hp = self.max_monster_hp
        self.battle_result = {
            "is_victory": False,
            "total_damage_dealt": 0,
            "total_damage_taken": 0,
            "turns": 0,
            "player_attack_count": 0,
            "monster_attack_count": 0,
            "total_healed": 0  # 흡혈로 회복한 총 체력
        }
        
        max_turns = 1000  # 무한 루프 방지를 위한 최대 턴 수
        
        while not self.is_battle_ended() and self.battle_result["turns"] < max_turns:
            self.battle_result["turns"] += 1
            
            # 플레이어 공격 (흡혈 및 대미지 배수 적용)
            self.simulate_player_attack(lifesteal_rate, damage_multiplier)
            
            # 몬스터가 살아있다면 몬스터 공격
            if not self.is_monster_dead() and not self.is_player_dead():
                self.simulate_monster_attack()
        
        # 최대 턴 수에 도달한 경우 무승부 처리
        if self.battle_result["turns"] >= max_turns:
            self.battle_result["is_victory"] = False  # 무승부
        else:
            self.battle_result["is_victory"] = self.is_monster_dead()
        
        self.battle_result["monster_hp_remaining"] = self.current_monster_hp
        self.battle_result["player_hp_remaining"] = min(self.current_player_hp, self.max_player_hp)
        return self.battle_result


def main():
    """메인 함수 - 시뮬레이션 실행"""
    # 명령행 인자 파싱
    parser = argparse.ArgumentParser(description='LoopLegacy 전투 시뮬레이터')
    parser.add_argument('--stat-points-per-level', type=int, default=4, 
                       help='레벨당 스탯 포인트 (기본값: 4)')
    parser.add_argument('--player-level', type=int, default=None, 
                       help='플레이어 레벨')
    parser.add_argument('--monster-level', type=int, default=None, 
                       help='몬스터 레벨')
    parser.add_argument('--monster-stats', type=str, metavar='HP,ATK',
                       help='몬스터 스탯 직접 지정 (예: --monster-stats "100,50")')
    parser.add_argument('--simulations', type=int, default=50, 
                       help='시뮬레이션 횟수 (기본값: 50)')
    parser.add_argument('--weapon', type=str, default='5,100', 
                       help='무기 스펙 (기본값: "5,100") - 형식: "base_damage,damage_multiplier"')
    parser.add_argument('--armor', type=str, default='5,100', 
                       help='방어구 스펙 (기본값: "5,100") - 형식: "base_defense,defense_multiplier"')
    parser.add_argument('--stat-ratios', type=str, default='0.25,0.5,0.25,0.0', 
                       help='스탯 투자 비율 (기본값: "0.25,0.5,0.25,0.0") - 형식: "HP,ATK,DEF,LUC"')
    parser.add_argument('--initial-stats', type=str, default='10,0,0,0', 
                       help='초기 스탯 (기본값: "10,0,0,0") - 형식: "HP,ATK,DEF,LUC"')
    parser.add_argument('--level-up-simulation', action='store_true', 
                       help='레벨업 시뮬레이션 모드 (전투 없이 레벨업만 계산)')
    parser.add_argument('--monster-stats-mode', action='store_true', 
                       help='몬스터 스탯 계산 모드 (시뮬레이션 없이 스탯만 계산)')
    parser.add_argument('--level-up-calculator', nargs=2, type=int, metavar=('LEVEL', 'EXP'),
                       help='레벨업 계산 모드: --level-up-calculator <레벨> <경험치>')
    parser.add_argument('--exp-calculator', nargs=2, type=int, metavar=('LEVEL', 'LEVEL_UPS'),
                       help='경험치 계산 모드: --exp-calculator <레벨> <레벨 업 횟수>')
    parser.add_argument('--battle-simulator', action='store_true',
                       help='전투 시뮬레이터 모드')
    parser.add_argument('--lifesteal-rate', type=float, default=0.0,
                       help='흡혈 비율 (입힌 피해의 몇 %%만큼 회복할지, 기본값: 0.0 = 0%%)')
    parser.add_argument('--damage-multiplier', type=float, default=1.0,
                       help='플레이어 대미지 배수 (기본값: 1.0) - 예: 1.2면 플레이어 대미지가 1.2배')
    
    args = parser.parse_args()
    
    # --monster-level과 --monster-stats 동시 사용 검증
    if args.monster_level is not None and args.monster_stats is not None:
        print("오류: --monster-level과 --monster-stats는 동시에 사용할 수 없습니다.")
        return
    
    # 몬스터 레벨이 명시적으로 설정되지 않은 경우에만 플레이어 레벨과 동일하게 설정
    # (기본값 700이 아닌 경우에만 덮어쓰기)
    if args.monster_level is None and args.player_level is not None and args.monster_stats is None:
        args.monster_level = args.player_level
    
    # 경험치 계산 모드
    if args.level_up_calculator:
        print("=== 경험치 계산 모드 ===\n")
        
        # 인자에서 레벨과 경험치 추출
        player_level, exp_amount = args.level_up_calculator
        
        # 초기 스탯 파싱
        try:
            initial_parts = args.initial_stats.split(',')
            if len(initial_parts) != 4:
                print(f"오류: 초기 스탯은 정확히 4개의 값이 필요합니다 (HP,ATK,DEF,LUC). 입력된 값: {len(initial_parts)}개")
                return
            
            initial_stats = {
                'hp': int(initial_parts[0].strip()),
                'atk': int(initial_parts[1].strip()),
                'def': int(initial_parts[2].strip()),
                'luc': int(initial_parts[3].strip())
            }
        except ValueError as e:
            print(f"오류: 초기 스탯 파싱 중 오류가 발생했습니다: {e}")
            return
        
        # 몬스터 스탯 계산기 초기화
        calculator = MonsterStatCalculator()
        
        # 경험치 계산 실행 (공통 함수 사용)
        current_level, current_exp = calculate_levelups_from_exp(player_level, exp_amount)
        
        result = {
            'initial_level': player_level,
            'final_level': current_level,
            'remaining_exp': current_exp
        }
        
        print(f"레벨 {player_level}에서 {exp_amount:,} 경험치를 획득했을 때:")
        print(f"초기 레벨: {result['initial_level']}")
        print(f"최종 레벨: {result['final_level']}")
        print(f"레벨업 횟수: {result['final_level'] - result['initial_level']}")
        print(f"획득한 경험치: {exp_amount:,}")
        print(f"레벨업에 사용된 경험치: {exp_amount - result['remaining_exp']:,}")
        print(f"남은 경험치: {result['remaining_exp']:,}")
        print(f"획득한 스탯 포인트: {(result['final_level'] - result['initial_level']) * args.stat_points_per_level}")
        
        next_level_exp = calculate_required_exp_for_level(result['final_level'])
        if result['remaining_exp'] < next_level_exp:
            print(f"다음 레벨까지 필요한 경험치: {next_level_exp - result['remaining_exp']:,}")
        else:
            print("다음 레벨까지 필요한 경험치: 0 (이미 다음 레벨업 가능)")
        
        return

    if args.exp_calculator:
        print("=== 경험치 계산 모드 ===\n")
        
        # 인자에서 레벨과 레벨 업 횟수 추출
        player_level, level_ups = args.exp_calculator

        # 공통 함수 사용
        result = calculate_total_exp_for_levelups(player_level, level_ups)
        
        print(f"레벨 {player_level}에서 {level_ups}번 레벨업 시 필요한 경험치: {result:,}({result})")
        
        return
    
    # 전투 시뮬레이션 모드
    if args.battle_simulator:
        print("=== 전투 시뮬레이션 모드 ===\n")
        
        # 플레이어 레벨과 몬스터 레벨 사용
        player_level = args.player_level
        monster_level = args.monster_level
        
        # 초기 스탯 파싱
        try:
            initial_parts = args.initial_stats.split(',')
            if len(initial_parts) != 4:
                print(f"오류: 초기 스탯은 정확히 4개의 값이 필요합니다 (HP,ATK,DEF,LUC). 입력된 값: {len(initial_parts)}개")
                return
        
            initial_stats = {
                'hp': int(initial_parts[0].strip()),
                'atk': int(initial_parts[1].strip()),
                'def': int(initial_parts[2].strip()),
                'luc': int(initial_parts[3].strip())
            }
        except ValueError as e:
            print(f"오류: 초기 스탯 파싱 중 오류가 발생했습니다: {e}")
            return
        
        # 몬스터 스탯 계산기 초기화
        calculator = MonsterStatCalculator()
        
        # 몬스터 스탯 결정
        if args.monster_stats is not None:
            # --monster-stats 인자 사용
            try:
                monster_stats_parts = args.monster_stats.split(',')
                if len(monster_stats_parts) != 2:
                    print(f"오류: 몬스터 스탯은 정확히 2개의 값이 필요합니다 (HP,ATK). 입력된 값: {len(monster_stats_parts)}개")
                    return
                
                monster_hp = int(monster_stats_parts[0].strip())
                monster_atk = int(monster_stats_parts[1].strip())
                
            except ValueError as e:
                print(f"오류: 몬스터 스탯 값 중 유효하지 않은 정수가 있습니다: {e}")
                return
        else:
            # 몬스터 레벨로 계산
            monster = calculator.calculate_stats(monster_level)
            monster_hp = monster.hp
            monster_atk = monster.atk
        
        # 스탯 투자 비율 파싱
        try:
            ratio_parts = args.stat_ratios.split(',')
            if len(ratio_parts) != 4:
                print(f"오류: 스탯 투자 비율은 정확히 4개의 값이 필요합니다 (HP,ATK,DEF,LUC). 입력된 값: {len(ratio_parts)}개")
                return
            
            stat_distribution = {
                StatType.HP: float(ratio_parts[0].strip()),
                StatType.ATK: float(ratio_parts[1].strip()),
                StatType.DEF: float(ratio_parts[2].strip()),
                StatType.LUC: float(ratio_parts[3].strip())
            }
        except ValueError as e:
            print(f"오류: 스탯 투자 비율 파싱 중 오류가 발생했습니다: {e}")
            return
        
        # 시뮬레이션 실행
        wins = 0
        total_turns = 0
        total_monster_hp_remaining = 0
        total_player_hp_remaining = 0
        total_player_damage = 0
        total_monster_damage = 0
        total_player_attacks = 0
        total_monster_attacks = 0
        simulations = 100
        
        if args.monster_stats is not None:
            print(f"레벨 {player_level} 플레이어 vs 커스텀 몬스터 (HP:{monster_hp}, ATK:{monster_atk})")
        else:
            print(f"레벨 {player_level} 플레이어 vs 레벨 {monster_level} 몬스터 (HP:{monster_hp}, ATK:{monster_atk})")
        print(f"시뮬레이션 횟수: {simulations}회\n")
        
        # 몬스터 객체 직접 생성
        custom_monster = MonsterStats(
            level=0,
            hp=monster_hp,
            atk=monster_atk,
            gold=0,
            exp=0
        )
        
        # 플레이어 생성
        player = PlayerStats(args.stat_points_per_level, initial_stats, player_level, stat_distribution)
        
        # 장비 스펙 파싱
        try:
            weapon_parts = args.weapon.split(',')
            if len(weapon_parts) != 2:
                print(f"오류: 무기 스펙은 정확히 2개의 값이 필요합니다 (base_damage,damage_multiplier). 입력된 값: {len(weapon_parts)}개")
                return
            
            weapon_base_damage = int(weapon_parts[0].strip())
            weapon_damage_multiplier = int(weapon_parts[1].strip())
            
            armor_parts = args.armor.split(',')
            if len(armor_parts) != 2:
                print(f"오류: 방어구 스펙은 정확히 2개의 값이 필요합니다 (base_defense,defense_multiplier). 입력된 값: {len(armor_parts)}개")
                return
            
            armor_base_defense = int(armor_parts[0].strip())
            armor_defense_multiplier = int(armor_parts[1].strip())
            
        except ValueError as e:
            print(f"오류: 장비 스펙 파싱 중 오류가 발생했습니다: {e}")
            return
            
            # 장비 생성
        weapon = Weapon(weapon_base_damage, weapon_damage_multiplier)
        armor = Armor(armor_base_defense, armor_defense_multiplier)
            
        total_healed = 0
        for i in range(simulations):
            # 전투 시뮬레이션 (흡혈 비율 및 대미지 배수 적용)
            simulator = BattleSimulator(weapon, armor, custom_monster, player)
            result = simulator.simulate_battle(lifesteal_rate=args.lifesteal_rate, damage_multiplier=args.damage_multiplier)
            
            if result['is_victory']:
                wins += 1
            
            total_turns += result['turns']
            total_monster_hp_remaining += result['monster_hp_remaining']
            total_player_hp_remaining += result.get('player_hp_remaining', 0)
            total_player_damage += result['total_damage_dealt']
            total_monster_damage += result['total_damage_taken']
            total_player_attacks += result.get('player_attack_count', 0)
            total_monster_attacks += result.get('monster_attack_count', 0)
            total_healed += result.get('total_healed', 0)
        
        # 결과 계산
        win_rate = (wins / simulations) * 100
        avg_turns = total_turns / simulations
        avg_monster_hp_remaining = total_monster_hp_remaining / simulations
        avg_player_hp_remaining = total_player_hp_remaining / simulations
        avg_player_damage_per_attack = total_player_damage / total_player_attacks if total_player_attacks > 0 else 0
        avg_monster_damage_per_attack = total_monster_damage / total_monster_attacks if total_monster_attacks > 0 else 0
        avg_healed = total_healed / simulations
        
        # 몬스터 객체 직접 생성
        monster = MonsterStats(
            level=0,  # 레벨은 표시용으로만 사용
            hp=monster_hp,
            atk=monster_atk,
            gold=0,  # 골드는 전투에 영향 없음
            exp=0    # 경험치는 전투에 영향 없음
        )
        
        # 결과 출력
        print("=== 전투 결과 ===")
        print(f"흡혈 비율: {args.lifesteal_rate * 100:.1f}%")
        print(f"대미지 배수: {args.damage_multiplier:.2f}x")
        print(f"승률: {win_rate:.1f}% ({wins}/{simulations})")
        print(f"평균 턴 수: {avg_turns:.1f}턴")
        print(f"평균 몬스터 체력 잔여율: {(avg_monster_hp_remaining / monster.hp) * 100:.1f}% (평균 {avg_monster_hp_remaining:.0f}/{monster.hp} HP)")
        print(f"평균 플레이어 체력 잔여율: {(avg_player_hp_remaining / player.get_stat(StatType.HP)) * 100:.1f}% (평균 {avg_player_hp_remaining:.0f}/{player.get_stat(StatType.HP)} HP)")
        print(f"평균 플레이어 대미지: {avg_player_damage_per_attack:.1f}")
        print(f"평균 몬스터 대미지: {avg_monster_damage_per_attack:.1f}")
        print(f"평균 흡혈 회복량: {avg_healed:.1f} HP (총 {total_healed:.0f} HP)")
        
        print(f"\n=== 몬스터 스탯 ===")
        print(f"Lv.{monster.level}")
        print(f"HP: {monster.hp:,}")
        print(f"ATK: {monster.atk:,}")
        
        print(f"\n=== 플레이어 스탯 (레벨 {player_level}) ===")
        print(f"HP: {player.get_stat(StatType.HP):,}")
        print(f"ATK: {player.get_stat(StatType.ATK):,}")
        print(f"DEF: {player.get_stat(StatType.DEF):,}")
        print(f"LUC: {player.get_stat(StatType.LUC):,}")
        
        # 플레이어 스탯 포인트 계산
        total_points = (player_level - 1) * args.stat_points_per_level
        
        print(f"\n=== 스탯 투자 비율 ===")
        # 스탯 투자 비율 파싱하여 표시
        try:
            ratio_parts = args.stat_ratios.split(',')
            hp_ratio = float(ratio_parts[0].strip())
            atk_ratio = float(ratio_parts[1].strip())
            def_ratio = float(ratio_parts[2].strip())
            luc_ratio = float(ratio_parts[3].strip())
            
            print(f"HP: {hp_ratio:.2f} ({int(total_points * hp_ratio)} 포인트)")
            print(f"ATK: {atk_ratio:.2f} ({int(total_points * atk_ratio)} 포인트)")
            print(f"DEF: {def_ratio:.2f} ({int(total_points * def_ratio)} 포인트)")
            print(f"LUC: {luc_ratio:.2f} ({int(total_points * luc_ratio)} 포인트)")
        except (ValueError, IndexError):
            print(f"스탯 투자 비율 파싱 오류: {args.stat_ratios}")
        
        return
    
    print("=== LoopLegacy 전투 시뮬레이터 ===\n")
    print(f"레벨당 스탯 포인트: {args.stat_points_per_level}")
    print(f"플레이어 레벨: {args.player_level}")
    print(f"몬스터 레벨: {args.monster_level}")
    print(f"시뮬레이션 횟수: {args.simulations}")
    print(f"스탯 투자 비율:")
    # 스탯 투자 비율 파싱하여 표시
    try:
        ratio_parts = args.stat_ratios.split(',')
        print(f"  HP: {float(ratio_parts[0].strip()):.2f}")
        print(f"  ATK: {float(ratio_parts[1].strip()):.2f}")
        print(f"  DEF: {float(ratio_parts[2].strip()):.2f}")
        print(f"  LUC: {float(ratio_parts[3].strip()):.2f}")
    except (ValueError, IndexError):
        print(f"  스탯 투자 비율: {args.stat_ratios}")
    print()
    
    # 몬스터 스탯 계산기 초기화
    calculator = MonsterStatCalculator()
    
    # 초기 스탯 파싱
    try:
        initial_parts = args.initial_stats.split(',')
        if len(initial_parts) != 4:
            print(f"오류: 초기 스탯은 정확히 4개의 값이 필요합니다 (HP,ATK,DEF,LUC). 입력된 값: {len(initial_parts)}개")
            return
        
        initial_stats = {
            'hp': int(initial_parts[0].strip()),
            'atk': int(initial_parts[1].strip()),
            'def': int(initial_parts[2].strip()),
            'luc': int(initial_parts[3].strip())
        }
    except ValueError as e:
        print(f"오류: 초기 스탯 파싱 중 오류가 발생했습니다: {e}")
        return
    
    # 몬스터 스탯 계산기 초기화 (이미 위에서 생성됨)
    
    
    if args.monster_stats_mode:
        # 몬스터 스탯 계산 모드
        print("=== 몬스터 스탯 계산 모드 ===\n")
        
        # 몬스터 스탯 계산
        monster = calculator.calculate_stats(args.monster_level)
        
        print(f"레벨 {args.monster_level} 몬스터 스탯:")
        print(f"  HP: {monster.hp:,} ({monster.hp})")
        print(f"  ATK: {monster.atk:,} ({monster.atk})")
        print(f"  Gold: {monster.gold:,} ({monster.gold})")
        print(f"  EXP: {monster.exp:,} ({monster.exp})")
        
        return
    
    if args.level_up_simulation:
        # 레벨업 시뮬레이션 모드
        print("=== 레벨업 시뮬레이션 모드 ===\n")
        
        # 몬스터 스탯 계산
        monster = calculator.calculate_stats(args.monster_level)
        
        # 레벨업 시뮬레이션 (PlayerStats 객체 사용하지 않음)
        initial_level = args.player_level
        initial_exp = 0
        initial_stat_points = 0
        
        # 몬스터 처치 시 획득 경험치
        exp_gain = monster.exp
        
        # 레벨업 계산
        current_level = initial_level
        current_exp = initial_exp + exp_gain
        current_stat_points = initial_stat_points
        
        # 레벨업 체크 (공통 함수 사용)
        final_level, current_exp = calculate_levelups_from_exp(current_level, current_exp)
        levels_gained = final_level - initial_level
        current_stat_points = initial_stat_points + (levels_gained * args.stat_points_per_level)
        current_level = final_level
        
        print(f"레벨 {initial_level} 플레이어가 레벨 {args.monster_level} 몬스터를 처치했을 때:")
        print(f"몬스터 경험치: {exp_gain:,}")
        print(f"초기 레벨: {initial_level}")
        print(f"최종 레벨: {current_level}")
        print(f"레벨업 횟수: {levels_gained}")
        print(f"획득 스탯 포인트: {current_stat_points}")
        print(f"남은 경험치: {current_exp:,}")
        
        # 몬스터 스탯 출력
        print(f"\n몬스터 스탯: {monster}")
        
        return


if __name__ == "__main__":
    main()
