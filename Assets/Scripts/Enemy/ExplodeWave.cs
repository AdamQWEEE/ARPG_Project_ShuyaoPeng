using StarterAssets;
using UnityEngine;

public class ExplodeWave : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ThirdPersonController player=other.GetComponent<ThirdPersonController>();
            GetComponent<Collider>().enabled = false;
            //player.playerState.TakeDamage(30f);
            //Destroy(gameObject);
        }
    }
}
