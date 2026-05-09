using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    const float MOVE_SPEED = 5f;

    [SerializeField] Rigidbody _rb;

    Vector3 _moveDirection = Vector3.zero;

    public Vector3 CurrentVelocity { get; private set; }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        _moveDirection = new Vector3(x, 0f, z).normalized;
    }

    private void FixedUpdate()
    {
        OnMove();
    }

    private void OnMove()
    {
        if(_rb == null)
        {
            Debug.LogError("Rigidbodyがないよ！！");
            return;
        }

        if(_moveDirection == Vector3.zero)
        {
            _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
            CurrentVelocity = Vector3.zero;
            return;
        }

        Vector3 targetVelocity = _moveDirection * MOVE_SPEED;

        _rb.linearVelocity = new Vector3(targetVelocity.x, _rb.linearVelocity.y, targetVelocity.z);
    }
}
