using System;
using Photon.Deterministic;
using Quantum;
using UnityEngine;

public class LocalInput : MonoBehaviour 
{
    private void OnEnable() 
    {
        QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
    }

    public void PollInput(CallbackPollInput callback) 
    {
        Quantum.Input input = new Quantum.Input();

        input.Jump = UnityEngine.Input.GetButton("Jump");
        input.Direction = new Vector2(UnityEngine.Input.GetAxis("Horizontal"), UnityEngine.Input.GetAxis("Vertical")).ToFPVector2();

        callback.SetInput(input, DeterministicInputFlags.Repeatable);
    }
}
