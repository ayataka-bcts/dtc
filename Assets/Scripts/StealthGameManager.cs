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
    }
    private GameState state;

    public float timer{  get; private set; } = 0.0f;

    [SerializeField]
    private AudioClip bgmAudioClip;

    // Start is called before the first frame update
    void Start()
    {
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
                        });
                    }
                }
                break;
            case GameState.InGame:
                if(!BGMManager.Instance.IsPlaying())
                {
                    BGMManager.Instance.Play(bgmAudioClip);
                }
                timer += Time.deltaTime;
                break;
            case GameState.Pause:
                break;
            default:
                break;
        }

    }

    private void OnDestroy()
    {
        PlayerPrefs.SetFloat("CurrentScore", timer);

        EnemyState.OnCatchPlayer -= GameOverTrans;
        Goal.OnSuccessGame -= GameClearTrans;
    }

    public static void GameOverTrans()
    {
        if(!SceneFadeManager.Instance.IsPlayingFadeOut())
        {
            SceneFadeManager.Instance.FadeOut(() =>
            {
                SceneManager.LoadScene("Failure");
            });
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
