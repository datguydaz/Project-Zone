using PolymindGames;
using PolymindGames.MovementSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectZone
{
    /// <summary>
    /// DEBUG: F1 toggles freefly noclip on the local HQFPS player (no gravity / no collision).
    /// WASD + mouse look · Space up · Ctrl/C down · Shift fast.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PzDebugNoclip : MonoBehaviour
    {
        private const string LogPrefix = "[ProjectZone][Debug]";

        [SerializeField]
        private Key _toggleKey = Key.F1;

        [SerializeField]
        private float _flySpeed = 14f;

        [SerializeField]
        private float _fastMultiplier = 3.5f;

        [SerializeField]
        private bool _showOnScreenHint = true;

        private bool _noclip;
        private Player _player;
        private CharacterControllerMotor _motor;
        private PlayerMovementController _movement;
        private CharacterController _controller;
        private Transform _look;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindAnyObjectByType<PzDebugNoclip>() != null)
                return;

            var go = new GameObject("PZ_DebugNoclip");
            DontDestroyOnLoad(go);
            go.AddComponent<PzDebugNoclip>();
        }

        private void Update()
        {
            if (!TryEnsurePlayer())
                return;

            if (WasTogglePressed())
                SetNoclip(!_noclip);

            if (_noclip)
                Fly();
        }

        private void OnGUI()
        {
            if (!_showOnScreenHint || !_noclip)
                return;

            const int w = 280;
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.Box(new Rect(12, 12, w, 54), GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(new Rect(20, 18, w - 16, 40), "NOCLIP (F1) — WASD fly · Space/Ctrl up/down · Shift fast");
        }

        private void OnDestroy()
        {
            if (_noclip)
                SetNoclip(false);
        }

        private bool TryEnsurePlayer()
        {
            if (_player != null && _motor != null)
                return true;

            if (Player.AllPlayers == null || Player.AllPlayers.Count == 0)
                return false;

            _player = Player.AllPlayers[0];
            if (_player == null)
                return false;

            _motor = _player.GetComponent<CharacterControllerMotor>();
            _movement = _player.GetComponent<PlayerMovementController>();
            _controller = _player.GetComponent<CharacterController>();
            _look = ResolveLookTransform(_player);
            return _motor != null || _controller != null;
        }

        private static Transform ResolveLookTransform(Player player)
        {
            Camera cam = player.GetComponentInChildren<Camera>();
            if (cam != null)
                return cam.transform;
            if (Camera.main != null)
                return Camera.main.transform;
            return player.transform;
        }

        private bool WasTogglePressed()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null)
                return false;
            return kb[_toggleKey].wasPressedThisFrame;
        }

        private void SetNoclip(bool enabled)
        {
            if (_noclip == enabled)
                return;

            _noclip = enabled;

            if (_movement != null)
                _movement.enabled = !enabled;
            if (_motor != null)
                _motor.enabled = !enabled;
            else if (_controller != null)
                _controller.enabled = !enabled;

            // Motor OnDisable turns CharacterController off; ensure stay off while noclipping.
            if (_controller != null)
                _controller.enabled = !enabled;

            Debug.Log($"{LogPrefix} Noclip {(enabled ? "ON" : "OFF")} (F1).", this);
        }

        private void Fly()
        {
            if (_player == null)
                return;

            Keyboard kb = Keyboard.current;
            if (kb == null)
                return;

            Transform basis = _look != null ? _look : _player.transform;
            Vector3 move = Vector3.zero;

            if (kb.wKey.isPressed) move += basis.forward;
            if (kb.sKey.isPressed) move -= basis.forward;
            if (kb.dKey.isPressed) move += basis.right;
            if (kb.aKey.isPressed) move -= basis.right;
            if (kb.spaceKey.isPressed) move += Vector3.up;
            if (kb.leftCtrlKey.isPressed || kb.cKey.isPressed)
                move += Vector3.down;

            if (move.sqrMagnitude > 0.0001f)
                move.Normalize();

            float speed = _flySpeed;
            if (kb.leftShiftKey.isPressed)
                speed *= _fastMultiplier;

            _player.transform.position += move * (speed * Time.deltaTime);
        }
    }
}
