using System;
using Gadgeteer.Modules.GHIElectronics;
using GHI.Premium.System;
using Microsoft.SPOT;
using GTM = Gadgeteer.Modules;

namespace CameraStreaming
{
    public partial class Program
    {
        Bitmap _bitmap = new Bitmap(320, 240);

        private Bluetooth.Client client;
        private Gadgeteer.Timer _connectTimer;

        void ProgramStarted()
        {
            camera.PictureCaptured += new Camera.PictureCapturedEventHandler(camera_PictureCaptured);

            button.ButtonPressed += new GTM.GHIElectronics.Button.ButtonEventHandler(button_ButtonPressed);

            bluetooth.SetDeviceName("Gadgeteer");
            bluetooth.SetPinCode("0000");

            client = bluetooth.ClientMode;

            _connectTimer = new Gadgeteer.Timer(3000, Gadgeteer.Timer.BehaviorType.RunContinuously);
            _connectTimer.Tick += new Gadgeteer.Timer.TickEventHandler(timer_Tick);
            _connectTimer.Start();
        }

        void camera_PictureCaptured(Camera sender, Gadgeteer.Picture picture)
        {
            var bitmap = picture.MakeBitmap();
            bitmap.Flush();

            var width = 320;
            var height = 240;

            var bmpBuffer = new byte[width * height * 3 + 54];

            Util.BitmapToBMPFile(bitmap.GetBitmap(), width, height, bmpBuffer);            

            var chunkSize = 2478;
            var sendArray = new byte[chunkSize];
            for (var i = 0; i < 93; ++i)
            {
                Array.Copy(bmpBuffer, i * chunkSize, sendArray, 0, chunkSize);
                bluetooth.serialPort.Write(sendArray);
                bluetooth.serialPort.Flush();
            }
        }

        void button_ButtonPressed(GTM.GHIElectronics.Button sender, GTM.GHIElectronics.Button.ButtonState state)
        {
            camera.TakePicture();
        }

        void timer_Tick(Gadgeteer.Timer timer)
        {
            _connectTimer.Stop();
            client.EnterPairingMode();
        }
    }
}
