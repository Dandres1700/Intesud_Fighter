public static class GameModeManager
{
    public enum ModoJuego { J1vsJ2, J1vsCPU, CPUvsCPU }
    public enum Dificultad { Facil, Medio, Dificil }

    public static ModoJuego ModoActual { get; private set; } = ModoJuego.J1vsJ2;
    public static Dificultad DificultadActual { get; private set; } = Dificultad.Medio;

    public static void SetModo(ModoJuego modo) => ModoActual = modo;
    public static void SetDificultad(Dificultad dificultad) => DificultadActual = dificultad;

    public static bool UsaCPUJugador1() => ModoActual == ModoJuego.CPUvsCPU;
    public static bool UsaCPUJugador2() => ModoActual == ModoJuego.J1vsCPU || ModoActual == ModoJuego.CPUvsCPU;
}