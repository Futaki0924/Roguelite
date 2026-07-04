using UnityEngine;

namespace TPSRoguelite.InGame.Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [field: SerializeField] public string EnemyName { get; private set; }
        [field: SerializeField] public int MaxHP { get; private set; }
        [field: SerializeField] public float MoveSpeed { get; private set; }
    }
}