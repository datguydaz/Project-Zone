using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjectZone
{
    /// <summary>
    /// Builds / refreshes the Milestone 3 Outskirts greybox layout.
    /// #14 region · #15 settlement · #16 checkpoint · #17 industrial · #18 underground.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class OutskirtsGreyboxBootstrap : MonoBehaviour
    {
        private const string ZonesRootName = "Outskirts_Zones";
        private const int LayoutVersion = 5;

        [SerializeField]
        private bool _rebuildOnEnable = true;

        [SerializeField]
        private int _builtLayoutVersion;

        private bool _isBuilding;
        private bool _rebuildScheduled;

        private void OnEnable()
        {
            if (!_rebuildOnEnable)
                return;

            if (transform.Find(ZonesRootName) != null && _builtLayoutVersion == LayoutVersion)
                return;

            ScheduleRebuild();
        }

        [ContextMenu("Rebuild Outskirts Greybox")]
        public void BuildLayout()
        {
            if (_isBuilding || this == null)
                return;

            _isBuilding = true;
            try
            {
                Transform zonesRoot = transform.Find(ZonesRootName);
                if (zonesRoot != null)
                {
                    ClearSelectionUnder(zonesRoot);
                    DestroyImmediateSafe(zonesRoot.gameObject);
                }

                EnsureGround();

                GameObject root = new GameObject(ZonesRootName);
                root.transform.SetParent(transform, false);

                BuildSettlement(root.transform);
                BuildCheckpoint(root.transform);
                BuildIndustrial(root.transform);
                BuildUndergroundEntrance(root.transform);

                CreatePath(root.transform, "Path_Settlement_to_Checkpoint",
                    new Vector3(22f, 0.05f, 0f), new Vector3(24f, 0.1f, 6f), new Color(0.38f, 0.36f, 0.32f));
                CreatePath(root.transform, "Path_Checkpoint_to_Industrial",
                    new Vector3(68f, 0.05f, 2f), new Vector3(28f, 0.1f, 6f), new Color(0.38f, 0.36f, 0.32f));
                CreatePath(root.transform, "Path_Industrial_to_Underground",
                    new Vector3(90f, 0.05f, 24f), new Vector3(6f, 0.1f, 20f), new Color(0.38f, 0.36f, 0.32f));

                _builtLayoutVersion = LayoutVersion;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(this);
                    Selection.activeGameObject = gameObject;
                }
#endif
            }
            finally
            {
                _isBuilding = false;
                _rebuildScheduled = false;
            }
        }

        private void ScheduleRebuild()
        {
            if (_isBuilding || _rebuildScheduled)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                _rebuildScheduled = true;
                EditorApplication.delayCall += DelayedEditorRebuild;
                return;
            }
#endif
            BuildLayout();
        }

#if UNITY_EDITOR
        private void DelayedEditorRebuild()
        {
            EditorApplication.delayCall -= DelayedEditorRebuild;
            _rebuildScheduled = false;

            if (this == null || !_rebuildOnEnable)
                return;

            if (transform.Find(ZonesRootName) != null && _builtLayoutVersion == LayoutVersion)
                return;

            BuildLayout();
        }
#endif

        private static void ClearSelectionUnder(Transform root)
        {
#if UNITY_EDITOR
            if (Application.isPlaying || root == null)
                return;

            Object[] selected = Selection.objects;
            if (selected == null || selected.Length == 0)
                return;

            bool touched = false;
            for (int i = 0; i < selected.Length; i++)
            {
                var go = selected[i] as GameObject;
                if (go == null)
                    continue;
                if (go.transform == root || go.transform.IsChildOf(root))
                {
                    touched = true;
                    break;
                }
            }

            if (touched)
                Selection.activeObject = null;
#endif
        }

        /// <summary>
        /// Safe hub for issue #15: enclosed yard, east gate toward the Outskirts path,
        /// habs, supply shed, watch post, and a marked safe volume for later systems.
        /// </summary>
        private static void BuildSettlement(Transform parent)
        {
            Color yard = new Color(0.32f, 0.5f, 0.34f);
            Color structure = new Color(0.42f, 0.58f, 0.4f);
            Color accent = new Color(0.55f, 0.7f, 0.45f);
            Color fence = new Color(0.25f, 0.38f, 0.28f);

            var zone = new GameObject("Zone_Settlement");
            zone.transform.SetParent(parent, false);
            zone.transform.position = Vector3.zero;

            CreateBlock(zone.transform, "Footprint", new Vector3(0f, 0.05f, 0f), new Vector3(36f, 0.1f, 32f), yard);
            CreateBlock(zone.transform, "Courtyard", new Vector3(0f, 0.12f, 0f), new Vector3(16f, 0.12f, 14f), accent);

            // Perimeter with an east gate opening toward Path_Settlement_to_Checkpoint.
            CreateBlock(zone.transform, "Wall_North", new Vector3(0f, 1.5f, 15f), new Vector3(36f, 3f, 0.6f), fence);
            CreateBlock(zone.transform, "Wall_South", new Vector3(0f, 1.5f, -15f), new Vector3(36f, 3f, 0.6f), fence);
            CreateBlock(zone.transform, "Wall_West", new Vector3(-17.5f, 1.5f, 0f), new Vector3(0.6f, 3f, 30f), fence);
            CreateBlock(zone.transform, "Wall_East_North", new Vector3(17.5f, 1.5f, 8f), new Vector3(0.6f, 3f, 12f), fence);
            CreateBlock(zone.transform, "Wall_East_South", new Vector3(17.5f, 1.5f, -8f), new Vector3(0.6f, 3f, 12f), fence);
            CreateBlock(zone.transform, "Gate_Pillar_N", new Vector3(17.5f, 2f, 2.5f), new Vector3(1.2f, 4f, 1.2f), accent);
            CreateBlock(zone.transform, "Gate_Pillar_S", new Vector3(17.5f, 2f, -2.5f), new Vector3(1.2f, 4f, 1.2f), accent);

            CreateBlock(zone.transform, "Hab_A", new Vector3(-10f, 2f, 8f), new Vector3(8f, 4f, 6f), structure);
            CreateBlock(zone.transform, "Hab_B", new Vector3(-10f, 2f, -8f), new Vector3(8f, 4f, 6f), structure);
            CreateBlock(zone.transform, "SupplyShed", new Vector3(8f, 1.5f, 9f), new Vector3(6f, 3f, 5f), structure);
            CreateBlock(zone.transform, "WatchPost", new Vector3(10f, 3f, -10f), new Vector3(3f, 6f, 3f), structure);
            CreateBlock(zone.transform, "WatchPost_Deck", new Vector3(10f, 5.5f, -10f), new Vector3(5f, 0.4f, 5f), accent);

            // Marker volume for future safe-zone logic (trigger, no collision).
            GameObject safeVolume = GameObject.CreatePrimitive(PrimitiveType.Cube);
            safeVolume.name = "SafeZoneVolume";
            safeVolume.transform.SetParent(zone.transform, false);
            safeVolume.transform.localPosition = new Vector3(0f, 2f, 0f);
            safeVolume.transform.localScale = new Vector3(34f, 4f, 30f);
            Collider col = safeVolume.GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            var renderer = safeVolume.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                ApplyColor(safeVolume, new Color(0.35f, 0.75f, 0.4f, 0.15f));
                SetMaterialTransparent(renderer.sharedMaterial);
            }
        }

        /// <summary>
        /// Abandoned checkpoint for issue #16: ruined gate lane, booth, broken boom,
        /// cover piles, and a clear east-west path toward Industrial.
        /// </summary>
        private static void BuildCheckpoint(Transform parent)
        {
            Color asphalt = new Color(0.38f, 0.34f, 0.3f);
            Color rust = new Color(0.55f, 0.38f, 0.28f);
            Color concrete = new Color(0.48f, 0.45f, 0.4f);
            Color debris = new Color(0.35f, 0.32f, 0.28f);

            var zone = new GameObject("Zone_Checkpoint");
            zone.transform.SetParent(parent, false);
            zone.transform.position = new Vector3(45f, 0f, 0f);

            CreateBlock(zone.transform, "Footprint", new Vector3(0f, 0.05f, 0f), new Vector3(28f, 0.1f, 22f), asphalt);
            CreateBlock(zone.transform, "RoadLane", new Vector3(0f, 0.12f, 0f), new Vector3(26f, 0.1f, 6f), new Color(0.32f, 0.3f, 0.28f));

            // Collapsed western approach (from Settlement) — walkable gap remains.
            CreateBlock(zone.transform, "GateRuin_West_N", new Vector3(-12f, 1.5f, 5f), new Vector3(4f, 3f, 1f), rust);
            CreateBlock(zone.transform, "GateRuin_West_S", new Vector3(-12f, 1.2f, -5.5f), new Vector3(5f, 2.4f, 1.2f), rust);
            CreateBlock(zone.transform, "FallenBoom", new Vector3(-8f, 0.4f, -1.5f), new Vector3(10f, 0.5f, 0.6f), concrete);

            // Guard booth + lean-to, still readable as checkpoint architecture.
            CreateBlock(zone.transform, "GuardBooth", new Vector3(-2f, 1.6f, 6f), new Vector3(5f, 3.2f, 4f), concrete);
            CreateBlock(zone.transform, "BoothCanopy", new Vector3(-2f, 3.4f, 6f), new Vector3(7f, 0.3f, 5f), rust);
            CreateBlock(zone.transform, "SandbagPile_A", new Vector3(2f, 0.7f, 5f), new Vector3(4f, 1.4f, 2f), debris);
            CreateBlock(zone.transform, "SandbagPile_B", new Vector3(3f, 0.6f, -5f), new Vector3(5f, 1.2f, 2.2f), debris);

            // Eastern exit toward Industrial — partial blockade with cover.
            CreateBlock(zone.transform, "JerseyBarrier_N", new Vector3(10f, 0.8f, 3.5f), new Vector3(6f, 1.6f, 1.2f), concrete);
            CreateBlock(zone.transform, "JerseyBarrier_S", new Vector3(11f, 0.7f, -3.8f), new Vector3(5f, 1.4f, 1.2f), concrete);
            CreateBlock(zone.transform, "OverturnedBarrier", new Vector3(8f, 1.1f, -1f), new Vector3(1.2f, 2.2f, 4f), rust);
            CreateBlock(zone.transform, "WatchCrate", new Vector3(6f, 1f, 7f), new Vector3(2.5f, 2f, 2.5f), debris);
            CreateBlock(zone.transform, "SignPost", new Vector3(-6f, 2.2f, 2f), new Vector3(0.4f, 4.4f, 0.4f), rust);
            CreateBlock(zone.transform, "SignBoard", new Vector3(-6f, 3.8f, 2f), new Vector3(3f, 1.4f, 0.3f), concrete);
        }

        /// <summary>
        /// Industrial site for issue #17: main warehouse shell with open bay,
        /// loading dock, tank farm, pipe run, and a north walk toward Underground.
        /// </summary>
        private static void BuildIndustrial(Transform parent)
        {
            Color slab = new Color(0.36f, 0.38f, 0.42f);
            Color steel = new Color(0.45f, 0.48f, 0.52f);
            Color rust = new Color(0.5f, 0.36f, 0.3f);
            Color hazard = new Color(0.55f, 0.5f, 0.28f);

            var zone = new GameObject("Zone_Industrial");
            zone.transform.SetParent(parent, false);
            zone.transform.position = new Vector3(90f, 0f, 8f);

            CreateBlock(zone.transform, "Footprint", new Vector3(0f, 0.05f, 0f), new Vector3(36f, 0.1f, 30f), slab);
            CreateBlock(zone.transform, "YardPad", new Vector3(8f, 0.12f, 0f), new Vector3(14f, 0.1f, 18f), new Color(0.32f, 0.34f, 0.36f));

            // Main warehouse — open south bay so the player can walk through interior volume.
            CreateBlock(zone.transform, "Warehouse_WestWall", new Vector3(-12f, 4f, 0f), new Vector3(0.8f, 8f, 16f), steel);
            CreateBlock(zone.transform, "Warehouse_EastWall", new Vector3(-2f, 4f, 0f), new Vector3(0.8f, 8f, 16f), steel);
            CreateBlock(zone.transform, "Warehouse_NorthWall", new Vector3(-7f, 4f, 7.6f), new Vector3(10f, 8f, 0.8f), steel);
            CreateBlock(zone.transform, "Warehouse_Roof", new Vector3(-7f, 8.2f, 0f), new Vector3(11f, 0.5f, 17f), rust);
            CreateBlock(zone.transform, "Warehouse_InteriorStack", new Vector3(-7f, 1.5f, 3f), new Vector3(4f, 3f, 3f), hazard);

            CreateBlock(zone.transform, "LoadingDock", new Vector3(4f, 1f, 8f), new Vector3(12f, 2f, 6f), steel);
            CreateBlock(zone.transform, "DockRamp", new Vector3(4f, 0.5f, 12.5f), new Vector3(8f, 1f, 3f), slab);
            CreateBlock(zone.transform, "OfficeAnnex", new Vector3(10f, 2f, 8f), new Vector3(6f, 4f, 5f), steel);

            CreateBlock(zone.transform, "Tank_A", new Vector3(10f, 3.5f, -6f), new Vector3(6f, 7f, 6f), rust);
            CreateBlock(zone.transform, "Tank_B", new Vector3(10f, 2.5f, -12f), new Vector3(4f, 5f, 4f), rust);
            CreateBlock(zone.transform, "PipeRun", new Vector3(2f, 2.5f, -9f), new Vector3(12f, 0.7f, 0.7f), hazard);
            CreateBlock(zone.transform, "CrateRow", new Vector3(-2f, 0.8f, -8f), new Vector3(8f, 1.6f, 2.5f), steel);
            CreateBlock(zone.transform, "FenceStub_N", new Vector3(0f, 1.5f, 14f), new Vector3(20f, 3f, 0.4f), rust);

            // Opening toward Path_Industrial_to_Underground (north).
            CreateBlock(zone.transform, "NorthGate_Pillar_W", new Vector3(-3f, 2f, 14f), new Vector3(1.2f, 4f, 1.2f), hazard);
            CreateBlock(zone.transform, "NorthGate_Pillar_E", new Vector3(3f, 2f, 14f), new Vector3(1.2f, 4f, 1.2f), hazard);
        }

        /// <summary>
        /// Underground entrance for issue #18: approach ramp from Industrial, bunker lip,
        /// hatch/stairwell mouth, and a short interior tunnel stub (greybox only — no load yet).
        /// </summary>
        private static void BuildUndergroundEntrance(Transform parent)
        {
            Color earth = new Color(0.22f, 0.24f, 0.28f);
            Color concrete = new Color(0.35f, 0.36f, 0.4f);
            Color metal = new Color(0.4f, 0.42f, 0.45f);
            Color warning = new Color(0.5f, 0.42f, 0.25f);

            var zone = new GameObject("Zone_UndergroundEntrance");
            zone.transform.SetParent(parent, false);
            zone.transform.position = new Vector3(90f, 0f, 40f);

            CreateBlock(zone.transform, "Footprint", new Vector3(0f, 0.05f, 0f), new Vector3(22f, 0.1f, 24f), earth);
            CreateBlock(zone.transform, "ApproachPad", new Vector3(0f, 0.12f, -8f), new Vector3(10f, 0.1f, 8f), concrete);

            // Descending approach from industrial path (south → north into bunker).
            CreateBlock(zone.transform, "Ramp_Upper", new Vector3(0f, 0.6f, -4f), new Vector3(8f, 0.4f, 6f), concrete);
            CreateBlock(zone.transform, "Ramp_Lower", new Vector3(0f, 0.15f, 1f), new Vector3(7f, 0.35f, 6f), concrete);
            CreateBlock(zone.transform, "Retaining_West", new Vector3(-5f, 1.8f, -1f), new Vector3(0.6f, 3.6f, 14f), concrete);
            CreateBlock(zone.transform, "Retaining_East", new Vector3(5f, 1.8f, -1f), new Vector3(0.6f, 3.6f, 14f), concrete);

            CreateBlock(zone.transform, "BunkerLip", new Vector3(0f, 2.5f, 7f), new Vector3(14f, 5f, 2f), concrete);
            CreateBlock(zone.transform, "BunkerWing_W", new Vector3(-6f, 2f, 5f), new Vector3(3f, 4f, 4f), concrete);
            CreateBlock(zone.transform, "BunkerWing_E", new Vector3(6f, 2f, 5f), new Vector3(3f, 4f, 4f), concrete);

            CreateBlock(zone.transform, "HatchFrame", new Vector3(0f, 1.2f, 4f), new Vector3(5f, 2.4f, 1f), metal);
            CreateBlock(zone.transform, "HatchDoor", new Vector3(0f, 1.4f, 3.2f), new Vector3(3.5f, 2f, 0.4f), warning);
            CreateBlock(zone.transform, "StairMouth", new Vector3(0f, 0.5f, 5.5f), new Vector3(4f, 1f, 3f), earth);

            // Short tunnel stub — marks future underground transition without a second scene yet.
            CreateBlock(zone.transform, "Tunnel_Ceiling", new Vector3(0f, 2.8f, 10f), new Vector3(5f, 0.5f, 8f), concrete);
            CreateBlock(zone.transform, "Tunnel_Floor", new Vector3(0f, 0.1f, 10f), new Vector3(5f, 0.2f, 8f), concrete);
            CreateBlock(zone.transform, "Tunnel_Wall_W", new Vector3(-2.5f, 1.5f, 10f), new Vector3(0.4f, 3f, 8f), concrete);
            CreateBlock(zone.transform, "Tunnel_Wall_E", new Vector3(2.5f, 1.5f, 10f), new Vector3(0.4f, 3f, 8f), concrete);
            CreateBlock(zone.transform, "Tunnel_EndCap", new Vector3(0f, 1.5f, 14f), new Vector3(5f, 3f, 0.5f), metal);

            // Marker for a future scene load / interact prompt.
            GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            portal.name = "UndergroundTransitionMarker";
            portal.transform.SetParent(zone.transform, false);
            portal.transform.localPosition = new Vector3(0f, 1.5f, 12f);
            portal.transform.localScale = new Vector3(3.5f, 2.5f, 1.5f);
            Collider col = portal.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
            ApplyColor(portal, new Color(0.35f, 0.45f, 0.7f, 0.2f));
            var renderer = portal.GetComponent<MeshRenderer>();
            if (renderer != null)
                SetMaterialTransparent(renderer.sharedMaterial);
        }

        private static void CreateZone(
            Transform parent,
            string name,
            Vector3 worldPosition,
            Vector3 footprint,
            Color color,
            params (string name, Vector3 localPos, Vector3 scale)[] blocks)
        {
            var zone = new GameObject(name);
            zone.transform.SetParent(parent, false);
            zone.transform.position = worldPosition;

            CreateBlock(zone.transform, "Footprint", new Vector3(0f, 0.05f, 0f), footprint, color);

            for (int i = 0; i < blocks.Length; i++)
            {
                (string blockName, Vector3 localPos, Vector3 scale) = blocks[i];
                CreateBlock(zone.transform, blockName, localPos, scale, color);
            }
        }

        private static void CreatePath(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject path = GameObject.CreatePrimitive(PrimitiveType.Cube);
            path.name = name;
            path.isStatic = true;
            path.transform.SetParent(parent, false);
            path.transform.position = position;
            path.transform.localScale = scale;
            ApplyColor(path, color);
        }

        private static void CreateBlock(Transform parent, string name, Vector3 localPos, Vector3 scale, Color color)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.isStatic = true;
            block.transform.SetParent(parent, false);
            block.transform.localPosition = localPos;
            block.transform.localScale = scale;
            ApplyColor(block, color);
        }

        private void EnsureGround()
        {
            Transform existing = transform.Find("Ground");
            if (existing != null)
                return;

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.isStatic = true;
            ground.transform.SetParent(transform, false);
            ground.transform.position = new Vector3(40f, -0.5f, 20f);
            ground.transform.localScale = new Vector3(160f, 1f, 100f);
            ApplyColor(ground, new Color(0.28f, 0.3f, 0.27f));
        }

        private static void ApplyColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer == null)
                return;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            mat.color = color;
            renderer.sharedMaterial = mat;
        }

        private static void SetMaterialTransparent(Material mat)
        {
            if (mat == null)
                return;

            // Best-effort URP Lit transparent so the safe volume reads as a soft field, not a solid wall.
            if (mat.HasProperty("_Surface"))
                mat.SetFloat("_Surface", 1f);
            if (mat.HasProperty("_Blend"))
                mat.SetFloat("_Blend", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }

        private static void DestroyImmediateSafe(GameObject go)
        {
            if (go == null)
                return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(go);
                return;
            }
#endif
            Object.Destroy(go);
        }
    }
}
