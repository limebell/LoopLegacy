using LoopLegacy.Manager;
using UnityEngine;
using R3;
using LoopLegacy.Battle.RelicEffects;
using LoopLegacy.Battle;
using LoopLegacy.Loader;
using LoopLegacy.State;

namespace LoopLegacy.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        public static PlayerMovement Instance { get; private set; }
        private float _moveSpeed = 7f;
        private Vector2 _moveDirection;
        private bool _isMoving = false;
        public ReactiveProperty<bool> CannotMove = new ReactiveProperty<bool>(false);

        private Animator animator;
        private Rigidbody2D rigidbody2d;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            Instance = this;

            animator = GetComponent<Animator>();
            rigidbody2d = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            var d = Disposable.CreateBuilder();
            if (GameManager.Instance is not null)
            {
                GameManager.Instance.IsMovingMap.Subscribe(isMovingMap =>
                {
                    CannotMove.Value = isMovingMap;
                    if (CannotMove.Value)
                    {
                        _isMoving = false;
                        animator.SetBool("IsMoving", false);
                    }
                })
                .AddTo(ref d);
            }

            if (BattleManager.Instance is not null)
            {
                // 배틀 매니저가 있을 때 전투 상태 구독
                BattleManager.Instance.CurrentBattleStep
                    .Subscribe(inBattle =>
                    {
                        CannotMove.Value = inBattle != BattleStep.None;
                        if (CannotMove.Value)
                        {
                            _isMoving = false;
                            animator.SetBool("IsMoving", false);
                        }
                    })
                    .AddTo(ref d);
            }

            if (TerritoryManager.Instance is not null)
            {
                // 배틀 매니저가 있을 때 전투 상태 구독
                TerritoryManager.Instance.IsInteractionInProgress
                    .Subscribe(inInteraction => 
                    {
                        CannotMove.Value = inInteraction;
                        if (inInteraction)
                        {
                            _isMoving = false;
                            animator.SetBool("IsMoving", false);
                        }
                    })
                    .AddTo(ref d);
            }

            d.RegisterTo(this.destroyCancellationToken);
        }
        

        public void SetCannotMove(bool cannotMove)
        {
            CannotMove.Value = cannotMove;
        }

        public void SetVector2Input(Vector2 direction)
        {
            if (direction != Vector2.zero)
            {
                _moveDirection = direction;
                _isMoving = true;
            }
            else
            {
                _isMoving = false;
            }
        }

        private void Update()
        {
            if (CannotMove.Value)
            {
                _isMoving = false;
                return;
            }
        }


        private void FixedUpdate()
        {
            if (_isMoving)
            {
                var movementSpeedMultiplier = 1.0f;
                if (PersistentGameState.Instance.IsInGame)
                {
                    movementSpeedMultiplier *= 1.0f + PersistentGameState.Instance.HouseState.GetUpgradeValue(UpgradeType.MovementSpeed) / 100f;
                    if (GameManager.Instance is { } manager)
                    {
                        // GameManager가 존재 할 때 이동 시도가 있을 때마다 엔카운터 게이지 증가
                        manager.EncounterManager.AddGauge(_moveDirection.magnitude * Time.fixedDeltaTime);
                        if (manager.TryGetRelic<SpeedBoostEffect>(out Relic relic))
                        {
                            movementSpeedMultiplier *= 1.0f + (relic.Effect as SpeedBoostEffect).GetSpeedIncreasePercentage() / 100f;
                        }
                    }
                }

                Vector2 nextVector = _moveDirection * _moveSpeed * movementSpeedMultiplier * Time.fixedDeltaTime;
                rigidbody2d.MovePosition(rigidbody2d.position + nextVector);
            }
        }

        private void LateUpdate()
        {
            animator.SetBool("IsMoving", _isMoving);

            if (_isMoving)
            {
                // x축과 y축의 이동 방향을 비교하여 더 큰 쪽을 기준으로 방향 설정
                float absX = Mathf.Abs(_moveDirection.x);
                float absY = Mathf.Abs(_moveDirection.y);

                if (absY > absX)
                {
                    // y축 이동이 더 큰 경우
                    animator.SetInteger("Direction", _moveDirection.y > 0 ? 2 : 0); // 위(2) 또는 아래(0)
                }
                else
                {
                    // x축 이동이 더 큰 경우
                    animator.SetInteger("Direction", _moveDirection.x < 0 ? 1 : 3); // 좌(1) 또는 우(3)
                }
            }
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            // 이동 상태 초기화
            _isMoving = false;
            _moveDirection = Vector2.zero;
            animator.SetBool("IsMoving", false);
        }
    }
} 