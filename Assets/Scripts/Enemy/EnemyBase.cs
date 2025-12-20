using System;
using UnityEngine;

public enum Faction
{
    Player,
    Enemy,
    Neutral
}

public class EnemyBase : MonoBehaviour
{
    [Header("Basic Info")]
    public string enemyName = "Enemy";
    public Faction faction = Faction.Enemy;

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
    private float currentHealth;

    [Header("Components (可选)")]
    public Animator animator;
    public Collider mainCollider;

    // 状态属性
    public bool IsDead { get; private set; }

    // 事件（用于 UI / 掉落等）
    public event Action<EnemyBase, float, float> OnHealthChanged; // (enemy, current, max)
    public event Action<EnemyBase> OnKilled;

    [Header("UI血条")]
    public WorldSpaceHealthBar hpBar;

    [Header("怪物模型")]
    public EnemyModel enemyModel;

    protected virtual void Awake()
    {
        if (currentHealth <= 0f)
            currentHealth = maxHealth;

        if (mainCollider == null)
            mainCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        if (hpBar != null)
        {
            hpBar.maxHp = maxHealth;
            hpBar.currentHp = maxHealth;
        }
        enemyModel = GetComponent<EnemyModel>();
    }

    protected virtual void OnEnable()
    {
        EnemyManager.Register(this);
    }

    protected virtual void OnDisable()
    {
        // 注意：OnDisable 也会在场景卸载/对象销毁时调用
        EnemyManager.Unregister(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {

            TakeDamage(50f);
        }
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
        OnHit(amount, hitPoint, hitNormal);

        if (currentHealth <= 0f)
        {
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


}
