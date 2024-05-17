using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip successBGM;

    private InputAction pressButtonAction;

    public bool isFailure = false;

    public float currentScore;
    public float[] rankingScores;

    private bool isRequested = false;

    private Ranking ranking;

    private void Start()
    {
        // 任意のキー入力に対するアクションを設定
        pressButtonAction = new InputAction(type: InputActionType.PassThrough);
        pressButtonAction.AddBinding("<Keyboard>/Space");
        pressButtonAction.AddBinding("<Gamepad>/ButtonSouth");
        pressButtonAction.performed += OnPressButtonPerformed;
        pressButtonAction.Enable();

        if(!isFailure)
        {
            currentScore = PlayerPrefs.GetFloat("CurrentScore");
            ranking = new Ranking(currentScore);
            rankingScores = ranking.GetRankingTime();
        }


        SceneFadeManager.Instance.FadeIn();

        BGMManager.Instance.Stop();
        if (successBGM != null && !isFailure)
        {
            BGMManager.Instance.Play(successBGM);
        }
    }

    private void Update()
    {

    }

    private void OnPressButtonPerformed(InputAction.CallbackContext context)
    {
        if (!isRequested)
        {
            isRequested = true;

            if(!isFailure)
            {
                ranking.SaveRankingData();
            }

            SceneFadeManager.Instance.FadeOut(() =>
            {
                SceneManager.LoadScene("Title");

            });
        }
    }

    private void OnDestroy()
    {
        pressButtonAction.Disable();
    }
}
