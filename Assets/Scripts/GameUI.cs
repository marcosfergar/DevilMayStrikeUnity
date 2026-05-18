using UnityEngine;
using TMPro; // Es obligatorio añadir esto para poder controlar TextMeshPro por código

public class GameUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    public TextMeshProUGUI textoVida;
    public TextMeshProUGUI textoOleada;

    [Header("Referencias del Juego")]
    private PlayerMovement scriptJugador;
    private WaveManager scriptWaveManager;

    void Start()
    {
        // Buscamos los scripts en la escena para leer sus datos
        scriptJugador = FindFirstObjectByType<PlayerMovement>();
        scriptWaveManager = FindFirstObjectByType<WaveManager>();
    }

    void Update()
    {
        // Actualizamos el texto de la vida si el jugador existe
        if (scriptJugador != null)
        {
            textoVida.text = "VIDA: " + scriptJugador.currentHealth;
        }

        // Actualizamos el texto de la oleada si el manager existe
        if (scriptWaveManager != null)
        {
            textoOleada.text = "OLEADA: " + scriptWaveManager.currentWave;
        }
    }
}