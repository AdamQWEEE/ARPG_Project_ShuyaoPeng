using UnityEngine;
using UnityEngine.UI;

public class PlayerStateUI : MonoBehaviour
{
    public bool recoverEnergy;
    public Image energyBar;
    public float energy_per_attack;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (recoverEnergy && energyBar.fillAmount<1)
        {
            energyBar.fillAmount +=0.2f* Time.deltaTime;
        }
        else
        {
            recoverEnergy = false;
        }
    }

    public void ConsumeEnergy()
    {
        if (energyBar.fillAmount>0f)
        {
            energyBar.fillAmount -= energy_per_attack;
        }
        
    }
}
