using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuJuego : MonoBehaviour
{
    [Header("Botones — arrastra en orden: J1vsJ2, J1vsCPU, CPUvsCPU")]
    [SerializeField] private Button[] botones;

    [Header("Navegación con mando")]
    [SerializeField] private float cooldownNavegacion = 0.25f;
    [SerializeField] private Color colorSeleccionado = new Color(0.25f, 0.25f, 0.25f, 1f);

    private int indiceSeleccionado = 0;
    private float timerCooldown = 0f;
    private bool stickEnReposo = true;

    private Image[] imagenesBotones;
    private Color[] coloresOriginales;

    void Start()
    {
        if (botones == null || botones.Length == 0)
        {
            Debug.LogError("[MenuJuego] Asigna los botones en el Inspector.");
            return;
        }

        imagenesBotones   = new Image[botones.Length];
        coloresOriginales = new Color[botones.Length];

        for (int i = 0; i < botones.Length; i++)
        {
            if (botones[i] == null) continue;
            imagenesBotones[i] = botones[i].GetComponent<Image>();
            if (imagenesBotones[i] != null)
                coloresOriginales[i] = imagenesBotones[i].color;

            int captura = i;
            AddPointerEnterListener(botones[i], () => AplicarSeleccion(captura));
        }

        indiceSeleccionado = 0;
        ResaltarVisual(0);
        EventSystem.current?.SetSelectedGameObject(botones[0].gameObject);
    }

    void Update()
    {
        if (botones == null || botones.Length == 0) return;

        timerCooldown -= Time.deltaTime;
        ManejarNavegacionMando();

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            AplicarSeleccion((indiceSeleccionado + 1) % botones.Length);
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            AplicarSeleccion((indiceSeleccionado - 1 + botones.Length) % botones.Length);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            botones[indiceSeleccionado]?.onClick.Invoke();

        if (Input.GetKeyDown(KeyCode.Escape))
            FlujoEscenasManager.IrAMenuPrincipal();
    }

    private void ManejarNavegacionMando()
    {
        float ejeV = PadInput.ReadAnyMove(2).y;

        if (Mathf.Abs(ejeV) < 0.3f)
            stickEnReposo = true;
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

        if (PadInput.AnyEastPressedThisFrame(2))
            FlujoEscenasManager.IrAMenuPrincipal();
    }

    private void AplicarSeleccion(int nuevoIndice)
    {
        if (nuevoIndice == indiceSeleccionado) return;

        RestaurarVisual(indiceSeleccionado);
        indiceSeleccionado = nuevoIndice;
        ResaltarVisual(indiceSeleccionado);

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
        var trigger = boton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
            trigger = boton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
        entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entry.callback.AddListener((_) => callback());
        trigger.triggers.Add(entry);
    }

    // Estos métodos se asignan en el OnClick de cada botón en el Inspector
    public void SeleccionarJ1vsJ2()
    {
        GameModeManager.SetModo(GameModeManager.ModoJuego.J1vsJ2);
        FlujoEscenasManager.IrASeleccionPersonajes();
    }

    public void SeleccionarJ1vsCPU()
    {
        GameModeManager.SetModo(GameModeManager.ModoJuego.J1vsCPU);
        FlujoEscenasManager.IrADificultad();
    }

    public void SeleccionarCPUvsCPU()
    {
        GameModeManager.SetModo(GameModeManager.ModoJuego.CPUvsCPU);
        FlujoEscenasManager.IrADificultad();
    }

    public void Volver() => FlujoEscenasManager.IrAMenuPrincipal();
}