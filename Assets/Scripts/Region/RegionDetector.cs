using System.Collections.Generic;
using R3;

namespace LoopLegacy.Region
{
    public class RegionDetector
    {
        private List<Region> _overlappingRegions = new List<Region>();

        public ReactiveProperty<Region> LastRegion { get; private set; }  // 마지막으로 있던 지역

        public RegionDetector()
        {
            LastRegion = new ReactiveProperty<Region>(null);
        }

        public void OnEnterRegion(Region region)
        {
            if (!_overlappingRegions.Contains(region))
            {
                _overlappingRegions.Add(region);
                UpdateLastRegion();
            }
        }

        public void OnExitRegion(Region region)
        {
            if (_overlappingRegions.Contains(region))
            {
                _overlappingRegions.Remove(region);
                UpdateLastRegion();
            }
        }

        private void UpdateLastRegion()
        {
            // 가장 최근에 들어온 지역을 마지막 지역으로 설정
            if (_overlappingRegions.Count > 0)
            {
                LastRegion.Value = _overlappingRegions[_overlappingRegions.Count - 1];
            }
            else
            {
                LastRegion.Value = null;
            }
        }
    }
} 