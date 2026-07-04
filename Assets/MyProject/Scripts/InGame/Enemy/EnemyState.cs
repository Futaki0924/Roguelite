using Core.Interface;
using TPSRoguelite.InGame.Data;
using UnityEngine;
using UnityEngine.Events;

namespace TPSRoguelite.InGame.Enemy
{
    public class EnemyState : MonoBehaviour, IDamageable
    {
        [field: SerializeField] public EnemyData EnemyDataAsset { get; private set; }

        public int CurrentHP { get; private set; }

        public event UnityAction<EnemyState> OnReturnToPoolAction;

        private void OnEnable()
        {
            if(EnemyDataAsset == null)
            {
                Debug.LogError("EnemyDataがセットされていません。");
                return;
            }

            CurrentHP = EnemyDataAsset.MaxHP;
        }

        public void TakeDamage(int damageAmount)
        {
            if(damageAmount <= 0)
            {
                return;
            }

            CurrentHP -= damageAmount;
            Debug.Log($"{EnemyDataAsset.EnemyName}に{damageAmount}のダメージ！残りHP: {CurrentHP}");

            if(CurrentHP <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"{EnemyDataAsset.EnemyName}を倒しました");
            gameObject.SetActive(false);
            OnReturnToPoolAction?.Invoke(this);
        }
    }
}
