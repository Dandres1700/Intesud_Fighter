using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Referencia al fondo del escenario (UI Image)")]
    [SerializeField] private Image stageBackground;

    [Header("Jugadores en la escena")]
    [SerializeField] private Animator jugador1Animator;
    [SerializeField] private Animator jugador2Animator;

    [Header("Referencias a los Transform de jugadores")]
    [SerializeField] private Transform jugador1Transform;
    [SerializeField] private Transform jugador2Transform;

    [Header("Animator Controllers")]
    [SerializeField] private RuntimeAnimatorController samuraiController;
    [SerializeField] private RuntimeAnimatorController knightController;
    [SerializeField] private RuntimeAnimatorController satyr1Controller;
    [SerializeField] private RuntimeAnimatorController satyr2Controller;
    [SerializeField] private RuntimeAnimatorController onreController;
    [SerializeField] private RuntimeAnimatorController gorgonaController;
    [SerializeField] private RuntimeAnimatorController esqueletoController;
    [SerializeField] private RuntimeAnimatorController kitsuneController;
    [SerializeField] private RuntimeAnimatorController reiController;
    [SerializeField] private RuntimeAnimatorController miguelController;

    [Header("Offset Y para cada personaje")]
    [SerializeField] private float satyrOffsetY = 3.5f;
    [SerializeField] private float samuraiOffsetY = 0f;
    [SerializeField] private float knightOffsetY = 0f;
    [SerializeField] private float onreOffsetY = 3.5f;
    [SerializeField] private float gorgonaOffsetY = 3.5f;
    [SerializeField] private float esqueletoOffsetY = 0f;
    [SerializeField] private float kitsuneOffsetY = 0f;
    [SerializeField] private float reiOffsetY = 0f;
    [SerializeField] private float miguelOffsetY = 0f;

    [Header("Escala por personaje")]
    [SerializeField] private float scaleSatyr = 1.8f;
    [SerializeField] private float scaleSamurai = 1.8f;
    [SerializeField] private float scaleKnight = 1.3f;
    [SerializeField] private float scaleOnre = 1.8f;
    [SerializeField] private float scaleGorgona = 1.8f;
    [SerializeField] private float scaleEsqueleto = 1.3f;
    [SerializeField] private float scaleKitsune = 1.3f;
    [SerializeField] private float scaleRei = 1.3f;
    [SerializeField] private float scaleMiguel = 1.3f;

    [Header("Audio del combate")]
    [SerializeField] private AudioSource musicSource;

    public StageData CurrentMap { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        LoadSelectedMap();
        LoadSelectedCharacters();
        ConfigurarModoJuego();
    }

    private void LoadSelectedMap()
    {
        CurrentMap = MapSelectManager.SelectedMap;

        if (CurrentMap == null)
        {
            Debug.LogWarning("[GameSession] No hay mapa seleccionado.");
            return;
        }

        if (stageBackground != null && CurrentMap.previewImage != null)
            stageBackground.sprite = CurrentMap.previewImage;

        if (musicSource != null && CurrentMap.stageMusic != null)
        {
            musicSource.clip = CurrentMap.stageMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        Debug.Log($"[GameSession] Mapa cargado: {CurrentMap.stageName}");
    }

    private void LoadSelectedCharacters()
    {
        var pJ1 = CharacterSelectManager.PersonajeJ1;
        var pJ2 = CharacterSelectManager.PersonajeJ2;

        if (pJ1 == null || pJ2 == null)
        {
            Debug.LogWarning("[GameSession] No hay personajes seleccionados.");
            return;
        }

        if (jugador1Animator != null)
            jugador1Animator.runtimeAnimatorController = ObtenerController(pJ1.nombre);

        if (jugador2Animator != null)
            jugador2Animator.runtimeAnimatorController = ObtenerController(pJ2.nombre);

        AjustarEscala(jugador1Transform, pJ1.nombre);
        AjustarEscala(jugador2Transform, pJ2.nombre);

        AjustarPosicionY(jugador1Transform, pJ1.nombre);
        AjustarPosicionY(jugador2Transform, pJ2.nombre);

        Debug.Log($"[GameSession] J1: {pJ1.nombre} | J2: {pJ2.nombre}");
    }

    private void ConfigurarModoJuego()
    {
        var modo = GameModeManager.ModoActual;

        // Jugador 1
        var pc1  = jugador1Transform?.GetComponent<PlayerController>();
        var cpu1 = jugador1Transform?.GetComponent<CPUController>();

        if (modo == GameModeManager.ModoJuego.CPUvsCPU)
        {
            // J1 es CPU
            if (pc1  != null) pc1.enabled  = false;
            if (cpu1 != null)
            {
                cpu1.enabled  = true;
                cpu1.objetivo = jugador2Transform;
            }
        }
        else
        {
            // J1 es humano
            if (pc1  != null) pc1.enabled  = true;
            if (cpu1 != null) cpu1.enabled  = false;
        }

        // Jugador 2
        var pc2  = jugador2Transform?.GetComponent<PlayerController2>();
        var cpu2 = jugador2Transform?.GetComponent<CPUController>();

        if (modo == GameModeManager.ModoJuego.J1vsCPU ||
            modo == GameModeManager.ModoJuego.CPUvsCPU)
        {
            // J2 es CPU
            if (pc2  != null) pc2.enabled  = false;
            if (cpu2 != null)
            {
                cpu2.enabled  = true;
                cpu2.objetivo = jugador1Transform;
            }
        }
        else
        {
            // J2 es humano
            if (pc2  != null) pc2.enabled  = true;
            if (cpu2 != null) cpu2.enabled  = false;
        }

        Debug.Log($"[GameSession] Modo: {modo} | Dificultad: {GameModeManager.DificultadActual}");
    }

    private void AjustarEscala(Transform t, string nombre)
    {
        if (t == null) return;
        string n = nombre.ToLower();

        float escala;
        if (n.Contains("ayanami") || n.Contains("asuka"))
            escala = scaleSatyr;
        else if (n.Contains("samurai"))
            escala = scaleSamurai;
        else if (n.Contains("onre"))
            escala = scaleOnre;
        else if (n.Contains("gorgona"))
            escala = scaleGorgona;
        else if (n.Contains("esqueleto"))
            escala = scaleEsqueleto;
        else if (n.Contains("kitsune"))
            escala = scaleKitsune;
        else if (n.Contains("rei"))
            escala = scaleRei;
        else if (n.Contains("miguel"))
            escala = scaleMiguel;
        else
            escala = scaleKnight;

        t.localScale = new Vector3(escala, escala, t.localScale.z);
    }

    private void AjustarPosicionY(Transform t, string nombre)
    {
        if (t == null) return;
        string n = nombre.ToLower();

        float offset = 0f;
        if (n.Contains("ayanami") || n.Contains("asuka"))
            offset = satyrOffsetY;
        else if (n.Contains("samurai"))
            offset = samuraiOffsetY;
        else if (n.Contains("onre"))
            offset = onreOffsetY;
        else if (n.Contains("gorgona"))
            offset = gorgonaOffsetY;
        else if (n.Contains("esqueleto"))
            offset = esqueletoOffsetY;
        else if (n.Contains("kitsune"))
            offset = kitsuneOffsetY;
        else if (n.Contains("rei"))
            offset = reiOffsetY;
        else if (n.Contains("miguel"))
            offset = miguelOffsetY;
        else
            offset = knightOffsetY;

        if (offset == 0f) return;

        Rigidbody2D rb = t.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.position = new Vector2(rb.position.x, rb.position.y + offset);
        else
        {
            Vector3 pos = t.position;
            pos.y += offset;
            t.position = pos;
        }

        Physics2D.SyncTransforms();
    }

    private RuntimeAnimatorController ObtenerController(string nombre)
    {
        string n = nombre.ToLower();
        if (n.Contains("ayanami")) return satyr1Controller;
        if (n.Contains("asuka"))   return satyr2Controller;
        if (n.Contains("samurai")) return samuraiController;
        if (n.Contains("onre"))    return onreController;
        if (n.Contains("gorgona")) return gorgonaController;
        if (n.Contains("esqueleto")) return esqueletoController;
        if (n.Contains("kitsune")) return kitsuneController;
        if (n.Contains("rei"))     return reiController;
        if (n.Contains("miguel"))  return miguelController;
        return knightController;
    }
}