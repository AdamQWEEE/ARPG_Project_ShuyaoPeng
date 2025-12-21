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
                
                other.GetComponent<EnemyBase>().TakeDamage(20);
               playerController.canTakeDamage = false;
            }
        }
    }
}
