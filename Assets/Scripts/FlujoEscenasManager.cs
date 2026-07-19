using UnityEngine;
using UnityEngine.SceneManagement;

public static class FlujoEscenasManager
{
    public const string MenuSceneName        = "MenuPrincipal";
    public const string MenuJuegoSceneName   = "Menu_Juego";
    public const string DificultadSceneName  = "Dificultad";
    public const string CharacterSceneName   = "CharacterSelect";
    public const string MapSceneName         = "MapSelect";
    public const string GameSceneName        = "GameScene";

    public static void IrAMenuPrincipal()       => CargarEscena(MenuSceneName);
    public static void IrAMenuJuego()           => CargarEscena(MenuJuegoSceneName);
    public static void IrADificultad()          => CargarEscena(DificultadSceneName);
    public static void IrASeleccionPersonajes() => CargarEscena(CharacterSceneName);
    public static void IrASeleccionMapa()       => CargarEscena(MapSceneName);
    public static void IrAGameScene()           => CargarEscena(GameSceneName);

    public static void SalirJuego()
    {
        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }

    private static void CargarEscena(string nombreEscena)
    {
        if (Application.CanStreamedLevelBeLoaded(nombreEscena))
            SceneManager.LoadScene(nombreEscena);
        else
            Debug.LogError($"No se pudo cargar la escena: '{nombreEscena}'. Revisá Build Settings.");
    }
}