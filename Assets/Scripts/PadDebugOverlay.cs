using System.Text;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PadDebugOverlay : MonoBehaviour
{
    private static PadDebugOverlay instance;

    [SerializeField] private bool visible = true;
    [SerializeField] private int maxPlayers = 2;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    [SerializeField] private Rect panelRect = new Rect(20f, 20f, 520f, 360f);

    private readonly StringBuilder builder = new StringBuilder(512);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (instance != null) return;

        GameObject go = new GameObject(nameof(PadDebugOverlay));
        instance = go.AddComponent<PadDebugOverlay>();
        DontDestroyOnLoad(go);
#endif
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            visible = !visible;
    }

    private void OnGUI()
    {
        if (!visible) return;

        GUI.depth = -1000;
        builder.Clear();
        builder.AppendLine("PAD DEBUG - F1 para ocultar/mostrar");
        builder.AppendLine($"Time: {Time.time:0.00}");
        builder.AppendLine($"Joysticks: {Input.GetJoystickNames().Length}");
#if ENABLE_INPUT_SYSTEM
        builder.AppendLine($"Gamepads: {Gamepad.all.Count}");
        builder.AppendLine($"Asignados: {PadInput.GetAssignedGamepadCount()}");
#endif

        builder.AppendLine($"P1 asignado: {PadInput.GetAssignedGamepadName(1)}");
        builder.AppendLine($"P2 asignado: {PadInput.GetAssignedGamepadName(2)}");

#if ENABLE_INPUT_SYSTEM
        builder.AppendLine();
        builder.AppendLine("Dispositivos Input System:");
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Gamepad pad = Gamepad.all[i];
            if (pad == null) continue;

            builder.AppendLine(
                $"  [{pad.deviceId}] {pad.displayName} | {pad.description.interfaceName} | {pad.description.product}"
            );
        }
#endif

        for (int playerIndex = 1; playerIndex <= maxPlayers; playerIndex++)
        {
            bool connected = PadInput.HasGamepad(playerIndex);
            string deviceName = PadInput.GetAssignedGamepadName(playerIndex);
            Vector2 move = PadInput.ReadMove(playerIndex);
            Vector2 menuMove = PadInput.ReadMenuMove(playerIndex);

            builder.AppendLine();
            builder.AppendLine($"P{playerIndex}: {(connected ? "CONNECTED" : "NOT CONNECTED")}");
            builder.AppendLine($"  Device: {deviceName}");
            builder.AppendLine($"  Move: x={move.x:0.00} y={move.y:0.00}");
            builder.AppendLine($"  Menu:  x={menuMove.x:0.00} y={menuMove.y:0.00}");
            builder.AppendLine($"  South/Cross: {PadInput.SouthPressedThisFrame(playerIndex)}");
            builder.AppendLine($"  East/Circle:  {PadInput.EastPressedThisFrame(playerIndex)}");
            builder.AppendLine($"  West/Square:  {PadInput.WestPressedThisFrame(playerIndex)}");
            builder.AppendLine($"  North/Triangle: {PadInput.NorthPressedThisFrame(playerIndex)}");
        }

        GUI.Box(panelRect, GUIContent.none);
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 12f, panelRect.width - 24f, panelRect.height - 24f), builder.ToString());
    }
}
