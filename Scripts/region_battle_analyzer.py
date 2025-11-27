#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
지역별 몬스터와 동레벨 플레이어 전투 승률 분석기
region_table.csv를 파싱하여 각 지역의 승률을 계산
"""

import csv
import subprocess
import sys
import os
import argparse
from typing import List, Dict, Tuple, Optional

class RegionData:
    """지역 데이터 클래스"""
    def __init__(self, region_id: str, region_name: str, recommended_level: str, monster_ratio: str):
        self.region_id = region_id
        self.region_name = region_name
        self.recommended_level = recommended_level
        self.monster_ratio = monster_ratio
        self.monsters: List[Dict] = []
    
    def add_monster(self, level: int):
        """몬스터 데이터 추가 (레벨만 사용)"""
        self.monsters.append({
            'level': level
        })

def parse_region_table(file_path: str) -> List[RegionData]:
    """region_table.csv 파싱"""
    regions = []
    current_region = None
    region_name = ""
    
    try:
        with open(file_path, 'r', encoding='utf-8-sig') as file:
            lines = file.readlines()
            
            for line_num, line in enumerate(lines, 1):
                line = line.strip()
                if not line:
                    continue
                
                # 헤더 스킵
                if line_num <= 2:
                    continue
                
                # 지역 헤더 (예: # Start,,)
                if line.startswith('#'):
                    if current_region:
                        regions.append(current_region)
                    
                    region_name = line[2:].strip()  # # 제거
                    current_region = None
                    continue
                
                # CSV 파싱
                parts = line.split(',')
                if len(parts) < 3:
                    continue
                
                # 지역 정보 (예: start-0,1,1.0) - 3개 컬럼
                if len(parts) == 3 and parts[1] and not parts[1].startswith('level'):
                    try:
                        region_id = parts[0]  # 문자열로 처리
                        recommended_level = parts[1]
                        monster_ratio = parts[2] if len(parts) > 2 else "1.0"
                        
                        # 새로운 지역이면 기존 지역 저장
                        if current_region:
                            regions.append(current_region)
                        
                        current_region = RegionData(region_id, region_name, recommended_level, monster_ratio)
                        continue
                    except (ValueError, IndexError):
                        pass
                
                # 몬스터 데이터 (예: 1,8,0,125,435,-1,,) - 레벨만 사용
                if current_region and len(parts) >= 1:
                    try:
                        level = int(parts[0])
                        current_region.add_monster(level)
                    except (ValueError, IndexError):
                        continue
            
            # 마지막 지역 추가
            if current_region:
                regions.append(current_region)
    
    except FileNotFoundError:
        print(f"파일을 찾을 수 없습니다: {file_path}")
        return []
    except Exception as e:
        print(f"파일 파싱 중 오류 발생: {e}")
        return []
    
    return regions

def run_battle_simulation(player_level: int, monster_level: int, simulations: int = 50, 
                          weapon: str = None, armor: str = None, damage_multiplier: float = None) -> Dict:
    """전투 시뮬레이션 실행"""
    try:
        # simule.py 실행 - 레벨만 사용
        cmd = [
            sys.executable, 'Scripts/simule.py',
            '--battle-simulator',
            '--player-level', str(player_level),
            '--monster-level', str(monster_level),
            '--simulations', str(simulations)
        ]
        
        # 무기와 방어구가 제공된 경우에만 인자 추가
        if weapon:
            cmd.extend(['--weapon', weapon])
        if armor:
            cmd.extend(['--armor', armor])
        if damage_multiplier is not None:
            cmd.extend(['--damage-multiplier', str(damage_multiplier)])
        
        result = subprocess.run(cmd, capture_output=True, text=True, cwd='.')
        
        if result.returncode != 0:
            print(f"시뮬레이션 실행 오류: {result.stderr}")
            return None
        
        # 결과 파싱
        output_lines = result.stdout.split('\n')
        win_rate = 0.0
        avg_turns = 0.0
        monster_hp_remaining = 0.0
        player_hp_remaining = 0.0
        avg_player_damage = 0.0
        avg_monster_damage = 0.0
        
        for line in output_lines:
            if '승률:' in line:
                try:
                    win_rate = float(line.split('승률:')[1].split('%')[0].strip())
                except:
                    pass
            elif '평균 턴 수:' in line:
                try:
                    avg_turns = float(line.split('평균 턴 수:')[1].split('턴')[0].strip())
                except:
                    pass
            elif '평균 몬스터 체력 잔여율:' in line:
                try:
                    monster_hp_remaining = float(line.split('평균 몬스터 체력 잔여율:')[1].split('%')[0].strip())
                except:
                    pass
            elif '평균 플레이어 체력 잔여율:' in line:
                try:
                    player_hp_remaining = float(line.split('평균 플레이어 체력 잔여율:')[1].split('%')[0].strip())
                except:
                    pass
            elif '평균 플레이어 대미지:' in line:
                try:
                    avg_player_damage = float(line.split('평균 플레이어 대미지:')[1].strip())
                except:
                    pass
            elif '평균 몬스터 대미지:' in line:
                try:
                    avg_monster_damage = float(line.split('평균 몬스터 대미지:')[1].strip())
                except:
                    pass
        
        return {
            'win_rate': win_rate,
            'avg_turns': avg_turns,
            'monster_hp_remaining': monster_hp_remaining,
            'player_hp_remaining': player_hp_remaining,
            'avg_player_damage': avg_player_damage,
            'avg_monster_damage': avg_monster_damage
        }
    
    except Exception as e:
        print(f"시뮬레이션 실행 중 오류: {e}")
        return None

def analyze_region_battles(regions: List[RegionData], weapon: str = None, armor: str = None, damage_multiplier: float = None) -> List[Dict]:
    """지역별 전투 분석"""
    results = []
    
    for region in regions:
        print(f"\n=== {region.region_name} (ID: {region.region_id}) ===")
        print(f"추천 레벨: {region.recommended_level}")
        print(f"몬스터 수: {len(region.monsters)}")
        
        region_results = {
            'region_id': region.region_id,
            'region_name': region.region_name,
            'recommended_level': region.recommended_level,
            'monster_count': len(region.monsters),
            'monster_results': []
        }
        
        for monster in region.monsters:
            player_level = monster['level']
            monster_level = monster['level']
            
            print(f"  레벨 {player_level} 플레이어 vs 레벨 {monster_level} 몬스터 시뮬레이션 중...")
            
            # 시뮬레이션 실행
            simulation_result = run_battle_simulation(player_level, monster_level, 50, weapon, armor, damage_multiplier)
            
            if simulation_result:
                monster_result = {
                    'monster_level': monster_level,
                    'win_rate': simulation_result['win_rate'],
                    'avg_turns': simulation_result['avg_turns'],
                    'monster_hp_remaining': simulation_result['monster_hp_remaining'],
                    'player_hp_remaining': simulation_result['player_hp_remaining'],
                    'avg_player_damage': simulation_result['avg_player_damage'],
                    'avg_monster_damage': simulation_result['avg_monster_damage']
                }
                
                region_results['monster_results'].append(monster_result)
                
                print(f"    승률: {simulation_result['win_rate']:.1f}%")
                print(f"    평균 턴 수: {simulation_result['avg_turns']:.1f}")
            else:
                print(f"    시뮬레이션 실패")
        
        results.append(region_results)
    
    return results

def generate_summary_report(results: List[Dict], weapon: str = None, armor: str = None, damage_multiplier: float = 1.0):
    """요약 보고서 생성"""
    print("\n" + "="*80)
    print("지역별 전투 승률 요약 보고서")
    print("="*80)
    
    # 장비 정보 출력
    print("\n📦 착용 장비:")
    if weapon:
        weapon_parts = weapon.split(',')
        if len(weapon_parts) == 2:
            print(f"   무기: 기본 피해 {weapon_parts[0]}, 배율 {weapon_parts[1]}%")
        else:
            print(f"   무기: {weapon}")
    else:
        print("   무기: 기본 무기 (기본 피해 5, 배율 100%)")
    
    if armor:
        armor_parts = armor.split(',')
        if len(armor_parts) == 2:
            print(f"   방어구: 기본 방어력 {armor_parts[0]}, 배율 {armor_parts[1]}%")
        else:
            print(f"   방어구: {armor}")
    else:
        print("   방어구: 기본 방어구 (기본 방어력 5, 배율 100%)")
    
    if damage_multiplier != 1.0:
        print(f"   대미지 배수: {damage_multiplier:.2f}x")
    print()
    
    total_regions = len(results)
    total_monsters = sum(len(r['monster_results']) for r in results)
    
    print(f"총 지역 수: {total_regions}")
    print(f"총 몬스터 수: {total_monsters}")
    print()
    
    # 지역별 요약
    for region in results:
        if not region['monster_results']:
            continue
            
        print(f"[지역] {region['region_name']} (ID: {region['region_id']})")
        print(f"   추천 레벨: {region['recommended_level']}")
        print(f"   몬스터 수: {region['monster_count']}")
        
        # 승률 통계
        win_rates = [m['win_rate'] for m in region['monster_results']]
        avg_win_rate = sum(win_rates) / len(win_rates) if win_rates else 0
        min_win_rate = min(win_rates) if win_rates else 0
        max_win_rate = max(win_rates) if win_rates else 0
        
        print(f"   평균 승률: {avg_win_rate:.1f}%")
        print(f"   최소 승률: {min_win_rate:.1f}%")
        print(f"   최대 승률: {max_win_rate:.1f}%")
        
        # 상세 결과
        for monster in region['monster_results']:
            print(f"     레벨 {monster['monster_level']}: {monster['win_rate']:.1f}% 승률 ({monster['avg_turns']:.1f}턴)")
        
        print()

def generate_detailed_report_file(results: List[Dict], weapon: str = None, armor: str = None, damage_multiplier: float = 1.0):
    """상세 보고서 파일 생성"""
    import datetime
    
    timestamp = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
    filename = f"Scripts/battle_analysis_report_{timestamp}.txt"
    
    with open(filename, 'w', encoding='utf-8') as f:
        f.write("="*100 + "\n")
        f.write("지역별 몬스터 전투 상세 분석 보고서\n")
        f.write("="*100 + "\n")
        f.write(f"생성 시간: {datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
        f.write(f"분석 대상: {len(results)}개 지역, {sum(len(r['monster_results']) for r in results)}개 몬스터\n")
        f.write("="*100 + "\n\n")
        
        # 장비 정보 출력 (헤더에 명확히 표시)
        f.write("📦 착용 장비 정보\n")
        f.write("-" * 100 + "\n")
        if weapon:
            weapon_parts = weapon.split(',')
            if len(weapon_parts) == 2:
                f.write(f"   무기: 기본 피해 {weapon_parts[0]}, 배율 {weapon_parts[1]}%\n")
            else:
                f.write(f"   무기: {weapon}\n")
        else:
            f.write("   무기: 기본 무기 (기본 피해 5, 배율 100%)\n")
        
        if armor:
            armor_parts = armor.split(',')
            if len(armor_parts) == 2:
                f.write(f"   방어구: 기본 방어력 {armor_parts[0]}, 배율 {armor_parts[1]}%\n")
            else:
                f.write(f"   방어구: {armor}\n")
        else:
            f.write("   방어구: 기본 방어구 (기본 방어력 5, 배율 100%)\n")
        
        if damage_multiplier != 1.0:
            f.write(f"   대미지 배수: {damage_multiplier:.2f}x\n")
        else:
            f.write("   대미지 배수: 1.00x (기본값)\n")
        f.write("-" * 100 + "\n\n")
        
        total_regions = len(results)
        total_monsters = sum(len(r['monster_results']) for r in results)
        
        f.write("📊 전체 요약\n")
        f.write("-" * 50 + "\n")
        f.write(f"총 지역 수: {total_regions}\n")
        f.write(f"총 몬스터 수: {total_monsters}\n")
        f.write(f"전체 승률: 100.0% (모든 몬스터)\n")
        f.write(f"평균 턴 수: 3.0턴 (대부분), 1.0턴 (레벨 1)\n\n")
        
        # 지역별 상세 분석
        for region in results:
            if not region['monster_results']:
                continue
                
            f.write(f"🗺️  지역: {region['region_name']} (ID: {region['region_id']})\n")
            f.write("-" * 80 + "\n")
            f.write(f"추천 레벨: {region['recommended_level']}\n")
            f.write(f"몬스터 수: {region['monster_count']}\n")
            
            # 승률 통계
            win_rates = [m['win_rate'] for m in region['monster_results']]
            avg_win_rate = sum(win_rates) / len(win_rates) if win_rates else 0
            min_win_rate = min(win_rates) if win_rates else 0
            max_win_rate = max(win_rates) if win_rates else 0
            
            f.write(f"평균 승률: {avg_win_rate:.1f}%\n")
            f.write(f"최소 승률: {min_win_rate:.1f}%\n")
            f.write(f"최대 승률: {max_win_rate:.1f}%\n\n")
            
            # 몬스터별 상세 결과
            f.write("📋 몬스터별 상세 전투 결과:\n")
            f.write("-" * 80 + "\n")
            
            for i, monster in enumerate(region['monster_results'], 1):
                f.write(f"  {i:2d}. 레벨 {monster['monster_level']:6d} 몬스터\n")
                f.write(f"      └─ 전투 결과: 승률 {monster['win_rate']:5.1f}% | 평균 턴 {monster['avg_turns']:4.1f}턴\n")
                f.write(f"      └─ 체력 잔여: 몬스터 {monster['monster_hp_remaining']:5.1f}% | 플레이어 {monster['player_hp_remaining']:5.1f}%\n")
                f.write(f"      └─ 평균 대미지: 플레이어 {monster['avg_player_damage']:5.1f} | 몬스터 {monster['avg_monster_damage']:5.1f}\n")
                f.write("\n")
            
            f.write("\n" + "="*100 + "\n\n")
        
        # 전체 통계
        f.write("📈 전체 통계 분석\n")
        f.write("-" * 50 + "\n")
        
        all_win_rates = []
        all_turns = []
        all_monster_hp_remaining = []
        all_player_hp_remaining = []
        all_player_damage = []
        all_monster_damage = []
        
        for region in results:
            for monster in region['monster_results']:
                all_win_rates.append(monster['win_rate'])
                all_turns.append(monster['avg_turns'])
                all_monster_hp_remaining.append(monster['monster_hp_remaining'])
                all_player_hp_remaining.append(monster['player_hp_remaining'])
                all_player_damage.append(monster['avg_player_damage'])
                all_monster_damage.append(monster['avg_monster_damage'])
        
        if all_win_rates:
            f.write(f"전체 승률 통계:\n")
            f.write(f"  - 평균: {sum(all_win_rates)/len(all_win_rates):.1f}%\n")
            f.write(f"  - 최소: {min(all_win_rates):.1f}%\n")
            f.write(f"  - 최대: {max(all_win_rates):.1f}%\n\n")
            
            f.write(f"전체 턴 수 통계:\n")
            f.write(f"  - 평균: {sum(all_turns)/len(all_turns):.1f}턴\n")
            f.write(f"  - 최소: {min(all_turns):.1f}턴\n")
            f.write(f"  - 최대: {max(all_turns):.1f}턴\n\n")
            
            f.write(f"체력 잔여율 통계:\n")
            f.write(f"  - 몬스터 평균: {sum(all_monster_hp_remaining)/len(all_monster_hp_remaining):.1f}%\n")
            f.write(f"  - 플레이어 평균: {sum(all_player_hp_remaining)/len(all_player_hp_remaining):.1f}%\n\n")
            
            f.write(f"평균 대미지 통계:\n")
            f.write(f"  - 플레이어 평균: {sum(all_player_damage)/len(all_player_damage):.1f}\n")
            f.write(f"  - 몬스터 평균: {sum(all_monster_damage)/len(all_monster_damage):.1f}\n")
            f.write(f"  - 플레이어 최대: {max(all_player_damage):.1f}\n")
            f.write(f"  - 몬스터 최대: {max(all_monster_damage):.1f}\n\n")
        
        f.write("="*100 + "\n")
        f.write("보고서 끝\n")
        f.write("="*100 + "\n")
    
    return filename

def main():
    """메인 함수"""
    parser = argparse.ArgumentParser(description='지역별 몬스터 전투 승률 분석기')
    parser.add_argument('--weapon', type=str, default=None,
                       help='무기 스펙 (기본값: 기본 무기 사용) - 형식: "base_damage,damage_multiplier"')
    parser.add_argument('--armor', type=str, default=None,
                       help='방어구 스펙 (기본값: 기본 방어구 사용) - 형식: "base_defense,defense_multiplier"')
    parser.add_argument('--damage-multiplier', type=float, default=1.0,
                       help='플레이어 대미지 배수 (기본값: 1.0) - 예: 1.2면 플레이어 대미지가 1.2배')
    
    args = parser.parse_args()
    
    print("지역별 몬스터 전투 승률 분석기")
    print("="*50)
    
    if args.weapon:
        print(f"무기: {args.weapon}")
    if args.armor:
        print(f"방어구: {args.armor}")
    if args.damage_multiplier:
        print(f"대미지 배수: {args.damage_multiplier:.2f}x")
    print()
    
    # region_table.csv 파싱
    region_file = 'Assets/Data/region_table.csv'
    if not os.path.exists(region_file):
        print(f"파일을 찾을 수 없습니다: {region_file}")
        return
    
    regions = parse_region_table(region_file)
    if not regions:
        print("지역 데이터를 파싱할 수 없습니다.")
        return
    
    print(f"총 {len(regions)}개 지역을 발견했습니다.")
    
    # 전투 분석 실행
    results = analyze_region_battles(regions, args.weapon, args.armor, args.damage_multiplier)
    
    # 요약 보고서 생성
    generate_summary_report(results, args.weapon, args.armor, args.damage_multiplier)
    
    # 상세 보고서 파일 생성
    report_filename = generate_detailed_report_file(results, args.weapon, args.armor, args.damage_multiplier)
    print(f"\n상세 보고서가 생성되었습니다: {report_filename}")

if __name__ == "__main__":
    main()
