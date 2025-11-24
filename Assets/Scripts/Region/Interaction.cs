using LoopLegacy.Battle;
using LoopLegacy.Loader;
using LoopLegacy.Manager;
using UnityEngine;

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
        [SerializeField] private BoxCollider2D _trigger; // 트리거
        [SerializeField] private Signboard _signboard; // 표지판 오브젝트
        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        private void SetupLineRenderer()
        {
            if (Debug.IsDebug && (_trigger == null || _lineRenderer == null)) return;

            // LineRenderer 기본 설정
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.loop = true;
            _lineRenderer.startWidth = 0.05f;
            _lineRenderer.endWidth = 0.05f;
            _lineRenderer.startColor = Color.lightGreen;
            _lineRenderer.endColor = Color.lightGreen;

            // BoxCollider2D의 범위를 사각형으로 그리기
            Vector2 size = _trigger.size;
            Vector2 offset = _trigger.offset;
            
            // 사각형의 네 모서리 점들
            Vector3[] points = new Vector3[5]; // 5개 점으로 닫힌 사각형 만들기
            points[0] = new Vector3(offset.x - size.x / 2, offset.y - size.y / 2, 0);
            points[1] = new Vector3(offset.x + size.x / 2, offset.y - size.y / 2, 0);
            points[2] = new Vector3(offset.x + size.x / 2, offset.y + size.y / 2, 0);
            points[3] = new Vector3(offset.x - size.x / 2, offset.y + size.y / 2, 0);
            points[4] = points[0]; // 닫힌 사각형을 위해 첫 번째 점으로 돌아가기

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
                _signboard.DisplayText = Utils.GetNPCName(_npcType);
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

            if (Debug.IsDebug)
            {
                SetupLineRenderer();
            }
        }

        private void StartBattle()
        {
            MonsterData monsterData = TableManager.GetBoss(_monsterCode);
            var battleResult = BattleManager.Instance.StartBattle(monsterData, OnBattleEnd);
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

        private void AdjustPlayerPosition(Collider2D collision)
        {
            // 중앙으로부터 플레이어 바깥쪽으로 Trigger 범위 벗어날 때 까지 밀기
            // 플레이어 collider의 실제 중심 위치 계산 (오프셋 고려)
            Bounds playerBounds = collision.bounds;
            Vector2 playerColliderCenter = playerBounds.center;
            Vector2 playerTransformPos = collision.transform.position;
            Vector2 colliderOffset = playerColliderCenter - playerTransformPos;
            
            // 방향 계산 (collider 중심 기준)
            Vector2 triggerCenter = (Vector2)transform.position + _trigger.offset;
            Vector2 direction = (playerColliderCenter - triggerCenter).normalized;
            
            // 트리거의 경계 계산 (BoxCollider2D 기준)
            Vector2 triggerHalfSize = _trigger.size * 0.5f;
            var tangent = Mathf.Abs(direction.y / Mathf.Max(Mathf.Abs(direction.x), Mathf.Epsilon));
            
            // 플레이어 collider의 크기 계산
            Vector3 playerExtents = playerBounds.extents;
            const float MARGIN = 0.1f;
            
            if (tangent > triggerHalfSize.y / triggerHalfSize.x)
            {
                // y축 면 충돌 - y방향으로 밀기
                float moveDistanceY = triggerHalfSize.y + playerExtents.y + MARGIN;
                if (direction.y < 0)
                {
                    moveDistanceY = -moveDistanceY;
                }
                
                // collider를 목표 위치로 이동시키기 위해 transform 위치 계산
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
                
                // collider를 목표 위치로 이동시키기 위해 transform 위치 계산
                float targetColliderX = triggerCenter.x + moveDistanceX;
                collision.transform.position = new Vector2(targetColliderX - colliderOffset.x, playerTransformPos.y);
            }
        }

        private void OnDrawGizmos()
        {
            if (_trigger == null) return;

            // 에디터에서만 콜라이더 영역 표시
            Gizmos.color = Color.lightGreen;
            Gizmos.matrix = transform.localToWorldMatrix;

            // BoxCollider2D의 범위를 사각형으로 그리기
            Vector2 size = _trigger.size;
            Vector2 offset = _trigger.offset;
            
            // 사각형의 네 모서리 점들
            Vector2[] points = new Vector2[4];
            points[0] = new Vector2(offset.x - size.x / 2, offset.y - size.y / 2);
            points[1] = new Vector2(offset.x + size.x / 2, offset.y - size.y / 2);
            points[2] = new Vector2(offset.x + size.x / 2, offset.y + size.y / 2);
            points[3] = new Vector2(offset.x - size.x / 2, offset.y + size.y / 2);

            // 사각형의 각 변을 그리기
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 currentPoint = points[i];
                Vector2 nextPoint = points[(i + 1) % points.Length];
                Gizmos.DrawLine(currentPoint, nextPoint);
            }
        }
    }
}
