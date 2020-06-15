using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PluggableAI/GeneralStats")]
public class GeneralStats : ScriptableObject
{
    [Header("General")]
    [Tooltip("npc 정찰 속도 clear state")]
    public float patrolSpeed = 2f;
    [Tooltip("npc 따라오는 속도 warning state")]
    public float chaseSpeed = 5f;
    [Tooltip("npc 회피하는 속도 engage state")]    
    public float evadeSpeed = 15;
    [Tooltip("웨이포인트에서 대기하는 시간")]
    public float patrolWaitTime = 2f;
    [Header("Animation")]
    [Tooltip("장애물 레이어 마스크")]
    public LayerMask obstacleMask;
    [Tooltip("조준시 깜빡임을 피하기위한 최소 확정 앵글")]
    public float angleDeadZone = 5f;
    [Tooltip("속도 댐핑 시간")]
    public float speedDampTime = 0.4f;
    [Tooltip("각속도 댐핑 시간")]
    public float angularSpeedDampTime = 0.2f;
    [Tooltip("각속도안에서 각도 회전에 따른 반응 시간")]    
    public float angleResponseTime = 0.2f;
    [Header("Cover")]
    [Tooltip("장애물에 숨었을때 고려해야할 최소 높이값.")]
    public float aboveCoverHeight = 1.5f;
    [Tooltip("장애물레이어 마스크.")]
    public LayerMask coverMask;
    [Tooltip("사격 레이어 마스크.")]
    public LayerMask shotMask;
    [Tooltip("타겟 레이어 마스크.")]
    public LayerMask targetMask;

}
