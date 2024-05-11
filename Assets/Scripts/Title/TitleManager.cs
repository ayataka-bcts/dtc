using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    private InputAction anyKeyAction;

    private bool isRequested = false;

    private void Awake()
    {
        // 任意のキー入力に対するアクションを設定
        anyKeyAction = new InputAction(type: InputActionType.PassThrough);
        anyKeyAction.AddBinding("<Keyboard>/anyKey");
        anyKeyAction.performed += OnAnyKeyPerformed;
        anyKeyAction.Enable();
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
        anyKeyAction.Disable();
    }
}
