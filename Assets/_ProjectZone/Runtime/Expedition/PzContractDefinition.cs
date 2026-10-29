using UnityEngine;

namespace ProjectZone
{
    public enum PzContractObjectiveType : byte
    {
        ReachLocation = 0,
        KillCount = 1,
        RecoverDeathCache = 2,
        TurnInAtBoard = 3
    }

    /// <summary>
    /// Milestone 5 / #30–#31 — greybox contract definition (ScriptableObject or runtime stub).
    /// </summary>
    [CreateAssetMenu(menuName = "Project Zone/Contract Definition", fileName = "PZ_Contract_")]
    public sealed class PzContractDefinition : ScriptableObject
    {
        [SerializeField]
        private string _id = "contract_outskirts_scout";

        [SerializeField]
        private string _displayName = "Outskirts Scout";

        [SerializeField, TextArea]
        private string _description = "Leave Settlement, reach the abandoned checkpoint, and return to the contract board.";

        [SerializeField]
        private PzContractObjectiveType _objective = PzContractObjectiveType.ReachLocation;

        [SerializeField]
        private Vector3 _targetWorldPosition = new(45f, 1f, 0f);

        [SerializeField]
        private float _reachRadius = 8f;

        [SerializeField]
        private int _killTarget = 1;

        [SerializeField]
        private int _currencyReward = 50;

        [SerializeField]
        private int _reputationReward = 1;

        public string Id => _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public PzContractObjectiveType Objective => _objective;
        public Vector3 TargetWorldPosition => _targetWorldPosition;
        public float ReachRadius => _reachRadius;
        public int KillTarget => _killTarget;
        public int CurrencyReward => _currencyReward;
        public int ReputationReward => _reputationReward;

        public static PzContractDefinition CreateRuntimeScout()
        {
            var def = CreateInstance<PzContractDefinition>();
            def._id = "contract_outskirts_scout";
            def._displayName = "Outskirts Scout";
            def._description =
                "Leave Settlement, reach the abandoned checkpoint (east), then return and turn in at this board.";
            def._objective = PzContractObjectiveType.ReachLocation;
            def._targetWorldPosition = new Vector3(45f, 1f, 0f);
            def._reachRadius = 8f;
            def._currencyReward = 50;
            def._reputationReward = 1;
            return def;
        }

        public static PzContractDefinition CreateRuntimeRecoverCache()
        {
            var def = CreateInstance<PzContractDefinition>();
            def._id = "contract_recover_cache";
            def._displayName = "Recover Lost Gear";
            def._description = "Die outside the safe zone to create a death cache, then recover it and return.";
            def._objective = PzContractObjectiveType.RecoverDeathCache;
            def._currencyReward = 25;
            def._reputationReward = 1;
            return def;
        }
    }
}
