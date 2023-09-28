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
        [InspectorName("\u83EF\u8A9E")] Mandarin, // 華語
        [InspectorName("\u53F0\u8A9E")] Taigi, // 台語
        [InspectorName("\u65E5\u672C\u8A9E")] Japanese // 日本語
    }

    [Serializable]
    public enum ResultType
    {
        Normal,
        Standing,
        Concentric
    }

    [Serializable]
    public struct PueResult
    {
        public string Question;
        public ResultType ResultType;
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
            public bool ShowExplaination;
        }

        private const char m_hollowCircle = '\u25cb';
        private const char m_solidCircle = '\u25cf';
        private const float m_probabilityOfStanding = 1f / 500f;
        private const float m_probabilityOfConcentric = 1f / 700f;

        private static GUIStyle m_divider;

        [SerializeField]
        private EditorData m_editorData;

        private Vector2 m_resultScroll;

        private static string EditorDataKey => $"{Application.companyName}/{Application.productName}/{typeof(PuahPueWindow).Name}/{nameof(m_editorData)}";

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

        private static string GetExplainationText(bool pueA, bool pueB, Language language)
        {
            if (pueA && pueB)
            {
                return PuahPueLocalization.Localize("LaughingAnswer", language);
            }
            if ((pueA && !pueB) || (!pueA && pueB))
            {
                return PuahPueLocalization.Localize("DivineAnswer", language);
            }
            return PuahPueLocalization.Localize("NoAnswer", language);
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
            m_editorData.Language = (Language)EditorGUILayout.EnumPopup(m_editorData.Language, GUILayout.MaxWidth(120));
            GUILayout.EndHorizontal();

            m_editorData.SearchText = EditorGUILayout.TextArea(m_editorData.SearchText, GUILayout.Height(50));
            if (ColoredButton(new GUIContent(PuahPueLocalization.Localize("Throw", m_editorData.Language)), Color.yellow, GUILayout.Height(30)))
            {
                var resultType = ResultType.Normal;
                var probabilityTest = UnityEngine.Random.Range(0f, 1f);
                if (probabilityTest <= m_probabilityOfStanding)
                {
                    resultType = ResultType.Standing;
                }
                else if (m_probabilityOfStanding < probabilityTest && probabilityTest <= m_probabilityOfStanding + m_probabilityOfConcentric)
                {
                    resultType = ResultType.Concentric;
                }
                var pueA = UnityEngine.Random.Range(0f, 1f) > 0.5f;
                var pueB = UnityEngine.Random.Range(0f, 1f) > 0.5f;
                m_editorData.Results.Add(new PueResult()
                {
                    Question = m_editorData.SearchText,
                    ResultType = resultType,
                    PueA = pueA,
                    PueB = pueB
                }); ;
            }

            GUILayout.Label($"{m_hollowCircle}: {PuahPueLocalization.Localize("Up", m_editorData.Language)} {m_solidCircle}: {PuahPueLocalization.Localize("Down", m_editorData.Language)}");
            EditorGUILayout.Space();
            DrawHorizontalLine(3, Color.gray);
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
                EditorGUILayout.HelpBox(PuahPueLocalization.Localize("HoverForExplaination", m_editorData.Language), MessageType.None);

                for (int i = 0; i < m_editorData.Results.Count; i++)
                {
                    var result = m_editorData.NewFirst ? m_editorData.Results[m_editorData.Results.Count - 1 - i] : m_editorData.Results[i];
                    switch (result.ResultType)
                    {
                        case ResultType.Normal:
                            var resultA = result.PueA ? m_hollowCircle : m_solidCircle;
                            var resultB = result.PueB ? m_hollowCircle : m_solidCircle;
                            var tooltipText = GetExplainationText(result.PueA, result.PueB, m_editorData.Language);
                            GUILayout.Label(new GUIContent($"{resultA}, {resultB}: {result.Question}", tooltipText));
                            break;
                        case ResultType.Standing:
                            DrawColoredLabel(new GUIContent($"{PuahPueLocalization.Localize("StandingAnswer", m_editorData.Language)}: {result.Question}"), Color.yellow);
                            break;
                        case ResultType.Concentric:
                            DrawColoredLabel(new GUIContent($"{PuahPueLocalization.Localize("ConcentricAnswer", m_editorData.Language)}: {result.Question}"), Color.yellow);
                            break;
                        default:
                            break;
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
                    "\u9858\u3044\u4E8B" // 願い事
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

            { "Up", new string[]
                {
                    "Up",
                    "\u967D", // 陽
                    "\u967D", // 陽
                    "\u967D" // 陽
                }
            },

            { "Down", new string[]
                {
                    "Down",
                    "\u9670", // 陰
                    "\u9670", // 陰
                    "\u9670" // 陰
                }
            },
            
            { "HoverForExplaination", new string[]
                {
                    "Hover on an item to show the answer name.",
                    "\u5C07\u6E38\u6A19\u79FB\u81F3\u9805\u76EE\u4E0A\u4EE5\u986F\u793A\u7D50\u679C\u3002", // 解釋
                    "\u4F6E\u6ED1\u9F20\u5F99\u5230\u60F3\u6B32\u77AD\u89E3\u7684\u7D50\u679C\u4F86\u986F\u793A\u89E3\u91CB\u3002", // 佮滑鼠徙到想欲瞭解的結果來顯示解釋。
                    "\u30A2\u30A4\u30C6\u30E0\u306B\u30DE\u30A6\u30B9\u30AA\u30FC\u30D0\u30FC\u3059\u308B\u3068\u7D50\u679C\u3092\u8868\u793A\u3059\u308B\u3002", // アイテムにマウスオーバーすると結果を表示する。
                }
            },

            { "DivineAnswer", new string[]
                {
                    "Divine Answer",
                    "\u8056\u7B4A", // 聖筊
                    "\u8C61\u686E", // 象桮
                    "\u8056\u676F" // 聖杯
                }
            },

            { "LaughingAnswer", new string[]
                {
                    "Laughing Answer",
                    "\u7B11\u7B4A", // 笑筊
                    "\u7B11\u686E", // 笑桮
                    "\u7B11\u676F" // 笑杯
                }
            },

            { "NoAnswer", new string[]
                {
                    "No Answer",
                    "\u7121\u7B4A", // 無筊
                    "\u7121\u686E", // 無桮
                    "\u7121\u676F" // 無杯
                }
            },

            { "StandingAnswer", new string[]
                {
                    "Standing Answer",
                    "\u7ACB\u7B4A", // 立筊
                    "\u5F9B\u686E", // 徛桮
                    "\u7ACB\u676F" // 立杯
                }
            },

            { "ConcentricAnswer", new string[]
                {
                    "Concentric Answer",
                    "\u540C\u5FC3\u7B4A", // 同心筊
                    "\u540C\u5FC3\u686E", // 同心桮
                    "\u540C\u5FC3\u676F" // 同心杯
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
