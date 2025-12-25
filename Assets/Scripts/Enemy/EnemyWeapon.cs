using StarterAssets;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public EnemyBase enemy;
    public Transform weaponEnd;
    public Transform explodePrefab;
    private Transform explodeItem;
    private bool isExplode;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        if (!isExplode && stateInfo.IsName("Jumpattack"))
        {
            if (weaponEnd.transform.position.y < 0.1f)
            {
                explodeItem= Instantiate(explodePrefab);
                explodeItem.position= weaponEnd.transform.position;
                Destroy(explodeItem.gameObject, 1f);
                isExplode=true;
                Invoke("ResetExplode", 3f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (enemy.canApplyDamage)
            {
                ThirdPersonController player=other.GetComponent<ThirdPersonController>();

                if (player.isCounter)
                {
                    player.ShowCounterEffect();
                    enemy.AddStance(30);
                }
                else
                {
                    player.playerState.TakeDamage(20);
                    Debug.Log("´òµ½Íæ¼Ò");
                    enemy.canApplyDamage = false;
                   
                }


                
            }
        }
    }

    private void ResetExplode()
    {
        isExplode=false;
    }
}
