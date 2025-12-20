using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// MonoBehaviour runner for animations. Allows AnimationService to remain a pure service.
/// </summary>
public class AnimationRunner : MonoBehaviour
{
    private static AnimationRunner instance;
    [SerializeField] private GameObject shufflePrefab;


    public Coroutine PlayShuffleAnimation(Vector3 start, Vector3 end, Transform parent, Action onComplete = null)
    {
        if (shufflePrefab == null) return null;
        var go = Instantiate(shufflePrefab, parent);
        var anim = go.GetComponent<AnimationSuffle>();
        if (anim != null)
        {
            anim.StartSequence(start, end, onComplete);
        }
        return null;
    }

    public static AnimationRunner Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("AnimationRunner");
                instance = go.AddComponent<AnimationRunner>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    public Coroutine Run(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }

    public void Stop(Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }
}