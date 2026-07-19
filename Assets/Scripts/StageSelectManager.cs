using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manager principal del menú de selección de escenarios estilo Street Fighter.
/// Adjuntar este script al GameObject "StageSelectManager" en la escena del menú.
/// </summary>
public class StageSelectManager : MonoBehaviour
{
    [Header("Datos de Escenarios")]
    [SerializeField] private StageData[] stages;

    [Header("UI - Grid de Escenarios")]
    [SerializeField] private Transform stagesGrid;          // Padre donde se generan las miniaturas
    [SerializeField] private GameObject stageThumbnailPrefab; // Prefab de cada casilla del grid

    [Header("UI - Panel de Preview")]
    [SerializeField] private Image previewImage;
    [SerializeField] private TextMeshProUGUI stageNameText;
    [SerializeField] private TextMeshProUGUI stageLocationText;
    [SerializeField] private TextMeshProUGUI stageDescriptionText;
    [SerializeField] private GameObject lockedOverlay;       // Panel "BLOQUEADO" sobre el preview

    [Header("UI - Selector (cursor estilo SF)")]
    [SerializeField] private RectTransform selectorCursor;  // Cursor que se mueve entre casillas
    [SerializeField] private float cursorMoveSpeed = 12f;

    [Header("UI - Botones")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveSFX;
    [SerializeField] private AudioClip selectSFX;
    [SerializeField] private AudioClip lockedSFX;

    [Header("Animación")]
    [SerializeField] private Animator previewAnimator;      // Animator del panel de preview (opcional)
    [SerializeField] private float previewTransitionTime = 0.15f;

    // --- Estado interno ---
    private int currentIndex = 0;
    private int previousIndex = -1;
    private StageThumbnail[] thumbnails;
    private bool isTransitioning = false;
    private bool inputEnabled = true;
    private bool stickEnReposo = true;
    private float timerCooldownMando = 0f;

    // Propiedad estática para pasar el escenario elegido a la escena de combate
    public static StageData SelectedStage { get; private set; }

    // -------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------

    private void Start()
    {
        BuildGrid();
        SelectStage(0, animate: false);

        confirmButton.onClick.AddListener(OnConfirm);
        backButton.onClick.AddListener(OnBack);
    }

    private void Update()
    {
        if (!inputEnabled) return;
        timerCooldownMando -= Time.deltaTime;
        HandleInput();
        MoveCursorToTarget();
    }

    // -------------------------------------------------------
    // Construcción del grid
    // -------------------------------------------------------

    private void BuildGrid()
    {
        thumbnails = new StageThumbnail[stages.Length];

        for (int i = 0; i < stages.Length; i++)
        {
            GameObject obj = Instantiate(stageThumbnailPrefab, stagesGrid);
            StageThumbnail thumb = obj.GetComponent<StageThumbnail>();

            if (thumb != null)
            {
                int capturedIndex = i; // Captura para el closure del listener
                thumb.Setup(stages[i], () => SelectStage(capturedIndex));
                thumbnails[i] = thumb;
            }
        }
    }

    // -------------------------------------------------------
    // Manejo de input
    // -------------------------------------------------------

    private void HandleInput()
    {
        // Obtener columnas del GridLayoutGroup para navegación
        GridLayoutGroup grid = stagesGrid.GetComponent<GridLayoutGroup>();
        int columns = (grid != null) ? Mathf.Max(1, grid.constraintCount) : 4;

        int newIndex = currentIndex;

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            newIndex = Mathf.Clamp(currentIndex + 1, 0, stages.Length - 1);

        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            newIndex = Mathf.Clamp(currentIndex - 1, 0, stages.Length - 1);

        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            newIndex = Mathf.Clamp(currentIndex + columns, 0, stages.Length - 1);

        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            newIndex = Mathf.Clamp(currentIndex - columns, 0, stages.Length - 1);

        if (newIndex != currentIndex)
            SelectStage(newIndex);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            OnConfirm();

        if (Input.GetKeyDown(KeyCode.Escape))
            OnBack();

        HandleGamepadInput(columns);
    }

    private void HandleGamepadInput(int columns)
    {
        Vector2 move = PadInput.ReadAnyMove(2);

        if (Mathf.Abs(move.x) < 0.3f && Mathf.Abs(move.y) < 0.3f)
        {
            stickEnReposo = true;
        }
        else if (stickEnReposo && timerCooldownMando <= 0f)
        {
            stickEnReposo = false;
            timerCooldownMando = 0.2f;

            if (Mathf.Abs(move.x) >= Mathf.Abs(move.y))
            {
                if (move.x > 0.6f)
                    SelectStage(Mathf.Clamp(currentIndex + 1, 0, stages.Length - 1));
                else if (move.x < -0.6f)
                    SelectStage(Mathf.Clamp(currentIndex - 1, 0, stages.Length - 1));
            }
            else
            {
                if (move.y > 0.6f)
                    SelectStage(Mathf.Clamp(currentIndex - columns, 0, stages.Length - 1));
                else if (move.y < -0.6f)
                    SelectStage(Mathf.Clamp(currentIndex + columns, 0, stages.Length - 1));
            }
        }

        if (PadInput.AnySouthPressedThisFrame(2))
            OnConfirm();

        if (PadInput.AnyEastPressedThisFrame(2))
            OnBack();
    }

    // -------------------------------------------------------
    // Selección de escenario
    // -------------------------------------------------------

    public void SelectStage(int index, bool animate = true)
    {
        if (isTransitioning && animate) return;
        if (index == currentIndex && previousIndex != -1) return;

        previousIndex = currentIndex;
        currentIndex = index;

        // Actualizar estado visual de las miniaturas
        for (int i = 0; i < thumbnails.Length; i++)
            thumbnails[i]?.SetSelected(i == currentIndex);

        // Mover cursor al thumbnail seleccionado
        if (selectorCursor != null && thumbnails[currentIndex] != null)
            selectorCursor.SetParent(thumbnails[currentIndex].transform, false);

        // SFX de movimiento
        if (animate && moveSFX != null)
            audioSource?.PlayOneShot(moveSFX);

        // Actualizar preview (con transición opcional)
        if (animate && previewTransitionTime > 0f)
            StartCoroutine(TransitionPreview());
        else
            UpdatePreview();
    }

    private IEnumerator TransitionPreview()
    {
        isTransitioning = true;

        // Fade out
        if (previewAnimator != null)
            previewAnimator.SetTrigger("FadeOut");
        else
            yield return new WaitForSeconds(previewTransitionTime);

        UpdatePreview();

        // Fade in
        if (previewAnimator != null)
            previewAnimator.SetTrigger("FadeIn");
        else
            yield return new WaitForSeconds(previewTransitionTime);

        isTransitioning = false;
    }

    private void UpdatePreview()
    {
        if (stages == null || currentIndex >= stages.Length) return;

        StageData stage = stages[currentIndex];

        if (previewImage != null && stage.previewImage != null)
            previewImage.sprite = stage.previewImage;

        if (stageNameText != null)
            stageNameText.text = stage.stageName.ToUpper();

        if (stageLocationText != null)
            stageLocationText.text = stage.stageLocation;

        if (stageDescriptionText != null)
            stageDescriptionText.text = stage.stageDescription;

        if (lockedOverlay != null)
            lockedOverlay.SetActive(stage.isLocked);

        // Cambiar música al escenario seleccionado (opcional)
        if (stage.stageMusic != null && audioSource != null)
        {
            if (audioSource.clip != stage.stageMusic)
            {
                audioSource.clip = stage.stageMusic;
                audioSource.Play();
            }
        }

        // Habilitar/deshabilitar botón de confirmar según bloqueo
        if (confirmButton != null)
            confirmButton.interactable = !stage.isLocked;
    }

    // -------------------------------------------------------
    // Cursor suave
    // -------------------------------------------------------

    private void MoveCursorToTarget()
    {
        if (selectorCursor == null || thumbnails == null) return;
        if (currentIndex >= thumbnails.Length || thumbnails[currentIndex] == null) return;

        RectTransform target = thumbnails[currentIndex].GetComponent<RectTransform>();
        if (target == null) return;

        selectorCursor.position = Vector3.Lerp(
            selectorCursor.position,
            target.position,
            Time.deltaTime * cursorMoveSpeed
        );
    }

    // -------------------------------------------------------
    // Confirmar / Volver
    // -------------------------------------------------------

    private void OnConfirm()
    {
        StageData stage = stages[currentIndex];

        if (stage.isLocked)
        {
            if (lockedSFX != null) audioSource?.PlayOneShot(lockedSFX);
            Debug.Log($"[StageSelect] Escenario bloqueado: {stage.stageName}");
            return;
        }

        if (selectSFX != null) audioSource?.PlayOneShot(selectSFX);

        SelectedStage = stage;
        inputEnabled = false;

        Debug.Log($"[StageSelect] Escenario seleccionado: {stage.stageName} -> {stage.sceneName}");

        // Pequeña pausa dramática antes de cargar (estilo SF)
        StartCoroutine(LoadStageWithDelay(stage.sceneName, 0.8f));
    }

    private IEnumerator LoadStageWithDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    private void OnBack()
    {
        Debug.Log("[StageSelect] Volviendo al menú anterior.");
        FlujoEscenasManager.IrAMenuPrincipal();
    }
}
