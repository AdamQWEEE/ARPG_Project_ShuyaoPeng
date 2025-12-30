using UnityEngine;

public class VolumeBall : MonoBehaviour
{
    public EnemyBase enemy;
    public Material volueball_bright;
    public Material volueball_dark;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemy=GameObject.Find("Boss").GetComponent<EnemyBase>();
        if (enemy.isDark)
        {
            GetComponent<MeshRenderer>().material = volueball_dark;
        }
        else
        {
            GetComponent<MeshRenderer>().material = volueball_bright;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale += new Vector3(1, 1, 1) * Time.deltaTime*4f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(enemy.isDark)
            {
                VolumeController.Instance.ChangeDarkVolume();
                Destroy(gameObject);
            }
            else
            {

                VolumeController.Instance.ChangeLightVolume();
                Destroy(gameObject);

            }
        }
    }
}
