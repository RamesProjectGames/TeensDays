using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SerializableList<T> 
{
    public List<T> list;
    public SerializableList() 
    {
        list = new List<T>();
    }
    public SerializableList(List<T> list) 
    {
        this.list = list;
    }
    public void Add(T item) 
    {
        list.Add(item);
    }
    public void Clear() 
    {
        list.Clear();
    }
    public bool Contains(T item) 
    {
        return list.Contains(item);
    }
    public int Count() 
    {
        return list.Count;
    }
    public void Remove(T item) 
    {
        list.Remove(item);
    }
    public T[] ToArray() 
    {
        return list.ToArray();
    }
}
