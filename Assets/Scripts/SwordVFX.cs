using UnityEngine;

public class SwordVFX : MonoBehaviour
{
    public GameObject[] swordEffects;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowVFX1()
    {
        foreach (var effect in swordEffects) 
        {
            if (effect!=null)
            {
                effect.SetActive(false);
            }
        }
        swordEffects[0].SetActive(true);
    }

    public void ShowVFX2()
    {
        foreach (var effect in swordEffects)
        {
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }
        swordEffects[1].SetActive(true);
    }
    public void ShowVFX3()
    {
        foreach (var effect in swordEffects)
        {
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }
        swordEffects[2].SetActive(true);
    }
}
