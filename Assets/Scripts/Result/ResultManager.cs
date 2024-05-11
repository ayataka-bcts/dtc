using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    private InputAction anyKeyAction;

    public float currentScore;
    public float[] rankingScores;

    private bool isRequested = false;

    private void Start()
    {
        currentScore = PlayerPrefs.GetFloat("CurrentScore");

        // 任意のキー入力に対するアクションを設定
        anyKeyAction = new InputAction(type: InputActionType.PassThrough);
        anyKeyAction.AddBinding("<Keyboard>/anyKey");
        anyKeyAction.performed += OnAnyKeyPerformed;
        anyKeyAction.Enable();

        Ranking ranking = new Ranking(currentScore);
        
        rankingScores = ranking.GetRankingTime();

        SceneFadeManager.Instance.FadeIn();

        BGMManager.Instance.Stop();
    }

    private void Update()
    {

    }

    private void OnAnyKeyPerformed(InputAction.CallbackContext context)
    {
        if (!isRequested)
        {
            isRequested = true;

            SceneFadeManager.Instance.FadeOut(() =>
            {
                SceneManager.LoadScene("Title");

            });
        }
    }

    private void OnDestroy()
    {
        anyKeyAction.Disable();
    }
}
