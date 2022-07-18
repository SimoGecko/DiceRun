// (c) Simone Guggiari 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

////////// PURPOSE: class that allows communication with a server to write and read to/from files, used to submit and retrieve highscores //////////

namespace sxg
{
    public class Highscores : MonoBehaviour
    {
        public event System.Action OnUpload, OnDownload;

        public int numLeadToDisplay = 10;
        public int maxUsernameLength = 16;
        public bool log = false;
        const string baseUrl = "https://simoneguggiari.altervista.org/gmtk22/"; 


        public ScoreEntry EDITOR_testEntry;
        [ReadOnly] [SerializeField] private ScoreEntry[] entries;

        [LayoutBeginHorizontal]
        [EditorButton]
        void EDITOR_UploadTest()
        {
            UploadEntry(EDITOR_testEntry.username, EDITOR_testEntry.score, EDITOR_testEntry.time);
        }
        [EditorButton]
        [LayoutEndHorizontal]
        void EDITOR_DownloadTest()
        {
            DownloadEntries();
        }

        public void UploadEntry(string username, int score, int time)
        {
            if (string.IsNullOrEmpty(username) || score < 0 || time < 0) return;
            username = Sanitize(username, maxUsernameLength);
            string timestamp = Utility.GetTimestampWeb(System.DateTime.Now);
            string uniqueDeviceID = SystemInfo.deviceUniqueIdentifier;
            uniqueDeviceID = "n/a";

            ScoreEntry entry = new ScoreEntry(timestamp, uniqueDeviceID, username, score, time);
            UploadEntry(entry);
        }

        void UploadEntry(ScoreEntry entry)
        {
            StartCoroutine(UploadRoutine("highscores.txt", entry.ToStream()));
        }

        public void DownloadEntries()
        {
            StartCoroutine(DownloadRoutine("highscores.txt"));
        }

        public ScoreEntry[] Entries { get { return entries; } }

        private ScoreEntry[] EntriesFromStream(string textStream)
        {
            string[] entries = textStream.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            List<ScoreEntry> result = new List<ScoreEntry>();

            for (int i = 0; i < entries.Length; i++)
            {
                ScoreEntry newEntry = new ScoreEntry();
                newEntry.FromStream(entries[i]);
                result.Add(newEntry);
            }
            return result.ToArray();
        }

        void SortEntries()
        {
            System.Array.Sort(entries, (e1, e2) => e2.score.CompareTo(e1.score));
        }

        bool RequestHadError(UnityWebRequest request)
        {
            //return !string.IsNullOrEmpty(request.error) || request.isHttpError || request.isNetworkError);
            return !string.IsNullOrEmpty(request.error)
             || request.result == UnityWebRequest.Result.ConnectionError
             || request.result == UnityWebRequest.Result.DataProcessingError
             || request.result == UnityWebRequest.Result.ProtocolError;
        }

        private IEnumerator UploadRoutine(string file, string data)
        {
            WWWForm form = new WWWForm();
            form.AddField("name", file);
            form.AddField("data", data);

            UnityWebRequest request = UnityWebRequest.Post(baseUrl + "write.php", form);
            yield return request.SendWebRequest();

            if (RequestHadError(request))
            {
                if (log) Debug.LogError("Upload Error: " + request.error);
            }
            else
            {
                if (log) Debug.Log("Upload successful");
                OnUpload?.Invoke();
            }
        }

        private IEnumerator DownloadRoutine(string file)
        {
            WWWForm form = new WWWForm();
            form.AddField("name", file);

            UnityWebRequest request = UnityWebRequest.Post(baseUrl + "read.php", form);
            yield return request.SendWebRequest();

            if (RequestHadError(request))
            {
                if (log) Debug.Log("Download Error: " + request.error);
            }
            else
            {
                if (log) Debug.Log("Download successful");
                string data = request.downloadHandler.text;
                entries = EntriesFromStream(data);
                SortEntries();
                OnDownload?.Invoke();
            }
        }

        #region QUERIES
        public string RankString()
        {
            return AggregateString(i => (i + 1) + ".");
        }
        public string UserString()
        {
            return AggregateString(i => entries[i].username);
        }
        public string ScoreString()
        {
            return AggregateString(i => entries[i].score.ToString());
        }
        public string TimeString()
        {
            return AggregateString(i => entries[i].TimeString());
        }
        private string AggregateString(System.Func<int, string> f)
        {
            string result = "";
            for (int i = 0; i < Mathf.Min(numLeadToDisplay, entries.Length); i++)
            {
                result += f(i) + '\n';
            }
            return result;
        }
        #endregion

        private string Sanitize(string username, int maxLength)
        {
            // TODO: SANITIZE, only [azAZ09.-]
            // TODO: remove bad words and spam
            if (username.Length > maxLength) return username.Substring(0, maxLength);
            return username;
        }

        private static Highscores instance;
        public static Highscores Instance
        {
            get
            {
                if (instance == null) instance = FindObjectOfType<Highscores>();
                return instance;
            }
        }
    }

    // Modify this to specify which data you would like to store
    // Only works with classes the deserialize
    [System.Serializable]
    public class ScoreEntry
    {
        public string timestamp;
        public string id;
        public string username;
        public int score;
        public int time;

        public ScoreEntry(string timestamp, string id, string username, int score, int time)
        {
            this.timestamp = timestamp;
            this.id = id;
            this.username = username;
            this.score = score;
            this.time = time;
        }

        public ScoreEntry() { }

        void CopyFrom(ScoreEntry other)
        {
            this.timestamp = other.timestamp;
            this.id = other.id;
            this.username = other.username;
            this.score = other.score;
            this.time = other.time;
        }

        public string ToStream()
        {
            return Utility.Serialize<ScoreEntry>(this) + '\n';
        }
        public bool FromStream(string stream)
        {
            ScoreEntry result = Utility.Deserialize<ScoreEntry>(stream);
            CopyFrom(result);
            return true; // success
        }
        
        public string TimeString()
        {
            return ScoreInterfaceManager.GetFormattedTime(time);
        }

    }

}