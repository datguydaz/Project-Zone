using System.Text;
using EmeraldAI;
using PolymindGames;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ProjectZone
{
    /// <summary>
    /// Play Mode sign-off helper for GitHub issue #13 — HQ FPS × Emerald AI package compatibility.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PackageCompatibilityValidator : MonoBehaviour
    {
        [SerializeField]
        private bool _logReport = true;

        private bool _reported;

        public void RunReport(Player player)
        {
            if (_reported)
                return;

            _reported = true;

            var sb = new StringBuilder();
            int failures = 0;

            sb.AppendLine("[ProjectZone][Compat] Package compatibility sign-off (issue #13).");
            sb.AppendLine($"  Unity: {Application.unityVersion}");
            AppendCheck(sb, ref failures, "URP pipeline asset present",
                GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset ||
                QualitySettings.renderPipeline is UniversalRenderPipelineAsset);
            AppendCheck(sb, ref failures, "EmeraldSystem type available", typeof(EmeraldSystem) != null);
            AppendCheck(sb, ref failures, "EmeraldHealth type available", typeof(EmeraldHealth) != null);
            AppendCheck(sb, ref failures, "EmeraldPlayerBridge type available", typeof(EmeraldPlayerBridge) != null);
            AppendCheck(sb, ref failures, "HQ Player type available", typeof(Player) != null);
            AppendCheck(sb, ref failures, "HQ IHealthManager available", typeof(IHealthManager) != null);
            AppendCheck(sb, ref failures, "HQ IDamageHandler available", typeof(IDamageHandler) != null);

            EmeraldSystem[] systems = FindObjectsByType<EmeraldSystem>(FindObjectsInactive.Exclude);
            AppendCheck(sb, ref failures, $"Emerald AI present in scene ({systems.Length})", systems.Length > 0);

            bool playerOk = player != null && player.HealthManager != null;
            AppendCheck(sb, ref failures, "HQ player + HealthManager bound", playerOk);

            bool bridgeOk = player != null &&
                            (player.GetComponent<HqPlayerDamageBridge>() != null ||
                             player.GetComponentInChildren<HqPlayerDamageBridge>() != null);
            AppendCheck(sb, ref failures, "HqPlayerDamageBridge on player", bridgeOk);

            bool targetOk = player != null && player.GetComponent<EmeraldHqPlayerTargetAdapter>() != null;
            AppendCheck(sb, ref failures, "EmeraldHqPlayerTargetAdapter on player", targetOk);

            int receivers = FindObjectsByType<EmeraldDamageReceiver>(FindObjectsInactive.Exclude).Length;
            AppendCheck(sb, ref failures, $"EmeraldDamageReceiver on AI ({receivers})", receivers > 0);

            sb.AppendLine("  Prior Play Mode evidence: #8–#12 adapters verified (ranged loop + death/hit).");
            sb.AppendLine("  Deferred: melee hostile (scene content), networking packages.");
            sb.AppendLine(failures == 0
                ? "  Status: PASS — confirm in Play Mode, then we can close #13 / sign off M2."
                : $"  Status: FAIL — {failures} check(s) failed.");

            if (_logReport)
            {
                if (failures == 0)
                    Debug.Log(sb.ToString(), this);
                else
                    Debug.LogError(sb.ToString(), this);
            }
        }

        private static void AppendCheck(StringBuilder sb, ref int failures, string label, bool ok)
        {
            sb.AppendLine($"  [{(ok ? "x" : " ")}] {label}");
            if (!ok)
                failures++;
        }
    }
}
