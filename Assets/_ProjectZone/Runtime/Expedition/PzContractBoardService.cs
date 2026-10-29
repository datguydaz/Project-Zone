using System;
using System.IO;
using PolymindGames;
using PolymindGames.SaveSystem;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Milestone 5 / #30–#31 — accept / track / complete repeatable contracts + grant rewards.
    /// </summary>
    [DefaultExecutionOrder(-53)]
    [DisallowMultipleComponent]
    public sealed class PzContractBoardService : MonoBehaviour
    {
        private const string FileName = "pz_contracts.sav";

        [SerializeField]
        private PzContractDefinition[] _availableContracts;

        private static PzContractBoardService _instance;
        private PzContractDefinition _active;
        private bool _objectiveComplete;
        private int _killProgress;
        private int _reputation;
        private bool _recoveredCacheThisLife;

        public static PzContractBoardService Instance => _instance;
        public PzContractDefinition ActiveContract => _active;
        public bool ObjectiveComplete => _objectiveComplete;
        public int Reputation => _reputation;
        public int KillProgress => _killProgress;

        public static string SavePath =>
            Path.Combine(Application.persistentDataPath, "ProjectZone", FileName);

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureDefaultContracts();
            LoadFromDisk();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                SaveToDisk();
                _instance = null;
            }
        }

        private void Update()
        {
            if (_active == null || _objectiveComplete || Player.AllPlayers.Count == 0)
                return;

            Player player = Player.AllPlayers[0];
            switch (_active.Objective)
            {
                case PzContractObjectiveType.ReachLocation:
                    if (Vector3.Distance(player.transform.position, _active.TargetWorldPosition) <=
                        _active.ReachRadius)
                    {
                        _objectiveComplete = true;
                        Debug.Log($"[ProjectZone][Contracts] Objective complete: reached {_active.DisplayName} target.", this);
                    }
                    break;

                case PzContractObjectiveType.RecoverDeathCache:
                    if (_recoveredCacheThisLife &&
                        (PzSafeZoneTracker.Instance == null || PzSafeZoneTracker.Instance.IsInSafeZone))
                    {
                        _objectiveComplete = true;
                        Debug.Log("[ProjectZone][Contracts] Objective complete: recovered death cache and returned.", this);
                    }
                    break;
            }
        }

        public string GetBoardStatusText()
        {
            if (_active == null)
                return $"No active contract. Reputation {_reputation}. Interact to accept Scout.";

            string state = _objectiveComplete ? "READY TO TURN IN" : "IN PROGRESS";
            return $"{_active.DisplayName} [{state}]\n{_active.Description}\nReward: {_active.CurrencyReward} credits, +{_active.ReputationReward} rep.";
        }

        public void Interact(ICharacter character)
        {
            if (_active != null && _objectiveComplete)
            {
                CompleteAndReward();
                return;
            }

            if (_active != null)
            {
                Debug.Log($"[ProjectZone][Contracts] Active: {_active.DisplayName} — {_objectiveComplete}", this);
                return;
            }

            Accept(_availableContracts != null && _availableContracts.Length > 0
                ? _availableContracts[0]
                : PzContractDefinition.CreateRuntimeScout());
        }

        public void Accept(PzContractDefinition definition)
        {
            if (definition == null)
                return;

            _active = definition;
            _objectiveComplete = false;
            _killProgress = 0;
            _recoveredCacheThisLife = false;
            SaveToDisk();
            Debug.Log($"[ProjectZone][Contracts] Accepted: {_active.DisplayName}", this);
        }

        public void NotifyKill()
        {
            if (_active == null || _active.Objective != PzContractObjectiveType.KillCount)
                return;

            _killProgress++;
            if (_killProgress >= _active.KillTarget)
            {
                _objectiveComplete = true;
                Debug.Log("[ProjectZone][Contracts] Kill objective complete.", this);
            }
        }

        public void NotifyDeathCacheRecovered()
        {
            _recoveredCacheThisLife = true;
        }

        private void CompleteAndReward()
        {
            if (_active == null)
                return;

            int credits = _active.CurrencyReward;
            int rep = _active.ReputationReward;
            string name = _active.DisplayName;

            PzWallet.Instance?.Add(credits, $"contract:{_active.Id}");
            _reputation += rep;

            Debug.Log(
                $"[ProjectZone][Contracts] Completed '{name}' → +{credits} credits, +{rep} rep (total rep {_reputation}).",
                this);

            _active = null;
            _objectiveComplete = false;
            SaveToDisk();

            // Repeatable: immediately offer again next interact.
        }

        private void EnsureDefaultContracts()
        {
            if (_availableContracts != null && _availableContracts.Length > 0)
                return;

            _availableContracts = new[]
            {
                PzContractDefinition.CreateRuntimeScout(),
                PzContractDefinition.CreateRuntimeRecoverCache()
            };
        }

        public void SaveToDisk()
        {
            string directory = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            SaveLoadManager.SaveToFile(new ContractSaveData
            {
                ActiveId = _active != null ? _active.Id : null,
                ObjectiveComplete = _objectiveComplete,
                KillProgress = _killProgress,
                Reputation = _reputation,
                SavedUtc = DateTime.UtcNow.ToString("o")
            }, SavePath);
        }

        public void LoadFromDisk()
        {
            if (!File.Exists(SavePath))
                return;

            var data = SaveLoadManager.LoadFromFile<ContractSaveData>(SavePath);
            if (data == null)
                return;

            _reputation = data.Reputation;
            _killProgress = data.KillProgress;
            _objectiveComplete = data.ObjectiveComplete;

            if (!string.IsNullOrEmpty(data.ActiveId) && _availableContracts != null)
            {
                for (int i = 0; i < _availableContracts.Length; i++)
                {
                    if (_availableContracts[i] != null && _availableContracts[i].Id == data.ActiveId)
                    {
                        _active = _availableContracts[i];
                        break;
                    }
                }
            }
        }

        [Serializable]
        private sealed class ContractSaveData
        {
            public string ActiveId;
            public bool ObjectiveComplete;
            public int KillProgress;
            public int Reputation;
            public string SavedUtc;
        }
    }
}
