﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using HidLibrary;
using MessageBoxServicing;
using Prism.Commands;
using Prism.Mvvm;

namespace FreeJoyConfigurator
{
    public class MainVM : BindableBase
    {
        private DeviceConfig _devConfig;
        private Joystick _joystick;

        public PinsVM PinsVM {get; set; }
        public AxesVM AxesVM { get; private set; }
        public ButtonsVM ButtonsVM { get; private set; }

        public string ActivityLogVM { get; private set; }
        public string ConnectionStatusVM
        {
            get
            {
                return string.Format("{0}", Hid.IsConnected ? "Connected" : "Disconnected");
            }
        }
        public bool IsConnectedVM
        {
            get
            {
                return Hid.IsConnected;
            }
        }

        #region Commands
        public DelegateCommand GetDeviceConfig { get; }
        public DelegateCommand SendDeviceConfig { get; }
        #endregion


        public MainVM()
        {
            Hid.Connect();
            Hid.DeviceAdded += DeviceAddedEventHandler;
            Hid.DeviceRemoved += DeviceRemovedEventHandler;

            _joystick = new Joystick();
            _devConfig = new DeviceConfig();

            PinsVM = new PinsVM(_devConfig);
            AxesVM = new AxesVM(_joystick, _devConfig);
            ButtonsVM = new ButtonsVM(_joystick, _devConfig);

            GetDeviceConfig = new DelegateCommand(() =>
            {
                _devConfig.GetConfigRequest();
                WriteLog("Requesting config..", false);
            });
            SendDeviceConfig = new DelegateCommand(() =>
            {
                _devConfig.SendConfig();
                WriteLog("Writting config..", false);
            });

            WriteLog("Program started", true);
        }


        #region HidEvents
        public void DeviceAddedEventHandler(HidDevice hd)
        {
            WriteLog("Device added", false);
            RaisePropertyChanged(nameof(ConnectionStatusVM));
            RaisePropertyChanged(nameof(IsConnectedVM));
        }

        public void DeviceRemovedEventHandler(HidDevice hd)
        {
            WriteLog("Device removed", false);
            RaisePropertyChanged(nameof(ConnectionStatusVM));
            RaisePropertyChanged(nameof(IsConnectedVM));
        }
        public void PacketReceivedEventHandler(HidReport hr)
        {
            //MessageBoxService mbs = new MessageBoxService();
            //WriteLog("Report received", false);
        }

        public void PacketSentEventHandler(HidReport hr)
        {
            WriteLog("Report sent", false);
        }
        #endregion


        #region Local functions
        // Add a line to the activity log text box
        private void WriteLog(string message, bool clear)
        {
            // Replace content
            if (clear)
            {
                ActivityLogVM = string.Format("{0}: {1}", DateTime.Now.ToString("HH:mm:ss"), message);
            }
            // Add new line
            else
            {
                ActivityLogVM += Environment.NewLine + string.Format("{0}: {1}", DateTime.Now.ToString("HH:mm:ss"), message);
            }
            RaisePropertyChanged("ActivityLogVM");
        }
        #endregion
    }
}

