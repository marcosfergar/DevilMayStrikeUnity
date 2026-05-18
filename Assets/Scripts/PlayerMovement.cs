using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 4;
    public float rotationSpeed = 10;
    private Vector3 forward, right;
    private Vector3 direction; // La hacemos variable de clase para usarla en el Dash

    [Header("Dash (Esquive)")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing = false;
    private bool canDash = true;

    [Header("Combate")]
    public Transform attackPoint; // Objeto vacío hijo frente al cubo
    public float attackRange = 1.5f;
    public LayerMask enemyLayers; // Capa de los enemigos
    public int attackDamage = 1;
    public float attackRate = 0.4f; // Cooldown entre golpes
    private float nextAttackTime = 0f;

    void Start()
    {
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);

        right = Camera.main.transform.right;
        right.y = 0;
        right = Vector3.Normalize(right);
    }

    void Update()
    {
        // Si está haciendo un Dash, bloqueamos el control normal y el ataque
        if (isDashing) return;

        float horizontalInput = Input.GetAxisRaw("Horizontal"); // Usamos GetAxisRaw para que sea más responsivo
        float verticalInput = Input.GetAxis("Vertical");
   
        direction = (horizontalInput * right) + (verticalInput * forward);

        // 1. Lógica de Movimiento y Rotación (Tu código original)
        if (direction.magnitude > 0.1f) 
        {
            transform.position += direction * speed * Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 2. Input del Dash (Shift Izquierdo)
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && direction.magnitude > 0.1f)
        {
            StartCoroutine(PerformDash());
        }

        // 3. Input de Ataque (Clic Izquierdo)
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Attack();
                nextAttackTime = Time.time + attackRate;
            }
        }
    }

    // Corrutina para el Dash estilo Hades (Inmune/Rápido)
    IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;

        // Calculamos la dirección fija del dash en el momento de pulsar el botón
        Vector3 dashDirection = direction.normalized;
        float timer = 0f;

        while (timer < dashDuration)
        {
            // Mueve al jugador hacia adelante en la dirección del dash independientemente de los inputs
            transform.position += dashDirection * dashSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null; // Espera al siguiente frame
        }

        isDashing = false;

        // Cooldown antes de poder usarlo otra vez
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void Attack()
    {
        // Detectar enemigos en un círculo al frente
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

        // Dañar enemigos
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.GetComponent<Enemy>() != null)
            {
                enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
            }
        }
    }

    // Dibuja el rango de ataque en el editor para que puedas calibrarlo
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}