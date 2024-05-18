using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneFadeManager : FadeManager<SceneFadeManager>
{
    //=================================================================================
    //初期化
    //=================================================================================

    //起動時に実行される
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        var managerPrefab = Resources.Load<SceneFadeManager>("Prefabs/FadeManager");
        Instantiate(managerPrefab);
    }

    private Image fadePannel;

    private Sequence fadeInSequence;
    private Sequence fadeOutSequence;

    ~SceneFadeManager()
    {
        fadeInSequence.Kill();
        fadeOutSequence.Kill();
    }

    protected override void Init()
    {
        base.Init();

        fadePannel = GetComponentInChildren<Image>();

        DontDestroyOnLoad(gameObject);
    }

    public bool IsPlayingFadeIn()
    {
        if(fadeInSequence == null || !fadeInSequence.IsActive())
        {
            return false;
        }

        return fadeInSequence.IsPlaying();
    }

    public void FadeIn(float time = 1.0f)
    {
        fadePannel.DOFade(0.0f, time);
    }

    public void FadeIn(TweenCallback callback, float time = 1.0f)
    {
        fadeInSequence = DOTween.Sequence()
            .SetAutoKill(true)
            .Append(fadePannel.DOFade(0.0f, time))
            .AppendInterval(1.0f)
            .OnComplete(callback);
    }

    public bool IsPlayingFadeOut()
    {
        if(fadeOutSequence == null || !fadeOutSequence.IsActive())
        {
            return false;
        }

        return fadeOutSequence.IsPlaying();
    }

    public void FadeOut(float time = 1.0f)
    {
        fadePannel.DOFade(1.0f, time);
    }

    public void FadeOut(TweenCallback callback, float time = 1.0f)
    {
        fadeOutSequence = DOTween.Sequence()
            .SetAutoKill(true)
            .Append(fadePannel.DOFade(1.0f, time))
            .AppendInterval(1.0f)
            .OnComplete(callback);
    }
}
