using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Singleton Template Class.
/// </summary>
public class SingletonMonobehaviour<T> : MonoBehaviour where T : Component
{
    private static T m_instance;
    public static T Instance => m_instance;

    public static T GetOrCreateInstance()
    {
        if (m_instance == null)
        {
            m_instance = (T)FindObjectOfType(typeof(T));

            if (m_instance == null)
            {
                GameObject _newGameObject = new GameObject(typeof(T).Name, typeof(T));
                m_instance = _newGameObject.GetComponent<T>();
            }
        }
        return m_instance;
    }

    protected virtual void Awake()
    {
        m_instance = this as T;
        if (Application.isPlaying == true)
        {
            if (transform.parent != null && transform.root != null)
            {
                DontDestroyOnLoad(transform.root.gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }
            
        }
    }
}