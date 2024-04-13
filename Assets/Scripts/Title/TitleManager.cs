using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    private InputAction anyKeyAction;

    private void Awake()
    {
        // �C�ӂ̃L�[���͂ɑ΂���A�N�V������ݒ�
        anyKeyAction = new InputAction(type: InputActionType.PassThrough);
        anyKeyAction.AddBinding("<Keyboard>/anyKey");
        anyKeyAction.performed += OnAnyKeyPerformed;
        anyKeyAction.Enable();
    }

    private void Start()
    {
        SceneFadeManager.Instance.FadeIn();
    }

    private void OnAnyKeyPerformed(InputAction.CallbackContext context)
    {
        SceneFadeManager.Instance.FadeOut(LoadScene);
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
