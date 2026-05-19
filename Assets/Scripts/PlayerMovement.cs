using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;

public class PlayerMovement : MonoBehaviour
{

    // Este "import" le permite a Unity buscar una función llamada 'EnviarOrbesAWeb' en tu código Javascript de la página web
    [DllImport("__Internal")]
    private static extern void EnviarOrbesAWeb(int cantidad);

    [Header("Vida del Jugador")]
    public int maxHealth = 100;
    public int currentHealth;

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

    [Header("Economia del Juego")]
    public int orbesRojosPartida = 0; // Orbes acumulados en ESTA partida
    public float multiplicadorPuntos = 1f;
    private bool yaHaMuerto = false;


    [System.Serializable]
    public class WebStats
    {
        public float danio_bonus;
        public float vida_bonus;
        public float mult_puntos;
        public int dash_adicional;
    }

    public void AplicarMejorasWeb(string jsonDeLaWeb)
    {
        try
        {
            // Convertimos el texto JSON en variables de C#
            WebStats datos = JsonUtility.FromJson<WebStats>(jsonDeLaWeb);

            // 1. Aplicamos el bonus de daño (ej: daño base * multiplicador)
            attackDamage = Mathf.RoundToInt(attackDamage * datos.danio_bonus);

            // 2. Aplicamos el bonus de vida
            maxHealth = Mathf.RoundToInt(maxHealth * datos.vida_bonus);
            currentHealth = maxHealth; // Rellenamos la vida con el nuevo tope

            // 3. Guardamos el multiplicador de puntos para usarlo cuando muera un enemigo
            multiplicadorPuntos = datos.mult_puntos;

            Debug.Log($"[WEB] Stats aplicadas con éxito: Daño x{datos.danio_bonus}, Vida Max: {maxHealth}, Mult. Puntos x{datos.mult_puntos}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al procesar las estadísticas de la tienda web: " + e.Message);
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);

        right = Camera.main.transform.right;
        right.y = 0;
        right = Vector3.Normalize(right);

        #if UNITY_EDITOR
        // Creamos un texto que imita exactamente al JSON de Flask
        // Daño x3, Vida x2, Multiplicador de puntos x5, y Dash activado (1)
        string jsonSimulado = "{\"danio_bonus\":3.0, \"vida_bonus\":2.0, \"mult_puntos\":5.0, \"dash_adicional\":1}";
        
        Debug.Log("[EDITOR] Simulando la carga de la tienda web...");
        AplicarMejorasWeb(jsonSimulado);
        #endif
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("¡El jugador ha recibido daño! Vida actual: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    // Esta función la llamará el enemigo al morir en lugar de soltar un orbe
    public void GanarOrbes(int cantidad)
    {
        orbesRojosPartida += cantidad;
        Debug.Log("¡Orbes Rojos obtenidos! Total: " + orbesRojosPartida);
    }

    public void Die()
    {
        // Si ya procesamos la muerte, ignoramos las siguientes llamadas del Update
        if (yaHaMuerto) return; 
        
        yaHaMuerto = true;
        Debug.Log("¡El jugador ha muerto! Enviando orbes cosechados: " + orbesRojosPartida);

        // Invoca el Javascript de juego.html pasándole los puntos reales
        #if !UNITY_EDITOR && UNITY_WEBGL
            EnviarOrbesAWeb(orbesRojosPartida);
        #endif
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