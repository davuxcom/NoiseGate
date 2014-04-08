using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoiseGate
{
    public class MicrophoneMonitor
    {
        public event Action<object, MaxSampleEventArgs> Sampled = delegate { };

        SampleAggregator sampleAggregator;

        public MicrophoneMonitor(int DeviceNumber = 0, int sampleRate = 48000, int notificationRate = 10)
        {
            var waveIn = new WaveIn();
            waveIn.DeviceNumber = DeviceNumber;
            waveIn.DataAvailable += OnDataAvailable;
            waveIn.WaveFormat = new WaveFormat(sampleRate, 1);
            waveIn.StartRecording();

            sampleAggregator = new SampleAggregator();
            sampleAggregator.MaximumCalculated += (sender, e) => Sampled(sender, e);
            sampleAggregator.NotificationCount = waveIn.WaveFormat.SampleRate / notificationRate;
        }

        void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesRecorded = e.BytesRecorded;

            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((buffer[index + 1] << 8) |
                                        buffer[index + 0]);
                float sample32 = sample / 32768f;
                sampleAggregator.Add(sample32);
            }
        }
    }

    public class SampleAggregator
    {
        public event EventHandler<MaxSampleEventArgs> MaximumCalculated;
        public event EventHandler Restart = delegate { };
        private float maxValue;
        private float minValue;
        public int NotificationCount { get; set; }
        int count;

        public void RaiseRestart()
        {
            Restart(this, EventArgs.Empty);
        }

        private void Reset()
        {
            count = 0;
            maxValue = minValue = 0;
        }

        public void Add(float value)
        {
            maxValue = Math.Max(maxValue, value);
            minValue = Math.Min(minValue, value);
            count++;

            if (count >= NotificationCount && NotificationCount > 0)
            {
                if (MaximumCalculated != null)
                {
                    MaximumCalculated(this, new MaxSampleEventArgs(minValue, maxValue));
                }
                Reset();
            }
        }
    }

    public class MaxSampleEventArgs : EventArgs
    {
        public MaxSampleEventArgs(float minValue, float maxValue)
        {
            MaxSample = maxValue;
            MinSample = minValue;
        }
        public float MaxSample { get; private set; }
        public float MinSample { get; private set; }
    }
}
