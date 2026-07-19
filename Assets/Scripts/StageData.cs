using UnityEngine;

/// <summary>
/// ScriptableObject que define los datos de cada escenario.
/// Crear en: Assets > Create > StageSelect > Stage Data
/// </summary>
[CreateAssetMenu(fileName = "StageData", menuName = "StageSelect/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("Información del Escenario")]
    public string stageName;
    public string stageLocation;
    [TextArea(2, 4)]
    public string stageDescription;

    [Header("Visuals")]
    public Sprite previewImage;       // Imagen de preview del escenario
    public Sprite thumbnailImage;     // Miniatura para el grid
    public string sceneName;          // Nombre de la escena en Build Settings

    [Header("Audio")]
    public AudioClip stageMusic;

    [Header("Configuración")]
    public bool isLocked = false;     // Si está bloqueado
    public int requiredWins = 0;      // Victorias necesarias para desbloquear (Por el momemto descativado)
}
