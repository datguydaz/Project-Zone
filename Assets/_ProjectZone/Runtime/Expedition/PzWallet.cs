using System;
using System.IO;
using PolymindGames.SaveSystem;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Milestone 5 / #29 — expedition currency (profile-backed; HQ has no wallet).
    /// </summary>
    [DefaultExecutionOrder(-55)]
    [DisallowMultipleComponent]
    public sealed class PzWallet : MonoBehaviour
    {
        private const string FileName = "pz_wallet.sav";

        [SerializeField]
        private int _startingBalance = 0;

        [SerializeField]
        private bool _loadOnAwake = true;

        [SerializeField]
        private bool _saveOnDestroy = true;

        private static PzWallet _instance;
        private int _balance;

        public static PzWallet Instance => _instance;
        public int Balance => _balance;
        public event Action<int> BalanceChanged;

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
            _balance = _startingBalance;

            if (_loadOnAwake)
                LoadFromDisk();
        }

        private void OnDestroy()
        {
            if (_instance != this)
                return;

            if (_saveOnDestroy && Application.isPlaying)
                SaveToDisk();

            _instance = null;
        }

        public void Add(int amount, string reason = null)
        {
            if (amount == 0)
                return;

            _balance = Mathf.Max(0, _balance + amount);
            BalanceChanged?.Invoke(_balance);
            Debug.Log($"[ProjectZone][Wallet] {(amount >= 0 ? "+" : "")}{amount} → {_balance}" +
                      (string.IsNullOrEmpty(reason) ? string.Empty : $" ({reason})"), this);
            SaveToDisk();
        }

        public bool TrySpend(int amount, string reason = null)
        {
            if (amount <= 0)
                return true;
            if (_balance < amount)
                return false;

            Add(-amount, reason);
            return true;
        }

        public void SaveToDisk()
        {
            string directory = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            SaveLoadManager.SaveToFile(new WalletSaveData
            {
                Balance = _balance,
                SavedUtc = DateTime.UtcNow.ToString("o")
            }, SavePath);
        }

        public bool LoadFromDisk()
        {
            if (!File.Exists(SavePath))
                return false;

            var data = SaveLoadManager.LoadFromFile<WalletSaveData>(SavePath);
            if (data == null)
                return false;

            _balance = Mathf.Max(0, data.Balance);
            BalanceChanged?.Invoke(_balance);
            return true;
        }

        [Serializable]
        private sealed class WalletSaveData
        {
            public int Balance;
            public string SavedUtc;
        }
    }
}
