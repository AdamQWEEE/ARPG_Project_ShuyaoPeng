using StarterAssets;
using UnityEngine;

public class SwordWave : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 1f;
    private bool canEmit;
    private Vector3 _dir;
    private Transform _owner;
    private ParticleSystem _ps;

    public void Init(Transform owner, Vector3 dir)
    {
        _owner = owner;
        _dir = dir.normalized;

        _ps = GetComponentInChildren<ParticleSystem>();
        if (_ps != null) _ps.Play();

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        
        if(canEmit)
        //transform.position += _dir * speed * Time.deltaTime;
            transform.position += ThirdPersonController.Instance.transform.forward * speed *3f* Time.deltaTime;
    }

    private void OnDestroy()
    {
        // 这里可以实例化单独的命中特效
        // Instantiate(hitVfxPrefab, transform.position, Quaternion.identity);
    }

    public void EmitWave()
    {
        canEmit = true;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<EnemyBase>().TakeDamage(25);
            other.gameObject.GetComponent<EnemyBase>().FallBack();
        }
    }
}
