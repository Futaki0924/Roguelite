using Core.Interface;
using UnityEngine;

namespace TPSRoguelite.InGame.Enemy
{
    public class EnemyState : MonoBehaviour, IDamageable
    {
        const int MAX_HP = 100;

        public int CurrentHP { get; private set; }

        private void Awake()
        {
            CurrentHP = MAX_HP;
        }

        public void TakeDamage(int damageAmount)
        {
            if(damageAmount <= 0)
            {
                return;
            }

            CurrentHP -= damageAmount;
            Debug.Log($"敵に{damageAmount}のダメージ！残りHP: {CurrentHP}");

            if(CurrentHP <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("敵を倒しました");
            Destroy(gameObject);
        }
    }
}
