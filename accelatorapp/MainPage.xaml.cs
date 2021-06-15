using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Linq;

namespace accelatorapp
{
    public partial class MainPage : ContentPage
    {
        readonly SensorSpeed speed = SensorSpeed.UI;
        List<float> floatList;
        List<float> afterDangerFloatList;
        bool isDanger;
        int dangerPoint;
        
        public MainPage()
        {
            InitializeComponent();
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            floatList = new List<float>();
            afterDangerFloatList = new List<float>();            
        }

        void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () => {
            
                var data = e.Reading;
                float thresholdValue = (float)Math.Sqrt(data.Acceleration.X * data.Acceleration.X + data.Acceleration.Y * data.Acceleration.Y + data.Acceleration.Z * data.Acceleration.Z);

                xOffsetLabel.Text = $" X: {data.Acceleration.X}";
                yOffsetLabel.Text = $" Y: {data.Acceleration.Y}";
                zOffsetLabel.Text = $" Z: {data.Acceleration.Z}";
                totalOffsetLabel.Text = $" Acceleration: {thresholdValue}";
                floatList.Add(thresholdValue);

                if (thresholdValue > floatList.Average() * 3.0f )
                {
                    if (dangerPoint >= 1)
                    {
                        if (floatList.Count() > dangerPoint + 10)
                        {
                            isDanger = true;
                        }
                        else
                        {
                            isDanger = false;
                        }
                    }
                    else
                    {
                        isDanger = true;
                    }

                }
                if (isDanger)
                {
                    Console.WriteLine($"After Danger Values: {thresholdValue} ");
                    afterDangerFloatList.Add(thresholdValue);
                    if (afterDangerFloatList.Count > 5 && afterDangerFloatList.Count < 16 && isDanger)
                    {
                        if (thresholdValue > 3.0f && isDanger)
                        {
                           isDanger = false;
                           afterDangerFloatList = new List<float>();
                           dangerPoint = floatList.Count();
                           await TextToSpeech.SpeakAsync("Jumped");
                        }
                    }
                    else if (afterDangerFloatList.Count > 16 && isDanger)
                    {
                        isDanger = false;
                        afterDangerFloatList = new List<float>();
                        dangerPoint = floatList.Count();
                        await TextToSpeech.SpeakAsync("Danger spotted! Are you okey?");
                    }
                }
            });
        }

        async void StopClicked(System.Object sender, System.EventArgs e)
        {
            stopButton.IsEnabled = false;
            
            Accelerometer.Stop();
            await TextToSpeech.SpeakAsync("Stoped");

            startButton.IsEnabled = true;
            floatList = new List<float>();
            afterDangerFloatList = new List<float>();
        }

        void StartButtonClicked(System.Object sender, System.EventArgs e)
        {
            if (Accelerometer.IsMonitoring)
                return;
            else
            {
                startButton.IsEnabled = false;
                TextToSpeech.SpeakAsync("Started");
                Accelerometer.Start(speed);
                stopButton.IsEnabled = true;
                isDanger = false;
            }
        }
    }
}
