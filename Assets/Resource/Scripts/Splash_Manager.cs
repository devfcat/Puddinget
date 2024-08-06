using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
 
[Header("스플래시 화면 구성 컴포넌트")]
public GameObject Splash;
public float timer;

public class Splash_Manager : MonoBehaviour
{
    public Awake()
    {
        UnityEngine.Application.runInBackground = true;
        UnityEngine.Application.targetFrameRate = 30;
        Init();
        Splash.SetActive(true);
    }

    public void Init()
    {
        Screen.SetResolution(300f, 400f, false);
    }

    public void Setting()
    {
        Screen.SetResolution(Screen.width, Screen.height, true);
    }
}