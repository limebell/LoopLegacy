#if UNITY_EDITOR 
#define ENABLE_LOGGING
#endif
#define ENABLE_LOGGING

using UnityEngine;
using System;
using LoopLegacy.UI.Controller;


/// 
/// It overrides UnityEngine.Debug to mute debug messages completely on a platform-specific basis.
/// 
/// Putting this inside of 'Plugins' foloder is ok.
/// 
/// Important:
///     Other preprocessor directives than 'UNITY_EDITOR' does not correctly work.
/// 
/// Note:
///     [Conditional] attribute indicates to compilers that a method call or attribute should be 
///     ignored unless a specified conditional compilation symbol is defined.
/// 
/// See Also: 
///     http://msdn.microsoft.com/en-us/library/system.diagnostics.conditionalattribute.aspx
/// 
/// 2012.11. @kimsama
/// 
public static class Debug 
{
    private static bool _isInitialized = false;
    
    public static bool isDebugBuild
    {
	get { return UnityEngine.Debug.isDebugBuild; }
    }
    
    // Debug 클래스 초기화 - Application.logMessageReceived 이벤트 등록
    public static void Initialize()
    {
        if (_isInitialized) return;
        
        Application.logMessageReceived += OnLogMessageReceived;
        _isInitialized = true;
    }
    
    // Debug 클래스 정리 - 이벤트 해제
    public static void Cleanup()
    {
        if (!_isInitialized) return;
        
        Application.logMessageReceived -= OnLogMessageReceived;
        _isInitialized = false;
    }
    
    // 모든 로그 메시지를 캐치하여 LogController에 전달
    private static void OnLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        if (LogController.Instance == null) return;
        
        switch (type)
        {
            case LogType.Log:
                LogController.Instance.Log(logString);
                break;
            case LogType.Warning:
                LogController.Instance.LogWarning(logString);
                break;
            case LogType.Error:
            case LogType.Exception:
                LogController.Instance.LogError(logString + (string.IsNullOrEmpty(stackTrace) ? "" : "\n" + stackTrace));
                break;
            case LogType.Assert:
                LogController.Instance.LogError("[ASSERT] " + logString + (string.IsNullOrEmpty(stackTrace) ? "" : "\n" + stackTrace));
                break;
        }
    }

#if !ENABLE_LOGGING
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
#endif
    public static void Log (object message)
    {   
        UnityEngine.Debug.Log (message);
    }

#if !ENABLE_LOGGING
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
#endif
    public static void Log (object message, UnityEngine.Object context)
    {   
        UnityEngine.Debug.Log (message, context);
    }

#if !ENABLE_LOGGING
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
#endif
    public static void LogError (object message)
    {   
        UnityEngine.Debug.LogError (message);
    }

#if !ENABLE_LOGGING
    [System.Diagnostics.Conditional("UNITY_EDITOR")]	
#endif
    public static void LogError (object message, UnityEngine.Object context)
    {   
        UnityEngine.Debug.LogError (message, context);
    }
 
#if !ENABLE_LOGGING
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
#endif
    public static void LogWarning (object message)
    {   
        UnityEngine.Debug.LogWarning (message.ToString ());
    }

#if !ENABLE_LOGGING
    [System.Diagnostics.Conditional("UNITY_EDITOR")] 
#endif
    public static void LogWarning (object message, UnityEngine.Object context)
    {   
        UnityEngine.Debug.LogWarning (message.ToString (), context);
    }

#if !ENABLE_LOGGING
    [System.Diagnostics.Conditional("UNITY_EDITOR")] 
#endif
    public static void DrawLine(Vector3 start, Vector3 end, Color color = default(Color), float duration = 0.0f, bool depthTest = true)
    {
 	UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
    } 
	
#if !ENABLE_LOGGING
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
#endif
    public static void DrawRay(Vector3 start, Vector3 dir, Color color = default(Color), float duration = 0.0f, bool depthTest = true)
    {
	UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
    }
 	
#if !ENABLE_LOGGING
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
#endif
    public static void Assert(bool condition)
    {
	if (!condition) throw new Exception();
    }
}