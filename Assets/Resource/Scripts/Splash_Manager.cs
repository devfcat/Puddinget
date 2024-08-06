using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

/// <summary>
/// 스플래시 화면을 보여준 후 전체화면 변경/메인 프로세스 실행
/// </summary>

public class Splash_Manager : MonoBehaviour
{
    [Header("스플래시 화면 구성 컴포넌트")]
    public GameObject Splash;
    public float timer = 0f;
    [Range(1f,10f)] public float limit_Time;

    public void Awake()
    {
        UnityEngine.Application.runInBackground = true;
        UnityEngine.Application.targetFrameRate = 30;
        this.Init();
        Splash.SetActive(true);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > limit_Time)
        {
            Setting();
            timer = 0f;
        }
    }

    public void Init()
    {
        UnityEngine.Screen.SetResolution(300, 400, false);
        timer = 0f;
    }
    public void Setting()
    {
        Screen.SetResolution(Screen.width, Screen.height, true);
        SceneManager.LoadSceneAsync("Puddinget");
    }
}