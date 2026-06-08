using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Combat")]
    public KeyCode attackKey = KeyCode.Mouse0;
    public float attackRange = 5f;
    public int attackDamage = 1;

    private Rigidbody _rb;
    private Camera _mainCamera;
    private Vector3 _moveInput;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        _mainCamera = Camera.main;
        gameObject.tag = "Player";
    }

    private void Update()
    {
        HandleInput();
        HandleAttack();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        _moveInput = new Vector3(horizontal, 0, vertical);
        if (_moveInput.magnitude > 1f)
            _moveInput.Normalize();
    }

    private void HandleMovement()
    {
        if (_moveInput.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_moveInput, Vector3.up);
            _rb.rotation = Quaternion.Slerp(_rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        Vector3 velocity = _moveInput * moveSpeed;
        velocity.y = _rb.velocity.y;
        _rb.velocity = velocity;
    }

    private void HandleAttack()
    {
        if (!Input.GetKeyDown(attackKey)) return;
        if (_mainCamera == null) return;

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange))
        {
            EnemyDummy enemy = hit.collider.GetComponent<EnemyDummy>();
            if (enemy != null)
                enemy.TakeDamage(attackDamage);
        }
    }
}
