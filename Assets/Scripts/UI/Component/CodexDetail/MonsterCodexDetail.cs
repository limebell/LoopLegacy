using System.Linq;
using System.Numerics;
using LoopLegacy.Loader;
using LoopLegacy.Manager;
using LoopLegacy.State;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopLegacy.UI.Component.CodexDetail
{
    public class MonsterCodexDetail : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _killCountText;
        [SerializeField] private TMP_Text _hpText;
        [SerializeField] private TMP_Text _atkText;
        [SerializeField] private TMP_Text _defText;
        [SerializeField] private TMP_Text _goldText;
        [SerializeField] private TMP_Text _expText;
        [SerializeField] private TMP_Text _specialEffectText;
        [SerializeField] private GameObject _dropTitleText;
        [SerializeField] private GameObject _dropsContainer;
        [SerializeField] private GameObject _dropElementPrefab;

        public void UpdateCodexDetail(MonsterData monsterData)
        {
            // DestroyImmediate를 사용하여 즉시 삭제 (레이아웃 계산에 영향을 주지 않도록)
            for (int i = _dropsContainer.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(_dropsContainer.transform.GetChild(i).gameObject);
            }
            
            if (monsterData == null)
            {
                _nameText.text = "???";
                _levelText.text = "???";
                _killCountText.text = "???";
                _hpText.text = "???";
                _atkText.text = "???";
                _defText.text = "???";
                _goldText.text = "???";
                _expText.text = "???";
                _specialEffectText.text = "";
                _dropTitleText.SetActive(false);
                return;
            }
            
            var killCount = PersistentGameState.Instance.CodexState.GetMobKillCount(monsterData.code);
            string killCountString = Utils.GetUIString("kill-count");
            _nameText.text = Utils.GetMonsterName(monsterData);
            _levelText.text = monsterData.level.ToString("n0");
            _killCountText.text = killCount.ToString("n0");
            _hpText.text = monsterData.hp.ToString("n0");
            _atkText.text = monsterData.atk.ToString("n0");
            _defText.text = monsterData.def.ToString("n0");
            _goldText.text = monsterData.gold.ToString("n0");
            _expText.text = monsterData.exp.ToString("n0");
            _dropTitleText.SetActive(monsterData.drops.Any());
            
            // 모든 drop element를 먼저 생성
            foreach (var drop in monsterData.drops)
            {
                var dropElement = Instantiate(_dropElementPrefab, _dropsContainer.transform);
                dropElement.GetComponent<CodexDropElement>().UpdateElement(drop);
            }

            if (LibraryManager.IsLibraryUnlocked())
            {
                _specialEffectText.text = $"\n{Utils.GetUIString("library_monster-killcount-effect")}: DROP +{(int)(LibraryManager.GetMonsterDropMultiplier(killCount) * 100 - 100)}%";
            }
            else
            {
                _specialEffectText.text = "";
            }

            // 모든 요소가 생성된 후 한 번에 레이아웃 강제 업데이트
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_dropsContainer.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();
        }
    }
}