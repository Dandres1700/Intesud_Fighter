using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Tiempo")]
    public float tiempoTotal = 99f;
    public TextMeshProUGUI timerText;

    [Header("Jugadores")]
    public GameObject jugador1;
    public GameObject jugador2;

    [Header("UI")]
    public GameObject pantallaVictoria;
    public GameObject botonRevancha;
    public GameObject botonSalir;
    public TextMeshProUGUI textoGanador;
    public TextMeshProUGUI textoRonda;

    [Header("Indicadores de Ronda")]
    public GameObject[] rondasJ1;
    public GameObject[] rondasJ2;

    [Header("Navegación mando - pantalla final")]
    [SerializeField] private Color colorSeleccionado = new Color(0.25f, 0.25f, 0.25f, 1f);
    [SerializeField] private float cooldownNavegacion = 0.25f;

    private float tiempoActual;
    private bool juegoTerminado = false;
    private bool listaParaJugar = false;
    private bool pantallaFinalActiva = false;

    private int rondasGanadasJ1 = 0;
    private int rondasGanadasJ2 = 0;
    private int rondaActual = 1;
    private const int rondasParaGanar = 2;

    private Vector3 posicionInicialJ1;
    private Vector3 posicionInicialJ2;
    private Vector3 escalaInicialJ1;
    private Vector3 escalaInicialJ2;

    private int indiceBotonFinal = 0;
    private float timerCooldown = 0f;
    private bool stickEnReposo = true;
    private Button[] botonesFinal;
    private Image[] imagenesBotonesFinal;
    private Color[] coloresOriginalesFinal;
    private GameObject[] controlesFinales = new GameObject[0];
    private bool controlesFinalesCacheados = false;

    // Referencias a los controllers para manejar CPU y humanos
    private PlayerController pc1;
    private PlayerController2 pc2;
    private CPUController cpu1;
    private CPUController cpu2;

    void Start()
    {
        // Cachear referencias
        pc1  = jugador1.GetComponent<PlayerController>();
        pc2  = jugador2.GetComponent<PlayerController2>();
        cpu1 = jugador1.GetComponent<CPUController>();
        cpu2 = jugador2.GetComponent<CPUController>();

        CachearControlesFinales();
        MostrarControlesFinales(false);

        StartCoroutine(IniciarConDelay());
    }

    IEnumerator IniciarConDelay()
    {
        yield return null;
        posicionInicialJ1 = jugador1.transform.position;
        posicionInicialJ2 = jugador2.transform.position;
        escalaInicialJ1   = jugador1.transform.localScale;
        escalaInicialJ2   = jugador2.transform.localScale;
        listaParaJugar = true;
        IniciarRonda();
    }

    void Update()
    {
        if (pantallaFinalActiva)
        {
            timerCooldown -= Time.deltaTime;
            ManejarInputMandoPantallaFinal();
            return;
        }

        if (!listaParaJugar) return;
        if (juegoTerminado) return;

        tiempoActual -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(tiempoActual).ToString();

        if (tiempoActual <= 0)
        {
            tiempoActual = 0;
            TerminarRondaPorTiempo();
            return;
        }

        if (!jugador1.activeSelf) TerminarRonda(2);
        else if (!jugador2.activeSelf) TerminarRonda(1);
    }

    private void ManejarInputMandoPantallaFinal()
    {
        if (botonesFinal == null || botonesFinal.Length == 0) return;

        float ejeV = PadInput.ReadAnyMove(2).y;

        if (Mathf.Abs(ejeV) < 0.3f)
            stickEnReposo = true;
        else if (stickEnReposo && timerCooldown <= 0f)
        {
            stickEnReposo = false;
            timerCooldown = cooldownNavegacion;

            int nuevoIndice = indiceBotonFinal;
            if (ejeV > 0.6f)
                nuevoIndice = (indiceBotonFinal - 1 + botonesFinal.Length) % botonesFinal.Length;
            else if (ejeV < -0.6f)
                nuevoIndice = (indiceBotonFinal + 1) % botonesFinal.Length;

            if (nuevoIndice != indiceBotonFinal)
            {
                RestaurarVisualFinal(indiceBotonFinal);
                indiceBotonFinal = nuevoIndice;
                ResaltarVisualFinal(indiceBotonFinal);
            }
        }

        if (PadInput.AnySouthPressedThisFrame(2))
            botonesFinal[indiceBotonFinal]?.onClick.Invoke();
    }

    private void ResaltarVisualFinal(int indice)
    {
        if (imagenesBotonesFinal == null || indice < 0 || indice >= imagenesBotonesFinal.Length) return;
        if (imagenesBotonesFinal[indice] != null)
            imagenesBotonesFinal[indice].color = colorSeleccionado;
    }

    private void RestaurarVisualFinal(int indice)
    {
        if (coloresOriginalesFinal == null || indice < 0 || indice >= coloresOriginalesFinal.Length) return;
        if (imagenesBotonesFinal[indice] != null)
            imagenesBotonesFinal[indice].color = coloresOriginalesFinal[indice];
    }

    void IniciarRonda()
    {
        DesactivarNavegacionFinal();
        tiempoActual = tiempoTotal;
        juegoTerminado = false;
        if (pantallaVictoria != null) pantallaVictoria.SetActive(false);
        MostrarControlesFinales(false);

        jugador1.SetActive(true);
        jugador2.SetActive(true);

        jugador1.transform.localScale = escalaInicialJ1;
        jugador2.transform.localScale = escalaInicialJ2;
        jugador1.transform.position   = posicionInicialJ1;
        jugador2.transform.position   = posicionInicialJ2;

        Rigidbody2D rbJ1 = jugador1.GetComponent<Rigidbody2D>();
        Rigidbody2D rbJ2 = jugador2.GetComponent<Rigidbody2D>();
        rbJ1.linearVelocity  = Vector2.zero;
        rbJ2.linearVelocity  = Vector2.zero;
        rbJ1.angularVelocity = 0f;
        rbJ2.angularVelocity = 0f;
        Physics2D.SyncTransforms();
        rbJ1.WakeUp();
        rbJ2.WakeUp();

        jugador1.GetComponent<PlayerHealth>().ResetVida();
        jugador2.GetComponent<PlayerHealth>().ResetVida();

        // Preparar según modo de juego
        if (pc1 != null && pc1.enabled)  pc1.PrepararNuevaRonda();
        if (cpu1 != null && cpu1.enabled) cpu1.PrepararNuevaRonda();
        if (pc2 != null && pc2.enabled)  pc2.PrepararNuevaRonda();
        if (cpu2 != null && cpu2.enabled) cpu2.PrepararNuevaRonda();

        if (textoRonda != null) textoRonda.text = "Ronda " + rondaActual;
    }

    void TerminarRondaPorTiempo()
    {
        float vidaJ1 = jugador1.GetComponent<PlayerHealth>().vida;
        float vidaJ2 = jugador2.GetComponent<PlayerHealth>().vida;

        if (vidaJ1 > vidaJ2)      TerminarRonda(1);
        else if (vidaJ2 > vidaJ1) TerminarRonda(2);
        else                       TerminarRonda(0);
    }

    void TerminarRonda(int ganador)
    {
        if (juegoTerminado) return;
        juegoTerminado = true;

        if (ganador == 1)      { rondasGanadasJ1++; ActualizarIconos(rondasJ1, rondasGanadasJ1); }
        else if (ganador == 2) { rondasGanadasJ2++; ActualizarIconos(rondasJ2, rondasGanadasJ2); }

        if (rondasGanadasJ1 >= rondasParaGanar)
            MostrarGanadorFinal("Player 1 WIN!");
        else if (rondasGanadasJ2 >= rondasParaGanar)
            MostrarGanadorFinal("Player 2 WIN!");
        else
        {
            rondaActual++;
            string msg = ganador == 0 ? "Empate!" : "Player " + ganador + " gana la ronda!";
            MostrarMensajeRonda(msg);
        }
    }

    void ActualizarIconos(GameObject[] iconos, int rondas)
    {
        for (int i = 0; i < iconos.Length; i++)
            if (iconos[i] != null) iconos[i].SetActive(i < rondas);
    }

    void MostrarMensajeRonda(string mensaje)
    {
        if (pantallaVictoria != null) pantallaVictoria.SetActive(true);
        MostrarControlesFinales(false);
        textoGanador.text = mensaje;
        Invoke("SiguienteRonda", 2f);
    }

    void SiguienteRonda() => IniciarRonda();

    void MostrarGanadorFinal(string ganador)
    {
        if (pantallaVictoria != null) pantallaVictoria.SetActive(true);
        textoGanador.text = ganador;

        MostrarControlesFinales(true);

        // Bloquear input de jugadores y CPU
        if (pc1  != null && pc1.enabled)  pc1.BloquearInput(true);
        if (cpu1 != null && cpu1.enabled) cpu1.BloquearInput(true);
        if (pc2  != null && pc2.enabled)  pc2.BloquearInput(true);
        if (cpu2 != null && cpu2.enabled) cpu2.BloquearInput(true);

        PrepararNavegacionFinal();
    }

    private void CachearControlesFinales()
    {
        if (controlesFinalesCacheados) return;

        var listaControles = new List<GameObject>();
        var agregados = new HashSet<GameObject>();

        AgregarControlFinal(botonRevancha, listaControles, agregados);
        AgregarControlFinal(botonSalir, listaControles, agregados);

        if (pantallaVictoria != null)
        {
            foreach (Button boton in pantallaVictoria.GetComponentsInChildren<Button>(true))
                AgregarControlFinal(boton.gameObject, listaControles, agregados);
        }

        foreach (Button boton in Resources.FindObjectsOfTypeAll<Button>())
        {
            if (boton == null || boton.gameObject == null) continue;
            if (!boton.gameObject.scene.IsValid()) continue;
            if (boton.gameObject.scene != gameObject.scene) continue;
            if (TieneAccionFinal(boton))
                AgregarControlFinal(boton.gameObject, listaControles, agregados);
        }

        controlesFinales = listaControles.ToArray();
        controlesFinalesCacheados = true;
    }

    private void AgregarControlFinal(GameObject control, List<GameObject> lista, HashSet<GameObject> agregados)
    {
        if (control == null) return;
        if (!agregados.Add(control)) return;
        lista.Add(control);
    }

    private bool TieneAccionFinal(Button boton)
    {
        for (int i = 0; i < boton.onClick.GetPersistentEventCount(); i++)
        {
            string metodo = boton.onClick.GetPersistentMethodName(i);
            if (boton.onClick.GetPersistentTarget(i) == this &&
                (metodo == nameof(ReiniciarJuego) || metodo == nameof(VolverAlMenuPrincipal)))
                return true;
        }

        return false;
    }

    private void MostrarControlesFinales(bool mostrar)
    {
        CachearControlesFinales();

        foreach (GameObject control in controlesFinales)
        {
            if (control != null)
                control.SetActive(mostrar);
        }
    }

    private void DesactivarNavegacionFinal()
    {
        if (imagenesBotonesFinal != null && coloresOriginalesFinal != null)
        {
            for (int i = 0; i < imagenesBotonesFinal.Length && i < coloresOriginalesFinal.Length; i++)
            {
                if (imagenesBotonesFinal[i] != null)
                    imagenesBotonesFinal[i].color = coloresOriginalesFinal[i];
            }
        }

        pantallaFinalActiva = false;
        botonesFinal = null;
        imagenesBotonesFinal = null;
        coloresOriginalesFinal = null;
        indiceBotonFinal = 0;
        stickEnReposo = true;
        timerCooldown = 0f;
    }

    private void PrepararNavegacionFinal()
    {
        var listaBotones   = new List<Button>();
        var listaImagenes  = new List<Image>();
        var listaColores   = new List<Color>();

        foreach (GameObject control in controlesFinales)
        {
            if (control == null) continue;

            var b   = control.GetComponent<Button>();
            var img = control.GetComponent<Image>();
            if (b != null && img != null)
            {
                listaBotones.Add(b);
                listaImagenes.Add(img);
                listaColores.Add(img.color);
            }
        }

        botonesFinal           = listaBotones.ToArray();
        imagenesBotonesFinal   = listaImagenes.ToArray();
        coloresOriginalesFinal = listaColores.ToArray();

        indiceBotonFinal    = 0;
        stickEnReposo       = true;
        timerCooldown       = 0.8f;
        pantallaFinalActiva = botonesFinal.Length > 0;

        if (pantallaFinalActiva)
            ResaltarVisualFinal(0);
    }

    public void ReiniciarJuego()
    {
        rondasGanadasJ1 = 0;
        rondasGanadasJ2 = 0;
        rondaActual     = 1;
        FlujoEscenasManager.IrASeleccionPersonajes();
    }

    public void VolverAlMenuPrincipal() => FlujoEscenasManager.IrAMenuPrincipal();
}
