using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("Enemy")]
    [Tooltip("歩くはやさが変わります(m/s)")]
    [Label("歩くはやさ")]
    public float MoveSpeed = 2.0f;

    [Tooltip("走る速さが変わります(m/s)")]
    [Label("走るはやさ")]
    public float SprintSpeed = 5.335f;

    [Tooltip("向きを変えるときの早さ")]
    [Label("向きのはやさ")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("はやくなりやすさとおそくなりやすさ")]
    [Label("はやさの変わり方")]
    public float SpeedChangeRate = 10.0f;

    [HideInInspector]
    public AudioClip LandingAudioClip;
    [HideInInspector]
    public AudioClip[] FootstepAudioClips;

    [Label("足音の大きさ")]
    [Range(0, 1)]
    public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("ジャンプで飛ぶ高さ")]
    [Label("ジャンプ力")]
    public float JumpHeight = 1.2f;

    [Tooltip("重力の強さ。落ちるスピードの強さ")]
    [Label("重力（落ちる力）")] 
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("再ジャンプが使えるまでの時間")]
    [Label("ジャンプの連続で使える時間")]
    public float JumpTimeout = 0.50f;

    [Tooltip("落ちる状態になるまでの時間")]
    [Label("落ちる状態になるまでの時間")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    [HideInInspector]
    public bool Grounded = true;

    [HideInInspector]
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [HideInInspector]
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    // Attached Component
    private NavMeshAgent _navMeshAgent;
    private CharacterController _controller;
    private Animator _animator;
    private EnemyStateManager _enemyStateManager;

    private bool _hasAnimator = false;
    private Vector3 targetPos = Vector3.zero;

    // player
    private float _speed;
    private float _animationBlend;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    // Start is called before the first frame update
    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        _enemyStateManager = GetComponent<EnemyStateManager>();

        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    // Update is called once per frame
    void Update()
    {
        _hasAnimator = TryGetComponent(out _animator);

        _enemyStateManager.Exec();
        targetPos = _enemyStateManager.GetTargetPos();

        JumpAndGravity();
        GroundedCheck();
        Move();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }
    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    private void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        //float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
        float targetSpeed = (_enemyStateManager.IsChase()) ? SprintSpeed : MoveSpeed;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = 1.0f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        if ((this.transform.position - targetPos).magnitude < 0.2f)
        {
            targetSpeed = 0.0f;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;


        _navMeshAgent.destination = targetPos;

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            //if (_jumpTimeoutDelta <= 0.0f)
            //{
            //    // the square root of H * -2 * G = how much velocity needed to reach desired height
            //    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

            //    // update animator if using character
            //    if (_hasAnimator)
            //    {
            //        _animator.SetBool(_animIDJump, true);
            //    }
            //}

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(this.transform.position), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(this.transform.position), FootstepAudioVolume);
        }
    }
}