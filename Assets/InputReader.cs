using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    public InputActionReference move;
    public InputActionReference jump;
    public InputActionReference interact;
    public InputActionReference hide;
    public InputActionReference transformAction;
    public InputActionReference pause;

    void OnEnable()
    {
        if (move) move.action.Enable();
        if (jump) jump.action.Enable();
        if (interact) interact.action.Enable();
        if (hide) hide.action.Enable();
        if (transformAction) transformAction.action.Enable();
        if (pause) pause.action.Enable();
    }
    void OnDisable()
    {
        if (move) move.action.Disable();
        if (jump) jump.action.Disable();
        if (interact) interact.action.Disable();
        if (hide) hide.action.Disable();
        if (transformAction) transformAction.action.Disable();
        if (pause) pause.action.Disable();
    }

    public Vector2 Move() => move ? move.action.ReadValue<Vector2>() : Vector2.zero;
    public bool JumpPressed() => jump && jump.action.WasPressedThisFrame();
    public bool InteractPressed() => interact && interact.action.WasPressedThisFrame();
    public bool HidePressed() => hide && hide.action.WasPressedThisFrame();
    public bool TransformPressed() => transformAction && transformAction.action.WasPressedThisFrame();
    public bool PausePressed() => pause && pause.action.WasPressedThisFrame();
}
