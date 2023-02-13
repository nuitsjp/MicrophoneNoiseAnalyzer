﻿namespace MicrophoneLevelLogger;

public class RemoteMediaPlayer : IMediaPlayer
{
    private readonly HttpClient _httpClient = new();

    public async Task PlayLoopingAsync()
    {
        RecordingSettings settings = await RecordingSettings.LoadAsync();
        await _httpClient.GetAsync($"http://{settings.MediaPlayerHost}:5000/Player/Play");
    }

    public async Task StopAsync()
    {
        RecordingSettings settings = await RecordingSettings.LoadAsync();
        await _httpClient.GetAsync($"http://{settings.MediaPlayerHost}:5000/Player/Stop");
    }
}