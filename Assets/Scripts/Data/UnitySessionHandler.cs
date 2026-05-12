using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using UnityEngine;
using Newtonsoft.Json;

namespace Data {
    public class UnitySessionHandler : IGotrueSessionPersistence<Session>
    {
        private const string SessionKey = "supabase_session";

        public void SaveSession(Session session)
        {
            if (session == null) return;
            string json = JsonConvert.SerializeObject(session);
            PlayerPrefs.SetString(SessionKey, json);
            PlayerPrefs.Save();
            Debug.Log("<color=yellow>Supabase Persistence: Session Saved!</color>");
        }

        public void DestroySession()
        {
            PlayerPrefs.DeleteKey(SessionKey);
            PlayerPrefs.Save();
            Debug.Log("<color=red>Supabase Persistence: Session Deleted!</color>");
        }

        public Session LoadSession()
        {
            if (PlayerPrefs.HasKey(SessionKey))
            {
                string json = PlayerPrefs.GetString(SessionKey);
                try
                {
                    Session session = JsonConvert.DeserializeObject<Session>(json);
                    Debug.Log("<color=cyan>Supabase Persistence: Session Loaded from Disk!</color>");
                    return session;
                }
                catch
                {
                    Debug.LogError("Supabase Persistence: Failed to deserialize session.");
                    return null;
                }
            }
            Debug.Log("<color=white>Supabase Persistence: No session found in PlayerPrefs.</color>");
            return null;
        }
    }
}