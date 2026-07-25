using UnityEngine;
using TPSRoguelite.InGame.Player;

namespace TPSRoguelite.InGame.Item
{
    public class ExperienceOrb : MonoBehaviour
    {
        const float MAGNET_RANGE = 5f;
        const float MAGNET_SPEED = 15f;
        const string PLAYER_TAG = "Player";

        Transform _playerTarget;
        bool _isFollowing;

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(PLAYER_TAG);

            if(playerObject != null)
            {
                _playerTarget = playerObject.transform;
            }
            else
            {
                Debug.LogWarning("Playerが見つかりませんでした。");
            }
        }

        private void Update()
        {
            if(_playerTarget == null)
            {
                return;
            }

            if (_isFollowing)
            {
                transform.position = Vector3.MoveTowards(transform.position, _playerTarget.position, MAGNET_SPEED * Time.deltaTime);
            }
            else
            {
                float distToPlayer = Vector3.Distance(transform.position, _playerTarget.position);
                if(distToPlayer <= MAGNET_RANGE)
                {
                    _isFollowing = true;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PLAYER_TAG))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if(player != null)
                {
                    player.AddExp(1);
                }
                else
                {
                    Debug.LogWarning("PlayerControllerが見つかりませんでした");
                }

                Destroy(gameObject);
            }
        }
    }
}