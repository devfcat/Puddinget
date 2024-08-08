using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 소프트웨어의 무료/유료 버전을 확인한 후 기능을 제한하는 매니저 클래스
/// </summary>
public class TrialManager : MonoBehaviour
{
    private static TrialManager _instance;
    public static TrialManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindObjectOfType(typeof(TrialManager)) as TrialManager;

                if (_instance == null)
                    Debug.Log("no Singleton obj");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    [Header("소프트웨어 타입")]
    public SoftewareType type; // 인스펙터에서 조절가능

    public enum SoftewareType 
    {
        Trial = 0, // 체험판
        Premium, // 유료 버전
    }
}
