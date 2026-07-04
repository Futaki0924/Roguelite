using UnityEngine;
using TPSRoguelite.InGame.Enum;

namespace TPSRoguelite.InGame.Data
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        [field: SerializeField] public string WeaponName { get; private set; }
        [field: SerializeField] public FireType WeaponFireType { get; private set; }
        [field: SerializeField] public int AttackPower { get; private set; }
        [field: SerializeField] public float FireInterval { get; private set; }
        [field: SerializeField] public float FireRate { get; private set; }
        [field: SerializeField] public int MaxAmmo { get; private set; }
        [field: SerializeField] public float ReloadTime { get; private set; }
    }
}
