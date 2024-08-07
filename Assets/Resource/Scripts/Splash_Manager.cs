using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Device;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Win32;

/// <summary>
/// 스플래시 화면을 보여준 후 전체화면 변경/메인 프로세스 실행
/// </summary>

public class Splash_Manager : MonoBehaviour
{
    public void Awake()
    {
        UnityEngine.Application.runInBackground = true;
        UnityEngine.Application.targetFrameRate = 30;
        this.Init();
        Splash.SetActive(true); // 스플래시 실행
    }

#region 배경 투명 오버레이 설정 변수
    public struct MARGINS
    {
        public int leftWidth;
        public int rightWidth;
        public int topHeight;
        public int bottomHeight;
    }
    
    OpenFileDialog OpenDialog;
    Stream openStream = null;

 
    [DllImport("user32.dll")]
    public static extern IntPtr GetActiveWindow();
 
    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
 
    [DllImport("user32.dll")]
    public static extern int BringWindowToTop(IntPtr hwnd);
 
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
 
    [DllImport("Dwmapi.dll")]
    public static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);
 
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
 
    IntPtr hWnd;
    const UInt32 SWP_NOSIZE = 0x0001;
    const UInt32 SWP_NOMOVE = 0x0002;
 
    const int GWL_EXSTYLE = -20;
    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TRANSTPARENT = 0x00000020;
 
    void Start()
    {
        hWnd = GetActiveWindow();
 
        MARGINS margins = new MARGINS { leftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);
        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
 
        BringWindowToTop(hWnd);
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE);
    }
#endregion

    [Header("스플래시 화면 구성 컴포넌트")]
    public GameObject Splash;
    public float timer = 0f;
    [Range(1f,10f)] public float limit_Time;

    void Update()
    {
#region 이미지 오버레이
        BringWindowToTop(hWnd);
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE);
        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSTPARENT);
#endregion

        timer += Time.deltaTime;
        if (timer > limit_Time)
        {
            Setting();
            timer = 0f;
        }
    }

    public void Init()
    {
        timer = 0f;
        OnStartProgram();
    }
    public void Setting()
    {
        SceneManager.LoadSceneAsync("Puddinget");
    }

    // 시작프로그램으로 이 앱을 등록
    public void OnStartProgram()
    {
        try
        {
            // 시작프로그램 등록하는 레지스트리
            string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            RegistryKey strUpKey = Registry.LocalMachine.OpenSubKey(runKey);
            if (strUpKey.GetValue("Puddinget") == null)
            {
                strUpKey.Close();
                strUpKey = Registry.LocalMachine.OpenSubKey(runKey, true);
                // 시작프로그램 등록명과 exe경로를 레지스트리에 등록
                strUpKey.SetValue("Puddinget", System.Windows.Forms.Application.ExecutablePath);
            }
            MessageBox.Show("시작프로그램에 푸딩젯을 등록했습니다." + "\n" + "(시작 시 실행을 취소하고 싶다면 작업관리자에서 삭제)");
        }
        catch
        {
            MessageBox.Show("시작프로그램에 등록하지 못했습니다." + "\n" + "(관리자 권한으로 실행 필요)");
        }
    }
}