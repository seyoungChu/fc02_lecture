using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyDelayed : MonoBehaviour
{
    public float DelayTime = 0.5f;

    private void Start()
    {
        Destroy(gameObject, DelayTime);        
    }
}
