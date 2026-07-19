using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectManager : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        public string nombre;
        public Sprite previewSprite;
        public Sprite thumbnailSprite;
        public GameObject prefab;
    }

    [Header("Personajes disponibles")]
    [SerializeField] private CharacterData[] personajes;

    [Header("Panel Jugador 1")]
    [SerializeField] private Image previewJ1;
    [SerializeField] private TextMeshProUGUI nombreJ1;
    [SerializeField] private GameObject readyTextJ1;

    [Header("Panel Jugador 2")]
    [SerializeField] private Image previewJ2;
    [SerializeField] private TextMeshProUGUI nombreJ2;
    [SerializeField] private GameObject readyTextJ2;

    [Header("Grid y cursores")]
    [SerializeField] private Transform gridPersonajes;
    [SerializeField] private GameObject characterSlotPrefab;
    [SerializeField] private RectTransform cursorJ1;
    [SerializeField] private RectTransform cursorJ2;
    [SerializeField] private float cursorSpeed = 15f;
    [SerializeField] private int columnas = 2; // Columnas del grid de los Personajes 

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveSFX;
    [SerializeField] private AudioClip confirmSFX;
    [SerializeField] private AudioClip cancelSFX;

    [Header("Velocidad de navegación con mando")]
    [SerializeField] private float cooldownNavegacion = 0.2f;

    private int indexJ1 = 0;
    private int indexJ2 = 1;
    private bool confirmedJ1 = false;
    private bool confirmedJ2 = false;
    private bool saliendo = false;

    private RectTransform[] slots;

    // Cooldown y estado de reposo del stick por jugador
    private float cooldownJ1 = 0f;
    private float cooldownJ2 = 0f;
    private bool stickJ1EnReposo = true;
    private bool stickJ2EnReposo = true;

    public static CharacterData PersonajeJ1 { get; private set; }
    public static CharacterData PersonajeJ2 { get; private set; }

    private void Start()
    {
        BuildGrid();
        if (personajes == null || personajes.Length == 0)
        {
            Debug.LogError("[CharacterSelect] No hay personajes configurados.");
            return;
        }

        indexJ1 = Mathf.Clamp(indexJ1, 0, personajes.Length - 1);
        indexJ2 = personajes.Length > 1 ? Mathf.Clamp(indexJ2, 0, personajes.Length - 1) : indexJ1;
        ActualizarPanelJ1();
        ActualizarPanelJ2();
        readyTextJ1?.SetActive(false);
        readyTextJ2?.SetActive(false);
    }

    void Update()
    {
        if (saliendo) return;
        cooldownJ1 -= Time.deltaTime;
        cooldownJ2 -= Time.deltaTime;
        ManejarInputJ1();
        ManejarInputJ2();
        MoverCursores();
        VerificarAmbosListos();
    }

    private void BuildGrid()
    {
        if (personajes == null || personajes.Length == 0)
        {
            slots = System.Array.Empty<RectTransform>();
            return;
        }

        slots = new RectTransform[personajes.Length];

        for (int i = 0; i < personajes.Length; i++)
        {
            GameObject slot = Instantiate(characterSlotPrefab, gridPersonajes);
            slots[i] = slot.GetComponent<RectTransform>();

            CharacterSlot characterSlot = slot.GetComponent<CharacterSlot>();
            if (characterSlot != null)
                characterSlot.Setup(personajes[i].thumbnailSprite, personajes[i].nombre);
        }
    }

    // ── Constantes PS4 ────────────────────────────────────────────────────
    private const int MandoJ1 = 1;
    private const int MandoJ2 = 2;
    private const int BotonCruz      = 1; // Confirmar
    private const int BotonCirculo   = 2; // Cancelar / Volver

    private void ManejarInputJ1()
    {
        if (confirmedJ1) return;

        int nuevoIndex = indexJ1;

        // ── Teclado ──────────────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.D)) nuevoIndex++;
        else if (Input.GetKeyDown(KeyCode.A)) nuevoIndex--;
        else if (Input.GetKeyDown(KeyCode.S)) nuevoIndex += columnas;
        else if (Input.GetKeyDown(KeyCode.W)) nuevoIndex -= columnas;

        // ── Mando J1 ─────────────────────────────────────────────────────
        Vector2 movePadJ1 = PadInput.ReadMenuMove(MandoJ1);
        float h1 = movePadJ1.x;
        float v1 = movePadJ1.y;
        float mag1 = Mathf.Max(Mathf.Abs(h1), Mathf.Abs(v1));

        // Reset de reposo
        if (mag1 < 0.3f) stickJ1EnReposo = true;

        // Solo mueve si el stick estaba en reposo y el cooldown expiró
        if (stickJ1EnReposo && cooldownJ1 <= 0f && mag1 > 0.6f)
        {
            stickJ1EnReposo = false;
            cooldownJ1 = cooldownNavegacion;

            if      (h1 >  0.6f) nuevoIndex++;
            else if (h1 < -0.6f) nuevoIndex--;
            else if (v1 >  0.6f) nuevoIndex -= columnas;
            else if (v1 < -0.6f) nuevoIndex += columnas;
        }

        nuevoIndex = Mathf.Clamp(nuevoIndex, 0, personajes.Length - 1);

        if (nuevoIndex != indexJ1)
        {
            indexJ1 = nuevoIndex;
            audioSource?.PlayOneShot(moveSFX);
            ActualizarPanelJ1();
        }

        // Confirmar: F o Cruz PS4
        bool confirmaTeclado = Input.GetKeyDown(KeyCode.F);
        bool confirmaPS4 = PadInput.SouthPressedThisFrame(MandoJ1);

        if (confirmaTeclado || confirmaPS4)
        {
            confirmedJ1 = true;
            readyTextJ1?.SetActive(true);
            audioSource?.PlayOneShot(confirmSFX);
            PersonajeJ1 = personajes[indexJ1];
        }

        // Volver: Escape o Círculo PS4
        bool vuelveTeclado = Input.GetKeyDown(KeyCode.Escape);
        bool vuelvePS4 = PadInput.EastPressedThisFrame(MandoJ1);

        if ((vuelveTeclado || vuelvePS4) && !confirmedJ1)
            OnBack();
    }

    private void ManejarInputJ2()
    {
        if (confirmedJ2) return;

        int nuevoIndex = indexJ2;

        // ── Teclado ──────────────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.RightArrow)) nuevoIndex++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) nuevoIndex--;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) nuevoIndex += columnas;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) nuevoIndex -= columnas;

        // ── Mando J2 ─────────────────────────────────────────────────────
        Vector2 movePadJ2 = PadInput.ReadMenuMove(MandoJ2);
        float h2 = movePadJ2.x;
        float v2 = movePadJ2.y;
        float mag2 = Mathf.Max(Mathf.Abs(h2), Mathf.Abs(v2));

        if (mag2 < 0.3f) stickJ2EnReposo = true;

        if (stickJ2EnReposo && cooldownJ2 <= 0f && mag2 > 0.6f)
        {
            stickJ2EnReposo = false;
            cooldownJ2 = cooldownNavegacion;

            if      (h2 >  0.6f) nuevoIndex++;
            else if (h2 < -0.6f) nuevoIndex--;
            else if (v2 >  0.6f) nuevoIndex -= columnas;
            else if (v2 < -0.6f) nuevoIndex += columnas;
        }

        nuevoIndex = Mathf.Clamp(nuevoIndex, 0, personajes.Length - 1);

        if (nuevoIndex != indexJ2)
        {
            indexJ2 = nuevoIndex;
            audioSource?.PlayOneShot(moveSFX);
            ActualizarPanelJ2();
        }

        // Confirmar: Enter o Cruz PS4
        bool confirmaTeclado = Input.GetKeyDown(KeyCode.Return);
        bool confirmaPS4 = PadInput.SouthPressedThisFrame(MandoJ2);

        if (confirmaTeclado || confirmaPS4)
        {
            confirmedJ2 = true;
            readyTextJ2?.SetActive(true);
            audioSource?.PlayOneShot(confirmSFX);
            PersonajeJ2 = personajes[indexJ2];
        }
    }

    // ── Utilidades (idénticas a PlayerController) ─────────────────────────
    private void ActualizarPanelJ1()
    {
        if (personajes == null || personajes.Length == 0) return;
        var p = personajes[indexJ1];
        if (previewJ1 != null && p.previewSprite != null) previewJ1.sprite = p.previewSprite;
        if (nombreJ1 != null) nombreJ1.text = p.nombre.ToUpper();
    }

    private void ActualizarPanelJ2()
    {
        if (personajes == null || personajes.Length == 0) return;
        var p = personajes[indexJ2];
        if (previewJ2 != null && p.previewSprite != null) previewJ2.sprite = p.previewSprite;
        if (nombreJ2 != null) nombreJ2.text = p.nombre.ToUpper();
    }

    private void MoverCursores()
    {
        if (slots == null) return;

        if (cursorJ1 != null && indexJ1 < slots.Length && slots[indexJ1] != null)
            cursorJ1.position = Vector3.Lerp(cursorJ1.position, slots[indexJ1].position, Time.deltaTime * cursorSpeed);

        if (cursorJ2 != null && indexJ2 < slots.Length && slots[indexJ2] != null)
            cursorJ2.position = Vector3.Lerp(cursorJ2.position, slots[indexJ2].position, Time.deltaTime * cursorSpeed);
    }

    private void VerificarAmbosListos()
    {
        if (confirmedJ1 && confirmedJ2 && !saliendo)
        {
            saliendo = true;
            StartCoroutine(IrAMapSelect());
        }
    }

    private IEnumerator IrAMapSelect()
    {
        yield return new WaitForSeconds(1f);
        FlujoEscenasManager.IrASeleccionMapa();
    }

    public void OnBack()
    {
        audioSource?.PlayOneShot(cancelSFX);
        FlujoEscenasManager.IrAMenuPrincipal();
    }
}
