using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UrbanFox.iPuahPue.Editor
{
    [Serializable]
    public enum Language
    {
        English,
        [InspectorName("\u83EF\u8A9E")] Mandarin,
        [InspectorName("\u53F0\u8A9E")] Taigi,
        [InspectorName("\u65E5\u672C\u8A9E")] Japanese
    }

    [Serializable]
    public struct PueResult
    {
        public string Question;
        public bool IsStanding;
        public bool PueA;
        public bool PueB;
    }

    public class PuahPueWindow : EditorWindow
    {
        [Serializable]
        public struct EditorData
        {
            public string SearchText;
            public List<PueResult> Results;
            public bool NewFirst;
            public Language Language;
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
            GUILayout.BeginHorizontal();
            GUILayout.Label(PuahPueLocalization.Localize("AskAQuestion", m_editorData.Language), EditorStyles.boldLabel);
            m_editorData.Language = (Language)EditorGUILayout.EnumPopup(m_editorData.Language, GUILayout.Width(120));
            GUILayout.EndHorizontal();

            m_editorData.SearchText = EditorGUILayout.TextArea(m_editorData.SearchText, GUILayout.Height(50));
            if (GUILayout.Button(PuahPueLocalization.Localize("Throw", m_editorData.Language)))
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
            GUILayout.Label($"{PuahPueLocalization.Localize("History", m_editorData.Language)} ({(m_editorData.Results != null ? m_editorData.Results.Count : "0")})", EditorStyles.boldLabel);
            if (GUILayout.Button(PuahPueLocalization.Localize(m_editorData.NewFirst ? "NewToOld" : "OldToNew", m_editorData.Language), GUILayout.MaxWidth(120)))
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
                EditorGUILayout.HelpBox(PuahPueLocalization.Localize("NoHistory", m_editorData.Language), MessageType.Info);
            }
            EditorGUILayout.EndScrollView();

            GUI.enabled = m_editorData.Results != null && m_editorData.Results.Count > 0;
            if (ColoredButton(new GUIContent(PuahPueLocalization.Localize("ClearHistory", m_editorData.Language)), Color.red))
            {
                m_editorData.Results = new List<PueResult>();
            }
            GUI.enabled = true;
        }
    }

    public class PuahPueLocalization
    {
        private static readonly Dictionary<string, string[]> m_dictionary = new Dictionary<string, string[]>()
        {
            { "AskAQuestion", new string[]
            {
                "Ask A Question",
                "\u5FC3\u8AA0\u5247\u9748", // 心誠則靈
                "\u6B32\u554F\u7684\u554F\u984C", // 欲問的問題
                "\u983C\u307F\u4E8B" // 頼み事
            }
            },

            { "Throw", new string[]
            {
                "Throw",
                "\u64F2\u7B4A", // 擲筊
                "\u8DCB\u686E", // 跋桮
                "\u5360\u3044" // 占い
            }
            },

            { "History", new string[]
            {
                "History",
                "\u6B77\u53F2\u7D00\u9304", // 歷史紀錄
                "\u7D00\u9304", // 紀錄
                "\u5C65\u6B74" // 履歴
            }
            },

            { "NewToOld", new string[]
            {
                "New to Old",
                "\u7531\u65B0\u81F3\u820A", // 由新至舊
                "\u8F03\u65B0\u7684\u5148", // 較新的先
                "\u65B0\u3057\u3044\u9806" // 新しい順
            }
            },

            { "OldToNew", new string[]
            {
                "Old to New",
                "\u7531\u820A\u81F3\u65B0", // 由舊至新
                "\u8F03\u820A\u7684\u5148", // 較舊的先
                "\u53E4\u3044\u9806" // 古い順
            }
            },

            { "NoHistory", new string[]
            {
                "No history.",
                "\u7121\u6B77\u53F2\u7D00\u9304\u3002", // 無歷史紀錄。
                "\u7121\u7D00\u9304\u3002", // 無紀錄。
                "\u5C65\u6B74\u306A\u3057\u3002" // 履歴なし。
            }
            },

            { "ClearHistory", new string[]
            {
                "Clear History",
                "\u6E05\u9664\u6B77\u53F2\u7D00\u9304", // 清除歷史紀錄
                "\u522A\u6389\u7D00\u9304", // 刪掉紀錄
                "\u5C65\u6B74\u3092\u6D88\u53BB" // 履歴を消去
            }
            },
        };

        public static string Localize(string key, Language language)
        {
            if (!m_dictionary.ContainsKey(key) || m_dictionary[key].Length < (int)language || string.IsNullOrEmpty(m_dictionary[key][(int)language]))
            {
                return key;
            }
            return m_dictionary[key][(int)language];
        }
    }
}
