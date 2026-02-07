using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting;


public struct BufferedInput {
    public InputActionType actionType;
    public float timestamp;

    public BufferedInput(InputActionType actionType, float timestamp) {
        this.actionType = actionType;
        this.timestamp = timestamp;
    }
}

public class InputBuffer : MonoBehaviour {

    [Header("Buffer Settings")]
    [SerializeField] private float bufferWindow = 0.2f;
    [SerializeField] private int maxBufferSize = 3;

    public Queue<BufferedInput> inputQueue = new Queue<BufferedInput>();
    private InputSystem_Actions inputActions;

    void Awake() {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable() {
        inputActions.Player.Enable();
        inputActions.Player.LightAttack.performed += OnLightAttack;
        inputActions.Player.HeavyAttack.performed += OnHeavyAttack;
        inputActions.Player.Parry.performed += OnParry;
        inputActions.Player.Dodge.performed += OnDodge;
    }

    private void OnDisable() {
        inputActions.Player.LightAttack.performed -= OnLightAttack;
        inputActions.Player.HeavyAttack.performed -= OnHeavyAttack;
        inputActions.Player.Parry.performed -= OnParry;
        inputActions.Player.Dodge.performed -= OnDodge;
        inputActions.Player.Disable();
    }

    void Update() {
        CleanExpiredInput();
    }

    private void OnLightAttack(InputAction.CallbackContext context) {
        AddInputToBuffer(InputActionType.LIGHT_ATTACK);
    }

    private void OnHeavyAttack(InputAction.CallbackContext context) {
        AddInputToBuffer(InputActionType.HEAVY_ATTACK);
    }

    private void OnParry(InputAction.CallbackContext context) {
        AddInputToBuffer(InputActionType.PARRY);
    }

    private void OnDodge(InputAction.CallbackContext context) {
        AddInputToBuffer(InputActionType.DODGE);
    }

    private void AddInputToBuffer(InputActionType actionType) {
        if (inputQueue.Count >= maxBufferSize)
            inputQueue.Dequeue();

        BufferedInput newInput = new BufferedInput(actionType, Time.time);
        inputQueue.Enqueue(newInput);
    }

    private void CleanExpiredInput() {
        while (inputQueue.Count > 0) {
            BufferedInput oldestInput = inputQueue.Peek();
            if (Time.time - oldestInput.timestamp > bufferWindow)
                inputQueue.Dequeue();
            else
                break;
        }
    }

    public bool PeekNextInput(out InputActionType inputActionType) {

        if (inputQueue.Count > 0) {
            BufferedInput bufferedInput = inputQueue.Peek();
            inputActionType = bufferedInput.actionType;
            return true;
        }

        inputActionType = InputActionType.LIGHT_ATTACK; // Default value
        return false;
    }

    public bool TryConsumeInput(out InputActionType inputActionType) {
        if (inputQueue.Count > 0) {
            BufferedInput bufferedInput = inputQueue.Dequeue();
            inputActionType = bufferedInput.actionType;
            return true;
        }
        inputActionType = InputActionType.LIGHT_ATTACK; // Default value
        return false;
    }

    public void ClearBuffer() {
        inputQueue.Clear();
    }

    public int GetBufferInputCount() {
        return inputQueue.Count;
    }
}
