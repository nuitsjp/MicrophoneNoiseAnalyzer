﻿namespace MicrophoneLevelLogger.Client.Controller;

public interface IMicrophoneView
{
    void NotifyMicrophonesInformation(IAudioInterface audioInterface);
    void StartNotifyMasterPeakValue(IAudioInterface audioInterface);
    void StopNotifyMasterPeakValue();
}