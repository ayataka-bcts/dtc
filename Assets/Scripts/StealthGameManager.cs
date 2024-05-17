using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StealthGameManager : MonoBehaviour
{
    public enum GameState
    {
        Ready,
        InGame,
        Pause,
        End,
    }
    private GameState state;

    public float timer{  get; private set; } = 0.0f;

    [SerializeField]
    [Tooltip("ゲーム中の音楽")]
    [Label("BGM")]
    private AudioClip bgmAudioClip;
    [SerializeField]
    [Tooltip("プレイヤーが敵に捕まった時の音")]
    [Label("捕まったときの音")]
    private AudioClip catchAudioClip;
    [SerializeField]
    [Tooltip("プレイヤーが敵に見つかった時の音")]
    [Label("見つかったときの音")]
    private AudioClip foundAudioClip;

    public static AudioClip s_catchAudioClip;
    public static AudioClip s_foundAudioClip;

    // Start is called before the first frame update
    void Start()
    {
        s_catchAudioClip = catchAudioClip;
        s_foundAudioClip = foundAudioClip;

        timer = 0.0f;

        EnemyState.OnCatchPlayer += GameOverTrans;
        Goal.OnSuccessGame += GameClearTrans;
    }

    // Update is called once per frame
    void Update()
    {
        switch(state)
        {
            case GameState.Ready:

                if (SceneFadeManager.Instance)
                {
                    if(!SceneFadeManager.Instance.IsPlayingFadeIn())
                    {
                        SceneFadeManager.Instance.FadeIn(() =>
                        {
                            state = GameState.InGame;
                        }, 0.2f);
                    }
                }
                break;
            case GameState.InGame:
                if(!BGMManager.Instance.IsPlaying() && bgmAudioClip != null)
                {
                    BGMManager.Instance.Play(bgmAudioClip);
                }
                timer += Time.deltaTime;

                if(SceneFadeManager.Instance.IsPlayingFadeOut())
                {
                    state = GameState.End;

                    EnemyState.OnCatchPlayer -= GameOverTrans;
                    Goal.OnSuccessGame -= GameClearTrans;
                }
                break;
            case GameState.Pause:
                break;
            case GameState.End:
                break;
            default:
                break;
        }

    }

    private void OnDestroy()
    {
        PlayerPrefs.SetFloat("CurrentScore", timer);
    }

    public static void GameOverTrans()
    {
        if(!SceneFadeManager.Instance.IsPlayingFadeOut())
        {
            SceneFadeManager.Instance.FadeOut(() =>
            {
                SceneManager.LoadScene("Failure");
            });
            if(s_catchAudioClip != null)
            {
                SEManager.Instance.Play(s_catchAudioClip);
            }
        }
    }

    public static void GameClearTrans()
    {
        if (!SceneFadeManager.Instance.IsPlayingFadeOut())
        {
            SceneFadeManager.Instance.FadeOut(() =>
            {
                SceneManager.LoadScene("Result");
            });
        }
    }
}
