using UnityEngine;

namespace LoopLegacy.Region
{
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Parallax Settings")]
        [SerializeField] [Range(0f, 1f)] private float _horizontalParallaxMultiplier = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float _verticalParallaxMultiplier = 0.5f;
        
        [Header("Infinite Scrolling (Optional)")]
        [SerializeField] private bool _infiniteHorizontal = false;
        [SerializeField] private bool _infiniteVertical = false;
        
        private Transform _cameraTransform;
        private Vector3 _lastCameraPosition;
        private Vector3 _initialBackgroundPosition;
        private Vector3 _initialCameraPosition;
        private float _textureUnitSizeX;
        private float _textureUnitSizeY;

        void Start()
        {
            _cameraTransform = Camera.main.transform;
            _initialCameraPosition = _cameraTransform.position;
            _lastCameraPosition = _cameraTransform.position;
            _initialBackgroundPosition = transform.position;
            
            // 카메라가 0,0이 아닌 위치에서 시작할 경우 배경 위치 초기화
            Vector3 cameraOffset = _initialCameraPosition;
            float offsetX = cameraOffset.x * _horizontalParallaxMultiplier;
            float offsetY = cameraOffset.y * _verticalParallaxMultiplier;
            transform.position = _initialBackgroundPosition + new Vector3(offsetX, offsetY, 0f);
            
            // 무한 스크롤을 위한 텍스처 크기 계산
            Sprite sprite = GetComponent<SpriteRenderer>()?.sprite;
            if (sprite != null)
            {
                Texture2D texture = sprite.texture;
                _textureUnitSizeX = texture.width / sprite.pixelsPerUnit;
                _textureUnitSizeY = texture.height / sprite.pixelsPerUnit;
            }
        }

        void LateUpdate()
        {
            // 카메라의 이동 거리 계산
            Vector3 deltaMovement = _cameraTransform.position - _lastCameraPosition;
            
            // 패럴랙스 효과 적용
            float moveX = deltaMovement.x * _horizontalParallaxMultiplier;
            float moveY = deltaMovement.y * _verticalParallaxMultiplier;
            
            transform.position += new Vector3(moveX, moveY, 0f);
            
            // 무한 스크롤 처리 (선택사항)
            if (_infiniteHorizontal)
            {
                if (Mathf.Abs(_cameraTransform.position.x - transform.position.x) >= _textureUnitSizeX)
                {
                    float offsetPositionX = (_cameraTransform.position.x - transform.position.x) % _textureUnitSizeX;
                    transform.position = new Vector3(_cameraTransform.position.x + offsetPositionX, transform.position.y, transform.position.z);
                }
            }
            
            if (_infiniteVertical)
            {
                if (Mathf.Abs(_cameraTransform.position.y - transform.position.y) >= _textureUnitSizeY)
                {
                    float offsetPositionY = (_cameraTransform.position.y - transform.position.y) % _textureUnitSizeY;
                    transform.position = new Vector3(transform.position.x, _cameraTransform.position.y + offsetPositionY, transform.position.z);
                }
            }
            
            _lastCameraPosition = _cameraTransform.position;
        }
    }
}