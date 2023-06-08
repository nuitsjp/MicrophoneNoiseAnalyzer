﻿using System.ComponentModel;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Quietrum;

public class RemoteDevice : ObservableObject, IDevice
{
    private readonly TcpClient _tcpClient;

    public RemoteDevice(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
        Id = new DeviceId(_tcpClient.Client.RemoteEndPoint!.ToString()!);
        SystemName = Id.AsPrimitive();
        Name = SystemName;
        VolumeLevel = new VolumeLevel(1f);
    }

    public void Dispose()
    {
        _tcpClient.Close();
        _tcpClient.Dispose();
    }

    public DeviceId Id { get; }
    public DataFlow DataFlow => DataFlow.Capture;
    public string Name { get; set; }
    public string SystemName { get; }
    public bool Measure { get; set; }
    public VolumeLevel VolumeLevel { get; set; }
    public IObservable<WaveInEventArgs> StartRecording(WaveFormat waveFormat, TimeSpan bufferSpan)
    {
        throw new NotImplementedException();
    }

    public void StopRecording()
    {
        throw new NotImplementedException();
    }
}

public class RemoteDeviceConnector
{
    private readonly IObservable<WaveInEventArgs> _source;
    private readonly TcpClient _tcpClient;
    private readonly string _address;

    public RemoteDeviceConnector(
        string address,
        IObservable<WaveInEventArgs> source)
    {
        _address = address;
        _source = source;
        _tcpClient = new TcpClient();
    }

    public void Connect()
    {
        _tcpClient.Connect(_address, RemoteDeviceServer.ServerPort.AsPrimitive());
    }
}