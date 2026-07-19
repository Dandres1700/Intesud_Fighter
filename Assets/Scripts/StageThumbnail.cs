using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

/// <summary>
/// Script para cada casilla/thumbnail del grid de escenarios.
/// Adjuntar al prefab "StageThumbnail".
///
/// Estructura recomendada del prefab:
/// StageThumbnail (Button + StageThumbnail)
///   ├── Background (Image)           <- fondo de la casilla
///   ├── ThumbnailImage (Image)       <- imagen del escenario
///   ├── StageName (TextMeshPro)      <- nombre del escenario
///   ├── LockedIcon (Image/GameObject) <- candado si está bloqueado
///   └── SelectionBorder (Image)      <- borde activo al seleccionar
/// </summary>
[RequireComponent(typeof(Button))]
public class StageThumbnail : MonoBehaviour, IPointerEnterHandler
{
    [Header("Referencias UI")]
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject lockedIcon;
    [SerializeField] private Image selectionBorder;
    [SerializeField] private Image backgroundImage;

    [Header("Colores")]
    [SerializeField] private Color normalColor = new Color(0.15f, 0.15f, 0.15f);
    [SerializeField] private Color selectedColor = new Color(0.25f, 0.20f, 0.05f);
    [SerializeField] private Color lockedColor = new Color(0.08f, 0.08f, 0.08f);
    [SerializeField] private Color borderSelectedColor = Color.yellow;

    [Header("Animación")]
    [SerializeField] private float scaleSelectedFactor = 1.08f;
    [SerializeField] private float animationSpeed = 10f;

    // --- Estado ---
    private StageData stageData;
    private Action onClickCallback;
    private Button button;
    private bool isSelected = false;
    private Vector3 targetScale;

    // -------------------------------------------------------
    // Inicialización
    // -------------------------------------------------------

    private void Awake()
    {
        button = GetComponent<Button>();
        targetScale = Vector3.one;
    }

    /// <summary>
    /// Inicializa la miniatura con los datos del escenario.
    /// </summary>
    public void Setup(StageData data, Action onClick)
    {
        stageData = data;
        onClickCallback = onClick;

        // Imagen
        if (thumbnailImage != null && data.thumbnailImage != null)
            thumbnailImage.sprite = data.thumbnailImage;

        // Nombre
        if (nameText != null)
            nameText.text = data.stageName.ToUpper();

        // Candado
        if (lockedIcon != null)
            lockedIcon.SetActive(data.isLocked);

        // Color de fondo según estado
        if (backgroundImage != null)
            backgroundImage.color = data.isLocked ? lockedColor : normalColor;

        // Borde apagado por defecto
        if (selectionBorder != null)
            selectionBorder.enabled = false;

        // Listener del botón
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickCallback?.Invoke());

        // Aplico alpha si está bloqueado
        if (thumbnailImage != null)
        {
            Color c = thumbnailImage.color;
            c.a = data.isLocked ? 0.4f : 1f;
            thumbnailImage.color = c;
        }
    }

    // -------------------------------------------------------
    // Selección visual
    // -------------------------------------------------------

    /// <summary>
    /// Activa o desactiva el estado "seleccionado" de la miniatura.
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        // Borde
        if (selectionBorder != null)
        {
            selectionBorder.enabled = selected;
            selectionBorder.color = borderSelectedColor;
        }

        // Color de fondo
        if (backgroundImage != null && stageData != null)
        {
            if (selected)
                backgroundImage.color = selectedColor;
            else
                backgroundImage.color = stageData.isLocked ? lockedColor : normalColor;
        }

        // Escala target
        targetScale = selected
            ? Vector3.one * scaleSelectedFactor
            : Vector3.one;
    }

    // -------------------------------------------------------
    // Hover con mouse (apunta al mismo escenario que el teclado)
    // -------------------------------------------------------

    public void OnPointerEnter(PointerEventData eventData)
    {
        onClickCallback?.Invoke();
    }

    // -------------------------------------------------------
    // Animación suave de escala
    // -------------------------------------------------------

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * animationSpeed
        );
    }
}
