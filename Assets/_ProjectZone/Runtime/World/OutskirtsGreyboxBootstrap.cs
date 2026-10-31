using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjectZone
{
    /// <summary>
    /// Builds / refreshes the Outskirts greybox layout.
    /// M3: #14–#18 · M6: zone markers + Greybox/Art/Collision/Spawns/Volumes (#41–#45).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class OutskirtsGreyboxBootstrap : MonoBehaviour
    {
        private const string ZonesRootName = "Outskirts_Zones";
        /// <summary>M6: zone markers + standard hierarchy folders.</summary>
        private const int LayoutVersion = 6;

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
                    // Zone style mats auto-apply after rebuild (Editor assembly).
                    EditorApplication.delayCall += NotifyZoneStyleAfterRebuild;
                }
#endif
            }
            finally
            {
                _isBuilding = false;
                _rebuildScheduled = false;
            }
        }

#if UNITY_EDITOR
        private static void NotifyZoneStyleAfterRebuild()
        {
            // Resolved by name so Runtime never hard-depends on Editor types.
            const string typeName = "ProjectZone.Editor.PzZoneStyleMenus";
            System.Type styleType = System.Type.GetType(typeName + ", Assembly-CSharp-Editor");
            if (styleType == null)
            {
                foreach (System.Reflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    styleType = assembly.GetType(typeName);
                    if (styleType != null)
                        break;
                }
            }

            styleType?.GetMethod(
                    "AutoApplyAfterGreyboxRebuild",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                ?.Invoke(null, null);
        }
#endif

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

        private static void BuildSettlement(Transform parent)
        {
            // Zone style: packed dirt / concrete panels (not bright farm green).
            Color yard = new Color(0.30f, 0.29f, 0.26f);
            Color structure = new Color(0.48f, 0.47f, 0.44f);
            Color accent = new Color(0.42f, 0.41f, 0.39f);
            Color fence = new Color(0.38f, 0.28f, 0.22f);

            PzZoneMarker marker = PzZoneHierarchy.CreateZoneRoot(
                parent, "Zone_Settlement", PzZoneId.Settlement, true, "Settlement", Vector3.zero);
            Transform g = marker.GreyboxRoot;

            CreateBlock(g, "Footprint", new Vector3(0f, 0.05f, 0f), new Vector3(36f, 0.1f, 32f), yard);
            CreateBlock(g, "Courtyard", new Vector3(0f, 0.12f, 0f), new Vector3(16f, 0.12f, 14f), accent);
            CreateBlock(g, "Wall_North", new Vector3(0f, 1.5f, 15f), new Vector3(36f, 3f, 0.6f), fence);
            CreateBlock(g, "Wall_South", new Vector3(0f, 1.5f, -15f), new Vector3(36f, 3f, 0.6f), fence);
            CreateBlock(g, "Wall_West", new Vector3(-17.5f, 1.5f, 0f), new Vector3(0.6f, 3f, 30f), fence);
            CreateBlock(g, "Wall_East_North", new Vector3(17.5f, 1.5f, 8f), new Vector3(0.6f, 3f, 12f), fence);
            CreateBlock(g, "Wall_East_South", new Vector3(17.5f, 1.5f, -8f), new Vector3(0.6f, 3f, 12f), fence);
            CreateBlock(g, "Gate_Pillar_N", new Vector3(17.5f, 2f, 2.5f), new Vector3(1.2f, 4f, 1.2f), accent);
            CreateBlock(g, "Gate_Pillar_S", new Vector3(17.5f, 2f, -2.5f), new Vector3(1.2f, 4f, 1.2f), accent);
            CreateBlock(g, "Hab_A", new Vector3(-10f, 2f, 8f), new Vector3(8f, 4f, 6f), structure);
            CreateBlock(g, "Hab_B", new Vector3(-10f, 2f, -8f), new Vector3(8f, 4f, 6f), structure);
            CreateBlock(g, "SupplyShed", new Vector3(8f, 1.5f, 9f), new Vector3(6f, 3f, 5f), structure);
            CreateBlock(g, "WatchPost", new Vector3(10f, 3f, -10f), new Vector3(3f, 6f, 3f), structure);
            CreateBlock(g, "WatchPost_Deck", new Vector3(10f, 5.5f, -10f), new Vector3(5f, 0.4f, 5f), accent);

            PzZoneHierarchy.CreateSpawnPoint(marker.SpawnsRoot, "Spawn_PlayerStart", new Vector3(0f, 0.1f, 0f));
            PzZoneHierarchy.CreateSpawnPoint(marker.SpawnsRoot, "Spawn_Stash", new Vector3(8f, 0.1f, 9f));
            PzZoneHierarchy.CreateSpawnPoint(marker.SpawnsRoot, "Spawn_ContractBoard", new Vector3(-4f, 0.1f, 4f));

            CreateTriggerVolume(marker.VolumesRoot, "SafeZoneVolume",
                new Vector3(0f, 2f, 0f), new Vector3(34f, 4f, 30f),
                new Color(0.35f, 0.75f, 0.4f, 0.15f));
        }

        private static void BuildCheckpoint(Transform parent)
        {
            Color asphalt = new Color(0.38f, 0.34f, 0.3f);
            Color rust = new Color(0.55f, 0.38f, 0.28f);
            Color concrete = new Color(0.48f, 0.45f, 0.4f);
            Color debris = new Color(0.35f, 0.32f, 0.28f);

            PzZoneMarker marker = PzZoneHierarchy.CreateZoneRoot(
                parent, "Zone_Checkpoint", PzZoneId.Checkpoint, false, "Checkpoint",
                new Vector3(45f, 0f, 0f));
            Transform g = marker.GreyboxRoot;

            CreateBlock(g, "Footprint", new Vector3(0f, 0.05f, 0f), new Vector3(28f, 0.1f, 22f), asphalt);
            CreateBlock(g, "RoadLane", new Vector3(0f, 0.12f, 0f), new Vector3(26f, 0.1f, 6f), new Color(0.32f, 0.3f, 0.28f));
            CreateBlock(g, "GateRuin_West_N", new Vector3(-12f, 1.5f, 5f), new Vector3(4f, 3f, 1f), rust);
            CreateBlock(g, "GateRuin_West_S", new Vector3(-12f, 1.2f, -5.5f), new Vector3(5f, 2.4f, 1.2f), rust);
            CreateBlock(g, "FallenBoom", new Vector3(-8f, 0.4f, -1.5f), new Vector3(10f, 0.5f, 0.6f), concrete);
            CreateBlock(g, "GuardBooth", new Vector3(-2f, 1.6f, 6f), new Vector3(5f, 3.2f, 4f), concrete);
            CreateBlock(g, "BoothCanopy", new Vector3(-2f, 3.4f, 6f), new Vector3(7f, 0.3f, 5f), rust);
            CreateBlock(g, "SandbagPile_A", new Vector3(2f, 0.7f, 5f), new Vector3(4f, 1.4f, 2f), debris);
            CreateBlock(g, "SandbagPile_B", new Vector3(3f, 0.6f, -5f), new Vector3(5f, 1.2f, 2.2f), debris);
            CreateBlock(g, "JerseyBarrier_N", new Vector3(10f, 0.8f, 3.5f), new Vector3(6f, 1.6f, 1.2f), concrete);
            CreateBlock(g, "JerseyBarrier_S", new Vector3(11f, 0.7f, -3.8f), new Vector3(5f, 1.4f, 1.2f), concrete);
            CreateBlock(g, "OverturnedBarrier", new Vector3(8f, 1.1f, -1f), new Vector3(1.2f, 2.2f, 4f), rust);
            CreateBlock(g, "WatchCrate", new Vector3(6f, 1f, 7f), new Vector3(2.5f, 2f, 2.5f), debris);
            CreateBlock(g, "SignPost", new Vector3(-6f, 2.2f, 2f), new Vector3(0.4f, 4.4f, 0.4f), rust);
            CreateBlock(g, "SignBoard", new Vector3(-6f, 3.8f, 2f), new Vector3(3f, 1.4f, 0.3f), concrete);

            PzZoneHierarchy.CreateSpawnPoint(marker.SpawnsRoot, "Spawn_CoverFight", new Vector3(0f, 0.1f, 0f));
            PzZoneHierarchy.CreateSpawnPoint(marker.SpawnsRoot, "Spawn_LootHint", new Vector3(-2f, 0.1f, 6f));
        }

        private static void BuildIndustrial(Transform parent)
        {
            Color slab = new Color(0.36f, 0.38f, 0.42f);
            Color steel = new Color(0.45f, 0.48f, 0.52f);
            Color rust = new Color(0.5f, 0.36f, 0.3f);
            Color hazard = new Color(0.55f, 0.5f, 0.28f);

            PzZoneMarker marker = PzZoneHierarchy.CreateZoneRoot(
                parent, "Zone_Industrial", PzZoneId.Industrial, false, "Industrial",
                new Vector3(90f, 0f, 8f));
            Transform g = marker.GreyboxRoot;

            CreateBlock(g, "Footprint", new Vector3(0f, 0.05f, 0f), new Vector3(36f, 0.1f, 30f), slab);
            CreateBlock(g, "YardPad", new Vector3(8f, 0.12f, 0f), new Vector3(14f, 0.1f, 18f), new Color(0.32f, 0.34f, 0.36f));
            CreateBlock(g, "Warehouse_WestWall", new Vector3(-12f, 4f, 0f), new Vector3(0.8f, 8f, 16f), steel);
            CreateBlock(g, "Warehouse_EastWall", new Vector3(-2f, 4f, 0f), new Vector3(0.8f, 8f, 16f), steel);
            CreateBlock(g, "Warehouse_NorthWall", new Vector3(-7f, 4f, 7.6f), new Vector3(10f, 8f, 0.8f), steel);
            CreateBlock(g, "Warehouse_Roof", new Vector3(-7f, 8.2f, 0f), new Vector3(11f, 0.5f, 17f), rust);
            CreateBlock(g, "Warehouse_InteriorStack", new Vector3(-7f, 1.5f, 3f), new Vector3(4f, 3f, 3f), hazard);
            CreateBlock(g, "LoadingDock", new Vector3(4f, 1f, 8f), new Vector3(12f, 2f, 6f), steel);
            CreateBlock(g, "DockRamp", new Vector3(4f, 0.5f, 12.5f), new Vector3(8f, 1f, 3f), slab);
            CreateBlock(g, "OfficeAnnex", new Vector3(10f, 2f, 8f), new Vector3(6f, 4f, 5f), steel);
            CreateBlock(g, "Tank_A", new Vector3(10f, 3.5f, -6f), new Vector3(6f, 7f, 6f), rust);
            CreateBlock(g, "Tank_B", new Vector3(10f, 2.5f, -12f), new Vector3(4f, 5f, 4f), rust);
            CreateBlock(g, "PipeRun", new Vector3(2f, 2.5f, -9f), new Vector3(12f, 0.7f, 0.7f), hazard);
            CreateBlock(g, "CrateRow", new Vector3(-2f, 0.8f, -8f), new Vector3(8f, 1.6f, 2.5f), steel);
            CreateBlock(g, "FenceStub_N", new Vector3(0f, 1.5f, 14f), new Vector3(20f, 3f, 0.4f), rust);
            CreateBlock(g, "NorthGate_Pillar_W", new Vector3(-3f, 2f, 14f), new Vector3(1.2f, 4f, 1.2f), hazard);
            CreateBlock(g, "NorthGate_Pillar_E", new Vector3(3f, 2f, 14f), new Vector3(1.2f, 4f, 1.2f), hazard);

            PzZoneHierarchy.CreateSpawnPoint(marker.SpawnsRoot, "Spawn_Hostile_Zombie", new Vector3(2f, 0.1f, -6f));
            PzZoneHierarchy.CreateSpawnPoint(marker.SpawnsRoot, "Spawn_Loot", new Vector3(-7f, 0.1f, 3f));
        }

        private static void BuildUndergroundEntrance(Transform parent)
        {
            Color earth = new Color(0.22f, 0.24f, 0.28f);
            Color concrete = new Color(0.35f, 0.36f, 0.4f);
            Color metal = new Color(0.4f, 0.42f, 0.45f);
            Color warning = new Color(0.5f, 0.42f, 0.25f);

            PzZoneMarker marker = PzZoneHierarchy.CreateZoneRoot(
                parent, "Zone_UndergroundEntrance", PzZoneId.Underground, false, "Underground Entrance",
                new Vector3(90f, 0f, 40f));
            Transform g = marker.GreyboxRoot;

            CreateBlock(g, "Footprint", new Vector3(0f, 0.05f, 0f), new Vector3(22f, 0.1f, 24f), earth);
            CreateBlock(g, "ApproachPad", new Vector3(0f, 0.12f, -8f), new Vector3(10f, 0.1f, 8f), concrete);
            CreateBlock(g, "Ramp_Upper", new Vector3(0f, 0.6f, -4f), new Vector3(8f, 0.4f, 6f), concrete);
            CreateBlock(g, "Ramp_Lower", new Vector3(0f, 0.15f, 1f), new Vector3(7f, 0.35f, 6f), concrete);
            CreateBlock(g, "Retaining_West", new Vector3(-5f, 1.8f, -1f), new Vector3(0.6f, 3.6f, 14f), concrete);
            CreateBlock(g, "Retaining_East", new Vector3(5f, 1.8f, -1f), new Vector3(0.6f, 3.6f, 14f), concrete);
            CreateBlock(g, "BunkerLip", new Vector3(0f, 2.5f, 7f), new Vector3(14f, 5f, 2f), concrete);
            CreateBlock(g, "BunkerWing_W", new Vector3(-6f, 2f, 5f), new Vector3(3f, 4f, 4f), concrete);
            CreateBlock(g, "BunkerWing_E", new Vector3(6f, 2f, 5f), new Vector3(3f, 4f, 4f), concrete);
            CreateBlock(g, "HatchFrame", new Vector3(0f, 1.2f, 4f), new Vector3(5f, 2.4f, 1f), metal);
            CreateBlock(g, "HatchDoor", new Vector3(0f, 1.4f, 3.2f), new Vector3(3.5f, 2f, 0.4f), warning);
            CreateBlock(g, "StairMouth", new Vector3(0f, 0.5f, 5.5f), new Vector3(4f, 1f, 3f), earth);
            CreateBlock(g, "Tunnel_Ceiling", new Vector3(0f, 2.8f, 10f), new Vector3(5f, 0.5f, 8f), concrete);
            CreateBlock(g, "Tunnel_Floor", new Vector3(0f, 0.1f, 10f), new Vector3(5f, 0.2f, 8f), concrete);
            CreateBlock(g, "Tunnel_Wall_W", new Vector3(-2.5f, 1.5f, 10f), new Vector3(0.4f, 3f, 8f), concrete);
            CreateBlock(g, "Tunnel_Wall_E", new Vector3(2.5f, 1.5f, 10f), new Vector3(0.4f, 3f, 8f), concrete);
            CreateBlock(g, "Tunnel_EndCap", new Vector3(0f, 1.5f, 14f), new Vector3(5f, 3f, 0.5f), metal);

            PzZoneHierarchy.CreateSpawnPoint(marker.SpawnsRoot, "Spawn_EntranceApproach", new Vector3(0f, 0.1f, -8f));

            CreateTriggerVolume(marker.VolumesRoot, "UndergroundTransitionMarker",
                new Vector3(0f, 1.5f, 12f), new Vector3(3.5f, 2.5f, 1.5f),
                new Color(0.35f, 0.45f, 0.7f, 0.2f));
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

        private static void CreateTriggerVolume(
            Transform parent,
            string name,
            Vector3 localPos,
            Vector3 scale,
            Color color)
        {
            GameObject volume = GameObject.CreatePrimitive(PrimitiveType.Cube);
            volume.name = name;
            volume.transform.SetParent(parent, false);
            volume.transform.localPosition = localPos;
            volume.transform.localScale = scale;
            Collider col = volume.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
            ApplyColor(volume, color);
            var renderer = volume.GetComponent<MeshRenderer>();
            if (renderer != null)
                SetMaterialTransparent(renderer.sharedMaterial);
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
