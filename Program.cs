using System;
using Gadgeteer.Modules.GHIElectronics;
using GHI.Premium.System;
using GTM = Gadgeteer.Modules;

namespace CameraStreaming
{
    public partial class Program
    {
        private Bluetooth.Client _bluetoothClient;
        private Gadgeteer.Timer _connectTimer;

        void ProgramStarted()
        {
            camera.PictureCaptured += (s, picture) => StreamPictureToBluetooth(picture);
            button.ButtonPressed += (s, b) => camera.TakePicture();

            bluetooth.SetDeviceName("Gadgeteer");
            bluetooth.SetPinCode("0000");

            _bluetoothClient = bluetooth.ClientMode;

            //Initialise bluetooth (give it 3 seconds to boot up, SOP)
            _connectTimer = new Gadgeteer.Timer(3000, Gadgeteer.Timer.BehaviorType.RunOnce);
            _connectTimer.Tick += t => _bluetoothClient.EnterPairingMode();
            _connectTimer.Start();
        }

        //void camera_PictureCaptured(Camera sender, Gadgeteer.Picture picture)
        void StreamPictureToBluetooth(Gadgeteer.Picture picture)
        {
            //Convert the Picture to a bitmap
            var bitmap = picture.MakeBitmap();

            //Display the picture on the LCD
            bitmap.Flush();

            //Get the bytes for the windows-compatible BMP format
            var bmpBuffer = GetBMPBytes(bitmap);

            ChunkStreamToBluetooth(bmpBuffer);
        }

        private void ChunkStreamToBluetooth(byte[] bmpBuffer)
        {
            // 2478 * 93 === 320 * 240 * 3 + 54 === 230454
            var chunkSize = 2478;

            //Create a byte array to hold the chunks prior to streaming
            var sendArray = new byte[chunkSize];

            for (var i = 0; i < 93; ++i)
            {
                //Copy the nect chunk
                Array.Copy(bmpBuffer, i * chunkSize, sendArray, 0, chunkSize);

                //Write to the bluetooth stream
                bluetooth.serialPort.Write(sendArray);
                bluetooth.serialPort.Flush();
            }
        }

        private static byte[] GetBMPBytes(Microsoft.SPOT.Bitmap bitmap)
        {
            //Convert the bitmap to a windows-compatible BMP
            var width = bitmap.Width;
            var height = bitmap.Height;
            var bmpBuffer = new byte[width * height * 3 + 54];
            Util.BitmapToBMPFile(bitmap.GetBitmap(), width, height, bmpBuffer);
            return bmpBuffer;
        }

        void button_ButtonPressed(GTM.GHIElectronics.Button sender, GTM.GHIElectronics.Button.ButtonState state)
        {
            camera.TakePicture();
        }

        void timer_Tick(Gadgeteer.Timer timer)
        {
            _connectTimer.Stop();
            _bluetoothClient.EnterPairingMode();
        }
    }
}
