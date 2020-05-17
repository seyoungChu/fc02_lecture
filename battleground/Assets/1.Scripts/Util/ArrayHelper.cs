using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ArrayHelper
{
    public static T[] Add<T>(T n, T[] list)
    {
        ArrayList tmp = new ArrayList();
        foreach (T item in list) tmp.Add(item);
        tmp.Add(n);
        return tmp.ToArray(typeof(T)) as T[];
    }

    public static T[] Remove<T>(int index, T[] list)
    {
        ArrayList tmp = new ArrayList();
        foreach (T item in list) tmp.Add(item);
        tmp.RemoveAt(index);
        return tmp.ToArray(typeof(T)) as T[];
    }

    public static T[] Copy<T>(T[] source)
    {
        if (source == null) return null;
        ArrayList tmp = new ArrayList();
        foreach (T item in source) tmp.Add(item);
        return tmp.ToArray(typeof(T)) as T[];
    }

    public static int[] Create(int p_count, int p_initialValue)
    {
        int[] __list = new int[p_count];
        for (int i = 0; i < p_count; i++) __list[i] = p_initialValue;
        return __list;
    }
    public static float[] Create(int p_count, float p_initialValue)
    {
        float[] __list = new float[p_count];
        for (int i = 0; i < p_count; i++) __list[i] = p_initialValue;
        return __list;
    }

    public static int[] Copy(int[] source)
    {
        if (source == null) return null;
        int[] __array = new int[source.Length];
        for (int i = 0; i < source.Length; i++) __array[i] = source[i];
        return __array;
    }

    public static float[] Copy(float[] source)
    {
        if (source == null) return null;
        float[] __array = new float[source.Length];
        for (int i = 0; i < source.Length; i++) __array[i] = source[i];
        return __array;
    }

    public static string[] Copy(string[] source)
    {
        if (source == null) return null;
        string[] __array = new string[source.Length];
        for (int i = 0; i < source.Length; i++) __array[i] = source[i];
        return __array;
    }

    public static string[] Add(string p_n, string[] p_list)
    {
        ArrayList __tmp = new ArrayList();
        foreach (string str in p_list) __tmp.Add(str);
        __tmp.Add(p_n);
        return __tmp.ToArray(typeof(string)) as string[];
    }

    public static string[] Remove(int p_index, string[] p_list)
    {
        ArrayList __tmp = new ArrayList();
        foreach (string str in p_list) __tmp.Add(str);
        __tmp.RemoveAt(p_index);
        return __tmp.ToArray(typeof(string)) as string[];
    }
    public static bool[] Add(bool p_n, bool[] p_list)
    {
        ArrayList __tmp = new ArrayList();
        foreach (bool str in p_list) __tmp.Add(str);
        __tmp.Add(p_n);
        return __tmp.ToArray(typeof(bool)) as bool[];
    }

    public static bool[] Remove(int p_index, bool[] p_list)
    {
        ArrayList __tmp = new ArrayList();
        foreach (bool str in p_list) __tmp.Add(str);
        __tmp.RemoveAt(p_index);
        return __tmp.ToArray(typeof(bool)) as bool[];
    }

    public static int[] Add(int p_n, int[] p_list)
    {
        ArrayList __tmp = new ArrayList();

        if (p_list != null)
            foreach (int str in p_list) __tmp.Add(str);

        __tmp.Add(p_n);
        return __tmp.ToArray(typeof(int)) as int[];
    }

    public static int[] Remove(int p_index, int[] p_list)
    {
        ArrayList __tmp = new ArrayList();
        foreach (int str in p_list) __tmp.Add(str);
        __tmp.RemoveAt(p_index);
        return __tmp.ToArray(typeof(int)) as int[];
    }

    public static float[] Add(float p_n, float[] p_list)
    {
        ArrayList __tmp = new ArrayList();
        foreach (float str in p_list) __tmp.Add(str);
        __tmp.Add(p_n);
        return __tmp.ToArray(typeof(float)) as float[];
    }

    public static float[] Remove(int p_index, float[] p_list)
    {
        ArrayList __tmp = new ArrayList();
        foreach (float str in p_list) __tmp.Add(str);
        __tmp.RemoveAt(p_index);
        return __tmp.ToArray(typeof(float)) as float[];
    }

    public static Vector3[] Add(Vector3 p_n, Vector3[] p_list)
    {
        ArrayList __tmp = new ArrayList();
        foreach (Vector3 str in p_list) __tmp.Add(str);
        __tmp.Add(p_n);
        return __tmp.ToArray(typeof(Vector3)) as Vector3[];
    }

    public static Vector3[] Merge(Vector3[] p_nList, Vector3[] p_list)
    {
        ArrayList __tmp = new ArrayList();
        foreach (Vector3 str in p_list) __tmp.Add(str);
        foreach (Vector3 str2 in p_nList) __tmp.Add(str2);
        return __tmp.ToArray(typeof(Vector3)) as Vector3[];
    }

    public static Vector3[] Remove(int p_index, Vector3[] p_list)
    {
        ArrayList __tmp = new ArrayList();
        foreach (Vector3 str in p_list) __tmp.Add(str);
        __tmp.RemoveAt(p_index);
        return __tmp.ToArray(typeof(Vector3)) as Vector3[];
    }

    public static Vector3[] Remove(Vector3[] p_list, params int[] index_list)
    {
        ArrayList __tmp = new ArrayList();
        for (int i = 0; i < p_list.Length; i++)
        {
            bool isContain = false;
            for (int j = 0; j < index_list.Length; j++)
            {
                if (i == index_list[j])
                {
                    isContain = true;
                    break;
                }
            }
            if (isContain == false)
            {
                __tmp.Add(p_list[i]);
            }
        }
        return __tmp.ToArray(typeof(Vector3)) as Vector3[];
    }
}
