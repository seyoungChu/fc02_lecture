using UnityEditor;
using UnityEngine;
using System.Collections;

/// <summary>
/// BaseInspector - 인스펙터에 보여줄 클래스들의 기반 클래스.
/// </summary>
public class BaseInspector : Editor
{

    /// <summary>
    /// 툴에 공용적으로 사용될 인자들. 에디터창에서 몇개의 Fold UI를 사용할지 몰라 넉넉히 만들어 두었다.
    /// </summary>
    protected bool _isFold1 = true;
    protected bool _isFold2 = true;
    protected bool _isFold3 = true;
    protected bool _isFold4 = true;
    protected bool _isFold5 = true;
    protected bool _isFold6 = true;
    protected bool _isFold7 = true;


    /// <summary>
    /// 인스펙터에서 공통적으로 사용될 Editor UI 함수 - 시작 타입 설정하는 곳에 사용됨.
    /// </summary>
    /// <param name="p_Target"></param>
    public void EventStartSettings(BaseInteraction p_Target)
    {
        //+ 시작 타입을 지정.
        p_Target.startType = (EventStartType)EditorGUILayout.EnumPopup("Start type", p_Target.startType);
        //+ 만약 마우스 클릭이나 터치가 시작타입이라면 클릭영역 크기를 지정.
        if (EventStartType.INTERACT.Equals(p_Target.startType))
        {
            p_Target.maxMouseDistance = EditorGUILayout.FloatField("Max mouse distance", p_Target.maxMouseDistance);
        } //+ 만약 시작타입이 키입력이라면 입력 키를 지정할 수 있다.
        else if (EventStartType.KEY_PRESS.Equals(p_Target.startType))
        {
            p_Target.keyToPress = EditorGUILayout.TextField("Key to press", p_Target.keyToPress); //ex) p,space,alpha1..
            p_Target.keyPressInTrigger = EditorGUILayout.Toggle("In trigger", p_Target.keyPressInTrigger);
        }
        //+ 수정되었다면 Dirty를 발생시켜 적용시켜 준다.
        if (GUI.changed)
        {
            EditorUtility.SetDirty(p_Target);
        }
    }
    /// <summary>
    /// 인스펙터에서 공통적으로 사용될 Editor UI함수 - 시작 값이나 필요 값들을 설정하는 곳에 사용됨.
    /// </summary>
    /// <param name="p_Target"></param>
    public void VariableSettings(BaseInteraction p_Target)
    {
        //+ 타이틀.
        GUILayout.Label("Variable conditions", EditorStyles.boldLabel);
        //+ 필요 조건과 조건이 일회성으로 자동 삭제 될것인지 설정.
        if (p_Target.variableKey.Length > 0 || p_Target.numberVarKey.Length > 0)
        {
            p_Target.needed = (AIConditionNeeded)EditorGUILayout.EnumPopup("Needed", p_Target.needed);
            p_Target.autoDestroyOnVariables = EditorGUILayout.Toggle("Auto destroy", p_Target.autoDestroyOnVariables);
        }
        //+ 값 추가 버튼.
        if (GUILayout.Button("Add Variable", GUILayout.Width(150)))
        {
            p_Target.AddVariableCondition();
        }
        if (p_Target.variableKey.Length > 0)
        {
            for (int i = 0; i < p_Target.variableKey.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Remove", GUILayout.Width(75)))
                {
                    p_Target.RemoveVariableCondition(i);
                    return;
                }
                //+값 설정.
                p_Target.checkType[i] = EditorGUILayout.Toggle(p_Target.checkType[i], GUILayout.Width(20));
                p_Target.variableKey[i] = EditorGUILayout.TextField(p_Target.variableKey[i]);
                if (p_Target.checkType[i]) GUILayout.Label("== ");
                else GUILayout.Label(" != ");
                p_Target.variableValue[i] = EditorGUILayout.TextField(p_Target.variableValue[i]);
                EditorGUILayout.EndHorizontal();
            }
        }
        //+ 숫자 값 추가.
        if (GUILayout.Button("Add Number Variable", GUILayout.Width(150)))
        {
            p_Target.AddNumberVariableCondition();
        }
        if (p_Target.numberVarKey.Length > 0)
        {
            for (int i = 0; i < p_Target.numberVarKey.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Remove", GUILayout.Width(75)))
                {
                    p_Target.RemoveNumberVariableCondition(i);
                    return;
                }
                //+ 숫자 값 설정.
                p_Target.numberCheckType[i] = EditorGUILayout.Toggle(p_Target.numberCheckType[i], GUILayout.Width(20));
                p_Target.numberVarKey[i] = EditorGUILayout.TextField(p_Target.numberVarKey[i]);
                if (!p_Target.numberCheckType[i]) GUILayout.Label("not");
                p_Target.numberValueCheck[i] = (ValueCheck)EditorGUILayout.EnumPopup(p_Target.numberValueCheck[i]);
                p_Target.numberVarValue[i] = EditorGUILayout.FloatField(p_Target.numberVarValue[i]);
                EditorGUILayout.EndHorizontal();
            }
        }
        //+ 수정된 것이 있다면  Dirty를 발생시켜 화면에 적용시켜 준다.
        if (GUI.changed)
            EditorUtility.SetDirty(p_Target);
    }

}
