#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectZone;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectZone.Editor
{
    /// <summary>
    /// Mesh/collider prefab painter — Tools → PROJECT ZONE → Prefab Paint Tool
    /// LMB paint · Shift+LMB erase. Palette persists with thumbnails + categories.
    /// </summary>
    public sealed class PzPrefabPaintTool : EditorWindow
    {
        private const string PrefKey = "PzPrefabPaint.";
        private const string UssPath = "Assets/_ProjectZone/Editor/PzPrefabPaintTool.uss";

        private PzPrefabPaintPalette _palette;
        private bool _paintingEnabled = true;
        private bool _alignToNormal = true;
        private bool _randomYaw = true;
        private bool _parentToHit;
        private Transform _parent;
        private float _radius = 2f;
        private float _spacing = 0.75f;
        private float _strength = 0.65f;
        private float _scaleMin = 0.85f;
        private float _scaleMax = 1.15f;
        private LayerMask _rayMask = ~0;
        private Vector3 _lastPaintPos = new(float.MaxValue, float.MaxValue, float.MaxValue);
        private bool _subscribed;
        private readonly HashSet<PzPaintCategory> _collapsed = new();

        private VisualElement _root;
        private VisualElement _dropZone;
        private VisualElement _paletteHost;
        private Label _statusLabel;
        private Label _badgeLabel;
        private double _nextPreviewRefresh;
        private string _statusText = string.Empty;

        [MenuItem("Tools/PROJECT ZONE/Prefab Paint Tool", priority = 60)]
        public static void Open()
        {
            var wnd = GetWindow<PzPrefabPaintTool>();
            wnd.titleContent = new GUIContent("PZ Prefab Paint");
            wnd.minSize = new Vector2(380, 560);
            wnd.Show();
        }

        private void OnEnable()
        {
            _palette = PzPrefabPaintPalette.GetOrCreate();
            LoadPrefs();
            Subscribe(true);
            AssetPreview.SetPreviewTextureCacheSize(512);
            EditorApplication.update += TickPreviewRefresh;
        }

        private void OnDisable()
        {
            EditorApplication.update -= TickPreviewRefresh;
            Subscribe(false);
            SavePrefs();
            SavePalette();
        }

        private void TickPreviewRefresh()
        {
            if (EditorApplication.timeSinceStartup < _nextPreviewRefresh)
                return;
            _nextPreviewRefresh = EditorApplication.timeSinceStartup + 0.5;
            if (_paletteHost != null && _palette != null && _palette.Entries.Count > 0)
                RebuildPaletteUi();
        }

        private void Subscribe(bool on)
        {
            if (on && !_subscribed)
            {
                SceneView.duringSceneGui += OnSceneGui;
                _subscribed = true;
            }
            else if (!on && _subscribed)
            {
                SceneView.duringSceneGui -= OnSceneGui;
                _subscribed = false;
            }
        }

        public void CreateGUI()
        {
            _root = rootVisualElement;
            _root.Clear();

            StyleSheet uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (uss != null)
                _root.styleSheets.Add(uss);

            var frame = new VisualElement();
            frame.AddToClassList("pz-root");
            _root.Add(frame);

            var header = new VisualElement();
            header.AddToClassList("pz-header");
            header.Add(MakeLabel("PREFAB PAINT", "pz-title"));
            _badgeLabel = MakeLabel("0", "pz-badge");
            header.Add(_badgeLabel);
            frame.Add(header);

            frame.Add(MakeLabel(
                "LMB paint · Shift+LMB erase · click thumb = toggle · drop folders OK",
                "pz-hint"));

            // —— Settings card ——
            var settings = new VisualElement();
            settings.AddToClassList("pz-card");
            settings.Add(MakeLabel("BRUSH", "pz-card-title"));

            settings.Add(MakeToggle("Painting enabled", _paintingEnabled, v => _paintingEnabled = v));

            var parentField = new ObjectField("Parent (Art root)")
            {
                objectType = typeof(Transform),
                allowSceneObjects = true,
                value = _parent
            };
            parentField.RegisterValueChangedCallback(evt => _parent = evt.newValue as Transform);
            settings.Add(parentField);

            settings.Add(MakeToggle("Parent to hit object", _parentToHit, v => _parentToHit = v));
            settings.Add(MakeSlider("Radius", _radius, 0.25f, 20f, v => _radius = v));
            settings.Add(MakeSlider("Spacing", _spacing, 0.05f, 10f, v => _spacing = v));
            settings.Add(MakeSlider("Density", _strength, 0.05f, 1f, v => _strength = v));
            settings.Add(MakeToggle("Align to normal", _alignToNormal, v => _alignToNormal = v));
            settings.Add(MakeToggle("Random yaw", _randomYaw, v => _randomYaw = v));

            var scaleRow = new VisualElement();
            scaleRow.AddToClassList("pz-row");
            scaleRow.Add(new Label("Scale min/max"));
            var scaleMin = new FloatField { value = _scaleMin, style = { width = 56 } };
            var scaleMax = new FloatField { value = _scaleMax, style = { width = 56, marginLeft = 4 } };
            scaleMin.RegisterValueChangedCallback(e => _scaleMin = Mathf.Clamp(e.newValue, 0.1f, _scaleMax));
            scaleMax.RegisterValueChangedCallback(e => _scaleMax = Mathf.Max(e.newValue, _scaleMin));
            scaleRow.Add(scaleMin);
            scaleRow.Add(scaleMax);
            settings.Add(scaleRow);
            frame.Add(settings);

            // —— Palette card ——
            var paletteCard = new VisualElement();
            paletteCard.AddToClassList("pz-card");
            paletteCard.style.flexGrow = 1;
            paletteCard.Add(MakeLabel("PALETTE", "pz-card-title"));

            _dropZone = new VisualElement();
            _dropZone.AddToClassList("pz-drop");
            _dropZone.pickingMode = PickingMode.Position;
            // IMGUI drop bridge — most reliable for Project-window prefab drags in EditorWindows
            var dropImgui = new IMGUIContainer(DrawDropZoneImgui);
            dropImgui.style.position = Position.Absolute;
            dropImgui.style.left = 0;
            dropImgui.style.right = 0;
            dropImgui.style.top = 0;
            dropImgui.style.bottom = 0;
            _dropZone.Add(dropImgui);
            var dropTitle = MakeLabel("Drop prefabs or folders here", "pz-drop-title");
            dropTitle.pickingMode = PickingMode.Ignore;
            var dropSub = MakeLabel("Project assets · .prefab / .fbx · or use Add selection", "pz-drop-sub");
            dropSub.pickingMode = PickingMode.Ignore;
            _dropZone.Add(dropTitle);
            _dropZone.Add(dropSub);
            RegisterDropZone(_dropZone);
            paletteCard.Add(_dropZone);

            _statusLabel = MakeLabel(string.Empty, "pz-status");
            paletteCard.Add(_statusLabel);

            var toolbar = new VisualElement();
            toolbar.AddToClassList("pz-toolbar");
            toolbar.Add(MakeButton("Add selection", AddFromProjectSelection));
            toolbar.Add(MakeButton("Enable all", () => { SetAllEnabled(true); RebuildPaletteUi(); }));
            toolbar.Add(MakeButton("Disable all", () => { SetAllEnabled(false); RebuildPaletteUi(); }));
            toolbar.Add(MakeButton("Re-sort", () => { ReGuessCategories(); RebuildPaletteUi(); }));
            toolbar.Add(MakeButton("Clear", () =>
            {
                if (EditorUtility.DisplayDialog("Clear palette?", "Remove all saved prefabs?", "Clear", "Cancel"))
                {
                    _palette.Entries.Clear();
                    SavePalette();
                    RebuildPaletteUi();
                    SetStatus("Palette cleared.");
                }
            }));
            paletteCard.Add(toolbar);

            var scroll = new ScrollView();
            scroll.AddToClassList("pz-scroll");
            _paletteHost = new VisualElement();
            scroll.Add(_paletteHost);
            paletteCard.Add(scroll);
            frame.Add(paletteCard);

            var footer = new VisualElement();
            footer.AddToClassList("pz-footer");
            footer.Add(new Label("Saved → Assets/_ProjectZone/Editor/PzPrefabPaintPalette.asset"));
            var footerRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 4 } };
            footerRow.Add(MakeButton("Use selection as parent", () =>
            {
                if (Selection.activeTransform != null)
                {
                    _parent = Selection.activeTransform;
                    parentField.value = _parent;
                    SetStatus($"Parent → {_parent.name}");
                }
            }));
            footerRow.Add(MakeButton("Mark children painted", MarkParentChildrenPainted));
            footerRow.Add(MakeButton("Organize hierarchy", () =>
            {
                if (_parent != null)
                    OrganizePaintedHierarchy();
                else
                    PzPrefabPaintOrganizer.OrganizeOpenScenes(showDialog: true);
            }));
            footer.Add(footerRow);
            frame.Add(footer);

            RebuildPaletteUi();
            SetStatus(_statusText);
        }

        private void RegisterDropZone(VisualElement zone)
        {
            zone.RegisterCallback<DragEnterEvent>(_ => zone.AddToClassList("pz-drop--hot"));
            zone.RegisterCallback<DragLeaveEvent>(_ => zone.RemoveFromClassList("pz-drop--hot"));
            zone.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                bool ok = DragHasPrefabs();
                DragAndDrop.visualMode = ok ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                if (ok)
                    evt.StopPropagation();
            });
            zone.RegisterCallback<DragPerformEvent>(evt =>
            {
                zone.RemoveFromClassList("pz-drop--hot");
                CommitDrag();
                evt.StopPropagation();
            });
        }

        private void DrawDropZoneImgui()
        {
            Event e = Event.current;
            Rect rect = new Rect(0, 0, _dropZone.layout.width, _dropZone.layout.height);
            if (rect.width < 1f || rect.height < 1f)
                return;

            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!rect.Contains(e.mousePosition))
                        break;
                    bool ok = DragHasPrefabs();
                    DragAndDrop.visualMode = ok ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                    if (e.type == EventType.DragPerform && ok)
                    {
                        DragAndDrop.AcceptDrag();
                        CommitDrag();
                    }
                    e.Use();
                    break;
            }
        }

        private void CommitDrag()
        {
            if (!DragHasPrefabs())
            {
                SetStatus("Drop rejected — use prefabs, models, or folders.");
                return;
            }

            int added = AcceptDragIntoPalette();
            RebuildPaletteUi();
            SetStatus(added > 0
                ? $"Added {added} prefab(s) to palette."
                : "Nothing new (duplicates or unsupported).");
            Repaint();
        }

        private void AddFromProjectSelection()
        {
            var seen = new HashSet<string>();
            int before = _palette.Entries.Count;
            foreach (UnityEngine.Object obj in Selection.objects)
            {
                if (obj is GameObject go)
                {
                    TryAddFromObject(go, seen);
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path))
                    continue;
                if (AssetDatabase.IsValidFolder(path))
                    AddFromFolder(path, seen);
                else
                {
                    GameObject go2 = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    TryAddFromObject(go2, seen);
                }
            }

            if (_palette.Entries.Count != before)
            {
                SortPalette();
                SavePalette();
            }

            int added = _palette.Entries.Count - before;
            RebuildPaletteUi();
            SetStatus(added > 0
                ? $"Added {added} from selection."
                : "Select prefabs/folders in the Project window first.");
        }

        private static bool DragHasPrefabs()
        {
            if (DragAndDrop.objectReferences != null)
            {
                foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
                {
                    if (obj is GameObject)
                        return true;
                    if (obj is DefaultAsset)
                    {
                        string p = AssetDatabase.GetAssetPath(obj);
                        if (AssetDatabase.IsValidFolder(p))
                            return true;
                    }
                }
            }

            if (DragAndDrop.paths != null)
            {
                foreach (string path in DragAndDrop.paths)
                {
                    if (string.IsNullOrEmpty(path))
                        continue;
                    if (AssetDatabase.IsValidFolder(path))
                        return true;
                    string ext = Path.GetExtension(path).ToLowerInvariant();
                    if (ext is ".prefab" or ".fbx" or ".obj" or ".blend")
                        return true;
                }
            }

            return false;
        }

        private int AcceptDragIntoPalette()
        {
            int before = _palette.Entries.Count;
            var seen = new HashSet<string>();

            if (DragAndDrop.objectReferences != null)
            {
                foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
                {
                    if (obj is GameObject go)
                    {
                        TryAddFromObject(go, seen);
                    }
                    else if (obj is DefaultAsset)
                    {
                        string path = AssetDatabase.GetAssetPath(obj);
                        if (AssetDatabase.IsValidFolder(path))
                            AddFromFolder(path, seen);
                    }
                }
            }

            if (DragAndDrop.paths != null)
            {
                foreach (string path in DragAndDrop.paths)
                {
                    if (string.IsNullOrEmpty(path))
                        continue;
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        AddFromFolder(path, seen);
                        continue;
                    }

                    string ext = Path.GetExtension(path).ToLowerInvariant();
                    if (ext is ".prefab" or ".fbx" or ".obj" or ".blend")
                    {
                        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        TryAddFromObject(go, seen);
                    }
                }
            }

            if (_palette.Entries.Count != before)
            {
                SortPalette();
                SavePalette();
            }

            return _palette.Entries.Count - before;
        }

        private void AddFromFolder(string folder, HashSet<string> seen)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab t:Model", new[] { folder });
            // FindAssets with two types can be flaky — also search prefabs alone
            var all = new HashSet<string>(guids);
            foreach (string g in AssetDatabase.FindAssets("t:Prefab", new[] { folder }))
                all.Add(g);
            foreach (string g in AssetDatabase.FindAssets("t:Model", new[] { folder }))
                all.Add(g);

            foreach (string guid in all)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                TryAddFromObject(go, seen);
            }
        }

        private void TryAddFromObject(GameObject go, HashSet<string> seen)
        {
            if (go == null)
                return;

            string path = AssetDatabase.GetAssetPath(go);
            if (string.IsNullOrEmpty(path))
            {
                GameObject src = PrefabUtility.GetCorrespondingObjectFromSource(go);
                if (src != null)
                {
                    go = src;
                    path = AssetDatabase.GetAssetPath(go);
                }
            }

            if (string.IsNullOrEmpty(path))
                return;

            // Prefer the main asset for FBX (root)
            UnityEngine.Object main = AssetDatabase.LoadMainAssetAtPath(path);
            if (main is GameObject mainGo)
                go = mainGo;

            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid) || !seen.Add(guid))
                return;
            if (_palette.Entries.Any(e => e.PrefabGuid == guid))
                return;

            _palette.Entries.Add(new PzPaintPrefabEntry
            {
                PrefabGuid = guid,
                Category = PzPrefabPaintPalette.GuessCategory(go),
                Enabled = true,
                CategoryLocked = false
            });
        }

        private void RebuildPaletteUi()
        {
            if (_paletteHost == null)
                return;
            if (_palette == null)
                _palette = PzPrefabPaintPalette.GetOrCreate();

            _paletteHost.Clear();
            int total = _palette.Entries.Count;
            int active = GetActivePrefabs().Count;
            if (_badgeLabel != null)
                _badgeLabel.text = $"{active}/{total}";

            foreach (PzPaintCategory cat in Enum.GetValues(typeof(PzPaintCategory)))
            {
                List<PzPaintPrefabEntry> group = _palette.Entries.Where(e => e != null && e.Category == cat).ToList();
                if (group.Count == 0)
                    continue;

                var block = new VisualElement();
                block.AddToClassList("pz-cat");

                var head = new VisualElement();
                head.AddToClassList("pz-cat-header");
                bool collapsed = _collapsed.Contains(cat);
                head.Add(new Label(collapsed ? "▸" : "▾") { style = { width = 14, color = new StyleColor(new Color(0.77f, 0.64f, 0.35f)) } });
                head.Add(MakeLabel(cat.ToString().ToUpperInvariant(), "pz-cat-name"));
                int on = group.Count(e => e.Enabled && e.LoadPrefab() != null);
                head.Add(MakeLabel($"{on}/{group.Count}", "pz-cat-count"));
                head.RegisterCallback<ClickEvent>(_ =>
                {
                    if (!_collapsed.Add(cat))
                        _collapsed.Remove(cat);
                    RebuildPaletteUi();
                });
                block.Add(head);

                if (!collapsed)
                {
                    var body = new VisualElement();
                    body.AddToClassList("pz-cat-body");
                    foreach (PzPaintPrefabEntry entry in group)
                        body.Add(BuildThumb(entry));
                    block.Add(body);
                }

                _paletteHost.Add(block);
            }

            if (total == 0)
            {
                _paletteHost.Add(new Label("Palette empty — drop some foliage prefabs above.")
                {
                    style = { color = new StyleColor(new Color(0.55f, 0.6f, 0.67f)), marginTop = 12, unityTextAlign = TextAnchor.MiddleCenter }
                });
            }
        }

        private VisualElement BuildThumb(PzPaintPrefabEntry entry)
        {
            GameObject prefab = entry.LoadPrefab();
            var card = new VisualElement();
            card.AddToClassList("pz-thumb");
            if (entry.Enabled)
                card.AddToClassList("pz-thumb--on");

            var img = new Image();
            img.AddToClassList("pz-thumb-img");
            img.scaleMode = ScaleMode.ScaleToFit;
            Texture2D preview = prefab != null ? AssetPreview.GetAssetPreview(prefab) : null;
            if (preview == null && prefab != null)
                preview = AssetPreview.GetMiniThumbnail(prefab);
            img.image = preview;
            card.Add(img);

            card.Add(MakeLabel(prefab != null ? prefab.name : "(missing)", "pz-thumb-name"));

            var footer = new VisualElement();
            footer.AddToClassList("pz-thumb-footer");

            var cat = new EnumField(entry.Category);
            cat.style.flexGrow = 1;
            cat.style.height = 18;
            cat.style.fontSize = 9;
            cat.RegisterValueChangedCallback(evt =>
            {
                entry.Category = (PzPaintCategory)evt.newValue;
                entry.CategoryLocked = true;
                SortPalette();
                SavePalette();
                RebuildPaletteUi();
            });
            footer.Add(cat);

            var remove = new Button(() =>
            {
                _palette.Entries.Remove(entry);
                SavePalette();
                RebuildPaletteUi();
                SetStatus("Removed from palette.");
            })
            { text = "✕" };
            footer.Add(remove);
            card.Add(footer);

            card.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target is EnumField || evt.target is Button || ((VisualElement)evt.target).FindAncestorOfType<EnumField>() != null)
                    return;

                if (evt.clickCount >= 2 && prefab != null)
                {
                    Selection.activeObject = prefab;
                    EditorGUIUtility.PingObject(prefab);
                    return;
                }

                entry.Enabled = !entry.Enabled;
                SavePalette();
                RebuildPaletteUi();
            });

            return card;
        }

        private void SetStatus(string msg)
        {
            _statusText = msg ?? string.Empty;
            if (_statusLabel != null)
                _statusLabel.text = _statusText;
        }

        private static Toggle MakeToggle(string label, bool value, Action<bool> onChange)
        {
            var t = new Toggle(label) { value = value };
            t.AddToClassList("pz-toggle");
            t.RegisterValueChangedCallback(e => onChange(e.newValue));
            return t;
        }

        private static VisualElement MakeSlider(string label, float value, float min, float max, Action<float> onChange)
        {
            var row = new VisualElement();
            row.AddToClassList("pz-row");
            row.Add(new Label(label));
            var slider = new Slider(min, max) { value = value, showInputField = true, style = { flexGrow = 1 } };
            slider.RegisterValueChangedCallback(e => onChange(e.newValue));
            row.Add(slider);
            return row;
        }

        private static Button MakeButton(string text, Action onClick)
        {
            var b = new Button(onClick) { text = text };
            return b;
        }

        private static Label MakeLabel(string text, string className)
        {
            var label = new Label(text);
            label.AddToClassList(className);
            return label;
        }

        private List<GameObject> GetActivePrefabs()
        {
            var list = new List<GameObject>();
            if (_palette == null)
                return list;
            foreach (PzPaintPrefabEntry entry in _palette.Entries)
            {
                if (entry == null || !entry.Enabled)
                    continue;
                GameObject go = entry.LoadPrefab();
                if (go != null)
                    list.Add(go);
            }
            return list;
        }

        private void SetAllEnabled(bool enabled)
        {
            foreach (PzPaintPrefabEntry entry in _palette.Entries)
                entry.Enabled = enabled;
            SavePalette();
            SetStatus(enabled ? "All enabled." : "All disabled.");
        }

        private void ReGuessCategories()
        {
            foreach (PzPaintPrefabEntry entry in _palette.Entries)
            {
                GameObject go = entry.LoadPrefab();
                entry.Category = PzPrefabPaintPalette.GuessCategory(go);
                entry.CategoryLocked = false;
            }
            SortPalette();
            SavePalette();
            SetStatus("Categories re-guessed.");
        }

        private void SortPalette()
        {
            _palette.Entries = _palette.Entries
                .OrderBy(e => e.Category)
                .ThenBy(e =>
                {
                    GameObject go = e.LoadPrefab();
                    return go != null ? go.name : e.PrefabGuid;
                })
                .ToList();
        }

        private void SavePalette()
        {
            if (_palette == null)
                return;
            EditorUtility.SetDirty(_palette);
            AssetDatabase.SaveAssets();
        }

        private void OnSceneGui(SceneView view)
        {
            List<GameObject> active = GetActivePrefabs();
            if (!_paintingEnabled || active.Count == 0)
                return;

            Event e = Event.current;
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlId);

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 5000f, _rayMask, QueryTriggerInteraction.Ignore))
            {
                if (e.type == EventType.Repaint)
                    SceneView.RepaintAll();
                return;
            }

            // Soft glow disc
            Handles.color = e.shift ? new Color(0.9f, 0.35f, 0.25f, 0.9f) : new Color(0.77f, 0.64f, 0.35f, 0.95f);
            Handles.DrawWireDisc(hit.point, hit.normal, _radius);
            Handles.color = e.shift ? new Color(0.9f, 0.25f, 0.2f, 0.12f) : new Color(0.77f, 0.64f, 0.35f, 0.14f);
            Handles.DrawSolidDisc(hit.point, hit.normal, _radius);
            Handles.color = e.shift ? new Color(0.9f, 0.35f, 0.25f, 0.35f) : new Color(0.95f, 0.85f, 0.55f, 0.35f);
            Handles.DrawWireDisc(hit.point, hit.normal, _radius * 0.15f);

            bool stroke = (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
                          && e.button == 0
                          && !e.alt;

            if (stroke)
            {
                if (e.shift)
                    EraseAt(hit.point, active);
                else
                    PaintAt(hit, active);

                e.Use();
                GUIUtility.hotControl = controlId;
            }

            if (e.type == EventType.MouseUp && e.button == 0)
                GUIUtility.hotControl = 0;

            if (e.type == EventType.Layout || e.type == EventType.Repaint)
                HandleUtility.Repaint();
        }

        private void PaintAt(RaycastHit centerHit, List<GameObject> active)
        {
            int attempts = Mathf.Max(1, Mathf.RoundToInt(8f * _strength));
            for (int i = 0; i < attempts; i++)
            {
                Vector2 disk = UnityEngine.Random.insideUnitCircle * _radius;
                Vector3 tangent = Vector3.Cross(centerHit.normal, Vector3.up);
                if (tangent.sqrMagnitude < 0.001f)
                    tangent = Vector3.Cross(centerHit.normal, Vector3.right);
                tangent.Normalize();
                Vector3 bitangent = Vector3.Cross(centerHit.normal, tangent);
                Vector3 candidate = centerHit.point + tangent * disk.x + bitangent * disk.y;

                Vector3 rayOrigin = candidate + centerHit.normal * 2f;
                if (!Physics.Raycast(rayOrigin, -centerHit.normal, out RaycastHit hit, 6f, _rayMask, QueryTriggerInteraction.Ignore))
                    continue;

                GameObject prefab = active[UnityEngine.Random.Range(0, active.Count)];
                if (prefab == null)
                    continue;

                PzPaintCategory category = GetCategoryForPrefab(prefab);
                float spacing = EffectiveSpacing(category);
                if (Vector3.Distance(hit.point, _lastPaintPos) < spacing * 0.35f && i == 0)
                    continue;
                if (TooClose(hit.point, spacing))
                    continue;

                if (!CheckPaintedPropBudget())
                    return;

                Transform rootParent = ResolveParent(hit.transform);
                if (rootParent == null)
                    rootParent = EnsureFallbackPaintRoot();
                Transform parent = EnsureCategoryFolder(rootParent, category);

                GameObject instance = null;

                // Prefer real prefab instances (no "(Clone)" name, keeps prefab link).
                if (PrefabUtility.IsPartOfPrefabAsset(prefab)
                    || !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(prefab)))
                {
                    instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
                }

                if (instance == null)
                {
                    instance = Instantiate(prefab, parent);
                    instance.name = prefab.name; // drop the "(Clone)" suffix
                }

                Undo.RegisterCreatedObjectUndo(instance, "PZ Prefab Paint");
                if (instance.GetComponent<PzPaintedProp>() == null)
                    Undo.AddComponent<PzPaintedProp>(instance);

                float scale = UnityEngine.Random.Range(_scaleMin, _scaleMax);
                instance.transform.position = hit.point;
                instance.transform.localScale = prefab.transform.localScale * scale;

                Quaternion rot = Quaternion.identity;
                if (_alignToNormal)
                    rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
                if (_randomYaw)
                    rot *= Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
                instance.transform.rotation = rot;

                _lastPaintPos = hit.point;
            }
        }

        private void EraseAt(Vector3 point, List<GameObject> active)
        {
            float r2 = _radius * _radius;
            var toDestroy = new HashSet<GameObject>();

            PzPaintedProp[] marked = UnityEngine.Object.FindObjectsByType<PzPaintedProp>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (PzPaintedProp marker in marked)
            {
                if (marker != null && (marker.transform.position - point).sqrMagnitude <= r2)
                    toDestroy.Add(marker.gameObject);
            }

            if (_parent != null)
                CollectEraseRecursive(_parent, point, r2, active, toDestroy);

            foreach (Collider col in Physics.OverlapSphere(point, _radius, ~0, QueryTriggerInteraction.Collide))
            {
                if (col == null)
                    continue;
                Transform t = col.transform;
                while (t != null)
                {
                    if (t.GetComponent<PzPaintedProp>() != null || MatchesBrushPrefab(t.gameObject, active))
                    {
                        GameObject victim = t.GetComponent<PzPaintedProp>() != null
                            ? t.gameObject
                            : PrefabUtility.GetOutermostPrefabInstanceRoot(t.gameObject);
                        if (victim == null)
                            victim = t.gameObject;
                        if ((victim.transform.position - point).sqrMagnitude <= r2
                            || (t.position - point).sqrMagnitude <= r2)
                            toDestroy.Add(victim);
                        break;
                    }
                    if (_parent != null && t == _parent)
                        break;
                    t = t.parent;
                }
            }

            foreach (GameObject go in toDestroy)
            {
                if (go != null)
                    Undo.DestroyObjectImmediate(go);
            }
        }

        private void CollectEraseRecursive(
            Transform root, Vector3 point, float r2, List<GameObject> active, HashSet<GameObject> toDestroy)
        {
            if (root == null)
                return;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if ((child.position - point).sqrMagnitude <= r2
                    && (child.GetComponent<PzPaintedProp>() != null || MatchesBrushPrefab(child.gameObject, active)))
                    toDestroy.Add(child.gameObject);
                CollectEraseRecursive(child, point, r2, active, toDestroy);
            }
        }

        private bool MatchesBrushPrefab(GameObject go, List<GameObject> active)
        {
            if (go == null || active == null || active.Count == 0)
                return false;

            UnityEngine.Object source = PrefabUtility.GetCorrespondingObjectFromSource(go)
                                        ?? PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);

            string goName = go.name;
            int clone = goName.IndexOf("(Clone)", StringComparison.Ordinal);
            if (clone >= 0)
                goName = goName.Substring(0, clone).TrimEnd();

            foreach (GameObject prefab in active)
            {
                if (prefab == null)
                    continue;
                if (source == prefab || (source != null && source.name == prefab.name))
                    return true;
                if (goName == prefab.name || goName.StartsWith(prefab.name + " "))
                    return true;
            }
            return false;
        }

        private bool TooClose(Vector3 point, float spacing)
        {
            float min = spacing * spacing;
            foreach (PzPaintedProp marker in UnityEngine.Object.FindObjectsByType<PzPaintedProp>(
                         FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (marker != null && (marker.transform.position - point).sqrMagnitude < min)
                    return true;
            }
            return _parent != null && TooCloseRecursive(_parent, point, min);
        }

        /// <summary>#55 density floors from World settings — brush spacing cannot undercut category mins.</summary>
        private float EffectiveSpacing(PzPaintCategory category)
        {
            return Mathf.Max(_spacing, CategoryMinSpacing(category));
        }

        private static float CategoryMinSpacing(PzPaintCategory category)
        {
            PzProjectSettings.WorldSettings world = PzProjectSettingsProvider.GetOrCreate().World;
            switch (category)
            {
                case PzPaintCategory.Tree: return Mathf.Max(0.5f, world.MinTreeSpacing);
                case PzPaintCategory.Bush: return Mathf.Max(0.25f, world.MinBushSpacing);
                case PzPaintCategory.Grass: return Mathf.Max(0.1f, world.MinGrassSpacing);
                case PzPaintCategory.Flower: return Mathf.Max(0.1f, world.MinFlowerSpacing);
                default: return 0.35f;
            }
        }

        private bool CheckPaintedPropBudget()
        {
            int cap = PzProjectSettingsProvider.GetOrCreate().World.MaxPaintedPropsPerZone;
            if (cap <= 0)
                return true;

            int count = UnityEngine.Object.FindObjectsByType<PzPaintedProp>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
            if (count < cap)
                return true;

            SetStatus($"Painted prop soft cap ({cap}) reached — thin or FPS-pass before more (#55).");
            return false;
        }

        private static bool TooCloseRecursive(Transform root, Vector3 point, float minSqr)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if ((child.position - point).sqrMagnitude < minSqr)
                    return true;
                if (TooCloseRecursive(child, point, minSqr))
                    return true;
            }
            return false;
        }

        private Transform ResolveParent(Transform hit) =>
            _parentToHit && hit != null ? hit : _parent;

        private static Transform EnsureFallbackPaintRoot()
        {
            const string name = "PZ_PaintedFoliage";
            GameObject existing = GameObject.Find(name);
            if (existing != null)
                return existing.transform;

            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "PZ Painted Foliage Root");
            return go.transform;
        }

        private PzPaintCategory GetCategoryForPrefab(GameObject prefab)
        {
            if (prefab == null || _palette == null)
                return PzPaintCategory.Other;

            string path = AssetDatabase.GetAssetPath(prefab);
            string guid = string.IsNullOrEmpty(path) ? null : AssetDatabase.AssetPathToGUID(path);
            if (!string.IsNullOrEmpty(guid))
            {
                PzPaintPrefabEntry entry = _palette.Entries.FirstOrDefault(e => e.PrefabGuid == guid);
                if (entry != null)
                    return entry.Category;
            }

            return PzPrefabPaintPalette.GuessCategory(prefab);
        }

        private static Transform EnsureCategoryFolder(Transform rootParent, PzPaintCategory category)
        {
            if (rootParent == null)
                return null;

            string folderName = category.ToString(); // Tree, Bush, Grass...
            Transform existing = rootParent.Find(folderName);
            if (existing != null)
                return existing;

            var folder = new GameObject(folderName);
            Undo.RegisterCreatedObjectUndo(folder, "PZ Paint Category Folder");
            folder.transform.SetParent(rootParent, false);
            folder.transform.localPosition = Vector3.zero;
            folder.transform.localRotation = Quaternion.identity;
            folder.transform.localScale = Vector3.one;
            return folder.transform;
        }

        /// <summary>
        /// Moves painted props under Parent into Tree/Bush/Grass/... folders and cleans "(Clone)" names.
        /// </summary>
        private void OrganizePaintedHierarchy()
        {
            if (_parent == null)
            {
                EditorUtility.DisplayDialog("PROJECT ZONE", "Assign a Parent (Art root) first.", "OK");
                return;
            }

            // Collect painted props under parent (including loose ones not yet in category folders).
            var props = _parent.GetComponentsInChildren<PzPaintedProp>(true)
                .Where(p => p != null)
                .Select(p => p.gameObject)
                .Distinct()
                .ToList();

            // Also include direct children that match palette prefabs but lack marker.
            List<GameObject> palettePrefabs = _palette.Entries
                .Select(e => e.LoadPrefab())
                .Where(p => p != null)
                .ToList();

            for (int i = 0; i < _parent.childCount; i++)
            {
                Transform child = _parent.GetChild(i);
                if (IsCategoryFolderName(child.name))
                    continue;
                if (MatchesBrushPrefab(child.gameObject, palettePrefabs))
                    props.Add(child.gameObject);
            }

            int moved = 0;
            int renamed = 0;
            foreach (GameObject go in props.Distinct())
            {
                if (go == null)
                    continue;

                // Don't move the category folder roots themselves.
                if (go.transform.parent == _parent && IsCategoryFolderName(go.name)
                    && go.GetComponent<PzPaintedProp>() == null)
                    continue;

                GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject
                                    ?? PrefabUtility.GetCorrespondingObjectFromOriginalSource(go) as GameObject;
                PzPaintCategory cat = GetCategoryForPrefab(source != null ? source : go);
                Transform folder = EnsureCategoryFolder(_parent, cat);

                if (go.name.EndsWith("(Clone)", StringComparison.Ordinal))
                {
                    Undo.RecordObject(go, "PZ Rename Clone");
                    go.name = go.name.Replace("(Clone)", string.Empty).TrimEnd();
                    renamed++;
                }

                if (go.transform.parent != folder)
                {
                    Undo.SetTransformParent(go.transform, folder, "PZ Organize Hierarchy");
                    moved++;
                }
            }

            // Sort category folders alphabetically under parent, and children by name inside each.
            SortChildrenByName(_parent);
            foreach (PzPaintCategory cat in Enum.GetValues(typeof(PzPaintCategory)))
            {
                Transform folder = _parent.Find(cat.ToString());
                if (folder != null)
                    SortChildrenByName(folder);
            }

            SetStatus($"Organized hierarchy — moved {moved}, renamed {renamed}.");
            EditorUtility.DisplayDialog(
                "PROJECT ZONE",
                $"Hierarchy sorted under '{_parent.name}'.\nMoved: {moved}\nRenamed: {renamed}\n\nFolders: Tree / Bush / Grass / Flower / Rock / Clutter / Other",
                "OK");
        }

        private static bool IsCategoryFolderName(string name)
        {
            foreach (PzPaintCategory cat in Enum.GetValues(typeof(PzPaintCategory)))
            {
                if (string.Equals(name, cat.ToString(), StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static void SortChildrenByName(Transform parent)
        {
            var children = new List<Transform>();
            for (int i = 0; i < parent.childCount; i++)
                children.Add(parent.GetChild(i));

            children = children.OrderBy(t => t.name, StringComparer.OrdinalIgnoreCase).ToList();
            for (int i = 0; i < children.Count; i++)
                children[i].SetSiblingIndex(i);
        }

        private void MarkParentChildrenPainted()
        {
            if (_parent == null)
            {
                EditorUtility.DisplayDialog("PROJECT ZONE", "Assign a Parent first.", "OK");
                return;
            }

            List<GameObject> active = GetActivePrefabs();
            if (active.Count == 0)
                active = _palette.Entries.Select(e => e.LoadPrefab()).Where(p => p != null).ToList();

            if (active.Count == 0)
            {
                EditorUtility.DisplayDialog("PROJECT ZONE", "Palette is empty.", "OK");
                return;
            }

            int n = 0;
            MarkRecursive(_parent, active, ref n);
            SetStatus($"Marked {n} children with PzPaintedProp.");
        }

        private void MarkRecursive(Transform root, List<GameObject> active, ref int count)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (MatchesBrushPrefab(child.gameObject, active) && child.GetComponent<PzPaintedProp>() == null)
                {
                    Undo.AddComponent<PzPaintedProp>(child.gameObject);
                    count++;
                }
                MarkRecursive(child, active, ref count);
            }
        }

        private void LoadPrefs()
        {
            _radius = EditorPrefs.GetFloat(PrefKey + "radius", 2f);
            _spacing = EditorPrefs.GetFloat(PrefKey + "spacing", 0.75f);
            _strength = EditorPrefs.GetFloat(PrefKey + "strength", 0.65f);
            _scaleMin = EditorPrefs.GetFloat(PrefKey + "scaleMin", 0.85f);
            _scaleMax = EditorPrefs.GetFloat(PrefKey + "scaleMax", 1.15f);
            _alignToNormal = EditorPrefs.GetBool(PrefKey + "align", true);
            _randomYaw = EditorPrefs.GetBool(PrefKey + "yaw", true);
        }

        private void SavePrefs()
        {
            EditorPrefs.SetFloat(PrefKey + "radius", _radius);
            EditorPrefs.SetFloat(PrefKey + "spacing", _spacing);
            EditorPrefs.SetFloat(PrefKey + "strength", _strength);
            EditorPrefs.SetFloat(PrefKey + "scaleMin", _scaleMin);
            EditorPrefs.SetFloat(PrefKey + "scaleMax", _scaleMax);
            EditorPrefs.SetBool(PrefKey + "align", _alignToNormal);
            EditorPrefs.SetBool(PrefKey + "yaw", _randomYaw);
        }
    }

    internal static class PzUiExtensions
    {
        public static T FindAncestorOfType<T>(this VisualElement ve) where T : VisualElement
        {
            VisualElement p = ve;
            while (p != null)
            {
                if (p is T match)
                    return match;
                p = p.parent;
            }
            return null;
        }
    }
}
#endif
