using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//using System.Runtime;
//using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using System.Threading.Tasks;

//using System.Runtime.CompilerServices;

using Windows;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Devices.Enumeration.Pnp;
using Windows.Foundation;


namespace LCA_SYNC
{

    public partial class Main : Form
    {

        public Main()
        {
            InitializeComponent();
            //var ble = CrossBleAdapter.Current.Status;
            //var results = CrossBleAdapter.Current.Scan();
            //Console.WriteLine(ble);
                
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // Click 'Sync'
        private void button1_Click(object sender, EventArgs e)
        {


        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        /*
        public static TResult Await<TResult>(this IAsyncOperation<TResult> operation)
        {
            try
            {
                return operation.GetResults();
            }
            finally
            {
                operation.Close();
            }
        }
        */
        


        private async void button1_Click_2(object sender, EventArgs e)
        {
            Console.WriteLine("start");
            var Uuid = GattServiceUuids.HeartRate;

            Console.WriteLine(GattServiceUuids.HeartRate);

            //GattDeviceService.GetDeviceSelectorFromUuid(
            //IAsyncOperation<DeviceInformationCollection>
            string selector = GattDeviceService.GetDeviceSelectorFromUuid(Uuid);

            //var results = Await<DeviceInformationCollection>(DeviceInformation.FindAllAsync(selector, new string[] { "System.Devices.ContainerId" }));
            Console.WriteLine("here0");

            
            var op = DeviceInformation.FindAllAsync(selector);  // Needs await? But await needs System.Runtime.WindowsRuntime which apparently isn't supported currently on .NET
            Console.WriteLine("here1");

            var tcs = new TaskCompletionSource<DeviceInformationCollection>();
            op.Completed = delegate
            {
                if (op.Status == Windows.Foundation.AsyncStatus.Completed) { tcs.SetResult(op.GetResults()); }
                else if (op.Status == Windows.Foundation.AsyncStatus.Error) { tcs.SetException(op.ErrorCode); }
                else { tcs.SetCanceled(); }
            };
            var result = await tcs.Task;


            Console.WriteLine("here2");

            Console.WriteLine(result.Count);
            
        }


    }


}
