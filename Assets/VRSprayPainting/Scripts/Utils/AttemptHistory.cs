using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Every assessment, saved as JSON on the device (Application.persistentDataPath/attempts.json), with a CSV
    /// export for instructors. On Quest the files are under Android/data/&lt;package&gt;/files.
    /// </summary>
    public static class AttemptHistory
    {
        private const string FileName = "attempts.json";
        private const string CsvName = "attempts.csv";
        private const int MaxAttempts = 2000;

        private static AttemptHistoryData data;

        public static IReadOnlyList<AttemptRecord> Attempts => Data.attempts;

        public static event Action Changed;

        private static string JsonPath => Path.Combine(Application.persistentDataPath, FileName);

        private static AttemptHistoryData Data
        {
            get
            {
                if (data != null)
                    return data;
                try
                {
                    data = File.Exists(JsonPath) ? JsonUtility.FromJson<AttemptHistoryData>(File.ReadAllText(JsonPath)) : null;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[AttemptHistory] Could not read history: {e.Message}");
                }
                data ??= new AttemptHistoryData();
                return data;
            }
        }

        public static void Record(AssessmentResult result, string activity, string sprayer)
        {
            if (result == null || !result.hasData)
                return;

            var defects = new StringBuilder();
            foreach (DefectScore d in result.defects)
                if (d.points >= 0.5f)
                    defects.Append(defects.Length > 0 ? "; " : "").Append($"{d.label} -{Mathf.RoundToInt(d.points)}");
            var skills = new StringBuilder();
            foreach (SkillScore s in result.skills)
                skills.Append(skills.Length > 0 ? "; " : "").Append($"{s.label} {Mathf.RoundToInt(s.score * 100f)}");

            Data.attempts.Add(new AttemptRecord
            {
                timestamp = DateTime.Now.ToString("s", CultureInfo.InvariantCulture),
                activity = activity,
                sprayer = sprayer,
                score = Mathf.RoundToInt(result.overall * 100f),
                technique = Mathf.RoundToInt(result.techniqueScore * 100f),
                passMark = Mathf.RoundToInt(result.passMark * 100f),
                passed = result.passed,
                coats = result.coatScores.Count,
                averageFilm = Mathf.RoundToInt(result.coat.averageFilm),
                onTargetPercent = Mathf.RoundToInt(result.coat.inTargetFraction * 100f),
                defects = defects.ToString(),
                skills = skills.ToString(),
            });
            if (Data.attempts.Count > MaxAttempts)
                Data.attempts.RemoveRange(0, Data.attempts.Count - MaxAttempts);
            Save();
        }

        /// <summary>Writes attempts.csv next to the JSON and returns its full path (null on failure).</summary>
        public static string ExportCsv()
        {
            var csv = new StringBuilder("Date,Activity,Sprayer,Score,Technique,Pass mark,Passed,Coats,Avg film (um),On target %,Defects,Skills\n");
            foreach (AttemptRecord a in Data.attempts)
                csv.Append(string.Join(",", Quote(a.timestamp), Quote(a.activity), Quote(a.sprayer), a.score, a.technique, a.passMark,
                    a.passed ? "yes" : "no", a.coats, a.averageFilm, a.onTargetPercent, Quote(a.defects), Quote(a.skills))).Append('\n');

            string path = Path.Combine(Application.persistentDataPath, CsvName);
            try
            {
                File.WriteAllText(path, csv.ToString());
                return path;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AttemptHistory] CSV export failed: {e.Message}");
                return null;
            }
        }

        public static void Clear()
        {
            Data.attempts.Clear();
            Save();
        }

        private static void Save()
        {
            try
            {
                File.WriteAllText(JsonPath, JsonUtility.ToJson(Data));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AttemptHistory] Could not save history: {e.Message}");
            }
            Changed?.Invoke();
        }

        private static string Quote(string value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
    }
}
