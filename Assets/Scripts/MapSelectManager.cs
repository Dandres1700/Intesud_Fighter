using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Map selection menu.
/// Supports keyboard and DualShock 4 / gamepads through PadInput.
/// </summary>
public class MapSelectManager : MonoBehaviour
{
    [Header("Datos de Mapas")]
    [SerializeField] private StageData[] maps;

    [Header("UI - Preview")]
    [SerializeField] private Image backgroundPreview;
    [SerializeField] private TextMeshProUGUI mapNameText;
    [SerializeField] private TextMeshProUGUI mapSubtitleText;
    [SerializeField] private GameObject lockedPanel;

    [Header("UI - Navegacion")]
    [SerializeField] private Button arrowLeft;
    [SerializeField] private Button arrowRight;

    [Header("UI - Puntos indicadores (opcional)")]
    [SerializeField] private Transform dotsContainer;
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private Color dotActive = Color.white;
    [SerializeField] private Color dotInactive = new Color(1f, 1f, 1f, 0.3f);

    [Header("UI - Botones")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;

    [Header("Transicion")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private CanvasGroup previewCanvasGroup;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip navigateSFX;
    [SerializeField] private AudioClip confirmSFX;
    [SerializeField] private AudioClip lockedSFX;

    private int currentIndex = 0;
    private bool inputEnabled = true;
    private bool stickRightPressed = false;
    private bool stickLeftPressed = false;
    private Image[] dots;

    public static StageData SelectedMap { get; private set; }

    private void Start()
    {
        if (maps == null || maps.Length == 0)
        {
            Debug.LogError("[MapSelect] No hay mapas configurados.");
            inputEnabled = false;
            return;
        }

        BuildDots();
        ShowMap(currentIndex, animate: false);

        if (arrowLeft != null) arrowLeft.onClick.AddListener(() => Navigate(-1));
        if (arrowRight != null) arrowRight.onClick.AddListener(() => Navigate(1));
        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
        if (backButton != null) backButton.onClick.AddListener(OnBack);
    }

    private void Update()
    {
        if (!inputEnabled) return;

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            Navigate(1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            Navigate(-1);
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            OnConfirm();
        else if (Input.GetKeyDown(KeyCode.Escape))
            OnBack();

        HandleGamepadInput();
    }

    private void HandleGamepadInput()
    {
        Vector2 move = PadInput.ReadAnyMove(2);

        if (move.x > 0.7f && !stickRightPressed)
        {
            Navigate(1);
            stickRightPressed = true;
        }
        else if (move.x <= 0.5f)
        {
            stickRightPressed = false;
        }

        if (move.x < -0.7f && !stickLeftPressed)
        {
            Navigate(-1);
            stickLeftPressed = true;
        }
        else if (move.x >= -0.5f)
        {
            stickLeftPressed = false;
        }

        if (PadInput.AnySouthPressedThisFrame(2))
            OnConfirm();

        if (PadInput.AnyEastPressedThisFrame(2))
            OnBack();
    }

    private void Navigate(int direction)
    {
        if (maps == null || maps.Length == 0) return;

        currentIndex = (currentIndex + direction + maps.Length) % maps.Length;

        audioSource?.PlayOneShot(navigateSFX);
        StartCoroutine(FadeAndShow(currentIndex));
    }

    private void ShowMap(int index, bool animate = true)
    {
        if (maps == null || maps.Length == 0 || index < 0 || index >= maps.Length) return;

        StageData map = maps[index];

        if (backgroundPreview != null && map.previewImage != null)
            backgroundPreview.sprite = map.previewImage;

        if (mapNameText != null)
            mapNameText.text = map.stageName.ToUpper();

        if (mapSubtitleText != null)
            mapSubtitleText.text = map.stageLocation;

        if (lockedPanel != null)
            lockedPanel.SetActive(map.isLocked);

        if (confirmButton != null)
            confirmButton.interactable = !map.isLocked;

        if (map.stageMusic != null && audioSource != null && audioSource.clip != map.stageMusic)
        {
            audioSource.clip = map.stageMusic;
            audioSource.Play();
        }

        UpdateDots(index);
    }

    private IEnumerator FadeAndShow(int index)
    {
        inputEnabled = false;

        if (previewCanvasGroup != null)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                previewCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
                yield return null;
            }
            previewCanvasGroup.alpha = 0f;
        }

        ShowMap(index);

        if (previewCanvasGroup != null)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                previewCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
                yield return null;
            }
            previewCanvasGroup.alpha = 1f;
        }

        inputEnabled = true;
    }

    private void BuildDots()
    {
        if (dotsContainer == null || dotPrefab == null || maps == null) return;

        dots = new Image[maps.Length];

        for (int i = 0; i < maps.Length; i++)
        {
            GameObject dot = Instantiate(dotPrefab, dotsContainer);
            dots[i] = dot.GetComponent<Image>();
        }

        UpdateDots(0);
    }

    private void UpdateDots(int activeIndex)
    {
        if (dots == null) return;

        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] != null)
                dots[i].color = (i == activeIndex) ? dotActive : dotInactive;
        }
    }

    private void OnConfirm()
    {
        if (maps == null || maps.Length == 0) return;

        StageData map = maps[currentIndex];

        if (map.isLocked)
        {
            audioSource?.PlayOneShot(lockedSFX);
            Debug.Log($"[MapSelect] Mapa bloqueado: {map.stageName}");
            return;
        }

        audioSource?.PlayOneShot(confirmSFX);
        SelectedMap = map;
        inputEnabled = false;

        Debug.Log($"[MapSelect] Mapa elegido: {map.stageName} -> cargando {map.sceneName}");

        StartCoroutine(LoadWithDelay(map.sceneName, 1f));
    }

    private IEnumerator LoadWithDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    private void OnBack()
    {
        FlujoEscenasManager.IrASeleccionPersonajes();
    }
}
