using UnityEngine;
using System;
using System.Collections.Generic;

namespace Core.MasterData
{
    [Serializable]
    public class EnemyDataRecord : IMasterData
    {
        [field: SerializeField] public ulong Id { get; private set; }
        [field: SerializeField] public string EnemyName { get; private set; }
        [field: SerializeField] public int MaxHP { get; private set; }
        [field: SerializeField] public float MoveSpeed { get; private set; }

    }

    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "Scriptable Objects/EnemyData")]
    public class EnemyData : ScriptableObject, IMasterDataContainer<EnemyDataRecord>
    {
        [field: SerializeField] public List<EnemyDataRecord> Records { get; private set; }
    }
}
