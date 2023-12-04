using Quantum;
using UnityEngine;

[RequireComponent(typeof(EntityView))]
public class PlayerColorizer : MonoBehaviour
{
    private Renderer renderer;
    private bool subscribed;
    private Color baseColor;

    private void Awake()
    {
        renderer = GetComponentInChildren<Renderer>();
        baseColor = renderer.material.color;
    }

    private void Update()
    {
        Frame frame = QuantumRunner.Default.Game.Frames.Predicted;
        EntityRef entityRef = GetComponent<EntityView>().EntityRef; 
        PlayerLink playerlink = frame.Get<PlayerLink>(entityRef);

        if (entityRef != playerlink.Entity)
            return;

        if (renderer.material.color != baseColor)
        {
            enabled = false;
            return;
        }

        renderer.material.color = playerlink.GetPlayerColor();
    }
}