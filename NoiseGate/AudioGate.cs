using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NoiseGate
{
    public class AudioGate
    {
        public event Action MuteRequested = delegate { };
        public event Action UnmuteRequested = delegate { };
        public event Action HardwareMuteDetected = delegate { };

        public TimeSpan MinimumOpen { get; set; }
        public int MinimumAverage { get; set; }
        public int MaximumInstantaneous { get; set; }
        public int SampleQueueLength { get; set; }

        public bool IsOpen { get; set; }


        bool last_mute = false;
        bool mute = false;
        Queue<double> Recentvalues = new Queue<double>();
        Queue<double> MutedValues = new Queue<double>();
        Queue<double> UnmutedValues = new Queue<double>();
        DateTime lastEvent = DateTime.Now;

        object g = new object();
        int counter = 0;

        public AudioGate()
        {
            UnmutedValues.Enqueue(1);
            MutedValues.Enqueue(1);
        }

        public void AddSample(float Sample)
        {
            lock (g)
            {
                ++counter;

                double val = Sample * 10000;
                val = Math.Round(val, 0);

                if (Recentvalues.Count > SampleQueueLength)
                {
                    Recentvalues.Dequeue();
                }

                Recentvalues.Enqueue(Math.Min(val, 1000));

                var sum = Recentvalues.Sum();

                var avg = sum / Recentvalues.Count;
                avg = Math.Round(avg, 0);


                if (avg < MinimumAverage)
                {
                    mute = true;
                }

                if (val > MaximumInstantaneous)
                {
                    mute = false;
                }

                var is_hardware_muted = false;
                if (avg == 0 && val == 0)
                {
                    is_hardware_muted = true;
                    HardwareMuteDetected();
                }

                if (MutedValues.Count > (SampleQueueLength))
                {
                    MutedValues.Dequeue();
                }

                if (UnmutedValues.Count > (SampleQueueLength))
                {
                    UnmutedValues.Dequeue();
                }

                if (!mute) { UnmutedValues.Enqueue(val); }
                if (mute && !is_hardware_muted) { MutedValues.Enqueue(val); }

                var muted_avg = Math.Round(MutedValues.Sum() / MutedValues.Count, 0);
                var unmuted_avg = Math.Round(UnmutedValues.Sum() / UnmutedValues.Count, 0);
                
                double gate = 250;
                if (muted_avg > 10)
                {
                    gate = muted_avg * 4;
                }
                

                MinimumAverage = MaximumInstantaneous = (int)gate;
                Debug.WriteLine(string.Format("mute={0}, inst_mute={1}, hw_mute={2}, inst={3}, avg={4}, muted_avg={5}, unmuted_avg={6}, gate={7}", last_mute, mute, is_hardware_muted, val, avg, muted_avg, unmuted_avg, gate));

                if (mute != last_mute && (!mute || lastEvent.AddMilliseconds(MinimumOpen.TotalMilliseconds).CompareTo(DateTime.Now) < 0))
                {
                    last_mute = mute;
                    lastEvent = DateTime.Now;
                    Debug.WriteLine("#### CHANGE MUTE");
                    if (mute)
                    {
                        MuteRequested();
                    }
                    else
                    {
                        UnmuteRequested();
                    }
                }


            }
        }
    }
}
