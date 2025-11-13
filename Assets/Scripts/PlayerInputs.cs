using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    public event EventHandler OnJump;
    public event EventHandler OnInteract;
    private PlayerInputActions pInputActions;
    private PlayerMovement playerMovement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        pInputActions = new PlayerInputActions();
        pInputActions.Player.Enable();
        pInputActions.Player.Jump.started += Jump_started;
        pInputActions.Player.Interact.performed += Interact_performed;
    }

    private void Interact_performed(InputAction.CallbackContext obj)
    {
        OnInteract?.Invoke(this, EventArgs.Empty);
    }

    private void Jump_started(InputAction.CallbackContext obj)
    {
            OnJump?.Invoke(this, EventArgs.Empty);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        pInputActions.Player.Enable();
    }

    public Vector2 GetMovementVector()
    {
        Vector2 inputVector = pInputActions.Player.Move.ReadValue<Vector2>();
        inputVector.Normalize();
        return inputVector;
    }

    public bool isJumpButtonHeld()
    {
        return pInputActions.Player.Jump.IsPressed();
    }

}
