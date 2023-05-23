﻿using System.Reactive.Linq;
using System.Reactive.Subjects;
using NAudio.Wave;
using MMDeviceEnumerator = NAudio.CoreAudioApi.MMDeviceEnumerator;

namespace Quietrum;

/// <summary>
/// マイク
/// </summary>
public class Microphone : IMicrophone
{
    /// <summary>
    /// インスタンスを生成する。
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="systemName"></param>
    /// <param name="deviceNumber"></param>
    /// <param name="status"></param>
    public Microphone(MicrophoneId id, string name, string systemName, int deviceNumber, MicrophoneStatus status)
    {
        Id = id;
        DeviceNumber = new DeviceNumber(deviceNumber);
        Name = name;
        SystemName = systemName;
        Status = status;
    }

    /// <summary>
    /// ID
    /// </summary>
    public MicrophoneId Id { get; }
    /// <summary>
    /// デバイス番号
    /// </summary>
    public DeviceNumber DeviceNumber { get; }
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Windows上の名称
    /// </summary>
    public string SystemName { get; }
    /// <summary>
    /// 状態
    /// </summary>
    public MicrophoneStatus Status { get; }
    /// <summary>
    /// 入力レベル
    /// </summary>
    public VolumeLevel VolumeLevel
    {
        get
        {
            using var enumerator = new MMDeviceEnumerator();
            using var mmDevice = enumerator.GetDevice(Id.AsPrimitive());
            return (VolumeLevel)mmDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
        }
        set
        {
            using var enumerator = new MMDeviceEnumerator();
            using var mmDevice = enumerator.GetDevice(Id.AsPrimitive());
            mmDevice.AudioEndpointVolume.MasterVolumeLevelScalar = (float)value;
        }
    }

    public IObservable<byte[]> StartRecording(WaveFormat waveFormat, TimeSpan bufferSpan, CancellationToken cancellationToken)
    {
        var subject = new Subject<byte[]>();
        var waveIn = new WaveInEvent()
        {
            DeviceNumber = DeviceNumber.AsPrimitive(),
            WaveFormat = waveFormat,
            BufferMilliseconds = (int)bufferSpan.TotalMilliseconds
        };

        waveIn.DataAvailable += (s, a) =>
        {
            var buffer = new byte[a.BytesRecorded];
            Buffer.BlockCopy(a.Buffer, 0, buffer, 0, a.BytesRecorded);
            subject.OnNext(buffer);
        };

        waveIn.RecordingStopped += (sender, e) =>
        {
            waveIn.Dispose();
            waveIn = null;
            subject.OnCompleted();
        };
        
        cancellationToken.Register(() =>
        {
            waveIn.StopRecording();
        });

        waveIn.StartRecording();
        return subject.AsObservable();
    }

    /// <summary>
    /// 文字列表現を取得する。
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Name;
}