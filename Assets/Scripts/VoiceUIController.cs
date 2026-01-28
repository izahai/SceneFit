using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public sealed class VoiceUIController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TranscriptionClient transcriptionClient;
    [SerializeField] private Button recordButton;

    [Header("Output")]
    [Tooltip("Optional: TextMeshPro text output.")]
    [SerializeField] private TextMeshProUGUI transcriptTMP;

    [Header("Button Labels")]
    [SerializeField] private string startLabel = "Record";
    [SerializeField] private string stopLabel = "Stop";

    [Header("Behavior")]
    [SerializeField] private bool clearOnStart = true;
    [SerializeField] private bool appendFinalWithNewline = true;

    private string _finalBuffer = "";

    private void Awake()
    {
        if (recordButton != null)
        {
            recordButton.onClick.AddListener(ToggleRecording);
        }

        if (transcriptionClient != null)
        {
            transcriptionClient.TranscriptReceived += OnTranscript;
            transcriptionClient.Error += OnError;
        }

        SetButtonLabel(startLabel);
    }

    private void OnDestroy()
    {
        if (recordButton != null)
        {
            recordButton.onClick.RemoveListener(ToggleRecording);
        }

        if (transcriptionClient != null)
        {
            transcriptionClient.TranscriptReceived -= OnTranscript;
            transcriptionClient.Error -= OnError;
        }
    }

    public void ToggleRecording()
    {
        if (transcriptionClient == null)
            return;

        if (!transcriptionClient.IsRecording)
        {
            if (clearOnStart)
            {
                _finalBuffer = "";
                SetText(string.Empty);
            }

            transcriptionClient.StartRecording();
            SetButtonLabel(stopLabel);
        }
        else
        {
            transcriptionClient.StopAndTranscribe();
            SetButtonLabel(startLabel);
        }
    }

    private void OnTranscript(string text)
    {
        Debug.Log($"[VoiceUI] OnTranscript called: [{text}]"); // added
        // Non-streaming: treat server response as the final transcript.
        if (appendFinalWithNewline && !string.IsNullOrEmpty(_finalBuffer))
        {
            _finalBuffer += "\n";
        }

        _finalBuffer += text;
        SetText(_finalBuffer);
        SetButtonLabel(startLabel);
    }

    private void OnError(string message)
    {
        // Keep it visible in the UI to speed up iteration.
        SetText($"[Error] {message}");
        SetButtonLabel(startLabel);
    }

    private void SetText(string message)
    {
        if (transcriptTMP != null)
        {
            transcriptTMP.text = message;
            return;
        }

    }

    private void SetButtonLabel(string label)
    {
        if (recordButton == null)
            return;

        // If the button has a TMP label, use it; else fallback to legacy Text.
        var tmp = recordButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null)
        {
            tmp.text = label;
            return;
        }

        var txt = recordButton.GetComponentInChildren<Text>(true);
        if (txt != null)
        {
            txt.text = label;
        }
    }
}
