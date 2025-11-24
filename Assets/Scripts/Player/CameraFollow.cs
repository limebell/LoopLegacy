using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LoopLegacy.Player
{
    public class CameraFollow : MonoBehaviour
    {
        public static CameraFollow Instance;
        [SerializeField] private Transform _target; // 따라갈 대상(플레이어)
        [SerializeField] private GameObject _fog;
        private Camera _camera;
        private PixelPerfectCamera _ppc;
        private Rigidbody2D _rigidbody2d;
        private BoxCollider2D _boxCollider;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            _boxCollider = gameObject.AddComponent<BoxCollider2D>();
            _camera = Camera.main;
            _boxCollider.isTrigger = false;

            _rigidbody2d = GetComponent<Rigidbody2D>();
            _ppc = GetComponent<PixelPerfectCamera>();
        }

        void Update()
        {
            // 카메라 투사 크기만큼 BoxCollider2D 조정
            float worldHeight = 2f * _camera.orthographicSize;
            float worldWidth  = worldHeight * _camera.aspect;
            _boxCollider.size = new Vector2(worldWidth, worldHeight);
        }

        void FixedUpdate()
        {
            if (_target != null)
            {
                // 카메라의 위치를 플레이어쪽으로 이동
                Vector3 position = _target.position;
                Vector3 moveVector = position - transform.position;
                if (moveVector.magnitude < 1.0f)
                {
                    return;
                }
                
                _rigidbody2d.MovePosition(transform.position + moveVector * 0.2f);
            }
        }
/*
        void LateUpdate()
        {
            transform.position = _ppc.RoundToPixel(transform.position);
        }
*/
        public void SetPosition(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        public void SetFogActive(bool active)
        {
            _fog.SetActive(active);
        }
    }
} 