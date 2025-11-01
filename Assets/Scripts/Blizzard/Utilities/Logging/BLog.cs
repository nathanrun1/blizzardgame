using System;
using System.Diagnostics;
using UnityEngine;

namespace Blizzard.Utilities.Logging
{
    /// <summary>
    /// Custom Logger Class
    /// </summary>
    public static class BLog
    {
        private const string infoColor = nameof(Color.white);
        private const string warningColor = nameof(Color.yellow);
        private const string errorColor = nameof(Color.lightCoral);

        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void Log(object message)
        {
            UnityEngine.Debug.Log(FormatMessage(infoColor, message));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void Log(string category, object message)
        {
            UnityEngine.Debug.Log(FormatMessageWithCategory(infoColor, category, message));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void LogFormat(string format, params object[] args)
        {
            UnityEngine.Debug.Log(FormatMessage(infoColor, string.Format(format, args)));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void LogFormat(string category, string format, params object[] args)
        {
            UnityEngine.Debug.Log(FormatMessageWithCategory(infoColor, category, string.Format(format, args)));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(FormatMessage(warningColor, message));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void LogWarning(string category, object message)
        {
            UnityEngine.Debug.LogWarning(FormatMessageWithCategory(warningColor, category, message));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void LogWarningFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(FormatMessage(warningColor, string.Format(format, args)));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void LogWarningFormat(string category, string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(FormatMessageWithCategory(warningColor, category,
                string.Format(format, args)));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void LogError(object message)
        {
            UnityEngine.Debug.LogError(FormatMessage(errorColor, message));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void LogError(string category, object message)
        {
            UnityEngine.Debug.LogError(FormatMessageWithCategory(errorColor, category, message));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void LogErrorFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(FormatMessage(errorColor, string.Format(format, args)));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void LogErrorFormat(string category, string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(FormatMessageWithCategory(errorColor, category,
                string.Format(format, args)));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void LogException(Exception exception)
        {
            UnityEngine.Debug.LogError(FormatMessage(errorColor, exception.Message));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [HideInCallstack]
        public static void LogException(string category, Exception exception)
        {
            UnityEngine.Debug.LogError(FormatMessageWithCategory(errorColor, category, exception.Message));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [HideInCallstack]
        private static string FormatMessage(string color, object message)
        {
            return $"<color={color}>{message}</color>";
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [HideInCallstack]
        private static string FormatMessageWithCategory(string color, string category, object message)
        {
            return $"<color={color}><b>[{category}]</b> {message}</color>";
        }
    }
    // Code used from https://github.com/JoanStinson/UnityLoggerExtended
}