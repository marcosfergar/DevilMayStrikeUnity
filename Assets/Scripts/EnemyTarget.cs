using System.Collections;
using UnityEngine;

public class EnemyTarget : MonoBehaviour
{
    [Header("Configuracion Movimiento")]
    public float speed = 3f;
    private Transform playerTransform;
    private Rigidbody rb;

    [Header("Combate y Vida")]
    public int maxHealth = 3;
    private int currentHealth;
    private bool isStunned = false; // Bloquea el movimiento al ser golpeado

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();

        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log("EnemyTarget: ¡Jugador localizado con éxito!");
        }
        else
        {
            Debug.LogError("EnemyTarget: ¡ERROR! No se encuentra al jugador.");
        }
    }

    void FixedUpdate()
    {
        // Si está aturdido o no hay jugador, frenamos por completo el movimiento
        if (isStunned || playerTransform == null)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // Lógica de persecución normal
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0;
        direction = direction.normalized;

        rb.linearVelocity = direction * speed;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
        }
    }

public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemigo golpeado. Vida restante: " + currentHealth);

        // Si la vida baja de 0, morimos INMEDIATAMENTE
        if (currentHealth <= 0)
        {
            Die();
            return; // Ponemos este return para cortar el script aquí y que no intente hacer el HitEffect
        }

        // Si no ha muerto, hace el parpadeo normal
        StartCoroutine(HitEffect());
    }

    IEnumerator HitEffect()
    {
        isStunned = true; 

        Renderer renderer = GetComponent<Renderer>();
        Color originalColor = renderer.material.color;

        renderer.material.color = Color.white;
        yield return new WaitForSeconds(0.15f); 
        
        renderer.material.color = originalColor;
        isStunned = false; 
    }

void Die()
{
    Debug.Log("EnemyTarget: Enemigo destruido.");
    Destroy(gameObject); // Solo se destruye a sí mismo
}
}