#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
region_table.csv에서 새로운 region_table_new.csv 생성
simule.py의 MonsterStatCalculator를 사용하여 새로운 경험치 공식으로 몬스터 스탯 계산
- 경험치 공식: level^2.3 * log(level + 1) * level * 2 / (10000 + level)
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

def update_region_table(input_file: str, output_file: str):
    """region_table.csv를 읽어 새로운 파일로 생성"""
    calculator = MonsterStatCalculator()
    
    try:
        with open(input_file, 'r', encoding='utf-8') as infile:
            lines = infile.readlines()
        
        updated_lines = []
        i = 0
        
        while i < len(lines):
            line = lines[i].strip()
            
            # 헤더나 빈 줄은 그대로 유지
            if not line or line.startswith('#') or line.startswith('id,') or line.startswith('level,'):
                updated_lines.append(lines[i])
                i += 1
                continue
            
            # CSV 파싱
            parts = line.split(',')
            if len(parts) < 5:
                updated_lines.append(lines[i])
                i += 1
                continue
            
            # 몬스터 데이터 행인지 확인 (5개 이상 컬럼)
            try:
                level = int(parts[0])
                hp = int(parts[1])
                atk = int(parts[2])
                gold = int(parts[3])
                exp = int(parts[4])
                
                # 새로운 몬스터 스탯 계산
                monster_stats = calculator.calculate_stats(level)
                
                # HP, ATK만 업데이트, gold와 exp는 원본 유지
                new_line = f"{level},{monster_stats.hp},{monster_stats.atk},{monster_stats.gold},{exp}"
                
                # 나머지 컬럼들 추가 (bp, drops, actions 등)
                if len(parts) > 5:
                    new_line += ',' + ','.join(parts[5:])
                
                updated_lines.append(new_line + '\n')
                
            except (ValueError, IndexError):
                # 몬스터 데이터가 아닌 행은 그대로 유지
                updated_lines.append(lines[i])
            
            i += 1
        
        # 업데이트된 파일 저장
        with open(output_file, 'w', encoding='utf-8', newline='') as outfile:
            outfile.writelines(updated_lines)
        
        print(f"새 파일 생성 완료: {output_file}")
        print(f"원본 파일({input_file})은 변경되지 않았습니다.")
        
    except Exception as e:
        print(f"오류 발생: {e}")
        print("파일 생성에 실패했습니다.")

def main():
    """메인 함수"""
    print("region_table.csv 업데이트 스크립트")
    print("gold_growth: 5 → 3으로 변경된 값 적용")
    print("="*50)
    
    input_file = 'Assets/Data/region_table.csv'
    output_file = 'Assets/Data/region_table_new.csv'
    
    if not os.path.exists(input_file):
        print(f"파일을 찾을 수 없습니다: {input_file}")
        return
    
    print(f"입력 파일: {input_file}")
    print(f"출력 파일: {output_file}")
    print()
    
    # 자동 실행 (사용자 확인 생략)
    print("gold_growth를 5에서 3으로 변경하여 파일을 생성합니다...")
    print("HP, ATK, Gold만 업데이트되며, EXP는 원본 값을 유지합니다.")
    print()
    
    update_region_table(input_file, output_file)

if __name__ == "__main__":
    main()
