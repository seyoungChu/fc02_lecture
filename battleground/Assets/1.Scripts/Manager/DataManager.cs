using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private static EffectData effectData = null;
    // Start is called before the first frame update
    void Start()
    {
        if(effectData == null)
        {
            effectData = ScriptableObject.CreateInstance<EffectData>();
            effectData.LoadData();
        }
    }
    public static EffectData EffectData()
    {
        if(effectData == null)
        {
            effectData = ScriptableObject.CreateInstance<EffectData>();
            effectData.LoadData();
        }
        return effectData;
    }

    
}
