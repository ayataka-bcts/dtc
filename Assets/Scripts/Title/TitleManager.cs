using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip audioClip;

    private InputAction pressButtonAction;

    private bool isRequested = false;

    private void Awake()
    {
        // 任意のキー入力に対するアクションを設定
        pressButtonAction = new InputAction(type: InputActionType.PassThrough);
        pressButtonAction.AddBinding("<Keyboard>/Space");
        pressButtonAction.AddBinding("<Gamepad>/ButtonSouth");
        pressButtonAction.performed += OnAnyKeyPerformed;
        pressButtonAction.Enable();

        if(audioClip != null)
        {
            BGMManager.Instance.Play(audioClip);
        }
    }

    private void Start()
    {
        isRequested = false;

        SceneFadeManager.Instance.FadeIn();
    }

    private void OnAnyKeyPerformed(InputAction.CallbackContext context)
    {
        if(!isRequested)
        {
            SceneFadeManager.Instance.FadeOut(LoadScene);
            isRequested = true;
        }
    }

    private void LoadScene()
    {
        SceneManager.LoadScene("Main");

    }

    private void OnDestroy()
    {
        pressButtonAction.Disable();
    }
}
