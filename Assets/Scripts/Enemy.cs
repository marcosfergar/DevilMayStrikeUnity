using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Recompensa")]
    public int orbesAlMorir = 15;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemigo golpeado! Vida restante: " + currentHealth);

        StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator FlashDamage()
    {
        GetComponent<Renderer>().material.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        GetComponent<Renderer>().material.color = Color.red;
    }

    void Die()
    {
        Debug.Log("Enemy: ¡Enemigo derrotado con éxito!");

        PlayerMovement jugador = FindFirstObjectByType<PlayerMovement>();
        
        if (jugador != null)
        {
            // Calculamos los orbes aplicando el multiplicador que vino de la web
            // Usamos 'multiplicadorPuntos' que añadimos en el script del jugador
            int orbesFinales = Mathf.RoundToInt(orbesAlMorir * jugador.multiplicadorPuntos);

            jugador.orbesRojosPartida += orbesFinales; 
            Debug.Log($"¡Orbes transferidos! Base: {orbesAlMorir} x Mult: {jugador.multiplicadorPuntos} = Total: {orbesFinales}. Acumulado: {jugador.orbesRojosPartida}");
        }
        else
        {
            Debug.LogError("Enemy: No se pudo encontrar al jugador en la escena.");
        }

        Destroy(gameObject);
    }
}