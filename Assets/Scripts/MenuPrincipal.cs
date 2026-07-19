using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuPrincipal : MonoBehaviour
{
    [Header("Botones del menú — arrastra en orden: Start, Customize, Quit")]
    [SerializeField] private Button[] botones;

    [Header("Navegación con mando")]
    [SerializeField] private float cooldownNavegacion = 0.25f;

    [Header("Color de selección")]
    [SerializeField] private Color colorSeleccionado = new Color(0.25f, 0.25f, 0.25f, 1f);

    private int indiceSeleccionado = 0;
    private float timerCooldown = 0f;
    private bool stickEnReposo = true;
    private bool inputBloqueado = false;

    private AnimarBoton[] animaciones;
    private Image[] imagenesBotones;
    private Color[] coloresOriginales;

    void Start()
    {
        if (botones == null || botones.Length == 0)
        {
            Debug.LogError("[MenuPrincipal] Asigna los botones en el Inspector en orden: Start, Customize, Quit.");
            return;
        }

        animaciones       = new AnimarBoton[botones.Length];
        imagenesBotones   = new Image[botones.Length];
        coloresOriginales = new Color[botones.Length];

        for (int i = 0; i < botones.Length; i++)
        {
            if (botones[i] == null) continue;
            animaciones[i]     = botones[i].GetComponent<AnimarBoton>();
            imagenesBotones[i] = botones[i].GetComponent<Image>();
            if (imagenesBotones[i] != null)
                coloresOriginales[i] = imagenesBotones[i].color;

            int captura = i;
            AddPointerEnterListener(botones[i], () => AplicarSeleccion(captura));
        }

        indiceSeleccionado = 0;
        ResaltarVisual(0);
        if (animaciones[0] != null)
            animaciones[0].DetenerAnimacion();
        EventSystem.current?.SetSelectedGameObject(botones[0].gameObject);
    }

    void Update()
    {
        if (inputBloqueado) return;
        if (botones == null || botones.Length == 0) return;

        timerCooldown -= Time.deltaTime;

        ManejarNavegacionMando();

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            AplicarSeleccion((indiceSeleccionado + 1) % botones.Length);
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            AplicarSeleccion((indiceSeleccionado - 1 + botones.Length) % botones.Length);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            botones[indiceSeleccionado]?.onClick.Invoke();
    }

    private void ManejarNavegacionMando()
    {
        float ejeV = PadInput.ReadAnyMove(2).y;

        if (Mathf.Abs(ejeV) < 0.3f)
        {
            stickEnReposo = true;
        }
        else if (stickEnReposo && timerCooldown <= 0f)
        {
            stickEnReposo = false;
            timerCooldown = cooldownNavegacion;

            if (ejeV > 0.6f)
                AplicarSeleccion((indiceSeleccionado - 1 + botones.Length) % botones.Length);
            else if (ejeV < -0.6f)
                AplicarSeleccion((indiceSeleccionado + 1) % botones.Length);
        }

        if (PadInput.AnySouthPressedThisFrame(2))
            botones[indiceSeleccionado]?.onClick.Invoke();
    }

    public void BloquearInput(bool bloquear)
    {
        inputBloqueado = bloquear;
    }

    private void AplicarSeleccion(int nuevoIndice)
    {
        if (botones == null || botones.Length == 0) return;
        if (nuevoIndice == indiceSeleccionado) return;

        RestaurarVisual(indiceSeleccionado);
        if (animaciones[indiceSeleccionado] != null)
            animaciones[indiceSeleccionado].IniciarAnimacion();

        indiceSeleccionado = nuevoIndice;

        ResaltarVisual(indiceSeleccionado);
        if (animaciones[indiceSeleccionado] != null)
            animaciones[indiceSeleccionado].DetenerAnimacion();

        EventSystem.current?.SetSelectedGameObject(botones[indiceSeleccionado].gameObject);
    }

    private void ResaltarVisual(int indice)
    {
        if (indice < 0 || indice >= botones.Length) return;
        if (imagenesBotones[indice] != null)
            imagenesBotones[indice].color = colorSeleccionado;
    }

    private void RestaurarVisual(int indice)
    {
        if (indice < 0 || indice >= botones.Length) return;
        if (imagenesBotones[indice] != null)
            imagenesBotones[indice].color = coloresOriginales[indice];
    }

    private void AddPointerEnterListener(Button boton, System.Action callback)
    {
        EventTrigger trigger = boton.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = boton.gameObject.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((_) => callback());
        trigger.triggers.Add(entry);
    }

    public void JugarPartida() => FlujoEscenasManager.IrAMenuJuego();
    public void SalirJuego()   => FlujoEscenasManager.SalirJuego();
}