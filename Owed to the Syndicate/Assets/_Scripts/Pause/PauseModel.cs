using UnityEngine;

[CreateAssetMenu(fileName = "PauseModel", menuName = "Game/Pause Model")]
public class PauseModel : ScriptableObject
{
    [Header("Settings")]
    [SerializeField] private bool pauseTimeOnOpen = true;
    [SerializeField] private bool disableInputOnPause = true;

    public bool PauseTimeOnOpen => pauseTimeOnOpen;
    public bool DisableInputOnPause => disableInputOnPause;
}