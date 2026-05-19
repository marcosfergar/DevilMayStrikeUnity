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
        Debug.Log("Enemy: ¡El método Die() real se está ejecutando!");

        // Buscamos al script del jugador en la escena
        PlayerMovement jugador = FindFirstObjectByType<PlayerMovement>();
        
        if (jugador != null)
        {
            // Le sumamos los orbes directamente a su variable de economía
            jugador.orbesRojosPartida += orbesAlMorir;
            Debug.Log("¡Orbes transferidos al jugador! Total acumulado: " + jugador.orbesRojosPartida);
        }
        else
        {
            Debug.LogError("Enemy: No se pudo encontrar al jugador en la escena para darle los orbes.");
        }

        // Destruimos el cubo de la escena
        Destroy(gameObject);
    }
}