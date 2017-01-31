// Project: MapleSeedL
// File: XInputController.cs
// Created By: Tsumes <github@tsumes.com>
// Created On: 01/30/2017 7:36 PM

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using MapleSeedU.ViewModels;
using XInputDotNetPure;

namespace MapleSeedU.Models.XInput
{
    public class XInputController
    {
        // Polling worker for game controller
        private static BackgroundWorker _pollingWorker;
        private static Task _pollingWorkerTask;

        // Game Controller reporter
        private readonly ReporterState _reporterState = new ReporterState();

        // DateTime var storing last input time (debouncing)
        private DateTime _lastInput = DateTime.Now;

        public XInputController()
        {
            _pollingWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _pollingWorker.DoWork += pollingWorker_DoWork;

            _pollingWorkerTask = new Task(() =>
            {
                while (true)
                {
                    UpdateState();
                    Task.Delay(1);
                }
            });
        }

        private FaceButton launchButton { get; } = FaceButton.A;

        public void Start()
        {
            //_pollingWorker.RunWorkerAsync();
            _pollingWorkerTask.Start();
        }

        private void UpdateState()
        {
            var state = _reporterState.LastActiveState;

            // 250ms debouncing for controller inputs
            if ((DateTime.Now - _lastInput).Milliseconds > 250)
            {
                if (state.DPad.Up == ButtonState.Pressed) DPadButtonPress(DpadButton.Up);
                if (state.DPad.Down == ButtonState.Pressed) DPadButtonPress(DpadButton.Down);
                if (state.DPad.Left == ButtonState.Pressed) DPadButtonPress(DpadButton.Left);
                if (state.DPad.Right == ButtonState.Pressed) DPadButtonPress(DpadButton.Right);

                if (state.Buttons.A == ButtonState.Pressed) FaceButtonPress(FaceButton.A);
                if (state.Buttons.B == ButtonState.Pressed) FaceButtonPress(FaceButton.B);
                if (state.Buttons.X == ButtonState.Pressed) FaceButtonPress(FaceButton.X);
                if (state.Buttons.Y == ButtonState.Pressed) FaceButtonPress(FaceButton.Y);

                // use this to exit Cemu, it's the "Xbox" button on a XBone controller
                if (state.Buttons.Guide == ButtonState.Pressed) FaceButtonPress(FaceButton.Guide);
                if (state.Buttons.Start == ButtonState.Pressed) FaceButtonPress(FaceButton.Start);
                if (state.Buttons.Back == ButtonState.Pressed) FaceButtonPress(FaceButton.Back);

                if (state.Buttons.LeftStick == ButtonState.Pressed) FaceButtonPress(FaceButton.LeftStick);
                if (state.Buttons.RightStick == ButtonState.Pressed) FaceButtonPress(FaceButton.RightStick);
                if (state.Buttons.LeftShoulder == ButtonState.Pressed) FaceButtonPress(FaceButton.LeftShoulder);
                if (state.Buttons.RightShoulder == ButtonState.Pressed) FaceButtonPress(FaceButton.RightShoulder);
            }
        }

        private void DPadButtonPress(DpadButton button)
        {
            var idx = MainWindowViewModel.Instance.TitleInfoEntries.IndexOf(MainWindowViewModel.Instance.TitleInfoEntry);

            if (button == DpadButton.Down)
                if (idx < MainWindowViewModel.Instance.TitleInfoEntries.Count - 1)
                    MainWindowViewModel.Instance.TitleInfoEntry = MainWindowViewModel.Instance.TitleInfoEntries[idx + 1];
            if (button == DpadButton.Up)
                if (idx > 0)
                    MainWindowViewModel.Instance.TitleInfoEntry = MainWindowViewModel.Instance.TitleInfoEntries[idx - 1];

            MainWindowViewModel.Instance.RaisePropertyChangedEvent("TitleInfoEntry");

            _lastInput = DateTime.Now;
        }

        private void FaceButtonPress(FaceButton button)
        {
            if (button == launchButton)
                MainWindowViewModel.Instance.PlayTitle();

            if (button == FaceButton.Guide)
            {
                var fileName = Path.GetFileName(MainWindowViewModel.Instance.CemuPath.GetValue())?.Replace(".exe", "");
                foreach (var cemuProcess in Process.GetProcessesByName(fileName)) cemuProcess.Kill();
            }

            _lastInput = DateTime.Now;
        }

        private void pollingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!e.Cancel && !MainWindowViewModel.Instance.IsClosing)
                if (_reporterState.Poll()) Application.Current.Dispatcher.Invoke(UpdateState);
        }

        private enum FaceButton
        {
            A,
            B,
            X,
            Y,
            Guide,
            Start,
            Back,
            LeftStick,
            RightStick,
            LeftShoulder,
            RightShoulder
        }

        private enum DpadButton
        {
            Up,
            Down,
            Left,
            Right
        }
    }
}