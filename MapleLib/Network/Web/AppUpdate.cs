// Project: MapleLib
// File: AppUpdate.cs
// Updated By: Jared
// 

using System;
using System.Deployment.Application;
using System.Windows;
using Application = System.Windows.Forms.Application;

namespace MapleLib.Network.Web
{
    public class AppUpdate
    {
        private AppUpdate()
        {
            if (!ApplicationDeployment.IsNetworkDeployed) return;

            Ad = ApplicationDeployment.CurrentDeployment;
            Ad.UpdateProgressChanged += Ad_UpdateProgressChanged;

            _updateCheckInfo = Ad.CheckForDetailedUpdate();
        }

        public static AppUpdate Instance { get; } = new AppUpdate();

        private UpdateCheckInfo _updateCheckInfo { get; }

        public ApplicationDeployment Ad { get; }

        public bool UpdateAvailable => ApplicationDeployment.IsNetworkDeployed && _updateCheckInfo.UpdateAvailable;

        public void Update()
        {
            if (Ad != null && !Ad.Update()) return;
            MessageBox.Show("The application has been updated. Shutting down, please restart the app.");
            RestartApp();
        }

        private static void RestartApp()
        {
            Application.Exit();
        }

        private void Ad_UpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            ProgressChangedEventHandler?.Invoke(sender, new ProgressChangedEventArgs
            {
                ProgressPercentage = e.ProgressPercentage,
                TotalBytesReceived = e.BytesTotal,
                BytesReceived = e.BytesCompleted
            });
        }

        public void InvokeProgressChangedEventHandler(object sender, double percentage, long bytesTotal,
            long bytesCompleted)
        {
            ProgressChangedEventHandler?.Invoke(sender, new ProgressChangedEventArgs
            {
                ProgressPercentage = (int) percentage,
                TotalBytesReceived = bytesTotal,
                BytesReceived = bytesCompleted
            });
        }

        public event EventHandler<ProgressChangedEventArgs> ProgressChangedEventHandler;

        #region Nested type: ProgressChangedEventArgs

        public class ProgressChangedEventArgs : EventArgs
        {
            public long BytesReceived;
            public int ProgressPercentage;
            public long TotalBytesReceived;
        }

        #endregion
    }
}