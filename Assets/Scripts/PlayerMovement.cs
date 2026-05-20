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
    private Vector3 direction; 

    [Header("Dash (Esquive)")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;        // Tiempo que tarda en recargarse UNA carga entera
    public float margenEntreDashes = 0.15f; // Margen mínimo para que no se ejecuten dos dashes en el mismo frame
    
    [Header("Progreso del Dash (Cargas)")]
    public int maxDashes = 1;               // Cuántas cargas máximas de dash tienes disponibles
    private int currentDashes;              // Cuántas cargas te quedan actualmente
    private bool isDashing = false;
    private float nextDashTimeAllowed = 0f;

    [Header("Combate Cuerpo a Cuerpo")]
    public Transform attackPoint; // Objeto vacío hijo frente al modelo
    public float attackRange = 1.5f; 
    public LayerMask enemyLayers; // Capa de los enemigos
    public int attackDamage = 1; 
    public float attackRate = 0.4f; // Cooldown entre golpes
    private float nextAttackTime = 0f;
    private int danioBaseInicial;   // Guarda el daño asignado en el Inspector antes del bonus web

    [Header("Ataque a Distancia (Ebony & Ivory)")]
    public GameObject bulletPrefab;       // El Prefab de la bala
    public Transform leftPistolMuzzle;    // Punto de salida de la pistola izquierda
    public Transform rightPistolMuzzle;   // Punto de salida de la pistola derecha
    public float fireRate = 0.2f;          // Cadencia de disparo
    
    private float nextFireTime = 0f;
    private bool fireLeftHand = true;      // Controla el orden alterno de las pistolas
    private Camera mainCamera;

    [Header("Economia del Juego")]
    public int orbesRojosPartida = 0; // Orbes acumulados en ESTA partida
    public float multiplicadorPuntos = 1f; 
    private bool yaHaMuerto = false; 

    // --- VARIABLES INTERNAS DEL MOTOR ---
    private Animator anim; // El "cerebro" de animación de Dante (¡CORREGIDO: Faltaba declarar!)

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
            WebStats datos = JsonUtility.FromJson<WebStats>(jsonDeLaWeb);

            // Ajustamos el daño basándonos en el valor inicial
            float factorDanio = datos.danio_bonus > 0 ? datos.danio_bonus : 1f;
            attackDamage = Mathf.RoundToInt(danioBaseInicial * factorDanio);

            float factorVida = datos.vida_bonus > 0 ? datos.vida_bonus : 1f;
            maxHealth = Mathf.RoundToInt(100 * factorVida); // Escalamos sobre la base de 100 de vida
            currentHealth = maxHealth;

            if (datos.mult_puntos > 0) {
                multiplicadorPuntos = datos.mult_puntos;
            } else {
                multiplicadorPuntos = 1f;
            }

            // Si hay dash adicional comprado en la web, aumentamos las cargas máximas disponibles
            if (datos.dash_adicional > 0) {
                maxDashes = 1 + datos.dash_adicional;
            } else {
                maxDashes = 1;
            }
            currentDashes = maxDashes;

            Debug.Log($"[WEB] Stats aplicadas -> Daño: {attackDamage} (x{factorDanio}), Vida Max: {maxHealth}, Mult. Puntos x{multiplicadorPuntos}, Dashes Totales: {maxDashes}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error leyendo el JSON: " + e.Message);
        }
    }

    void Start()
    {
        danioBaseInicial = attackDamage; // Guardamos el valor original puesto en el inspector
        currentHealth = maxHealth; 
        
        forward = Camera.main.transform.forward; 
        forward.y = 0; 
        forward = Vector3.Normalize(forward); 

        right = Camera.main.transform.right; 
        right.y = 0; 
        right = Vector3.Normalize(right); 

        mainCamera = Camera.main; 

        // Inicializamos componentes del jugador y animaciones
        currentDashes = maxDashes;
        anim = GetComponentInChildren<Animator>();
        if (anim == null) { Debug.LogError("¡ALERTA! El script no encuentra el Animator en Dante."); }
        #if UNITY_EDITOR
        // Simulador de la pasarela Web de Flask para pruebas locales
        string jsonSimulado = "{\"danio_bonus\":3.0, \"vida_bonus\":2.0, \"mult_puntos\":5.0, \"dash_adicional\":1}";
        Debug.Log("[EDITOR] Simulando la carga de la tienda web...");
        AplicarMejorasWeb(jsonSimulado);
        #endif
    }

    void Update()
    {
        if (isDashing) return; 

        float horizontalInput = Input.GetAxisRaw("Horizontal"); 
        float verticalInput = Input.GetAxis("Vertical"); 
   
        direction = (horizontalInput * right) + (verticalInput * forward); 

        // --- MOVIMIENTO DEL JUGADOR ---
        if (direction.magnitude > 0.1f) 
        {
            transform.position += direction * speed * Time.deltaTime; 
        }

        // --- ROTACIÓN CONTINUA ---
        GirarHaciaElRaton();

        // --- RECARGA PASIVA DE DASHES ---
        if (currentDashes < maxDashes && !isDashing && Time.time >= nextDashTimeAllowed)
        {
            StartCoroutine(RecargarCargaDash());
        }

        // --- INPUT DEL DASH ---
        if (Input.GetKeyDown(KeyCode.LeftShift) && currentDashes > 0 && direction.magnitude > 0.1f && Time.time >= nextDashTimeAllowed)
        {
            StartCoroutine(PerformDash());
        }

        // --- ENTRADA DE COMBATE ---
        // Clic Izquierdo: Espadazo (Rebellion)
        if (Time.time >= nextAttackTime && Input.GetMouseButtonDown(0)) 
        {
            Attack(); 
            nextAttackTime = Time.time + attackRate; 
        }

        // Clic Derecho: Disparar pistolas (Ebony & Ivory)
        if (Input.GetMouseButton(1) && Time.time >= nextFireTime)
        {
            DispararPistolas();
            nextFireTime = Time.time + fireRate;
        }

        // --- CONTROL DE ANIMACIÓN DE MOVERSE (¡CORREGIDO: Ahora usa 'direction'!) ---
        if (anim != null)
        {
            bool estaMoviendose = (direction.magnitude > 0.1f);
            anim.SetBool("isWalking", estaMoviendose);
        }
    }

    // Corrutina para el Dash continuo con consumo de cargas individuales
    IEnumerator PerformDash()
    {
        isDashing = true;
        currentDashes--; // Consumimos una carga de forma inmediata
        Debug.Log($"[DASH] Esquive realizado. Cargas restantes: {currentDashes}/{maxDashes}");

        Vector3 dashDirection = direction.normalized;
        float timer = 0f;

        while (timer < dashDuration)
        {
            transform.position += dashDirection * dashSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null; 
        }

        isDashing = false;
        nextDashTimeAllowed = Time.time + margenEntreDashes;
    }

    // Corrutina de refresco pasivo para recuperar cargas del Dash
    IEnumerator RecargarCargaDash()
    {
        nextDashTimeAllowed = Time.time + dashCooldown;
        yield return new WaitForSeconds(dashCooldown);
        
        if (currentDashes < maxDashes)
        {
            currentDashes++;
            Debug.Log($"[DASH] Carga recuperada. Disponibles: {currentDashes}/{maxDashes}");
        }
    }

    void Attack()
    {
        // Activa la animación de ataque si existe en tu controlador (ej: anim.SetTrigger("attack");)
        if (anim != null) anim.SetTrigger("attack"); 

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.GetComponent<Enemy>() != null)
            {
                enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
            }
            else if (enemy.GetComponent<EnemyTarget>() != null)
            {
                enemy.GetComponent<EnemyTarget>().TakeDamage(attackDamage);
            }
        }
    }

    void GirarHaciaElRaton()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        float rayDistance;

        if (playerPlane.Raycast(ray, out rayDistance))
        {
            Vector3 pointToLook = ray.GetPoint(rayDistance);
            
            // 🔥 NOTA: Mantenemos tu inversión para orientar correctamente la espalda de la malla si viene girada
            Vector3 lookDirection = -(pointToLook - transform.position);
            lookDirection.y = 0f;

            if (lookDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 25f * Time.deltaTime);
            }
        }
    }

    void DispararPistolas()
    {
        Transform puntoDisparoActual = fireLeftHand ? leftPistolMuzzle : rightPistolMuzzle;
        if (puntoDisparoActual == null) puntoDisparoActual = transform;

        GameObject nuevaBala = Instantiate(bulletPrefab, puntoDisparoActual.position, transform.rotation);
        
        Bullet scriptBala = nuevaBala.GetComponent<Bullet>();
        if (scriptBala != null)
        {
            scriptBala.SetDamage(attackDamage);
        }

        Debug.Log($"[GUNS] {(fireLeftHand ? "Ebony (Izquierda)" : "Ivory (Derecha)")} disparó. Daño aplicado: {attackDamage}");
        fireLeftHand = !fireLeftHand;
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

    public void GanarOrbes(int cantidad)
    {
        orbesRojosPartida += cantidad;
        Debug.Log("¡Orbes Rojos obtenidos! Total: " + orbesRojosPartida);
    }

    public void Die()
    {
        if (yaHaMuerto) return; 
        
        yaHaMuerto = true;
        Debug.Log("¡El jugador ha muerto! Enviando orbes cosechados: " + orbesRojosPartida);

        #if !UNITY_EDITOR && UNITY_WEBGL
            EnviarOrbesAWeb(orbesRojosPartida);
        #endif
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}