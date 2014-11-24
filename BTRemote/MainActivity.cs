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
using System.Threading.Tasks;

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
    private Button btnLEDBlink;
    private Button btnLEDOff;
    private Button btnLEDOn;
    private Button btnDisconnect;
    private Button btnConnect;

    protected override void OnCreate(Bundle bundle)
    {
      base.OnCreate(bundle);

      SetContentView(Resource.Layout.Main);

      this.btnConnect = FindViewById<Button>(Resource.Id.btnConnect);
      this.btnDisconnect = FindViewById<Button>(Resource.Id.btnDisconnect);
      this.btnLEDOn = FindViewById<Button>(Resource.Id.btnLEDOn);
      this.btnLEDOff = FindViewById<Button>(Resource.Id.btnLEDOff);
      this.btnLEDBlink = FindViewById<Button>(Resource.Id.btnLEDBlink);

      this.DisableControls();

      this.txtStatus = FindViewById<TextView>(Resource.Id.txtStatus);

      btnConnect.Click += async (sender, e) =>
      {
          bool res;
          res = await FindBT();

          if (res)
          {
            res = await OpenBT();
            if (res)
            {
              EnableControls();
            }
        }
      };

      btnDisconnect.Click += (sender, e) =>
      {
        CloseBT();
        DisableControls();
      };

      btnLEDOn.Click += (sender, e) =>
      {
        var bytes = System.Text.Encoding.UTF8.GetBytes("A");
        this.outputStream.Write(bytes, 0, bytes.Length);
      };

      btnLEDOff.Click += (sender, e) =>
      {
        var bytes = System.Text.Encoding.UTF8.GetBytes("a");
        this.outputStream.Write(bytes, 0, bytes.Length);
      };

      btnLEDBlink.Click += (sender, e) =>
      {
        var bytes = System.Text.Encoding.UTF8.GetBytes("B");
        this.outputStream.Write(bytes, 0, bytes.Length);
      };
    }

    private async bool FindBT()
    {
      return await Task.Factory.StartNew<bool>(() =>
        {
          this.bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
          if (this.bluetoothAdapter == null)
          {
            SetStatus("No Bluetooth Adapter found...");
            return false;
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
            return false;
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
            return false;
          }
          else
          {
            SetStatus("Found Bluetooth Device HC-06");
            return true;
          }
        });
    }

    private async Task<bool> OpenBT()
    {
      return await Task.Factory.StartNew<bool>(() =>
        {
          try
          {
            UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"); //Standard SerialPortService ID
            this.socket = this.bluetoothDevice.CreateRfcommSocketToServiceRecord(uuid);
            this.socket.Connect();
            this.outputStream = this.socket.OutputStream;

            SetStatus("Bluetooth Opened");

            return true;
          }
          catch (Exception)
          {
            SetStatus("Unable to open Bluetooth Connection to device");
            return false;
          }
        });
    }

    private void CloseBT()
    {
      if (outputStream != null)
      {
        this.outputStream.Close();
      }
      if (this.socket != null)
      {
        this.socket.Close();
      }
    }

    private void SetStatus(string text)
    {
      RunOnUiThread(() =>
        {
          this.txtStatus.Text = text;
        });
    }

    private void EnableControls()
    {
      btnLEDOn.Enabled = true;
      btnConnect.Enabled = false;
      btnLEDBlink.Enabled = true;
      btnDisconnect.Enabled = true;
      btnLEDOff.Enabled = true;
    }

    private void DisableControls()
    {
      btnLEDOn.Enabled = false;
      btnConnect.Enabled = true;
      btnLEDBlink.Enabled = false;
      btnDisconnect.Enabled = false;
      btnLEDOff.Enabled = false;
    }
  }
}
