using System;
using System.Collections;
using System.Collections.Generic;
using MoonetixOfferwall;
using UnityEngine;
using UnityEngine.Networking;

namespace MoonetixOfferwall.Samples
{
    /// <summary>
    /// Demo screen: gets a session token from YOUR server and opens the Moonetix offerwall with it.
    /// Your server endpoint calls POST https://api.moonetix.com/v1/session with your API key and returns
    /// JSON like {"token":"sst_..."}. Never put the API key in the game.
    /// </summary>
    public class MoonetixOfferwallDemo : MonoBehaviour
    {
        [Tooltip("Your Moonetix app ID (24 characters) from the Integration tab of your app.")]
        [SerializeField] private string appId = "";

        [Tooltip("An endpoint on YOUR server that creates a Moonetix session for the signed-in user and returns {\"token\":\"sst_...\"}.")]
        [SerializeField] private string tokenUrl = "";

        [Tooltip("Optional title for the top bar. Empty uses the title from your Design tab.")]
        [SerializeField] private string title = "";

        private readonly List<string> _log = new List<string>();
        private string _pastedToken = "";
        private bool _busy;

        [Serializable]
        private class TokenResponse
        {
            public string token = "";
        }

        private void Awake()
        {
            Moonetix.Init(appId);
        }

        private void OnEnable()
        {
            Moonetix.OnOpened += HandleOpened;
            Moonetix.OnClosed += HandleClosed;
            Moonetix.OnError += HandleError;
        }

        private void OnDisable()
        {
            Moonetix.OnOpened -= HandleOpened;
            Moonetix.OnClosed -= HandleClosed;
            Moonetix.OnError -= HandleError;
        }

        private void HandleOpened()
        {
            Log("Offerwall opened");
        }

        private void HandleClosed()
        {
            Log("Offerwall closed. Load the balance from your server here.");
        }

        private void HandleError(MoonetixError error)
        {
            Log("Error " + error.CodeName + ": " + error.Message);
        }

        /// <summary>Opens the offerwall with a token from your server. Call it from your own Earn button.</summary>
        public void OpenOfferwall()
        {
            if (!_busy)
            {
                StartCoroutine(FetchTokenAndOpen());
            }
        }

        private IEnumerator FetchTokenAndOpen()
        {
            if (string.IsNullOrEmpty(tokenUrl))
            {
                Log("Set Token Url on this component to your server endpoint, or paste a token below.");
                yield break;
            }
            _busy = true;
            var form = new WWWForm();
            form.AddField("user_id", SystemInfo.deviceUniqueIdentifier);
            using (UnityWebRequest request = UnityWebRequest.Post(tokenUrl, form))
            {
                request.timeout = 15;
                yield return request.SendWebRequest();
                _busy = false;
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Log("Your server did not return a token: " + request.error);
                    yield break;
                }
                TokenResponse response = null;
                try
                {
                    response = JsonUtility.FromJson<TokenResponse>(request.downloadHandler.text);
                }
                catch (ArgumentException)
                {
                }
                if (response == null || string.IsNullOrEmpty(response.token))
                {
                    Log("Your server answered without a \"token\" field.");
                    yield break;
                }
                Moonetix.Open(response.token, Options());
            }
        }

        private MoonetixOptions Options()
        {
            return new MoonetixOptions(title: string.IsNullOrEmpty(title) ? null : title);
        }

        private void Log(string line)
        {
            _log.Insert(0, DateTime.Now.ToString("HH:mm:ss") + "  " + line);
            if (_log.Count > 12)
            {
                _log.RemoveAt(_log.Count - 1);
            }
            Debug.Log("[Moonetix demo] " + line);
        }

        private void OnGUI()
        {
            float scale = Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height) / 480f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale - 32f;
            Rect safe = Screen.safeArea;
            GUILayout.BeginArea(new Rect(16f, 16f + (Screen.height - safe.yMax) / scale, width, Screen.height / scale - 32f));
            GUILayout.Label("Moonetix Offerwall " + Moonetix.Version + (Moonetix.IsSupported ? string.Empty : " (Editor: the offerwall opens on Android and iOS)"));
            GUI.enabled = !_busy;
            if (GUILayout.Button(_busy ? "Getting a token..." : "Earn coins", GUILayout.Height(48f)))
            {
                OpenOfferwall();
            }
            GUI.enabled = true;
            GUILayout.Space(12f);
            GUILayout.Label("Or paste a token from your server (it works once, for 10 minutes):");
            _pastedToken = GUILayout.TextField(_pastedToken, GUILayout.Height(36f));
            if (GUILayout.Button("Open with this token", GUILayout.Height(40f)))
            {
                Moonetix.Open(_pastedToken, Options());
            }
            GUILayout.Space(12f);
            foreach (string line in _log)
            {
                GUILayout.Label(line);
            }
            GUILayout.EndArea();
        }
    }
}
