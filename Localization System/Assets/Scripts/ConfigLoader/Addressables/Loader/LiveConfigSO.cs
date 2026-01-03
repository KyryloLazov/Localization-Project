using System.Collections.Generic;
using UnityEngine;

public abstract class LiveConfigSO : ScriptableObject
{
    [SerializeField] private string _key;

    public string Key
    {
        get { return _key; }
    }

    public virtual string GetSection()
    {
        return null;
    }
}