using LoopLegacy.Loader;
using LoopLegacy.Manager;
using LoopLegacy.State;
using R3;
using TMPro;
using UnityEngine;

namespace LoopLegacy.Region
{
    public class Region : MonoBehaviour
    {
        [SerializeField] private string _regionCode;
        private float _lineWidth = 0.08f;

        private TextMeshPro _levelText;
        private PolygonCollider2D _collider;
        private LineRenderer _lineRenderer;
        private RegionEntry _regionEntry;

        private void Awake()
        {
            _levelText = GetComponentInChildren<TextMeshPro>();
            _collider = GetComponent<PolygonCollider2D>();
            _lineRenderer = GetComponent<LineRenderer>();
            if (_collider == null)
            {
                Debug.LogError("[Region] Region does not have a PolygonCollider2D");
            }

            if (_lineRenderer == null)
            {
                Debug.LogError("[Region] Region does not have a LineRenderer");
            }

            SetupLineRenderer();
        }

        private void SetupLineRenderer()
        {
            if (_collider == null) return;

            // LineRenderer 기본 설정
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.loop = true;
            _lineRenderer.startWidth = _lineWidth;
            _lineRenderer.endWidth = _lineWidth;

            // 폴리곤 콜라이더의 점들을 LineRenderer에 적용
            Vector2[] points = _collider.points;
            _lineRenderer.positionCount = points.Length;
            
            for (int i = 0; i < points.Length; i++)
            {
                _lineRenderer.SetPosition(i, new Vector3(points[i].x, points[i].y, 0));
            }

            // 내부 영역을 위한 스프라이트 렌더러 설정
            /*var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            spriteRenderer.color = new Color(_outlineColor.r, _outlineColor.g, _outlineColor.b, _innerAlpha);*/
        }

        private void Start()
        {
            _regionEntry = TableManager.GetRegion(_regionCode);
            _levelText.text = "Lv. " + _regionEntry.label.ToString();

            Color color = GetColor(GameManager.Instance.GameState.PlayerStats.Level.Value);
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;

            var d = Disposable.CreateBuilder();
            GameManager.Instance.GameState.PlayerStats.Level.Subscribe(level =>
            {
                var color1 = GetColor(level);
                _lineRenderer.startColor = color1;
                _lineRenderer.endColor = color1;
            }).AddTo(ref d);
            d.RegisterTo(this.destroyCancellationToken);
        }

        public int GetRelativeLevel(int level)
        {
            int reqLevel = 0;
            if (int.TryParse(_regionEntry.label, out int result))
            {
                reqLevel = result;
            }
            else
            {
                if (_regionEntry.label.Contains("-"))
                {
                    var parts = _regionEntry.label.Split('-');
                    reqLevel = int.Parse(parts[0]);
                }
                else
                {
                    reqLevel = int.MaxValue;
                }
            }

            if (reqLevel < level * 0.5f)
                return -2;
            if (reqLevel < level * 0.8f)
                return -1;
            if (reqLevel < level * 1.2f)
                return 0;
            if (reqLevel < level * 1.5f)
                return 1;
            return 2;
        }

        public Color GetColor(int level)
        {
            switch (GetRelativeLevel(level))
            {
                case -2:
                    return Color.gray;
                case -1:
                    return Color.darkGray;
                case 0:
                    return Color.white;
                case 1:
                    return Color.orangeRed;
                case 2:
                    return Color.red;
                default:
                    return Color.white;
            }
        }

        public RegionEntry GetRegionEntry()
        {
            return _regionEntry;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // RegionDetector에 현재 지역 정보 전달
            GameManager.Instance?.RegionDetector?.OnEnterRegion(this);
            PersistentGameState.Instance?.CodexState?.VisitRegion(_regionCode);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // RegionDetector에 지역 이탈 정보 전달
            GameManager.Instance?.RegionDetector?.OnExitRegion(this);
        }

        private void OnDrawGizmos()
        {
            var collider = GetComponent<PolygonCollider2D>();
            if (collider == null) return;

            // 에디터에서만 콜라이더 영역 표시
            Gizmos.color = Color.hotPink;
            Gizmos.matrix = transform.localToWorldMatrix;

            // 폴리곤 콜라이더의 각 점을 그리기
            Vector2[] points = collider.points;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 currentPoint = points[i];
                Vector2 nextPoint = points[(i + 1) % points.Length];
                Gizmos.DrawLine(currentPoint, nextPoint);
            }
        }
    }
}