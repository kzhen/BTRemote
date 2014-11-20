using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using Java.Util;
using System.IO;

namespace BTRemote
{
    [Activity(Label = "BTRemote", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private BluetoothDevice bluetoothDevice;
        private BluetoothAdapter bluetoothAdapter;
        private BluetoothSocket socket;
        private Stream outputStream;

        private TextView txtStatus;

        private Action<bool> A;

        protected override void OnCreate(Bundle bundle)
        {
            A += (a) =>
            {
            };
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            Button btnConnect = FindViewById<Button>(Resource.Id.btnConnect);
            Button btnDisconnect = FindViewById<Button>(Resource.Id.btnDisconnect);
            Button btnLEDOn = FindViewById<Button>(Resource.Id.btnLEDOn);
            Button btnLEDOff = FindViewById<Button>(Resource.Id.btnLEDOff);
            Button btnLEDBlink = FindViewById<Button>(Resource.Id.btnLEDBlink);

            this.txtStatus = FindViewById<TextView>(Resource.Id.txtStatus);

            btnConnect.Click += (sender, e) => 
                {
                    FindBT();
                    OpenBT();

                    btnConnect.Enabled = false;
                    btnDisconnect.Enabled = true;
                };

            btnDisconnect.Click += (sender, e) => 
                {
                    CloseBT();
                };

            btnLEDOn.Click += (sender, e) => 
                {
                };

            btnLEDOff.Click += (sender, e) => 
                {
                };

            btnLEDBlink.Click += (sender, e) => 
                {
                };
        }

        private void FindBT()
        {
            this.bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (this.bluetoothAdapter == null)
            {
                SetStatus("No Bluetooth Adapter found...");
                return;
            }

            if (!this.bluetoothAdapter.IsEnabled)
            {
                Intent enableBluetooth = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableBluetooth, 0);
            }

            var pairedDevices = this.bluetoothAdapter.BondedDevices;
            if (pairedDevices.Count == 0)
            {
                SetStatus("No paired devices found...");
                return;
            }

            foreach (var item in pairedDevices)
            {
                if (item.Name.Equals("HC-06"))
                {
                    this.bluetoothDevice = item;
                    break;
                }
            }

            if (this.bluetoothDevice == null)
            {
                SetStatus("Bluetooth Device HC-06 not found...");
                return;
            }
            else
            {
                SetStatus("Found Bluetooth Device HC-06");
            }
        }

        private void OpenBT()
        {
            try
            {
                UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"); //Standard SerialPortService ID
                this.socket = this.bluetoothDevice.CreateRfcommSocketToServiceRecord(uuid);
                this.socket.Connect();
                this.outputStream = this.socket.OutputStream;
                
                SetStatus("Bluetooth Opened");
            }
            catch (Exception)
            {
                SetStatus("Unable to open Bluetooth Connection to device");
            }
        }

        private void CloseBT()
        {
            this.outputStream.Close();
            this.socket.Close();
        }

        private void SetStatus(string text)
        {
            this.txtStatus.Text = text;
        }
    }
}