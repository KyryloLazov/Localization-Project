using UnityEngine;

public sealed class WaitConfigReady : MonoBehaviour
{
    private async void Awake()
    {
        await ConfigInitGate.WaitReady();
    }
}