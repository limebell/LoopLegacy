using LoopLegacy.Battle;
using LoopLegacy.Loader;
using LoopLegacy.Manager;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using R3;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace LoopLegacy.UI.Controller
{
    public class BattleController : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioClip _playerAttackSound;
        [SerializeField] private AudioClip _enemyAttackSound;
        [SerializeField] private AudioClip _battleBGM;

        [Header("Battle Speed Control")]
        [SerializeField] private Button[] _speedButtons;

        [Header("Battle UI")]
        [SerializeField] private HealthBar _enemyHealthBar;
        [SerializeField] private HealthBar _playerHealthBar;
        [SerializeField] private Image _enemyImage;
        [SerializeField] private Image _playerImage;
        [SerializeField] private GameObject _damageTextPrefab;
        [SerializeField] private GameObject[] _hitEffectPrefabs;
        [SerializeField] private TextMeshProUGUI _battleLog;
        [SerializeField] private Button _giveUpButton;

        private Queue<string> _battleLogQueue = new Queue<string>();
        private const int MAX_BATTLE_LOG_LINES = 10;
        private BattleSimulator _currentBattleSimulator;

        // Spawned Objects
        private List<GameObject> _spawnedObjects = new List<GameObject>();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnBattleElementClicked);
            if (_speedButtons.Length == 3)
            {
                _speedButtons[0].onClick.AddListener(() => OptionState.Instance.CombatSpeed.Value = 0);
                _speedButtons[1].onClick.AddListener(() => OptionState.Instance.CombatSpeed.Value = 1);
                _speedButtons[2].onClick.AddListener(() => OptionState.Instance.CombatSpeed.Value = 2);
                SubscribeToOptionState();
            }
            else
            {
                Debug.LogError("BattleHUDController: Speed buttons length is not 3");
            }
            _giveUpButton.onClick.AddListener(OnGiveUpButtonClicked);
        }

        private void SubscribeToOptionState()
        {
            var d = Disposable.CreateBuilder();
            OptionState.Instance.CombatSpeed.Subscribe(value => {
                for (int i = 0; i < _speedButtons.Length; i++)
                {
                    _speedButtons[i].interactable = i != value;
                }
            }).AddTo(ref d);
            d.RegisterTo(this.destroyCancellationToken);
        }

        public void PrepareBattle(MonsterData monsterData)
        {
            UpdateEnemyImage(monsterData.sprite);
            ShowBattleUI();
            string monsterName = new LocalizedString {
                TableReference = "Monster",
                TableEntryReference = monsterData.code }.GetLocalizedString();
            string encounterLog = Utils.GetUIString(
                "battle-log_encounter",
                new object[] { monsterName });
            AddBattleLog(encounterLog);
            
            // 현재 맵 배경음악 상태 저장
            AudioManager.Instance.SaveBGMState();
            
            // 배틀 BGM 재생
            AudioManager.Instance.PlayBGM(_battleBGM, true);
        }

        private void OnBattleElementClicked()
        {
            BattleManager.Instance.OnClickBattleUI();
        }

        private void OnGiveUpButtonClicked()
        {
            ConfirmationController.Instance.ShowConfirmation(
                Utils.GetUIString("battle_giveup-confirmation"),
                onConfirm: () => {
                    BattleManager.Instance.GiveupBattle();
                });
        }

        public void AddBattleLog(string message)
        {
            _battleLogQueue.Enqueue(message);
            
            // 최대 라인 수를 초과하면 가장 오래된 메시지 제거
            while (_battleLogQueue.Count > MAX_BATTLE_LOG_LINES)
            {
                _battleLogQueue.Dequeue();
            }

            // 모든 메시지를 개행문자로 연결하여 표시
            _battleLog.text = string.Join("\n", _battleLogQueue);
        }

        public void ClearBattleLog()
        {
            _battleLogQueue.Clear();
            _battleLog.text = string.Empty;
        }

        public void ShowBattleUI()
        {
            foreach (var obj in _spawnedObjects)
            {
                Destroy(obj);
            }
            _spawnedObjects.Clear();

            gameObject.SetActive(true);
            ClearBattleLog();
            
            _enemyImage.GetComponent<Animator>().SetTrigger("Spawn");
        }

        public void HideBattleUI()
        {
            gameObject.SetActive(false);
            AudioManager.Instance.StopBGM();
        }

        public void UpdateEnemyImage(Sprite sprite)
        {
            if (sprite != null)
            {
                // 이전 이미지 제거
                _enemyImage.sprite = sprite;
                const int minImageSize = 50;
                const int maxImageSize = 300;
                var imageWidth = sprite.rect.width * 2;
                var imageHeight = sprite.rect.height * 2;
                var imageRatio = imageWidth / imageHeight;
                if (imageWidth > maxImageSize || imageHeight > maxImageSize)
                {
                    if (imageRatio > 1)
                    {
                        _enemyImage.rectTransform.sizeDelta = new Vector2(maxImageSize / imageRatio, maxImageSize);
                    }
                    else
                    {
                        _enemyImage.rectTransform.sizeDelta = new Vector2(maxImageSize, maxImageSize * imageRatio);
                    }
                }
                else if (imageWidth < minImageSize || imageHeight < minImageSize)
                {
                    if (imageRatio > 1)
                    {
                        _enemyImage.rectTransform.sizeDelta = new Vector2(minImageSize * imageRatio, minImageSize);
                    }
                    else
                    {
                        _enemyImage.rectTransform.sizeDelta = new Vector2(minImageSize, minImageSize / imageRatio);
                    }
                }
                else
                {
                    _enemyImage.rectTransform.sizeDelta = new Vector2(imageWidth, imageHeight);
                }

                _enemyImage.rectTransform.anchoredPosition = new Vector2(0, 0);
            }
            else
            {
                _enemyImage.sprite = null;
            }
        }

        public void SetBattleSimulator(BattleSimulator simulator)
        {
            // 이전 구독 해제
            if (_currentBattleSimulator != null)
            {
                // 이전 구독은 AddTo(this)로 자동 해제됨
            }

            _currentBattleSimulator = simulator;

            // 새로운 구독 설정
            if (simulator != null)
            {
                var d = Disposable.CreateBuilder();
                simulator.CurrentPlayerHP
                    .Subscribe(hp => 
                    {
                        _playerHealthBar.UpdateHP(hp, simulator.MaxPlayerHP);
                    })
                    .AddTo(ref d);

                simulator.CurrentMonsterHP
                    .Subscribe(hp => 
                    {
                        _enemyHealthBar.UpdateHP(hp, simulator.MaxMonsterHP);
                    })
                    .AddTo(ref d);
                d.RegisterTo(this.destroyCancellationToken);
            }
        }

        public void RenderExecution(string monsterName)
        {
            AddBattleLog(Utils.GetUIString("battle-log_execution", new object[] { monsterName }));
            AudioManager.Instance.PlaySFXSound(_playerAttackSound);
            StartCoroutine(SummonHitEffectCoroutine(_playerImage.transform));
            StartCoroutine(SummonDamageTextCoroutine(
                0,
                DamageTextType.Execution,
                0,
                _enemyImage.transform.parent));
        }

        public void RenderHeal(int amount)
        {
            AddBattleLog(Utils.GetUIString("battle-log_heal", new object[] { amount }));
            StartCoroutine(SummonHitEffectCoroutine(_playerImage.transform));
            StartCoroutine(SummonDamageTextCoroutine(
                amount,
                DamageTextType.Heal,
                0,
                _playerImage.transform.parent));
        }

        public void RenderPlayerAttack(int damage, string monsterName, bool isCrit, int combo, bool isReflect)
        {
            _playerImage.GetComponent<Animator>().SetTrigger("Attack");

            string criticalText = Utils.GetUIString("battle-log_critical");
            string comboTextString = Utils.GetUIString("battle-log_combo", new object[] { combo });
            string comboText = combo > 1 ? $" ({comboTextString})" : "";
            string damageText;
            if (isReflect)
            {
                damageText = Utils.GetUIString("battle-log_player-attack-reflect", new object[] { monsterName, damage });
            }
            else
            {
                damageText = Utils.GetUIString("battle-log_player-attack", new object[] { monsterName, damage });
            }
            AddBattleLog($"{damageText} {comboText} {(isCrit ? $"{criticalText}" : "")}");
            AudioManager.Instance.PlaySFXSound(_playerAttackSound);
            StartCoroutine(SummonHitEffectCoroutine(
                _enemyImage.transform.parent,
                new Vector2(0, _enemyImage.rectTransform.sizeDelta.y / 2)));
            StartCoroutine(SummonDamageTextCoroutine(
                damage,
                isReflect ? DamageTextType.Reflect : isCrit ? DamageTextType.Critical : DamageTextType.Normal,
                combo,
                _enemyImage.transform.parent,
                new Vector2(0, _enemyImage.rectTransform.sizeDelta.y / 2)));
        }

        public void RenderEnemyAttack(int damage, bool isEvaded, string monsterName)
        {
            string damageText;
            if (isEvaded)
            {
                damageText = Utils.GetUIString("battle-log_monster-attack-evaded", new object[] { monsterName });
            }
            else
            {
                damageText = Utils.GetUIString("battle-log_monster-attack", new object[] { monsterName, damage });
            }

            AddBattleLog($"{damageText}");
            AudioManager.Instance.PlaySFXSound(_enemyAttackSound);
            StartCoroutine(SummonHitEffectCoroutine(_playerImage.transform));
            StartCoroutine(SummonDamageTextCoroutine(
                damage,
                isEvaded ? DamageTextType.Evasion : DamageTextType.Normal,
                0,
                _playerImage.transform.parent));
        }

        private IEnumerator SummonDamageTextCoroutine(
            int damage,
            DamageTextType type,
            int combo,
            Transform target,
            Vector2 offset = default(Vector2))
        {
            var damageText = Instantiate(_damageTextPrefab, target);
            damageText.GetComponent<DamageText>().SetDamage(damage, type, combo);
            
            // 이미지의 자식으로 직접 추가
            var randX = Random.Range(-30.0f, 30.0f);
            var randY = Random.Range(-5.0f, 5.0f);
            float translateX = randX;
            float translateY = 30 + randY;
            damageText.gameObject.transform.localPosition = new Vector3(translateX, translateY, 0) + new Vector3(offset.x, offset.y, 0);
            _spawnedObjects.Add(damageText);
            yield return new WaitForSeconds(0.8f);
            Destroy(damageText);
            _spawnedObjects.Remove(damageText);
        }

        private IEnumerator SummonHitEffectCoroutine(Transform target, Vector2 offset = default(Vector2))
        {
            var hitEffect = _hitEffectPrefabs[Random.Range(0, _hitEffectPrefabs.Length)];
            var prefab = Instantiate(hitEffect, target);
            prefab.transform.localPosition += new Vector3(offset.x, offset.y, 0);
            _spawnedObjects.Add(prefab);
            yield return new WaitForSeconds(1.0f);
            Destroy(prefab);
            _spawnedObjects.Remove(prefab);
        }

        public void RenderVictory()
        {
            AddBattleLog(Utils.GetUIString("battle-log_victory"));
            _enemyImage.GetComponent<Animator>().SetTrigger("Dead");
        }

        public void RenderDefeat()
        {
            _playerImage.GetComponent<Animator>().SetTrigger("Dead");
            AddBattleLog(Utils.GetUIString("battle-log_defeat"));
        }
    }
}
