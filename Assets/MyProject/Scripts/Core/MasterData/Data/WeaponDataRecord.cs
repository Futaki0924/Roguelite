using UnityEngine;
using System;
using System.Collections.Generic;

namespace Core.MasterData
{
    [Serializable]
    public class WeaponDataRecord : IMasterData
    {
        [field: SerializeField] public ulong Id { get; private set; }
        [field: SerializeField] public string WeaponName { get; private set; }
        [field: SerializeField] public int WeaponFireType { get; private set; }
        [field: SerializeField] public int AttackPower { get; private set; }
        [field: SerializeField] public float FireInterval { get; private set; }
        [field: SerializeField] public float FireRate { get; private set; }
        [field: SerializeField] public int MaxAmmo { get; private set; }
        [field: SerializeField] public float ReloadTime { get; private set; }
    }

    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "Scriptable Objects/WeaponData")]
    public class WeaponData : ScriptableObject, IMasterDataContainer<WeaponDataRecord>
    {
        [field: SerializeField] public List<WeaponDataRecord> Records { get; private set; }
    }
}
