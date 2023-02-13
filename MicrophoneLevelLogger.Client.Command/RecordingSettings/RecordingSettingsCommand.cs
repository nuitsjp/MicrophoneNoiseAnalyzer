﻿namespace MicrophoneLevelLogger.Client.Command.RecordingSettings;

public class RecordingSettingsCommand : ICommand
{
    private IRecordingSettingsView _view;

    public RecordingSettingsCommand(IRecordingSettingsView view)
    {
        _view = view;
    }

    public string Name => "Recoding Settings    : 録音設定を確認・変更する。";

    public async Task ExecuteAsync()
    {
        var settings = await MicrophoneLevelLogger.RecordingSettings.LoadAsync();
        _view.ShowSettings(settings);

        if (_view.ConfirmModify())
        {
            var recordingSpan = _view.InputRecodingSpan();
            var isEnableRemoteRecording = _view.ConfirmEnableRemoteRecording();
            var recorderHost = isEnableRemoteRecording
                ? _view.InputRecorderHost()
                : "localhost";
            var isEnableRemotePlaying = _view.ConfirmEnableRemotePlaying();
            var mediaPlayerHost = isEnableRemotePlaying
                ? _view.InputMediaPlayerHost()
                : "localhost";

            await MicrophoneLevelLogger.RecordingSettings.SaveAsync(
                new MicrophoneLevelLogger.RecordingSettings(
                    mediaPlayerHost,
                    recorderHost,
                    TimeSpan.FromSeconds(recordingSpan),
                    isEnableRemotePlaying,
                    isEnableRemoteRecording));

            _view.ShowSettings(await MicrophoneLevelLogger.RecordingSettings.LoadAsync());
        }
    }
}