using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Device;
using UnityEditor;

/// <summary>
/// 레퍼런스: https://wizemean.tistory.com/14
/// </summary>

public enum appState
{
    Menu = 0,
    Processing = 1
}

public enum animation_List
{
    Default = 0,
    Rotation = 1,
    Jumping = 2
}

public class Puddinget_Manager : MonoBehaviour
{
    public void Awake()
    {
        UnityEngine.Application.runInBackground = true;
        UnityEngine.Application.targetFrameRate = 30;
        Init();
    }

    public struct MARGINS
    {
        public int leftWidth;
        public int rightWidth;
        public int topHeight;
        public int bottomHeight;
    }
 
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

    [Header("제어 변수")]
    public bool toggle = true;
    public bool positionLock = true; 
    public bool isDrag = false;
    [Range(0.1f, 100f)]  public float speed_UI;
    public float minSize = 0.1f;
    public float maxSize = 2f;
    public float nowSize = 1f;
    public int nowAnimation = 0;


    [Header("커스텀 이미지")]
    public Image puddinget_IMG = null;
    public Image default_IMG; // 기본 이미지 (푸딩젯)
    public Image main_Texture; // 실제 위젯에 적용할 이미지
    public string url_IMG; // 이미지 경로


    [Header("오브젝트")]
    public GameObject puddinget; // 푸딩젯 메인 오브젝트
    public RectTransform puddinget_RectTransform; // 푸딩젯 트랜스폼
    public GameObject popup_Setting;

    [Header("사용자 환경변수")]
    private Vector2 mousePos;

    public GameObject panel_menu;
    public GameObject panel_process;

    public void Init()
    {
        SetState(appState.Menu);

        if (main_Texture != null)
        {
            puddinget_IMG = main_Texture;
        }
        else
        {
            puddinget_IMG = default_IMG;
        }
        
        Load_Scale();
        Load_Animation();
        puddinget_RectTransform.localScale = new Vector2(nowSize, nowSize);
        puddinget_RectTransform.anchoredPosition = Vector2.zero;
    }

    public void Update()
    {

#region 이미지 오버레이
        BringWindowToTop(hWnd);
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE);
        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSTPARENT);
#endregion

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnityEngine.Application.Quit();
        }
#endif

        // 포지션 락을 풀고 드래그하면 이미지 위치 조정 가능
        if (isDrag && !positionLock)
        {
            mousePos = Input.mousePosition;
            puddinget_RectTransform.anchoredPosition = mousePos;
        }

        /*
        if (Input.GetKeyUp(KeyCode.Space))
        {
            toggle = !toggle;
 
            BringWindowToTop(hWnd);
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE);
            
            if (toggle)
            {
                SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
            }
            else
            {
                SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSTPARENT);
            }
        }
        */
    }

    public void GetIMGFile()
    {

    }

#region 앱 상태 관리
    public void SetState(appState state)
    {
        switch (state)
        {
            case appState.Menu:
                panel_menu.SetActive(true);
                panel_process.SetActive(false);
                break;
            case appState.Processing:
                panel_menu.SetActive(false);
                panel_process.SetActive(true);
                break;
            default:
                panel_menu.SetActive(true);
                panel_process.SetActive(false);
                break;
        }
    }
#endregion

    public void Drag()
    {
        isDrag = true;
    }

    public void Drop()
    {
        isDrag = false;
    }

#region 이미지 사이즈 조절
    public void Load_Slider(Slider sizeSlider)
    {
        sizeSlider.value = nowSize;
        sizeSlider.minValue = minSize;
        sizeSlider.maxValue = maxSize;
    }

    public void Resizing(Slider sizeSlider)
    {
        nowSize = sizeSlider.value;
        puddinget_RectTransform.localScale = new Vector2(sizeSlider.value, sizeSlider.value);
        Save_scale();
    }

    public void Save_scale()
    {
        PlayerPrefs.SetFloat("scale", nowSize);
    }

    public void Load_Scale()
    {
        float scale; 

        try
        {
            scale = PlayerPrefs.GetFloat("scale");
        }
        catch
        {
            scale = 1f;
        }

        nowSize = scale;
    }
#endregion

#region 이미지 애니메이션 설정
    public void Select_Animation(animation_List index)
    {
        switch (index)
        {
            case (animation_List)0:
                break;
            case (animation_List)1:
                break;
            case (animation_List)2:
                break;
            default:
                break;
        }
    }

    public void Save_Animation()
    {
        PlayerPrefs.SetInt("animation", nowAnimation);
    }

    public void Load_Animation()
    {
        int animation;
        try
        {
            animation = PlayerPrefs.GetInt("animation");
        }
        catch
        {
            animation = 0;
        }

        Select_Animation((animation_List)animation);
    }
#endregion

#region 설정 관련 메서드
    public void Toggle_Popup()
    {
        popup_Setting.SetActive(!popup_Setting.activeSelf);

        if (popup_Setting.activeSelf)
        {
            //Load_Slider(Slider sizeSlider);
        }
    }

    public void Toggle_Lock()
    {
        positionLock = !positionLock;
    
        if (positionLock)
        {
            // 자물쇠 스프라이트 활성화
        }
        else
        {
            // 자물쇠 열림 스프라이트 활성화
        }
    }

    public void Onclick_MainMenu()
    {
        SetState(appState.Menu);
    }

    void Exit_Program()
    {
        UnityEngine.Application.Quit();
    }
#endregion

}
