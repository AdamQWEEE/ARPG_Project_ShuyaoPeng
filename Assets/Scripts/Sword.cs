using StarterAssets;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public ThirdPersonController playerController;
    public Transform stabWeaponTransform;
    public Transform originalTransform;
    public float knockCoolTime;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        knockCoolTime-=Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
      
            if (playerController.canTakeDamage)
            {
                EnemyBase enemy = other.GetComponent<EnemyBase>();
                if (playerController.isDark != enemy.isDark) { 

                    enemy.TakeDamage(8);
                    enemy.AddStance(5f);
                    if (enemy.currentHealth > 0)
                    {
                        if (Random.Range(0, 100) > 40 && knockCoolTime<=0)
                        {
                            enemy.ApplyKnockback(playerController.transform.position);
                            knockCoolTime = 2f;

                        }
                    }

                }
                else
                {
                    enemy.TakeDamage(2);
                }
                

                playerController.canTakeDamage = false;
            }
        }
    }

    public void SetStabTransform()
    {
        CancelInvoke();
        transform.localPosition = stabWeaponTransform.localPosition;
        transform.localRotation = stabWeaponTransform.localRotation;
        Invoke("ResetSwordTransform", 2f);
    }

    public void ResetSwordTransform()
    {
        transform.localPosition=originalTransform.localPosition;
        transform.localRotation = originalTransform.localRotation;
    }
}
