#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
boss_table.csv에서 새로운 boss_table_new.csv 생성
simule.py의 MonsterStatCalculator를 사용하여 actualLevel 기준으로 보스 스탯 계산
- actualLevel을 기준으로 HP, ATK, Gold, EXP 계산
- 원본 파일은 변경하지 않음
"""

import csv
import math
import os
import sys
from typing import List, Dict, Tuple

# simule.py에서 MonsterStatCalculator 가져오기
sys.path.append(os.path.dirname(os.path.abspath(__file__)))
from simule import MonsterStatCalculator

def update_boss_table(input_file: str, output_file: str):
    """boss_table.csv를 읽어 새로운 파일로 생성"""
    calculator = MonsterStatCalculator()
    
    try:
        with open(input_file, 'r', encoding='utf-8') as infile:
            reader = csv.DictReader(infile)
            fieldnames = reader.fieldnames
            
            rows = []
            update_count = 0
            
            for row in reader:
                try:
                    # statLevel을 기준으로 스탯 계산
                    stat_level = int(row['statLevel'])
                    
                    # 새로운 몬스터 스탯 계산
                    monster_stats = calculator.calculate_stats(stat_level)
                    
                    # HP, ATK, DEF, Gold, EXP 업데이트
                    old_hp = row['hp']
                    old_atk = row['atk']
                    old_def = row.get('def', '0')
                    old_gold = row['gold']
                    old_exp = row['exp']
                    
                    # 보스는 골드를 2배로 설정
                    boss_gold = monster_stats.gold * 2
                    
                    row['hp'] = str(monster_stats.hp)
                    row['atk'] = str(monster_stats.atk)
                    row['def'] = str(monster_stats.defense)
                    row['gold'] = str(boss_gold)
                    row['exp'] = str(monster_stats.exp)
                    
                    update_count += 1
                    
                except (ValueError, KeyError) as e:
                    print(f"행 처리 중 오류 발생: {e}")
                    continue
                
                rows.append(row)
        
        # 업데이트된 파일 저장
        with open(output_file, 'w', encoding='utf-8', newline='') as outfile:
            writer = csv.DictWriter(outfile, fieldnames=fieldnames)
            writer.writeheader()
            writer.writerows(rows)
        
        print("="*60)
        print(f"새 파일 생성 완료: {output_file}")
        print(f"총 {update_count}개 보스 업데이트됨")
        print(f"원본 파일({input_file})은 변경되지 않았습니다.")
        
    except Exception as e:
        print(f"오류 발생: {e}")
        print("파일 생성에 실패했습니다.")
        import traceback
        traceback.print_exc()

def main():
    """메인 함수"""
    print("boss_table.csv 업데이트 스크립트")
    print("statLevel 기준으로 보스 스탯 재계산")
    print("="*60)
    
    input_file = 'Assets/Data/boss_table.csv'
    output_file = 'Assets/Data/boss_table_new.csv'
    
    if not os.path.exists(input_file):
        print(f"파일을 찾을 수 없습니다: {input_file}")
        return
    
    print(f"입력 파일: {input_file}")
    print(f"출력 파일: {output_file}")
    print()
    
    print("statLevel을 기준으로 HP, ATK, DEF, Gold, EXP를 업데이트합니다...")
    print("BP, Drops, Actions는 원본 값을 유지합니다.")
    print()
    
    update_boss_table(input_file, output_file)

if __name__ == "__main__":
    main()

