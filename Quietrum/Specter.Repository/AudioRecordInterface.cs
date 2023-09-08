﻿using System.Reactive.Disposables;
using System.Text.Json;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Specter.Repository;

public class AudioRecordInterface : IAudioRecordInterface, IDisposable
{
    private static readonly string RootDirectory = "Record";
    private static readonly string AudioRecordFile = "AudioRecord.json";

    private readonly CompositeDisposable _compositeDisposable = new();
    private readonly ReactiveCollection<AudioRecord> _audioRecords = new();
    public ReadOnlyReactiveCollection<AudioRecord> AudioRecords { get; }

    public AudioRecordInterface()
    {
        _audioRecords.AddTo(_compositeDisposable);
        
        AudioRecords = _audioRecords.ToReadOnlyReactiveCollection();
    }

    public bool Activated { get; private set; }

    public async Task ActivateAsync()
    {
        if (Activated) return;
        
        var records = await LoadAsync();
        foreach (var record in records)
        {
            _audioRecords.Add(record);
        }
        
        Activated = true;
    }

    public static string GetAudioRecordPath(AudioRecord audioRecord)
    {
        var targetDevice = audioRecord.DeviceRecords.Single(x => x.Id == audioRecord.TargetDeviceId);
        return Path.Combine(
            RootDirectory,
            $"{audioRecord.StartTime:yyyy.MM.dd-HH.mm.ss}_{targetDevice.Name}_{audioRecord.Direction}");
    }

    public async Task SaveAsync(AudioRecord audioRecord)
    {
        var filePath = Path.Combine(
            GetAudioRecordPath(audioRecord),
            AudioRecordFile);
        
        var directoryName = Path.GetDirectoryName(filePath)!;
        if (!Directory.Exists(directoryName))
            Directory.CreateDirectory(directoryName);
        
        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(stream, audioRecord, JsonEnvironments.Options);
        _audioRecords.Add(audioRecord);
    }

    public async Task<IEnumerable<AudioRecord>> LoadAsync()
    {
        var directories = Directory.GetDirectories(RootDirectory);

        List<AudioRecord> records = new();
        foreach (var directory in directories)
        {
            var file = Path.Combine(directory, AudioRecordFile);
            if (File.Exists(file) is false)
                continue;
            
            records.Add(await LoadAsync(file));
        }

        return records;
    }

    private async Task<AudioRecord> LoadAsync(string file)
    {
        await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
        return (await JsonSerializer.DeserializeAsync<AudioRecord>(stream, JsonEnvironments.Options))!;
    }

    public void Dispose()
    {
        _compositeDisposable.Dispose();
    }
}