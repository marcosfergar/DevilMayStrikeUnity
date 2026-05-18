using UnityEngine;

public class EnemyTarget : MonoBehaviour
{
    [Header("Configuracion Movimiento")]
    public float speed = 3f; // Velocidad de persecución
    private Transform playerTransform;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // El enemigo busca al jugador en la escena usando tu script de movimiento
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log("EnemyTarget: ¡Jugador localizado con éxito!");
        }
        else
        {
            Debug.LogError("EnemyTarget: ¡ERROR! No se encuentra al jugador en la escena. Asegúrate de que tu jugador tiene el script 'PlayerMovement'.");
        }
    }

    void FixedUpdate()
    {
        // Si el jugador existe en la escena, el enemigo lo persigue
        if (playerTransform != null)
        {
            // 1. Calculamos la dirección (Posición Jugador - Posición Enemigo)
            Vector3 direction = playerTransform.position - transform.position;
            direction.y = 0; // Evita que el cubo intente volar o enterrarse

            // 2. Normalizamos el vector para que la velocidad sea constante
            direction = direction.normalized;

            // 3. Aplicamos velocidad directa al Rigidbody
            rb.linearVelocity = direction * speed;

            // 4. Rotamos al enemigo para que mire al jugador en todo momento
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
            }
        }
    }
}