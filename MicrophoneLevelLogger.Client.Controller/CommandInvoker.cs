﻿using MicrophoneLevelLogger.Client.Controller.CalibrateInput;
using MicrophoneLevelLogger.Client.Controller.CalibrateOutput;
using MicrophoneLevelLogger.Client.Controller.DeleteCalibrates;
using MicrophoneLevelLogger.Client.Controller.DeleteRecord;
using MicrophoneLevelLogger.Client.Controller.DisplayCalibrates;
using MicrophoneLevelLogger.Client.Controller.DisplayMicrophones;
using MicrophoneLevelLogger.Client.Controller.DisplayRecords;
using MicrophoneLevelLogger.Client.Controller.MonitorVolume;
using MicrophoneLevelLogger.Client.Controller.Record;
using MicrophoneLevelLogger.Client.Controller.RecordingSettings;
using MicrophoneLevelLogger.Client.Controller.SetInputLevel;
using MicrophoneLevelLogger.Client.Controller.SetMaxInputLevel;

namespace MicrophoneLevelLogger.Client.Controller;

public class CommandInvoker : ICommandInvoker
{
    private readonly IAudioInterfaceProvider _audioInterfaceProvider;
    private readonly ICommandInvokerView _view;
    private readonly RecordController _recordController;
    private readonly DisplayRecordsController _displayRecordsController;
    private readonly MonitorVolumeController _monitorVolumeController;
    private readonly RecordingSettingsController _recordingSettingsController;
    private readonly DisplayMicrophonesController _displayMicrophonesController;
    private readonly CompositeController _calibrateController;
    private readonly CompositeController _deleteController;
    private readonly ExitController _exitController = new();
    private readonly BorderController _borderController = new();

    public CommandInvoker(
        IAudioInterfaceProvider audioInterfaceProvider,
        ICommandInvokerView view, 
        CalibrateInputController calibrateInputController, 
        RecordController recordController, 
        SetMaxInputLevelController setMaxInputLevelController, 
        MonitorVolumeController monitorVolumeController, 
        DeleteRecordController deleteRecordController, 
        RecordingSettingsController recordingSettingsController, 
        DeleteCalibratesController deleteCalibratesController, 
        DisplayCalibratesController displayCalibratesController, 
        CalibrateOutputController calibrateOutputController, 
        SetInputLevelController setInputLevelController, 
        DisplayMicrophonesController displayMicrophonesController, 
        DisplayRecordsController displayRecordsController, 
        ICompositeControllerView compositeControllerView)
    {
        _audioInterfaceProvider = audioInterfaceProvider;
        _view = view;
        _recordController = recordController;
        _monitorVolumeController = monitorVolumeController;
        _recordingSettingsController = recordingSettingsController;
        _displayMicrophonesController = displayMicrophonesController;
        _displayRecordsController = displayRecordsController;

        _calibrateController =
            new CompositeController(
                "Calibrate            : マイク・スピーカーを調整します。",
                compositeControllerView,
                setMaxInputLevelController,
                setInputLevelController,
                calibrateInputController,
                displayCalibratesController,
                calibrateOutputController);
        _deleteController =
            new CompositeController(
                "Delete               : 各種情報を削除します。",
                compositeControllerView,
                deleteCalibratesController,
                deleteRecordController);
    }

    public async Task InvokeAsync()
    {
        while (true)
        {
            var microphones = _audioInterfaceProvider.Resolve();
            _view.NotifyMicrophonesInformation(microphones);

            var commands = new IController[]
            {
                _displayMicrophonesController,
                _monitorVolumeController,
                _recordController,
                _displayRecordsController,
                _recordingSettingsController,
                _borderController,
                _calibrateController,
                _deleteController,
                _exitController
            };
            var selected = _view.SelectCommand(commands.Select(x => x.Name));
            if (selected == _exitController.Name)
            {
                break;
            }

            try
            {
                var command = commands.Single(x => x.Name == selected);
                await command.ExecuteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private class ExitController : IController
    {
        public string Name => "Exit                 : 終了する。";

        public Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }

    private class CompositeController : IController
    {
        private readonly ICompositeControllerView _view;
        private readonly List<IController> _controllers;

        public CompositeController(
            string name,
            ICompositeControllerView view,
            params IController[] controllers)
        {
            Name = name;
            _view = view;
            _controllers = controllers.ToList();
        }

        public string Name { get; }
        public async Task ExecuteAsync()
        {
            if (_view.TrySelectController(_controllers, out var selected))
            {
                await selected.ExecuteAsync();
            }
        }
    }

    private class BorderController : IController
    {
        public string Name => "-----------------------------------------------------------";
        public Task ExecuteAsync() => Task.CompletedTask;
    }
}