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

        private static GUIStyle m_divider;

        [SerializeField]
        private EditorData m_editorData;

        private Vector2 m_resultScroll;

        private static string EditorDataKey => $"{Application.companyName}/{Application.productName}/{nameof(m_editorData)}";

        private static GUIStyle Divider
        {
            get
            {
                if (m_divider == null)
                {
                    var whiteTexture = new Texture2D(1, 1);
                    whiteTexture.SetPixel(0, 0, Color.white);
                    whiteTexture.Apply();
                    m_divider = new GUIStyle();
                    m_divider.normal.background = whiteTexture;
                    m_divider.margin = new RectOffset(2, 2, 2, 2);
                }
                return m_divider;
            }
        }

        private static void DrawHorizontalLine(float height, Color color)
        {
            Divider.fixedHeight = height;
            var cachedGUIColor = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, Divider);
            GUI.color = cachedGUIColor;
        }

        private static void DrawColoredLabel(GUIContent content, Color color, params GUILayoutOption[] options)
        {
            var cachedGUIColor = GUI.color;
            GUI.color = color;
            GUILayout.Label(content, options);
            GUI.color = cachedGUIColor;
        }

        private static bool ColoredButton(GUIContent content, Color color, params GUILayoutOption[] options)
        {
            var cachedBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            if (GUILayout.Button(content, options))
            {
                GUI.backgroundColor = cachedBackgroundColor;
                return true;
            }
            GUI.backgroundColor = cachedBackgroundColor;
            return false;
        }

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
            m_editorData.SearchText = EditorGUILayout.TextArea(m_editorData.SearchText, GUILayout.Height(50));
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
                        DrawColoredLabel(new GUIContent($"*****: {result.Question}"), Color.yellow);
                    }
                    else
                    {
                        GUILayout.Label($"{(result.PueA ? "+" : "-")}, {(result.PueB ? "+" : "-")}: {result.Question}");
                    }
                    if (i < m_editorData.Results.Count - 1)
                    {
                        DrawHorizontalLine(1, Color.gray);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No history.", MessageType.Info);
            }
            EditorGUILayout.EndScrollView();

            GUI.enabled = m_editorData.Results != null && m_editorData.Results.Count > 0;
            if (ColoredButton(new GUIContent("Clear History"), Color.red))
            {
                m_editorData.Results = new List<PueResult>();
            }
            GUI.enabled = true;
        }
    }
}
