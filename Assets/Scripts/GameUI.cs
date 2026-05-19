using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // OBLIGATORIO para poder reiniciar y cargar escenas

public class GameUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    public TextMeshProUGUI textoVida;
    public TextMeshProUGUI textoOleada;
    public GameObject menuGameOver; // Aquí arrastraremos el contenedor del menú de muerte

    [Header("Referencias del Juego")]
    private PlayerMovement scriptJugador;
    private WaveManager scriptWaveManager;
    private bool juegoTerminado = false;

    void Start()
    {
        // Aseguramos que el juego funcione a velocidad normal al empezar
        Time.timeScale = 1f;

        scriptJugador = FindFirstObjectByType<PlayerMovement>();
        scriptWaveManager = FindFirstObjectByType<WaveManager>();

        // Al empezar la partida, el menú de Game Over tiene que estar ESCONDIDO
        if (menuGameOver != null)
        {
            menuGameOver.SetActive(false);
        }
    }

    void Update()
    {
        if (scriptJugador != null)
        {
            textoVida.text = "VIDA: " + scriptJugador.currentHealth;

            // Si el jugador se queda sin vida y aún no hemos activado el Game Over...
            if (scriptJugador.currentHealth <= 0 && !juegoTerminado)
            {
                ActivarGameOver();
            }
        }

        if (scriptWaveManager != null)
        {
            textoOleada.text = "OLEADA: " + scriptWaveManager.currentWave;
        }
    }

    public void ActivarGameOver()
    {
        juegoTerminado = true;
        
        // Mostramos la pantalla de muerte
        if (menuGameOver != null)
        {
            menuGameOver.SetActive(true);
        }

        // Congelamos el tiempo del juego
        Time.timeScale = 0f;
    }

    // Esta función la ejecutará el botón al hacer clic
    public void ReiniciarPartida()
    {
        // Volvemos a cargar la escena actual desde cero
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}