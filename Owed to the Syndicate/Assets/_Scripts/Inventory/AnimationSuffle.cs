using UnityEngine;
using System.Collections;
using System;

public class AnimationSuffle : MonoBehaviour
{
    private Animator _animator;

    [Header("Settings")]
    [SerializeField] private string _triggerName = "StartAnim";
    [SerializeField] private float _moveDuration = 0.7f;
    public event Action OnShuffleRequested;

    
    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Lance la séquence : Déplacement + Animation + Destruction
    /// </summary>
    /// <param name="startPos">Position de départ</param>
    /// <param name="endPos">Position d'arrivée</param>
    public void StartSequence(Vector3 startPos, Vector3 endPos, Action onComplete = null)
    {
        transform.position = startPos;
        StartCoroutine(MoveAndAnimateRoutine(startPos, endPos, onComplete));
    }
    private void ShuffleDeck()
    {
        // logique shuffle du modèle...
        OnShuffleRequested?.Invoke();
    }
    private IEnumerator MoveAndAnimateRoutine(Vector3 start, Vector3 end, Action onComplete)
    {
        float elapsed = 0;

        if (_animator != null)
        {
            _animator.SetTrigger(_triggerName);
        }

        while (elapsed < _moveDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / _moveDuration;

            transform.position = Vector3.Lerp(start, end, percent);
            yield return null;
        }

        transform.position = end;

        yield return new WaitForSeconds(0.7f);

        try
        {
            onComplete?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"AnimationSuffle onComplete threw: {ex}");
        }

        Destroy(gameObject);
    }
}
