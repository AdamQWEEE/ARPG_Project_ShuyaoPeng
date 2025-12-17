using Cinemachine;
using UnityEngine;

public class CameraModeController : Singleton<CameraModeController>
{
    public CinemachineVirtualCamera vcamFree;
    public CinemachineVirtualCamera vcamLock;
    public CinemachineTargetGroup lockTargetGroup;

    public Transform player;      // 玩家
    public Transform currentEnemy; // 当前锁定敌人，由锁定系统设置

    
    bool isLockOn;

    public void SetLockOn(bool value, Transform enemy = null)
    {
        isLockOn = value;
        currentEnemy = enemy;

        if (isLockOn && currentEnemy != null)
        {
            // 更新 TargetGroup：玩家 + 敌人
            lockTargetGroup.m_Targets = new CinemachineTargetGroup.Target[]
            {
                new CinemachineTargetGroup.Target { target = player,       weight = 1, radius = 0.5f },
                new CinemachineTargetGroup.Target { target = currentEnemy, weight = 1, radius = 0.5f }
            };

            // 提升锁定相机 Priority
            vcamLock.Priority = 20;   // > vcamFree
            vcamFree.Priority = 10;

            // 关闭自由相机的输入
            
        }
        else
        {
            // 退出锁定：只保留玩家在 TargetGroup 里（可选）
            lockTargetGroup.m_Targets = new CinemachineTargetGroup.Target[]
            {
                new CinemachineTargetGroup.Target { target = player, weight = 1, radius = 0.5f }
            };

            // 切回自由相机
            vcamLock.Priority = 5;
            vcamFree.Priority = 15;

            
        }
    }
}
