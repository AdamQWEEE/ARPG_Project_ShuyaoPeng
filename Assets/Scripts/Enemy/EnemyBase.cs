using StarterAssets;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;
using static UnityEditor.Experimental.GraphView.GraphView;

public enum Faction
{
    Warrior,
    Archer,
    Boss,
}



public class EnemyBase : MonoBehaviour
{
    [Header("Basic Info")]
    public string enemyName = "Enemy";
    public Faction faction;

    [Header("玩家")]
    public ThirdPersonController player;

    [Header("Lock-On")]
    //[Tooltip("锁定时摄像机/准星指向的位置（一般挂在胸口/头部空物体上）")]
    //public Transform lockOnPoint;
    [Tooltip("是否允许被锁定")]
    public bool canBeLocked = true;
    [Tooltip("锁定优先级（Boss/精英可以设大一点）")]
    public float lockOnPriority = 0f;

    [Header("Health")]
    public float maxHealth = 100f;

    [SerializeField, Tooltip("初始血量，为空则自动 = maxHealth")]
    public float currentHealth;

    [Header("Components (可选)")]
    public Animator animator;
    public Collider mainCollider;
    public NavMeshAgent agent;

    // 状态属性
    public bool IsDead { get; private set; }

    // 事件（用于 UI / 掉落等）
    public event Action<EnemyBase, float, float> OnHealthChanged; // (enemy, current, max)
    public event Action<EnemyBase> OnKilled;

    [Header("UI血条")]
    public WorldSpaceHealthBar hpBar;

    [Header("怪物模型")]
    public EnemyModel enemyModel;

    public bool canFallBackMove;
    public bool canHitBackMove;

    [Header("刚体组件")]
    public Rigidbody rb;

    [Header("KnockBack")]

    public float knockbackHorizontal = 4f;   // 水平方向力度
    public float knockbackVertical = 3f;   // 向上力度

    [Header("感知 & 距离")]
    public float sightRange = 12f;      // 发现玩家距离
    public float chaseRange = 15f;      // 超出则放弃追击
    public float attackRange = 2.5f;     // 进入攻击距离

    [Header("移动速度")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public float backOffSpeed = 4f;
    public float backOffDistance = 2f;

    [Header("巡逻点(可选)")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 1.5f;

    [Header("回位点")]
    public Transform homePoint; //回位点
    public float backHomeSpeed = 3.5f;
    public float losePlayerTime = 3f;
    Vector3 _initialPosition;
    float _losePlayerTimer;

    [Header("攻击节奏（秒）")]
    public float comboDuration = 1.0f;  // 三连击动画总长
    public float heavyDuration = 1.0f;  // 重攻击动画总长
    public float attackRecoverDelay = 0.3f;  // 每段攻击间的间隔

    [Header("击退效果")]
    public float hitBackForce = 4f;
    public float hitUpForce = 2f;
    public float stunDuration = 0.25f;     // 被打硬直时间
    public bool isHit;

    [Header("动画参数")]
    public int _patrolIndex = 0;
    public bool _isAttacking = false;
    public bool _isStunned = false;
    public bool _superArmor = false;

    public enum EnemyState
    {
        Idle,
        Patrol,
        ReturnHome,
        Chase,
        GetHit,
        Attack,
        BackOff,
        Dead
    }

    [Header("敌人当前状态")]
    public EnemyState state = EnemyState.Idle;
   
   

    [Header("Behaviour")]
    public bool canPatrol = true;

    protected virtual void Awake()
    {
        if (currentHealth <= 0f)
            currentHealth = maxHealth;

        if (mainCollider == null)
            mainCollider = GetComponent<Collider>();
        agent=GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        SetNavMode(true);
    }

    private void Start()
    {
        if (homePoint == null)
        {
            // 默认就把出生点当作回位点
            homePoint = transform;
        }

        if (hpBar != null)
        {
            hpBar.maxHp = maxHealth;
            hpBar.currentHp = maxHealth;
        }
        enemyModel = GetComponent<EnemyModel>();
        
        player = ThirdPersonController.Instance;
    }

    protected virtual void OnEnable()
    {
        EnemyManager.Register(this);
        state = patrolPoints != null && patrolPoints.Length > 0 ? EnemyState.Patrol : EnemyState.Idle;
    }

    protected virtual void OnDisable()
    {
        // 注意：OnDisable 也会在场景卸载/对象销毁时调用
        EnemyManager.Unregister(this);
    }

    private void Update()
    {
        

        FallBackMove();
        HitBackMove();

        if (state == EnemyState.Dead || player == null) return;

        switch (state)
        {
            case EnemyState.Idle: UpdateIdle(); break;
            case EnemyState.Patrol: UpdatePatrol(); break;
            case EnemyState.Chase: UpdateChase(); break;
            case EnemyState.GetHit: UpdateGetHit(); break;
            case EnemyState.ReturnHome: UpdateReturnHome(); break;
            case EnemyState.Attack: UpdateAttack(); break;
            case EnemyState.BackOff: UpdateBackOff(); break;
            case EnemyState.Dead:  break;
        }
    }
    void ChangeState(EnemyState newState)
    {
        if (state == newState) return;
        state = newState;
        

        switch (state)
        {
            

            case EnemyState.Attack:
                agent.isStopped = true;
                animator.SetFloat("MoveSpeed", 0f);
                if (!_isAttacking) StartCoroutine(AttackLoop());
                break;

            case EnemyState.BackOff:
                _isAttacking = false;
                break;
        }
    }

    void UpdateIdle()
    {
        FacePlayerIfNear();
        animator.SetFloat("MoveSpeed", 0f);

        // 看见玩家就进入追击
        if (DistanceToPlayer() <= sightRange)
        {
            ChangeState(EnemyState.Chase);
        }

        if (canPatrol && patrolPoints != null && patrolPoints.Length > 0)
        {
            ChangeState(EnemyState.Patrol);
        }
    }

    void UpdatePatrol()
    {
        if (!canPatrol)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        agent.speed = patrolSpeed;
        animator.SetFloat("MoveSpeed", agent.velocity.magnitude);
        agent.isStopped = false;

        Transform targetPoint = patrolPoints[_patrolIndex];
        agent.SetDestination(targetPoint.position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            StartCoroutine(SwitchPatrolPointAfterWait());
        }

        if (DistanceToPlayer() <= sightRange)
        {
            ChangeState(EnemyState.Chase);
        }
    }

    IEnumerator SwitchPatrolPointAfterWait()
    {
        // 避免多个协程叠加
        if (state != EnemyState.Patrol) yield break;

        state = EnemyState.Idle;
        agent.isStopped = true;
        animator.SetFloat("MoveSpeed", 0f);

        yield return new WaitForSeconds(patrolWaitTime);

        if (state == EnemyState.Idle)
        {
            _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
            ChangeState(EnemyState.Patrol);
        }
    }

    void UpdateChase()
    {
        float dist = DistanceToPlayer();

        if (dist > chaseRange)
        {
            // 超出追击范围，回到巡逻 / Idle
            ChangeState(EnemyState.ReturnHome);
            return;
        }

        agent.speed = chaseSpeed;
        animator.SetFloat("MoveSpeed", agent.velocity.magnitude);
        agent.isStopped = false;
        agent.SetDestination(player.transform.position);

        

        FacePlayerIfNear();

        if (dist <= attackRange && !_isAttacking && !_isStunned)
        {
            //ChangeState(EnemyState.Attack);
        }
    }

    void UpdateReturnHome()
    {
        agent.isStopped = false;
        agent.speed = backHomeSpeed;
        animator.SetFloat("MoveSpeed", agent.velocity.magnitude);

        Vector3 targetPos = homePoint.position;       // 或 homePoint.position
        agent.SetDestination(targetPos);

        // 如果离原位足够近，就认为已经回到了
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            agent.isStopped = true;

            // ★ 回来之后根据类型决定去 Idle 还是 Patrol
            if (canPatrol && patrolPoints != null && patrolPoints.Length > 0)
                ChangeState(EnemyState.Patrol);
            else
                ChangeState(EnemyState.Idle);

            return;
        }
    }

    void UpdateGetHit()
    {
        if (isHit)
        {
            SetNavMode(false);
            //agent.isStopped = true;
            //agent.speed = 0f;
            animator.SetFloat("MoveSpeed", 0f);
        }
        else
        {
            SetNavMode(true);
            agent.enabled = true;
            ChangeState(EnemyState.Idle);
        }
    }

    public void FinishGetHit()
    {
        isHit = false;
    }

    void UpdateAttack()
    {
        // 攻击状态主要由协程驱动，这里只负责面向玩家
        FacePlayerIfNear();
    }

    IEnumerator AttackLoop()
    {
        _isAttacking = true;

        while (state == EnemyState.Attack && !_isStunned && state != EnemyState.Dead)
        {
            float dist = DistanceToPlayer();
            if (dist > attackRange * 1.2f)
            {
                // 玩家跑远了，结束攻击转回追击
                ChangeState(EnemyState.Chase);
                break;
            }

            // 1. 三连击（一个动画，内部用动画事件打三段）
            FacePlayerIfNear();
            animator.SetTrigger("Combo");
            yield return new WaitForSeconds(comboDuration + attackRecoverDelay);

            // 2. 后撤
            ChangeState(EnemyState.BackOff);
            yield return new WaitForSeconds(0.3f); // 给一点时间让 BackOff 状态 Update 生效
            while (state == EnemyState.BackOff)
                yield return null;

            // 如果已经不在攻击距离，退出攻击循环
            if (DistanceToPlayer() > attackRange * 1.2f)
            {
                ChangeState(EnemyState.Chase);
                break;
            }

            // 3. 重攻击
            FacePlayerIfNear();
            animator.SetTrigger("HeavyAttack");
            yield return new WaitForSeconds(heavyDuration + attackRecoverDelay);

            // 4. 再后撤
            ChangeState(EnemyState.BackOff);
            yield return new WaitForSeconds(0.3f);
            while (state == EnemyState.BackOff)
                yield return null;

            // 循环：若仍在攻击范围内，会再次进入 AttackLoop
            if (DistanceToPlayer() <= attackRange * 1.2f)
            {
                ChangeState(EnemyState.Attack);
            }
            else
            {
                ChangeState(EnemyState.Chase);
                break;
            }
        }

        _isAttacking = false;
    }

    void UpdateBackOff()
    {
        // 用 NavMeshAgent 向后退，退到一定距离后回到 Chase
        if (agent.isStopped) agent.isStopped = false;

        Vector3 dirToPlayer = (transform.position - player.transform.position).normalized;
        Vector3 backTarget = transform.position + dirToPlayer * backOffDistance;

        agent.speed = backOffSpeed;
        agent.SetDestination(backTarget);

        animator.SetFloat("MoveSpeed", agent.velocity.magnitude);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            ChangeState(EnemyState.Chase);
        }
    }
    float DistanceToPlayer()
    {
        if (player == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position, player.transform.position);
    }

    void FacePlayerIfNear()
    {
        if (player == null) return;

        Vector3 toPlayer = player.transform.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(toPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
    }

    #region 生命 / 受击 / 死亡

    public virtual void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (IsDead || amount <= 0f) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);
        if (hpBar != null)
            hpBar.TakeDamage(amount);

        OnHealthChanged?.Invoke(this, currentHealth, maxHealth);

        // 播放受击反馈
        //OnHit(amount, hitPoint, hitNormal);

        if (currentHealth <= 0f)
        {
            ChangeState(EnemyState.Dead);
            Die();
        }
    }

    // 如果你不关心 hitPoint，可以用这个重载
    public void TakeDamage(float amount)
    {
        TakeDamage(amount, transform.position, Vector3.up);
    }

    /// <summary>
    /// 敌人被击中时的通用处理（播放受击动画、硬直等），子类覆写。
    /// </summary>
    protected virtual void OnHit(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        // 示例：小硬直动画
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        // TODO: 在这里触发受击特效 / 闪白等
    }

    /// <summary>
    /// 敌人死亡时的通用处理，子类可以覆写扩展（掉落、特殊演出等）。
    /// </summary>
    protected virtual void Die()
    {
        if (IsDead) return;
        IsDead = true;
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.speed = 0f;
            agent.enabled=false;
        }
        animator.SetFloat("MoveSpeed", 0f);
        hpBar.gameObject.SetActive(false);
        ThirdPersonController.Instance.ReleaseLock();

        // 禁用碰撞
        //if (mainCollider != null)
        //    mainCollider.enabled = false;

        // 播放死亡动画
        if (animator != null)
        {
            animator.SetBool("Dead", true);
            animator.SetTrigger("Die");
        }

        canBeLocked = false;
        enemyModel.Die();
        OnKilled?.Invoke(this);
        Destroy(gameObject, 2f);

        // 这里不直接 Destroy，给子类机会控制消失时机
        // 比如：StartCoroutine(DelayedDestroy());
    }

    #endregion

    #region 朝向 / 工具方法（AI 可复用）

    /// <summary>
    /// 让敌人平滑朝向某个位置（只转 Y 轴）。
    /// </summary>
    protected void FaceTarget(Vector3 worldPos, float turnSpeedDegPerSec)
    {
        Vector3 dir = worldPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion current = transform.rotation;
        Quaternion target = Quaternion.LookRotation(dir);

        float maxStep = turnSpeedDegPerSec * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(current, target, maxStep);
    }

    #endregion

    public void FallBack()
    {
        CancelInvoke();
        enemyModel.FallBack();
        canFallBackMove = true;
        isHit = true;
        SetNavMode(false);
        ChangeState(EnemyState.GetHit);
        
    }

    public void FallBackMove()
    {
        if (canFallBackMove)
        {
            transform.position+= transform.forward * (-2f) * Time.deltaTime;
        }
    }

    public void StopFallBackMove()
    {
        canFallBackMove=false;
    }

    public void HitBack()
    {
        enemyModel.GetHit();
        canHitBackMove = true;
       // GetComponent<Rigidbody>().addvel
    }

    public void ApplyKnockback(Vector3 attackerPosition)
    {
        CancelInvoke();
        enemyModel.GetHit();
        SetNavMode(false);
        if (rb == null) return;

        // 计算从攻击者指向敌人的方向
        //Vector3 dir = (transform.position - attackerPosition);
        //dir.y = 0f;
        //dir.Normalize();

        //// 最终击退方向 = 水平后退 + 向上
        //Vector3 knockDir = dir * knockbackHorizontal + Vector3.up * knockbackVertical;

        //// 清掉原来的水平速度，避免叠加得太怪
        //Vector3 v = rb.linearVelocity;
        //v.x = 0; v.z = 0;
        //rb.linearVelocity = v;

        //// 一次性施加速度变化（冲量）
        //rb.AddForce(knockDir, ForceMode.VelocityChange);

        isHit=true;
        Invoke("FinishGetHit", 3f);
        ChangeState(EnemyState.GetHit);

        //_isKnockback = true;
        //StopCoroutine(nameof(EndKnockback));
        //StartCoroutine(EndKnockback());
    }

    public void HitBackMove()
    {
        if (canHitBackMove)
        {
            transform.position += transform.forward * (-1f) * Time.deltaTime;
        }
    }

    public void StopHitBackMove()
    {
        canHitBackMove=false;
    }

    void SetNavMode(bool useNav)
    {
        if (useNav)
        {
            // 导航接管
            //rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            agent.enabled = true;
        }
        else
        {
            if (agent.enabled)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }
            //rb.isKinematic = true;
        }
    }

}
