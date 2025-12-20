 using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : Singleton<ThirdPersonController>
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

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

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
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

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;
        public bool isAttack;
        public int attack_num;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;
        public bool canMove;
        private bool roll_accelerate;
        Vector3 _rollDir;
        private AnimatorStateInfo stateInfo;
        public bool canChainNext;//是否进入连招区间
        public bool bufferAttack;
        public bool canRollAttack;
        public bool isRolling;
        public bool canImmediatelyAttack;
        
        public PlayerStateUI playerState;

        private bool isTargeting;
        public Transform lockTarget;
        public float lockOnCameraLerpSpeed = 2f;

        private bool isInCombo;
        public bool canRotateDuringAttack;

        [SerializeField] private LockOnMarker lockOnPrefab;
        private LockOnMarker currentMarker;

        public GameObject defenseEffect;
        //public Transform sword;
        //public Transform defenceEffectPoint;

        [Header("Spell Settings")]
        public FireBall fireballPrefab;
        public Transform firePoint;        // 手/武器前端的挂点
        public float baseTravelTime = 0.7f; // 基础飞行时间
        public float maxExtraTime = 0.3f;   // 远距离时额外增加一点时间

        [Header("Lock-On")]
        //public EnemyBase currentLockTarget;
        public Transform cameraTransform;


        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        private static readonly List<string> comboStateNames = new List<string>
        {
            "combo_01_1",
            "combo_01_2",
            "combo_01_3",
            "combo_04_1",
            "combo_04_2",
            "combo_04_3",
            "combo_04_4",
            "combo_04_5",
        // 之后有新的连招，直接在这里加就行
        
        };


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
            playerState = GetComponent<PlayerStateUI>();
            //Time.timeScale = 0.5f;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            
            Attack();
            UpdateAttackFacing();
            Defense();
            Roll();
            RollAccelerate();
            Stab();
            Toss();
            ChangeToSneak();
            ChangeCombo();
            ChangeMovement();

            stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            isTargeting = _animator.GetFloat("LockOn") == 1;



            isInCombo = comboStateNames.Any(name => stateInfo.IsName(name));

            if (isInCombo)
            {
                if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.8f)
                {
                    _animator.applyRootMotion = true;
                }
                else
                {
                    

                    _animator.applyRootMotion = false;
                    //attack_num = 0;
                    //_animator.SetInteger("Attack_num", attack_num);
                    
                }
                //_animator.applyRootMotion = true;
                // 当前正在播放名为“你的动画状态名称”的动画
                //Debug.Log("当前动画是：你的动画状态名称");
            }
            else if(stateInfo.IsName("Dodge Roll"))
            {
                if(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.3f)
                {
                    _animator.applyRootMotion = false;
                    canRollAttack=false;
                    
                    
                    //canMove = false;
                }
                else if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >0.3f && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.87f)
                {
                    canMove = false;
                    roll_accelerate = false;
                    _animator.applyRootMotion = true;
                }
                else
                {
                    if(_animator.GetFloat("LockOn") != 1)
                    {
                        _animator.applyRootMotion = false;
                        canMove = true;
                    }
                    

                }
            }

            else if (_animator.GetBool("isSneak"))
            {
                _animator.applyRootMotion = true;
            }
            else
            {
                _animator.applyRootMotion = false;

            }

            

            if (_animator.GetBool("isSneak") || isTargeting)
            {
                EightDirectionMove();                    
            }
            else
            {
                if(canMove)
                    Move();
            }
            

            

            //Debug.Log(_input.attack);
        }

        private void LateUpdate()
        {
            if (isTargeting)
            {
                
                LockOnCameraRotation();
            }
            else
            {
                
                CameraRotation();
            }
            
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

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }


        private void LockOnCameraRotation()
        {
            if (lockTarget == null)
            {
                // 没有锁定目标时退回自由视角逻辑
                CameraRotation();
                return;
            }

            // ★ 1. 用“玩家指向敌人”的方向来算 yaw，而不是用玩家自身的欧拉角
            Vector3 toTarget = lockTarget.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f)
                return;

            // 世界空间转成角度：z 轴为前
            float targetYaw = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;

            // ★ 2. 平滑插值到目标 yaw（无论是否在翻滚，都对着敌人）
            _cinemachineTargetYaw = Mathf.LerpAngle(
                _cinemachineTargetYaw,
                targetYaw,
                Time.deltaTime * lockOnCameraLerpSpeed   // 比如 10f
            );

            // pitch 继续用你现有的逻辑（可以保持原高度，或稍微固定一个角度）
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // ★ 3. 把旋转应用到 Cinemachine 的跟随目标
            CinemachineCameraTarget.transform.rotation =
                Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                                 _cinemachineTargetYaw,
                                 0f);
        }


        private void Attack()
        {
            bool canStartFirstAttackNow =(attack_num == 0) && (!isRolling || canRollAttack); 
            if ((canChainNext || canStartFirstAttackNow) && GameObject.Find("Dialogue Panel") == null)
            {
                if (bufferAttack)
                {
                    if (playerState.energyBar.fillAmount > playerState.energy_per_attack) { 

                        ExecuteAttack();
                        Debug.Log("触发预输入");
                        bufferAttack = false;
                    }
                }
                else
                {
                    canImmediatelyAttack = true;
                    

                }
            }

            if (_input.attack )
            {
                _input.attack = false;
                
                if (GameObject.Find("Dialogue Panel") == null)
                {
                    if (attack_num == 0)
                    {
                        if (isRolling && !canRollAttack)
                        {
                            bufferAttack = true;
                            return;
                        }
                        canChainNext = false;
                        canImmediatelyAttack = true;
                        //ExecuteAttack();
                    }
                    

                    if (canImmediatelyAttack)
                    {
                        if (playerState.energyBar.fillAmount > playerState.energy_per_attack)
                        {

                            ExecuteAttack();
                            //Debug.Log("进行连招");
                            canImmediatelyAttack = false;
                        }
                    }
                    else
                    {
                        bufferAttack = true;  // 预输入
                    }

                    
                }
            }
        }


        private void ExecuteAttack()
        {
            if (isTargeting && lockTarget != null)
            {
                Vector3 toTarget = lockTarget.position - transform.position;
                toTarget.y = 0f;

                if (toTarget.sqrMagnitude > 0.0001f)
                {
                    Quaternion currentRot = transform.rotation;
                    Quaternion targetRot = Quaternion.LookRotation(toTarget);

                    // 计算当前与目标的夹角
                    float angle = Quaternion.Angle(currentRot, targetRot);
                    if (angle > 0.01f)
                    {
                        // 这一帧最多能转多少度
                        float maxStep = 720f * Time.deltaTime;
                        // 本帧插值比例 t（0~1）
                        float t = Mathf.Clamp01(maxStep / angle);

                        // ★ 用 Slerp 进行“部分旋转”，而不是一次旋完
                        transform.rotation = Quaternion.Slerp(currentRot, targetRot, t);
                        // 等价也可以写成：
                        // transform.rotation = Quaternion.RotateTowards(currentRot, targetRot, maxStep);
                    }
                }
            }
            Debug.Log("执行一次");
            _animator.applyRootMotion = true;
            attack_num =attack_num+1;
            _animator.SetInteger("Attack_num", attack_num);
            _animator.SetTrigger("Attack");
            //_input.attack = false;
            _animator.SetBool("isSneak", false);
            playerState.ConsumeEnergy();
            playerState.recoverEnergy = false;
            canImmediatelyAttack = false;            
            canChainNext = false;
            bufferAttack = false;
            
        }

        private void UpdateAttackFacing()
        {
            
            if (!isTargeting || lockTarget == null) return;
            if (!canRotateDuringAttack) return;

            Vector3 toTarget = lockTarget.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f) return;

            Quaternion currentRot = transform.rotation;
            Quaternion targetRot = Quaternion.LookRotation(toTarget);

            float maxStep = 720f * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(currentRot, targetRot, maxStep);
        }

        private void Defense()
        {
            if (_input.defense)
            {
                _input.defense=false;
                _animator.SetTrigger("Defense");
                defenseEffect.SetActive(false);
                defenseEffect.SetActive(true);
            }
        }

        private void Stab()
        {
            if (_input.stab)
            {
                _input.stab=false;
                _animator.SetTrigger("Stab");
            }
        }

        private void Toss()
        {
            if (_input.toss)
            {
                _input.toss = false;
                _animator.SetTrigger("Toss");
                //CastFireball();
                print("投掷");
            }
        }

        void CastFireball()
        {
            if (fireballPrefab == null || firePoint == null) return;

            // 1. 生成火球
            FireBall proj = Instantiate(
                fireballPrefab,
                firePoint.position,
                firePoint.rotation
            );

            // 2. 计算目标位置
            Vector3 targetPos;

            if (lockTarget != null)
            {
                // 锁定时瞄准敌人锁定点，稍微往上抬一点
                targetPos = lockTarget.position + Vector3.up * 0.2f;
            }
            else
            {
                // 无锁定：沿摄像机前方打一段距离
                Vector3 forward = cameraTransform != null
                    ? cameraTransform.forward
                    : transform.forward;

                forward.y = Mathf.Clamp(forward.y, -0.1f, 0.5f); // 避免打太高/太低
                forward.Normalize();

                float distance = 15f; // 无锁定时默认射程
                targetPos = firePoint.position + forward * distance;
            }

            // 3. 根据距离调整飞行时间，让远处多飞一会儿，近处快一点
            Vector3 displacement = targetPos - firePoint.position;
            float horizontalDist = new Vector3(displacement.x, 0, displacement.z).magnitude;

            // 简单：水平距离越远，时间稍微长一点（上限 maxExtraTime）
            float t = baseTravelTime + Mathf.Clamp01(horizontalDist / 20f) * maxExtraTime;

            // 4. 计算初始速度：v = (Δp - 0.5 * g * t^2) / t
            Vector3 g = Physics.gravity;
            Vector3 velocity = (displacement - 0.5f * g * t * t) / t;

            // 5. 发射
            proj.Launch(velocity);
        }


        public void AttackRotateOn()
        {
            canRotateDuringAttack = true;
        }

        // 在挥刀中后段锁死方向
        public void AttackRotateOff()
        {
            canRotateDuringAttack = false;
        }

        public void OpenComboWindow()
        {
            canChainNext = true;
            //Debug.Log("可以切换连招");
        }

        public void CloseComboWindow()
        {
            canChainNext = false;
        }
        public void OpenRollAttackWindow()
        {
            canRollAttack = true;
            isRolling = false;
            playerState.recoverEnergy = true;
        }

        public void ResetCombo()
        {
            attack_num = 0;
            _animator.SetInteger("Attack_num", attack_num);
            if(!isRolling)
                playerState.recoverEnergy = true;
        }
        private void RollAccelerate()
        {
            if (roll_accelerate)
            {
                _controller.Move(transform.forward * 5f * Time.deltaTime);
            }
        }

        public void EnableMove()
        {
            canMove=true;
        }
        private void Roll()
        {
            if (_input.roll)
            {
                if ((canMove ||isTargeting) && playerState.energyBar.fillAmount > playerState.energy_per_attack) {
                    // 1. 计算翻滚方向（这里举例：相机方向 + 输入方向）
                    if(isTargeting ||_animator.GetBool("isSneak"))
                    {
                        Vector2 move = _input.move;
                        _animator.SetBool("isSneak", false);

                        if (move.sqrMagnitude < 0.01f)
                        {
                            // 没有输入时，你可以选择：
                            // _rollDir = transform.forward;      // 始终向前滚
                            // 或者 _rollDir = -transform.forward; // 后撤步
                            _rollDir = transform.forward;
                        }
                        else
                        {
                            // 相机前/右方向投射到地面
                            Vector3 camFwd = _mainCamera.transform.forward;
                            camFwd.y = 0f;
                            camFwd.Normalize();

                            Vector3 camRight = _mainCamera.transform.right;
                            camRight.y = 0f;
                            camRight.Normalize();

                            // 上下左右输入组合成世界空间的滚动方向
                            _rollDir = (camFwd * move.y + camRight * move.x).normalized;
                        }

                        // 2. 把角色朝向旋转到滚动方向
                        if (_rollDir.sqrMagnitude > 0.0001f)
                            transform.rotation = Quaternion.LookRotation(_rollDir);

                    }


                    //_animator.applyRootMotion = true;
                    _animator.SetTrigger("Roll");
                    roll_accelerate = true;


                    canMove =false;
                    isRolling = true;
                    playerState.ConsumeEnergy();
                    playerState.recoverEnergy = false;

                }
                
                _input.roll = false;
            }
        }

        

        private void ChangeToSneak()
        {
            if (_input.sneak)
            {
                Debug.Log("变为潜行"+ _animator.GetBool("isSneak"));
                _animator.SetBool("isSneak", !_animator.GetBool("isSneak"));
                _input.sneak=false;
            }
        }

        private void ChangeMovement()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (_animator.GetFloat("LockOn") == 0f)
                {
                    CameraModeController.Instance.SetLockOn(true, lockTarget);
                    _animator.SetFloat("LockOn", 1f);
                    LockCameraPosition=true;
                    if (currentMarker != null)
                        Destroy(currentMarker.gameObject);

                    currentMarker = Instantiate(lockOnPrefab, lockTarget.GetChild(0));
                    
                }
                else
                {
                    CameraModeController.Instance.SetLockOn(false,null);
                    _animator.SetFloat("LockOn", 0f);
                    canMove = true;
                    LockCameraPosition = false;
                    if (currentMarker != null)
                        Destroy(currentMarker.gameObject);
                }
                    
            }
        }

        private void ChangeCombo()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _animator.SetBool("changeCombo", !_animator.GetBool("changeCombo"));
            }
        }

        private void EightDirectionMove()
        {
            Vector2 move = _input.move;

            if (comboStateNames.Any(name => stateInfo.IsName(name)))
            {
                // 输入参数平滑归零，避免动画树切到移动
                float x0 = Mathf.Lerp(_animator.GetFloat("inputX"), 0f, Time.deltaTime * 10f);
                float y0 = Mathf.Lerp(_animator.GetFloat("inputY"), 0f, Time.deltaTime * 10f);
                _animator.SetFloat("inputX", x0);
                _animator.SetFloat("inputY", y0);
                return;    // ★ 关键：直接返回，不再改 transform.position / rotation
            }



            // ---------- 锁定视角移动 ----------
            if (isTargeting && lockTarget != null && !isRolling)
            {
                // ★ 修改 1：无论有没有输入，都先让角色朝向目标
                Vector3 toTarget = lockTarget.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(toTarget);
                    // 用 Slerp 平滑一点，也可以直接赋值
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRot,
                        Time.deltaTime * 10f);
                }

                // 下面再处理移动和动画
                if (move.sqrMagnitude < 0.0001f)
                {
                    // 没有输入：inputX/inputY 平滑回 0
                    float x0 = Mathf.Lerp(_animator.GetFloat("inputX"), 0f, Time.deltaTime * 10f);
                    float y0 = Mathf.Lerp(_animator.GetFloat("inputY"), 0f, Time.deltaTime * 10f);
                    _animator.SetFloat("inputX", x0);
                    _animator.SetFloat("inputY", y0);
                    return;
                }

                // ★ 修改 2：用 Cross 计算“右侧切线”，解决左右反的问题
                // up × forward = right
                //Vector3 tangentRight = Vector3.Cross(Vector3.up, toTarget);
                //tangentRight.y = 0f;
                //tangentRight.Normalize();

                //// move.y 控制靠近/远离，move.x 控制左/右绕圈
                //// A 键为 -1：右向切线 * -1 = 向左移动
                //Vector3 moveWorld = toTarget * move.y + tangentRight * move.x;

                //float moveMag = Mathf.Clamp01(move.magnitude);
                //_controller.Move(moveWorld.normalized * 4f * moveMag * Time.deltaTime);

                Vector3 localMove = new Vector3(move.x, 0f, move.y);
                localMove = Vector3.ClampMagnitude(localMove, 1f);  // 保证最大长度 1

                // 转到世界空间：forward = 面向敌人，right = 围着敌人绕圈
                Vector3 worldMoveDir = transform.TransformDirection(localMove).normalized;

                // 5. 统一速度，防止斜向看起来“慢一档”或“滑步” ★
                float moveMag = localMove.magnitude;   // 0~1，手柄可用
                _controller.Move(worldMoveDir * 4f * moveMag * Time.deltaTime);

                // ★ 修改 3：动画仍然用原始输入做 2D Blend，斜向不会显得“变慢”
                float inputX_blend = Mathf.Lerp(_animator.GetFloat("inputX"), localMove.x, Time.deltaTime * 10f);
                float inputY_blend = Mathf.Lerp(_animator.GetFloat("inputY"), localMove.z, Time.deltaTime * 10f);
                _animator.SetFloat("inputX", inputX_blend);
                _animator.SetFloat("inputY", inputY_blend);

                return;
            }
            else
            {
                
                if (_input.move != Vector2.zero && !isRolling)
                {

                    //_animator.applyRootMotion = false;

                    _targetRotation = _mainCamera.transform.eulerAngles.y;//潜行方向始终对着摄像机方向
                    float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                        RotationSmoothTime);

                    // rotate to face input direction relative to camera position
                    //if(_input.move.x>0)
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                    //Animator animator = GetComponent<Animator>();

                }
                float inputX_blend = Mathf.Lerp(_animator.GetFloat("inputX"), _input.move.x, Time.deltaTime * 10f);
                float inputY_blend = Mathf.Lerp(_animator.GetFloat("inputY"), _input.move.y, Time.deltaTime * 10f);
                /*_animator.SetFloat("inputX", _input.move.x);
                _animator.SetFloat("inputY",_input.move.y);*/

                _animator.SetFloat("inputX", inputX_blend);
                _animator.SetFloat("inputY", inputY_blend);

            }

        }
        private void Move()
        {

            
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

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

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            //Debug.Log("x方向位移"+_input.move.x);
            //Debug.Log("y方向位移"+_input.move.y);
            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                
                //_animator.applyRootMotion = false;
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                //Animator animator = GetComponent<Animator>();
                
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            
            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

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
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

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

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}