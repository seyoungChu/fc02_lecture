using UnityEngine;
using System.Collections;

public class EventHandler
{
    //이벤트 활성화에 필요한 컨디션 변수들.
    private static Hashtable variables;
    private static Hashtable numberVariables;
    
    /// <summary>
    /// 조건 클리어.
    /// </summary>
    public static void ClearData()
    {
        EventHandler.variables = new Hashtable();
        EventHandler.numberVariables = new Hashtable();
    }
    /// <summary>
    /// 키로 해당 조건을 가져온다.
    /// </summary>
    public static string GetVariable(string key)
    {
        if (EventHandler.variables.ContainsKey(key))
        {
            return EventHandler.variables[key] as string;
        }
        return string.Empty;
    }
    /// <summary>
    /// 조건 값 세팅.
    /// </summary>
    public static void SetVariable(string key, string value)
    {
        //Debug.Log("SetVariable Called");
        if (EventHandler.variables.ContainsKey(key))
        {
            EventHandler.variables[key] = value;
        }
        else
        {
            EventHandler.variables.Add(key, value);
        }
    }
    /// <summary>
    /// 조건 값 제거.
    /// </summary>
    public static void RemoveVariable(string key)
    {
        if (EventHandler.variables.ContainsKey(key))
            EventHandler.variables.Remove(key);
    }
    /// <summary>
    /// 조건 체크.
    /// </summary>
    public static bool CheckVariable(string key, string value)
    {
        if (EventHandler.variables == null)
            return false;

        bool check = false;
        if (EventHandler.variables.ContainsKey(key) &&
            EventHandler.variables[key] as string == value)
        {
            check = true;
        }
        return check;
    }
    /// <summary>
    /// 이미 있는 조건 값인지 확인.
    /// </summary>
    public static bool HasVariable(string key)
    {
        bool check = false;
        if (EventHandler.variables != null && EventHandler.variables.ContainsKey(key))
        {
            check = true;
        }
        return check;

    }

    ///   Number variable functions

    ///<summary>
    /// 키로 숫자 조건을 가져온다.
    ///</summary>
    public static float GetNumberVariable(string key)
    {
        float value = 0;
        if (EventHandler.numberVariables.ContainsKey(key))
        {
            value = (float)EventHandler.numberVariables[key];
        }
        return value;
    }
    /// <summary>
    /// 숫자 조건을 세팅.
    /// </summary>
    public static void SetNumberVariable(string key, float value)
    {
        if (EventHandler.numberVariables.ContainsKey(key))
        {
            EventHandler.numberVariables[key] = value;
        }
        else
        {
            EventHandler.numberVariables.Add(key, value);
        }
    }
    /// <summary>
    /// 숫자 조건 제거.
    /// </summary>
    public static void RemoveNumberVariable(string key)
    {
        EventHandler.numberVariables.Remove(key);
    }
    /// <summary>
    /// 숫자 조건 확인.
    /// </summary>
    public static bool CheckNumberVariable(string key, float value, ValueCheck type)
    {
        bool check = false;
        if (EventHandler.numberVariables.ContainsKey(key) &&             // numberVariables : hashTable
           ((ValueCheck.EQUALS.Equals(type) && (float)EventHandler.numberVariables[key] == value) ||       //enum ValueCheck {EQUALS, LESS, GREATER};
            (ValueCheck.LESS.Equals(type) && (float)EventHandler.numberVariables[key] < value) ||             // type : ValueCheck type
            (ValueCheck.GREATER.Equals(type) && (float)EventHandler.numberVariables[key] > value)))
        {
            check = true;
        }
        return check;
    }

}
