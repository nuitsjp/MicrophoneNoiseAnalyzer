﻿using NAudio.CoreAudioApi;

namespace MicrophoneNoiseAnalyzer.Domain;

public class Microphone : IMicrophone
{
    private static readonly TimeSpan SamplingRate = TimeSpan.FromMilliseconds(50);

    private readonly MMDevice _mmDevice;

    private readonly WasapiCapture _capture;

    private readonly List<float> _buffer = new();

    private readonly Timer _timer;

    public Microphone(MMDevice mmDevice)
    {
        _mmDevice = mmDevice;
        _capture = new WasapiCapture(_mmDevice);
        _timer = new Timer(OnElapsed, null, Timeout.InfiniteTimeSpan, SamplingRate);
    }

    public void Dispose()
    {
        _mmDevice.DisposeQuiet();
        _capture.DisposeQuiet();
        _timer.DisposeQuiet();
    }


    public string Name => _mmDevice.FriendlyName;

    public float MasterVolumeLevelScalar
    {
        get => _mmDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
        set => _mmDevice.AudioEndpointVolume.MasterVolumeLevelScalar = value;
    }

    public float MasterPeakValue => _mmDevice.AudioMeterInformation.MasterPeakValue;
    public IReadOnlyList<float> Buffer => _buffer;

    public void Activate()
    {
        _capture.StartRecording();
    }

    public void StartRecording()
    {
        _buffer.Clear();
        _timer.Change(TimeSpan.Zero, SamplingRate);
    }

    private void OnElapsed(object? state)
    {
        _buffer.Add(MasterPeakValue);
    }

    public IMasterPeakValues StopRecording()
    {
        _timer.Change(Timeout.InfiniteTimeSpan, SamplingRate);
        return new MasterPeakValues(this, _buffer.ToList());
    }

    public void Deactivate()
    {
        _capture.StopRecording();
    }
}