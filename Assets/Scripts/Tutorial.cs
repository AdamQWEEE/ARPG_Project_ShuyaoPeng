using StarterAssets;
using UnityEngine;

public class Tutorial : Singleton<Tutorial>
{
    [Header("当前显示的提示")]
    public GameObject currentTip;

    [Header("各种提示")]
    public GameObject moveTip;
    public GameObject perspectiveTip;
    public GameObject jumpTip;
    public GameObject sneakTip;
    public GameObject lockTip;
    public GameObject attackTip;
    public GameObject heavyAttackTip;
    public GameObject tossTip;
    public GameObject executionTip;
    public GameObject counterTip;
    public GameObject rollTip;
    public GameObject swapTip;

    [Header("是否成功展示提示,后面就不再展示")]
    public bool isFinishMoveTip;
    public bool isFinishPerspectiveTip;
    public bool isFinishJumpTip;
    public bool isFinishSneakTip;
    public bool isFinishLockTip;
    public bool isFinishAttackTip;
    public bool isFinishHeavyAttackTip;
    public bool isFinishTossTip;
    public bool isFinishExecutionTip;
    public bool isFinishCounterTip;
    public bool isFinishRollTip;
    public bool isFinishSwapTip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public void HideTip()
    {
        currentTip.SetActive(false);
    }

    

    public void ShowMoveTip()
    {
        if(isFinishMoveTip) return;
        currentTip = moveTip;
        currentTip.SetActive(true);
    }

    public void ShowPerspectiveTip()
    {
        if(isFinishPerspectiveTip) return;
        currentTip=perspectiveTip;
        currentTip.SetActive(true);
    }

    public void ShowJumpTip()
    {
        if(isFinishJumpTip) return;
        currentTip=jumpTip;
        currentTip.SetActive(true);
    }

    public void ShowSneakTip()
    {
        if(isFinishSneakTip) return;
        currentTip=sneakTip;
        currentTip.SetActive(true);
    }



    public void ShowLockTip()
    {
        if(isFinishLockTip) return;
        currentTip=lockTip;
        currentTip.SetActive(true);
    }

    public void ShowAttackTip()
    {
        if(isFinishAttackTip) return;
        currentTip=attackTip;
        currentTip.SetActive(true);
    }

    public void ShowHeavyAttackTip()
    {
        if(isFinishHeavyAttackTip) return;
        currentTip=heavyAttackTip;
        currentTip.SetActive(true);
    }

    public void ShowTossTip()
    {
        if(isFinishTossTip) return;
        currentTip=tossTip;
        currentTip.SetActive(true);
    }

    public void ShowExecutionTip()
    {
        
        currentTip = executionTip;
        currentTip.SetActive(true);
    }
    
    
    public void ShowCounterTip()
    {
        if (isFinishCounterTip) return;
        Time.timeScale = 0.03f;
        currentTip = counterTip;
        currentTip.SetActive(true);
        //ThirdPersonController.Instance.ChangeCharacterControllerRadius(0.3f);
    }

    public void ShowRollTip()
    {
        if(isFinishRollTip) return;
        Time.timeScale = 0.03f;
        currentTip = rollTip;
        currentTip.SetActive(true);
        ThirdPersonController.Instance.ChangeCharacterControllerRadius(0.25f);
        Debug.Log("showrolltip");

    }

    public void ShowSwapTip()
    {
        if(isFinishSwapTip) return;
        currentTip=swapTip;
        currentTip.SetActive(true);
    }

    
}
