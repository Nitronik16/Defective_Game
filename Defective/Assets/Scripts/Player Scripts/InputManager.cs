using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput PlayerInput;

    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;
    public static bool AdrenalineIsHeld;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction adrenalineAction;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        moveAction = PlayerInput.actions["Movement"];
        jumpAction = PlayerInput.actions["Jump"];
        adrenalineAction = PlayerInput.actions["Adrenaline"];
    }

    private void Update()
    {
        Movement = moveAction.ReadValue<Vector2>();

        JumpWasPressed = jumpAction.WasPerformedThisFrame();
        JumpIsHeld = jumpAction.IsPressed();
        JumpWasReleased = jumpAction.WasReleasedThisFrame();

        AdrenalineIsHeld = adrenalineAction.IsPressed();
    }
}
