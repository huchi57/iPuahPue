using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UrbanFox.iPuahPue.Editor
{
    public class PuahPueWindow : EditorWindow
    {
        [Serializable]
        public struct PueResult
        {
            public string Question;
            public bool IsStanding;
            public bool PueA;
            public bool PueB;
        }

        [Serializable]
        public struct EditorData
        {
            public string SearchText;
            public List<PueResult> Results;
            public bool NewFirst;
        }

        private const float m_probabilityOfStanding = 1f / 350f;

        [SerializeField]
        private EditorData m_editorData;

        private Vector2 m_resultScroll;

        private string EditorDataKey => $"{Application.companyName}/{Application.productName}/{nameof(m_editorData)}";

        [MenuItem("OwO/Window/iPuahPue")]
        private static void ShowWindow()
        {
            var window = GetWindow<PuahPueWindow>();
            window.titleContent = new GUIContent("iPuahPue");
            window.minSize = new Vector2(150, 250);
            window.Show();
        }

        [MenuItem("Window/OwO/iPuahPue")]
        private static void ShowWindowAlternate()
        {
            ShowWindow();
        }

        private void OnEnable()
        {
            m_editorData = EditorPrefs.HasKey(EditorDataKey) ? JsonUtility.FromJson<EditorData>(EditorPrefs.GetString(EditorDataKey)) : new EditorData()
            {
                SearchText = string.Empty,
                Results = new List<PueResult>()
            };
        }

        private void OnDisable()
        {
            EditorPrefs.SetString(EditorDataKey, JsonUtility.ToJson(m_editorData));
        }

        private void OnGUI()
        {
            GUILayout.Label("¤ß¸Û«hÆF...", EditorStyles.boldLabel);
            m_editorData.SearchText = EditorGUILayout.TextField(m_editorData.SearchText, GUILayout.Height(100));
            if (GUILayout.Button("Throw"))
            {
                var isStanding = UnityEngine.Random.Range(0f, 1f) < m_probabilityOfStanding;
                var pueA = UnityEngine.Random.Range(0f, 1f) > 0.5f;
                var pueB = UnityEngine.Random.Range(0f, 1f) > 0.5f;
                m_editorData.Results.Add(new PueResult()
                {
                    Question = m_editorData.SearchText,
                    IsStanding = isStanding,
                    PueA = pueA,
                    PueB = pueB
                });
            }

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.Label($"History ({(m_editorData.Results != null ? m_editorData.Results.Count : "0")})", EditorStyles.boldLabel);
            if (GUILayout.Button($"{(m_editorData.NewFirst ? "New to Old" : "Old to New")}", GUILayout.MaxWidth(120)))
            {
                m_editorData.NewFirst = !m_editorData.NewFirst;
            }
            GUILayout.EndHorizontal();
            m_resultScroll = EditorGUILayout.BeginScrollView(m_resultScroll);
            if (m_editorData.Results != null && m_editorData.Results.Count > 0)
            {
                for (int i = 0; i < m_editorData.Results.Count; i++)
                {
                    var result = m_editorData.NewFirst ? m_editorData.Results[m_editorData.Results.Count - 1 - i] : m_editorData.Results[i];
                    if (result.IsStanding)
                    {
                        var cachedContentColor = GUI.contentColor;
                        GUI.contentColor = Color.yellow;
                        GUILayout.Label($"*****: {result.Question}");
                        GUI.contentColor = cachedContentColor;
                    }
                    else
                    {
                        GUILayout.Label($"{(result.PueA ? "+" : "-")}, {(result.PueB ? "+" : "-")}: {result.Question}");
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No history.", MessageType.Info);
            }
            EditorGUILayout.EndScrollView();

            var cachedBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Clear History"))
            {
                m_editorData.Results = new List<PueResult>();
            }
            GUI.backgroundColor = cachedBackgroundColor;
        }
    }
}
