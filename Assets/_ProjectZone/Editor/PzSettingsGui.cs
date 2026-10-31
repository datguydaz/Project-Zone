#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectZone.Editor
{
    /// <summary>
    /// Lightweight Hierarchy-Designer-inspired styles for the PROJECT ZONE settings window.
    /// </summary>
    internal static class PzSettingsGui
    {
        private static bool _ready;
        private static GUIStyle _primaryPanel;
        private static GUIStyle _headerPanel;
        private static GUIStyle _headerLabel;
        private static GUIStyle _tabLabel;
        private static GUIStyle _headerButton;
        private static GUIStyle _sectionHeader;
        private static GUIStyle _helpBox;
        private static GUIStyle _versionLabel;

        public static Color Accent => EditorGUIUtility.isProSkin
            ? new Color(0.42f, 0.95f, 0.38f)
            : new Color(0.28f, 0.72f, 0.28f);

        public static GUIStyle PrimaryPanelStyle
        {
            get
            {
                Ensure();
                return _primaryPanel;
            }
        }

        public static GUIStyle HeaderPanelStyle
        {
            get
            {
                Ensure();
                return _headerPanel;
            }
        }

        public static GUIStyle HeaderLabelStyle
        {
            get
            {
                Ensure();
                return _headerLabel;
            }
        }

        public static GUIStyle TabLabelStyle
        {
            get
            {
                Ensure();
                return _tabLabel;
            }
        }

        public static GUIStyle HeaderButtonStyle
        {
            get
            {
                Ensure();
                return _headerButton;
            }
        }

        public static GUIStyle SectionHeaderStyle
        {
            get
            {
                Ensure();
                return _sectionHeader;
            }
        }

        public static GUIStyle HelpBoxStyle
        {
            get
            {
                Ensure();
                return _helpBox;
            }
        }

        public static GUIStyle VersionLabelStyle
        {
            get
            {
                Ensure();
                return _versionLabel;
            }
        }

        public static void Ensure()
        {
            if (_ready && _primaryPanel != null)
                return;

            bool pro = EditorGUIUtility.isProSkin;

            _primaryPanel = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(12, 12, 10, 12),
                margin = new RectOffset(4, 4, 4, 4)
            };

            _headerPanel = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 8, 8),
                margin = new RectOffset(0, 0, 0, 8)
            };

            _headerLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                richText = true,
                alignment = TextAnchor.MiddleLeft
            };

            _tabLabel = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                richText = true
            };

            _headerButton = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                fixedHeight = 28,
                padding = new RectOffset(12, 12, 4, 4)
            };

            _sectionHeader = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                margin = new RectOffset(0, 0, 10, 4)
            };

            _helpBox = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                fontSize = 11,
                richText = true,
                padding = new RectOffset(4, 4, 4, 4)
            };

            _versionLabel = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = pro ? new Color(0.65f, 0.65f, 0.65f) : new Color(0.35f, 0.35f, 0.35f) }
            };

            _ready = true;
        }

        public static void DrawSection(string title)
        {
            GUILayout.Space(4);
            EditorGUILayout.LabelField(title, SectionHeaderStyle);
            Rect r = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(r, Accent * new Color(1f, 1f, 1f, 0.55f));
            GUILayout.Space(6);
        }
    }
}
#endif
