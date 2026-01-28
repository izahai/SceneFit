using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[DisallowMultipleComponent]
public sealed class TranscriptionClient : MonoBehaviour
{
    [Header("HTTP")]
    [Tooltip("Endpoint URL")]
    [InspectorName("URL")]
    [SerializeField] private string url;

    [Header("Mic capture")]
    [Tooltip("Leave empty to use the default microphone.")]
    [SerializeField] private string microphoneDevice = "";
    [SerializeField] private int sampleRateHz = 16000;
    [SerializeField] private int maxRecordSeconds = 15;

    [Header("Debug")]
    [SerializeField] private bool logRequests = false;

    public bool IsRecording { get; private set; }

    public event Action<string> TranscriptReceived;
    public event Action<string> Error;

    private AudioClip _clip;


    public void StartRecording()
    {
        if (IsRecording)
            return;

        string device = string.IsNullOrEmpty(microphoneDevice) ? null : microphoneDevice;
        _clip = Microphone.Start(device, false, maxRecordSeconds, sampleRateHz);
        IsRecording = true;
    }

    public void StopAndTranscribe()
    {
        if (!IsRecording)
            return;

        string device = string.IsNullOrEmpty(microphoneDevice) ? null : microphoneDevice;
        int position = Microphone.GetPosition(device);
        Microphone.End(device);
        IsRecording = false;

        if (_clip == null)
        {
            Error?.Invoke("No AudioClip recorded.");
            return;
        }

        if (position <= 0)
        {
            Error?.Invoke("No microphone samples captured.");
            return;
        }

        // Extract recorded samples (float [-1..1])
        float[] samples = new float[position * _clip.channels];
        _clip.GetData(samples, 0);

        // Convert to WAV (PCM16, mono)
        byte[] wav = WavEncodePcm16Mono(samples, _clip.channels, _clip.frequency);
        StartCoroutine(PostWavCoroutine(wav));
    }

    private IEnumerator PostWavCoroutine(byte[] wavBytes)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("audio", wavBytes, "recording.wav", "audio/wav");

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            request.timeout = 300;

            if (logRequests)
                Debug.Log($"[ASR] POST {url} ({wavBytes.Length} bytes)");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Error?.Invoke($"HTTP error: {request.error}");
                yield break;
            }

            string json = request.downloadHandler.text ?? string.Empty;
            if (logRequests)
                Debug.Log($"[ASR] Response: {json}");

            // Defensive: trim BOM / invisible chars and surrounding whitespace
            json = json.Trim();
            json = json.Trim('\uFEFF', '\u200B');

            TranscriptResponse resp = null;
            string transcriptText = null;

            // Try to parse JSON normally but be tolerant of extra characters
            try
            {
                int start = json.IndexOf('{');
                int end = json.LastIndexOf('}');
                if (start >= 0 && end > start)
                {
                    string obj = json.Substring(start, end - start + 1);
                    resp = JsonUtility.FromJson<TranscriptResponse>(obj);
                }
                else
                {
                    resp = JsonUtility.FromJson<TranscriptResponse>(json);
                }
            }
            catch (Exception ex)
            {
                if (logRequests)
                    Debug.LogWarning($"[ASR] JsonUtility parse failed: {ex}");
            }

            // If parsing failed or transcript empty, try a simple manual extraction
            if (resp == null || string.IsNullOrEmpty(resp.transcript))
            {
                const string key = "\"transcript\"";
                int idx = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    int colon = json.IndexOf(':', idx + key.Length);
                    if (colon >= 0)
                    {
                        int q1 = json.IndexOf('"', colon);
                        if (q1 >= 0)
                        {
                            int q2 = json.IndexOf('"', q1 + 1);
                            // handle escaped quotes
                            while (q2 > q1 && q2 + 1 < json.Length && json[q2 - 1] == '\\')
                            {
                                q2 = json.IndexOf('"', q2 + 1);
                                if (q2 < 0) break;
                            }
                            if (q2 > q1)
                            {
                                transcriptText = json.Substring(q1 + 1, q2 - q1 - 1);
                            }
                        }
                    }
                }

                // final fallback: use entire response trimmed (and strip surrounding quotes)
                if (string.IsNullOrEmpty(transcriptText))
                {
                    transcriptText = json.Trim().Trim('"');
                }
            }
            else
            {
                transcriptText = resp.transcript;
            }

            // Unescape common escapes and normalize whitespace
            if (!string.IsNullOrEmpty(transcriptText))
            {
                transcriptText = transcriptText.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\\"", "\"");
                transcriptText = transcriptText.Trim();
            }

            if (logRequests)
                Debug.Log($"[ASR] Parsed transcript: [{transcriptText}]");

            if (string.IsNullOrEmpty(transcriptText))
            {
                Error?.Invoke("Empty transcript response.");
                yield break;
            }

            TranscriptReceived?.Invoke(transcriptText);
        }
    }

    private static byte[] WavEncodePcm16Mono(float[] interleavedSamples, int channels, int sampleRate)
    {
        // Convert to mono by averaging channels if needed.
        float[] mono;
        if (channels <= 1)
        {
            mono = interleavedSamples;
        }
        else
        {
            int frames = interleavedSamples.Length / channels;
            mono = new float[frames];
            for (int f = 0; f < frames; f++)
            {
                float sum = 0f;
                int baseIndex = f * channels;
                for (int c = 0; c < channels; c++)
                {
                    sum += interleavedSamples[baseIndex + c];
                }
                mono[f] = sum / channels;
            }
        }

        int sampleCount = mono.Length;
        int bytesPerSample = 2;
        int dataSize = sampleCount * bytesPerSample;

        using (var ms = new MemoryStream(44 + dataSize))
        using (var bw = new BinaryWriter(ms))
        {
            // RIFF header
            bw.Write(Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(36 + dataSize);
            bw.Write(Encoding.ASCII.GetBytes("WAVE"));

            // fmt chunk
            bw.Write(Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16); // PCM
            bw.Write((short)1); // AudioFormat PCM
            bw.Write((short)1); // NumChannels = 1
            bw.Write(sampleRate);
            bw.Write(sampleRate * bytesPerSample); // ByteRate
            bw.Write((short)bytesPerSample); // BlockAlign
            bw.Write((short)16); // BitsPerSample

            // data chunk
            bw.Write(Encoding.ASCII.GetBytes("data"));
            bw.Write(dataSize);

            // Samples
            for (int i = 0; i < sampleCount; i++)
            {
                short s = (short)Mathf.Clamp(mono[i] * 32767f, short.MinValue, short.MaxValue);
                bw.Write(s);
            }

            bw.Flush();
            return ms.ToArray();
        }
    }
}
