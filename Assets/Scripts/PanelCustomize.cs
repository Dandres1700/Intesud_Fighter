using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PanelCustomize : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panelCustomize;

    [Header("Referencias")]
    [SerializeField] private Slider sliderVolumen;
    [SerializeField] private Button botonCerrar;

    [Header("Navegación con mando")]
    [SerializeField] private float incrementoVolumen = 0.1f;
    [SerializeField] private Color colorSeleccionado = new Color(0.25f, 0.25f, 0.25f, 1f);

    [Header("Referencia al MenuPrincipal")]
    [SerializeField] private MenuPrincipal menuPrincipal;

    private bool panelAbierto = false;
    private Image imagenBotonCerrar;
    private Color colorOriginalCerrar;

    void Start()
    {
        if (panelCustomize != null)
            panelCustomize.SetActive(false);

        if (botonCerrar != null)
        {
            imagenBotonCerrar = botonCerrar.GetComponent<Image>();
            if (imagenBotonCerrar != null)
                colorOriginalCerrar = imagenBotonCerrar.color;
        }
    }

    void Update()
    {
        if (!panelAbierto) return;

        ManejarMando();

        // Teclado — volumen
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            CambiarVolumen(incrementoVolumen);
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            CambiarVolumen(-incrementoVolumen);

        // Teclado — cerrar
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
            CerrarPanel();
    }

    private void ManejarMando()
    {
        Vector2 move = PadInput.ReadAnyMove(2);

        // Stick horizontal → sube/baja volumen
        if (move.x > 0.5f)
            CambiarVolumen(incrementoVolumen * Time.deltaTime * 2f);
        else if (move.x < -0.5f)
            CambiarVolumen(-incrementoVolumen * Time.deltaTime * 2f);

        // Cruz o Círculo → cerrar
        if (PadInput.AnySouthPressedThisFrame(2) || PadInput.AnyEastPressedThisFrame(2))
            CerrarPanel();
    }

    private void CambiarVolumen(float cantidad)
    {
        if (sliderVolumen == null) return;
        sliderVolumen.value = Mathf.Clamp01(sliderVolumen.value + cantidad);
        AudioListener.volume = sliderVolumen.value;
        PlayerPrefs.SetFloat("Volumen", sliderVolumen.value);
        PlayerPrefs.Save();
    }

    public void AbrirPanel()
    {
        if (panelCustomize != null)
            panelCustomize.SetActive(true);

        panelAbierto = false; // lo dejamos falso por un frame
        if (menuPrincipal != null) menuPrincipal.BloquearInput(true);

        if (sliderVolumen != null)
            sliderVolumen.value = PlayerPrefs.GetFloat("Volumen", 1f);

        if (imagenBotonCerrar != null)
            imagenBotonCerrar.color = colorSeleccionado;

        EventSystem.current?.SetSelectedGameObject(botonCerrar?.gameObject);

        StartCoroutine(HabilitarInputConDelay());
    }

    private System.Collections.IEnumerator HabilitarInputConDelay()
    {
        yield return new WaitForSeconds(0.2f);
        panelAbierto = true;
    }

    public void CerrarPanel()
    {
        if (panelCustomize != null)
            panelCustomize.SetActive(false);

        if (imagenBotonCerrar != null)
            imagenBotonCerrar.color = colorOriginalCerrar;

        panelAbierto = false;

        // Devolver input al MenuPrincipal
        if (menuPrincipal != null) menuPrincipal.BloquearInput(false);
    }
}