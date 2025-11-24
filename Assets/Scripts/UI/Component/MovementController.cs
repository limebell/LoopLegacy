using LoopLegacy.Player;
using LoopLegacy.State;
using R3;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace LoopLegacy.UI.Component
{
    public class MovementController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private GameObject _joystick;
        [SerializeField]
        private GameObject _handle;
        [SerializeField]
        private Vector2 _staticPosition;
        [SerializeField]
        private float _range;

        private float _minTouchRange = 1.0f;

        private Transform _playerTransform;
        private bool _isPointerDown = false;
        private Vector2 _initialPosition;
        private ControlType _controlType;
        private Canvas _canvas;

        public ReactiveProperty<Vector2> Direction = new ReactiveProperty<Vector2>(Vector2.zero);

        private void OnEnable()
        {
#if UNITY_EDITOR
            UpdateJoystickPosition();
#endif
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall += UpdateJoystickPosition;
#endif
        }

        private void UpdateJoystickPosition()
        {
            if (_joystick != null && _joystick.GetComponent<RectTransform>() != null)
            {
                _joystick.GetComponent<RectTransform>().anchoredPosition = _staticPosition;
            }
        }

        private Vector2 ClampHandlePosition(Vector2 position, Vector2 center)
        {
            Vector2 offset = position - center;
            float distance = offset.magnitude;
            
            if (distance > _range)
            {
                offset = offset.normalized * _range;
                return center + offset;
            }
            
            return position;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            _canvas = GetComponentInParent<Canvas>();
            SubscribeToControlType();
            SubscribeCannotMove();
            Direction.Value = Vector2.zero;
        }

        private void SubscribeCannotMove()
        {
            var d = Disposable.CreateBuilder();
            PlayerMovement.Instance.CannotMove.Subscribe(cannotMove => {
                if (cannotMove)
                {
                    ResetControll();
                }
            }).AddTo(ref d);
            d.RegisterTo(this.destroyCancellationToken);
        }
        
        // Update is called once per frame
        void Update()
        {
            if (!_isPointerDown || PlayerMovement.Instance.CannotMove.Value)
            {
                if (Direction.Value != Vector2.zero)
                {
                    Direction.Value = Vector2.zero;
                }
                return;
            }

            Vector2 screenPosition = GetCurrentInputPosition();
            Vector2 rawDirection = Vector2.zero;
            if (_controlType == ControlType.Touch)
            {
                Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
                rawDirection = worldPosition - new Vector2(_playerTransform.position.x, _playerTransform.position.y);
                Direction.Value = rawDirection.magnitude > _minTouchRange ? rawDirection.normalized : Vector2.zero;
            }
            else if (_controlType == ControlType.Static)
            {
                Vector2 canvasPosition = ScreenToCanvasPosition(screenPosition);
                Vector2 clampedPosition = ClampHandlePosition(canvasPosition, _staticPosition);
                rawDirection = clampedPosition - _staticPosition;
                _handle.GetComponent<RectTransform>().anchoredPosition = rawDirection;
                Direction.Value = rawDirection / Mathf.Max(rawDirection.magnitude, _range);
            }
            else if (_controlType == ControlType.Dynamic)
            {
                Vector2 canvasPosition = ScreenToCanvasPosition(screenPosition);
                Vector2 clampedPosition = ClampHandlePosition(canvasPosition, _initialPosition);
                rawDirection = clampedPosition - _initialPosition;
                _handle.GetComponent<RectTransform>().anchoredPosition = rawDirection;
                Direction.Value = rawDirection / Mathf.Max(rawDirection.magnitude, _range);
            }
        }

        private Vector2 GetCurrentInputPosition()
        {
            // 터치 입력이 있는 경우 마지막 터치 확인
            if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
            {
                var touches = Touchscreen.current.touches;
                
                // 마지막 터치를 찾기 (역순으로 순회)
                for (int i = touches.Count - 1; i >= 0; i--)
                {
                    var touch = touches[i];
                    if (touch.press.isPressed)
                    {
                        return touch.position.ReadValue();
                    }
                }
            }
            
            // 마우스 입력이 있는 경우 마우스 확인
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                return Mouse.current.position.ReadValue();
            }
            
            return Vector2.zero;
        }

        private bool HasActiveTouch()
        {
            // 터치 입력이 있는 경우 마지막 터치 확인
            if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
            {
                var touches = Touchscreen.current.touches;
                
                // 마지막 터치를 찾기 (역순으로 순회)
                for (int i = touches.Count - 1; i >= 0; i--)
                {
                    var touch = touches[i];
                    if (touch.press.isPressed)
                    {
                        return true;
                    }
                }
            }
            
            // 마우스 입력이 있는 경우 마우스 확인
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                return true;
            }
            
            return false;
        }

        private Vector2 ScreenToCanvasPosition(Vector2 screenPosition)
        {
            if (_canvas == null)
            {
                return screenPosition;
            }
            
            return screenPosition / _canvas.scaleFactor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPointerDown = true;
            _initialPosition = ScreenToCanvasPosition(eventData.position);

            if (_controlType == ControlType.Dynamic)
            {
                _joystick.SetActive(true);
                _joystick.GetComponent<RectTransform>().anchoredPosition = _initialPosition;
                _handle.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // 다른 터치가 여전히 활성화되어 있는지 확인
            if (!HasActiveTouch())
            {
                ResetControll();
            }
        }

        private void ResetControll()
        {
            _isPointerDown = false;
            Direction.Value = Vector2.zero;
            
            // handle을 중앙으로 리셋
            if (_handle != null && _handle.GetComponent<RectTransform>() != null)
            {
                _handle.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            
            if (_controlType == ControlType.Dynamic)
            {
                _joystick.SetActive(false);
            }
        }

        private void SubscribeToControlType()
        {
            var d = Disposable.CreateBuilder();
            OptionState.Instance.Control.Subscribe(control => {
                _controlType = control;
                if (control == ControlType.Static)
                {
                    _joystick.SetActive(true);
                    UpdateJoystickPosition();
                }
                else
                {
                    _joystick.SetActive(false);
                }
            }).AddTo(ref d);
            d.RegisterTo(this.destroyCancellationToken);
        }
    }
}

