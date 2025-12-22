using StarterAssets;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public ThirdPersonController playerController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
      
            if (playerController.canTakeDamage)
            {
                EnemyBase enemy = other.GetComponent<EnemyBase>();
                enemy.TakeDamage(8);
                if(enemy.currentHealth>0)
                    enemy.ApplyKnockback(playerController.transform.position);
                playerController.canTakeDamage = false;
            }
        }
    }
}
