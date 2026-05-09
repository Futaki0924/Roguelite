using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    const float MOVE_SPEED = 5f;

    [SerializeField] Rigidbody _rb;

    Vector3 _moveDirection;

    PlayerInputActions _inputActions;

    Vector2 _moveInput;

    public Vector3 CurrentVelocity { get; private set; }

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
        _inputActions.Player.Fire.performed += OnFire;
    }

    private void OnEnable()
    {
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    private void Update()
    {
        _moveInput = _inputActions.Player.Move.ReadValue<Vector2>();
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

        if(_moveInput == Vector2.zero)
        {
            _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
            CurrentVelocity = Vector3.zero;
            return;
        }

        Vector3 targetVelocity = new Vector3(_moveInput.x, _rb.linearVelocity.y, _moveInput.y) * MOVE_SPEED;

        _rb.linearVelocity = new Vector3(targetVelocity.x, _rb.linearVelocity.y, targetVelocity.z);
    }

    private void OnFire(InputAction.CallbackContext context)
    {
        Debug.Log("撃った！");
    }
}
