using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class EnemyVariables
{
    public bool feelAlert;
    public bool hearAlert;
    public bool advanceCoverDecision;
    public int waitRounds;
    public bool repeatShot;
    public float waitInCoverTime;
    public float coverTime;
    public float patrolTimer;
    public float shotTimer;
    public float startShootTimer;
    public float currentShots;
    public float shotsInRounds;
    public float blindEngageTimer;
}
