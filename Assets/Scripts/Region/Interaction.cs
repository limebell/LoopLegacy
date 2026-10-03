using LoopLegacy.Battle;
using LoopLegacy.Loader;
using LoopLegacy.Manager;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace LoopLegacy.Region
{
    public enum InteractionType
    {
        Move,
        NPC,
        Boss,
    }

    public class Interaction : MonoBehaviour
    {
        [SerializeField] private InteractionType _type;
        [SerializeField] private string _mapCode;
        [SerializeField] private Vector2 _position;
        [SerializeField] private NPCType _npcType;
        [SerializeField] private string _monsterCode;
        [SerializeField] private BoxCollider2D _trigger;
        [SerializeField] private Signboard _signboard;
        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        private void OnDestroy()
        {
            if (_type == InteractionType.NPC)
            {
                LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            }
        }

        // Debug용 충돌 범위 표시
        private void SetupLineRenderer()
        {
            if (Debug.isDebugBuild && (_trigger == null || _lineRenderer == null)) return;

            _lineRenderer.useWorldSpace = false;
            _lineRenderer.loop = true;
            _lineRenderer.startWidth = 0.05f;
            _lineRenderer.endWidth = 0.05f;
            _lineRenderer.startColor = Color.lightGreen;
            _lineRenderer.endColor = Color.lightGreen;

            Vector2 size = _trigger.size;
            Vector2 offset = _trigger.offset;
            
            Vector3[] points = new Vector3[5];
            points[0] = new Vector3(offset.x - size.x / 2, offset.y - size.y / 2, 0);
            points[1] = new Vector3(offset.x + size.x / 2, offset.y - size.y / 2, 0);
            points[2] = new Vector3(offset.x + size.x / 2, offset.y + size.y / 2, 0);
            points[3] = new Vector3(offset.x - size.x / 2, offset.y + size.y / 2, 0);
            points[4] = points[0];

            _lineRenderer.positionCount = points.Length;
            for (int i = 0; i < points.Length; i++)
            {
                _lineRenderer.SetPosition(i, points[i]);
            }
        }

        private void Start()
        {
            if (_type == InteractionType.NPC)
            {
                _signboard.gameObject.SetActive(true);
                UpdateSignboardText();
                
                LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            }
            else
            {
                _signboard.gameObject.SetActive(false);
                
                if (_type == InteractionType.Boss &&
                    GameManager.Instance.GameState.DefeatedBosses.Contains(_monsterCode))
                {
                    gameObject.SetActive(false);
                }
            }

            if (Debug.isDebugBuild)
            {
                SetupLineRenderer();
            }
        }

        private void OnLocaleChanged(Locale locale)
        {
            UpdateSignboardText();
        }

        private void UpdateSignboardText()
        {
            if (_type == InteractionType.NPC && _signboard != null)
            {
                _signboard.DisplayText = Utils.GetNPCName(_npcType);
            }
        }

        private void StartBattle()
        {
            MonsterData monsterData = TableManager.GetBoss(_monsterCode);
            Region region = GameManager.Instance.RegionDetector.LastRegion.Value;
            var regionEffect = GameManager.Instance.GameState.GetRegionEffect(region.GetRegionEntry().code);
            var battleResult = BattleManager.Instance.StartBattle(monsterData, regionEffect.Type, OnBattleEnd);
        }

        private void OnBattleEnd(BattleContext result)
        {
            if (result.IsVictory)
            {
                GameManager.Instance.GameState.DefeatedBosses.Add(_monsterCode);
                gameObject.SetActive(false);
                GameManager.Instance.Save();
            }
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.CompareTag("Player")) return;
            if (GameManager.Instance is not null && GameManager.Instance.IsMovingMap.Value) return;

            if (collision.IsTouching(_trigger))
            {
                if (_type != InteractionType.Move)
                {
                    AdjustPlayerPosition(collision);
                }

                switch (_type)
                {
                    case InteractionType.Move:
                        GameManager.Instance.MoveToMap(_mapCode, _position);
                        break;
                    case InteractionType.NPC:
                        TerritoryManager.Instance.Interact(_npcType);
                        break;
                    case InteractionType.Boss:
                        StartBattle();
                        break;
                }
            }
        }

        // 플레이어가 Interaction 영역을 벗어나는 가장 가까운 위치로 강제 이동
        private void AdjustPlayerPosition(Collider2D collision)
        {
            Bounds playerBounds = collision.bounds;
            Vector2 playerColliderCenter = playerBounds.center;
            Vector2 playerTransformPos = collision.transform.position;
            Vector2 colliderOffset = playerColliderCenter - playerTransformPos;
            
            Vector2 triggerCenter = (Vector2)transform.position + _trigger.offset;
            Vector2 direction = (playerColliderCenter - triggerCenter).normalized;
            
            Vector2 triggerHalfSize = _trigger.size * 0.5f;
            var tangent = Mathf.Abs(direction.y / Mathf.Max(Mathf.Abs(direction.x), Mathf.Epsilon));
            
            Vector3 playerExtents = playerBounds.extents;
            const float MARGIN = 0.3f;
            
            if (tangent > triggerHalfSize.y / triggerHalfSize.x)
            {
                // y축 면 충돌 - y방향으로 밀기
                float moveDistanceY = triggerHalfSize.y + playerExtents.y + MARGIN;
                if (direction.y < 0)
                {
                    moveDistanceY = -moveDistanceY;
                }
                
                float targetColliderY = triggerCenter.y + moveDistanceY;
                collision.transform.position = new Vector2(playerTransformPos.x, targetColliderY - colliderOffset.y);
            }
            else
            {
                // x축 면 충돌 - x방향으로 밀기
                float moveDistanceX = triggerHalfSize.x + playerExtents.x + MARGIN;
                if (direction.x < 0)
                {
                    moveDistanceX = -moveDistanceX;
                }
                
                float targetColliderX = triggerCenter.x + moveDistanceX;
                collision.transform.position = new Vector2(targetColliderX - colliderOffset.x, playerTransformPos.y);
            }
        }

        private void OnDrawGizmos()
        {
            if (_trigger == null) return;

            Gizmos.color = Color.lightGreen;
            Gizmos.matrix = transform.localToWorldMatrix;

            Vector2 size = _trigger.size;
            Vector2 offset = _trigger.offset;
            
            Vector2[] points = new Vector2[4];
            points[0] = new Vector2(offset.x - size.x / 2, offset.y - size.y / 2);
            points[1] = new Vector2(offset.x + size.x / 2, offset.y - size.y / 2);
            points[2] = new Vector2(offset.x + size.x / 2, offset.y + size.y / 2);
            points[3] = new Vector2(offset.x - size.x / 2, offset.y + size.y / 2);

            for (int i = 0; i < points.Length; i++)
            {
                Vector2 currentPoint = points[i];
                Vector2 nextPoint = points[(i + 1) % points.Length];
                Gizmos.DrawLine(currentPoint, nextPoint);
            }
        }
    }
}
