using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Kind
{
    Weapon, Helmet, Chest,
    NONE = 99
} // 아이템 종류

         [Serializable]
[CreateAssetMenu(fileName = "Weapon", menuName = "Item/Weapon")]
public class Weapon : ScriptableObject
{
    public Kind kind = Kind.Weapon;
    public int code = 0;
    public float Damage = 0;
    public float CrashDefense = 0;
    public float Defense = 0;
    public float StaminaMinus = 0;
    public float AttackSpeed = 0;
    public float AttackPointTime = 0;
    public float StunTime = 0;
    public float AttackDelay = 0;
}

[Serializable]
[CreateAssetMenu(fileName = "Helmet", menuName = "Item/Helmet")]
public class Helmet : ScriptableObject
{
    public Kind kind = Kind.Helmet;
    public int code = 0;
    public float Damage = 0;
    public float CrashDefense = 0;
    public float Defense = 0;
    public float StaminaMinus = 0;
    public float AttackSpeed = 0;
    public float AttackPointTime = 0;
    public float StunTime = 0;
    public float AttackDelay = 0;

}

[Serializable]
[CreateAssetMenu(fileName = "Chest", menuName = "Item/Chest")]
public class Chest : ScriptableObject
{
    public Kind kind = Kind.Chest;
    public int code = 0;
    public float Damage = 0;
    public float CrashDefense = 0;
    public float Defense = 0;
    public float StaminaMinus = 0;
    public float AttackSpeed = 0;
    public float AttackPointTime = 0;
    public float StunTime = 0;
    public float AttackDelay = 0;

}

public class ItemManager : MonoBehaviour
{
    public List<Weapon> weapons = new List<Weapon>();
    public List<Helmet> helmets = new List<Helmet>();
    public List<Chest> chests  = new List<Chest>();

    public static ItemManager instance = null;

    void Awake()
    {
        if (instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);      
    }
}