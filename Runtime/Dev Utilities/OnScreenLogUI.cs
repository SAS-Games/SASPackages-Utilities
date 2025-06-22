using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SAS
{
    public class OnScreenLogUI : MonoBehaviour
    {
        [SerializeField] private GameObject m_LogEntryPrefab;
        [SerializeField] private Transform m_ContentParent;
        [SerializeField] private ScrollRect m_ScrollRect;
        [SerializeField] private Button m_ClearButton;
        [SerializeField] private Button m_ToggleButton;
        [SerializeField] private Slider m_LifetimeSlider;
        [SerializeField] private TextMeshProUGUI m_LifetimeLabel;

        private float logEntryLifetime = 0f;
        private readonly Queue<GameObject> pool = new();
        private readonly List<LogEntry> activeLogs = new();

        private class LogEntry
        {
            public GameObject GameObject;
            public TMP_Text Text;
            public float CreationTime;
        }

        private void Awake()
        {
            m_ClearButton.onClick.AddListener(ClearLogs);
            m_ToggleButton.onClick.AddListener(() => gameObject.SetActive(!gameObject.activeSelf));
            m_LifetimeSlider.onValueChanged.AddListener(SetLifetime);
            SetLifetime(m_LifetimeSlider.value);
            PrewarmPool();
        }

        private void Update()
        {
            float now = Time.realtimeSinceStartup;
            for (int i = activeLogs.Count - 1; i >= 0; i--)
            {
                float age = now - activeLogs[i].CreationTime;
                if (logEntryLifetime > 0 && age > logEntryLifetime)
                {
                    ReturnToPool(activeLogs[i]);
                    activeLogs.RemoveAt(i);
                }
                else if (logEntryLifetime > 0 && age > logEntryLifetime * 0.7f)
                {
                    float fadeRatio = 1f - (age - logEntryLifetime * 0.7f) / (logEntryLifetime * 0.3f);
                    var color = activeLogs[i].Text.color;
                    color.a = Mathf.Clamp01(fadeRatio);
                    activeLogs[i].Text.color = color;
                }
            }
        }

        public void AddLog(string message, LogLevel level, string tag = "")
        {
            var entryGO = GetFromPool();
            entryGO.transform.SetParent(m_ContentParent, false);
            TMP_Text tmp = entryGO.GetComponent<TMP_Text>();
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string tagDisplay = string.IsNullOrEmpty(tag) ? "" : $"[{tag}] ";
            Color baseColor = level switch
            {
                LogLevel.Warning => Color.yellow,
                LogLevel.Error => Color.red,
                _ => Color.white
            };
            tmp.text = $"[{timestamp}] {tagDisplay}{message}";
            tmp.color = baseColor;
            activeLogs.Add(new LogEntry { GameObject = entryGO, Text = tmp, CreationTime = Time.realtimeSinceStartup });
            m_ScrollRect.verticalNormalizedPosition = 0f;
        }

        private void ClearLogs()
        {
            foreach (var log in activeLogs)
                ReturnToPool(log);
            activeLogs.Clear();
        }

        private void SetLifetime(float value)
        {
            logEntryLifetime = value;
            if (m_LifetimeLabel != null)
                m_LifetimeLabel.text = logEntryLifetime > 0 ? $"{logEntryLifetime:F1}s" : "∞";
        }

        private void PrewarmPool()
        {
            for (int i = 0; i < 30; i++)
            {
                var obj = Instantiate(m_LogEntryPrefab);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        private GameObject GetFromPool()
        {
            if (pool.Count > 0)
            {
                var obj = pool.Dequeue();
                obj.SetActive(true);
                return obj;
            }

            return Instantiate(m_LogEntryPrefab);
        }

        private void ReturnToPool(LogEntry entry)
        {
            entry.GameObject.SetActive(false);
            entry.GameObject.transform.SetParent(transform);
            pool.Enqueue(entry.GameObject);
        }
    }
}