using Quantum;
using UnityEngine;

public class ClientLocalInput : MonoBehaviour
{
    private void OnEnable()
    {
        QuantumCallback.Subscribe<CallbackPollInput>(this, (callback) => PollInput(callback));
    }

    private void OnDisable()
    {
        QuantumCallback.UnsubscribeListener(this);
    }

    private void PollInput(CallbackPollInput callback)
    {
        Quantum.Input input = new Quantum.Input();

        input.Jump = UnityEngine.Input.GetButton("Jump");
        input.Direction = new Vector2(UnityEngine.Input.GetAxis("Horizontal"), UnityEngine.Input.GetAxis("Vertical")).ToFPVector2();
        input.Dash = UnityEngine.Input.GetKey(KeyCode.J);

        callback.SetInput(input, Photon.Deterministic.DeterministicInputFlags.Repeatable);

    }
}
