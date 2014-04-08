//using CoreAudioApi;
using NAudio.Mixer;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Forms;

namespace NoiseGate
{
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool Beep(uint dwFreq, uint dwDuration);

        AudioGate gate;
        MicrophoneMonitor mic;

        int counter = 0;

        public MainWindow()
        {
            InitializeComponent();

            gate = new AudioGate();
            gate.MinimumOpen = TimeSpan.FromSeconds(1);
            gate.MinimumAverage = 250;
            gate.MaximumInstantaneous = 240;
            gate.SampleQueueLength = 25;
            gate.MuteRequested += () =>
            {
                Beep(50, 100);
                SkypeApp.Mute();
            };
            gate.UnmuteRequested += () => 
            {
                Beep(100, 100);
                SkypeApp.Unmute();
            };
            gate.HardwareMuteDetected += () =>
            {
                ++counter;
                if (counter % 100 == 0)
                {
                    Beep(300, 100);
                }
            };

            mic = new MicrophoneMonitor();
            mic.Sampled += (_, e) => gate.AddSample(e.MaxSample);
        }
    }

    /*
    public class AudioRecorder
    {
        WaveIn waveIn;
        readonly SampleAggregator sampleAggregator;
        UnsignedMixerControl volumeControl;
        double desiredVolume = 100;
        WaveFormat recordingFormat;

        public event EventHandler Stopped = delegate { };

        public AudioRecorder()
        {
            sampleAggregator = new SampleAggregator();
            RecordingFormat = new WaveFormat(48000, 1);

            sampleAggregator.MaximumCalculated += sampleAggregator_MaximumCalculated;

            waveIn = new WaveIn();
            waveIn.DeviceNumber = 0;
            waveIn.DataAvailable += OnDataAvailable;
            waveIn.WaveFormat = recordingFormat;
            waveIn.StartRecording();

        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool Beep(uint dwFreq, uint dwDuration);


        bool last_mute = false;
        bool mute = false;
        const int recent_count = 25; // 200;
        Queue<double> Recentvalues = new Queue<double>();

        const int avg_mute = 250; // 25
        const int inst_unmute = 510; // 100

        DateTime lastEvent = DateTime.Now;

        object g = new object();


        int counter = 0;

        void sampleAggregator_MaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            lock (g)
            {
                ++counter;

                double val = e.MaxSample * 10000;
                val = Math.Round(val, 0);

                if (Recentvalues.Count > recent_count)
                {
                    Recentvalues.Dequeue();
                }

                Recentvalues.Enqueue(Math.Min(val, 1000));

                var sum = Recentvalues.Sum();

                var avg = sum / Recentvalues.Count;
                avg = Math.Round(avg, 0);


                if (avg < avg_mute)
                {
                    mute = true;
                }

                if (val > inst_unmute)
                {
                    mute = false;
                }

                Debug.WriteLine(string.Format("mute={0}, inst_mute={1} inst={2}, avg={3}", last_mute, mute, val, avg));

                if (mute != last_mute && (!mute || lastEvent.AddSeconds(1).CompareTo(DateTime.Now) < 0))
                {
                    last_mute = mute;
                    lastEvent = DateTime.Now;
                    Debug.WriteLine("#### CHANGE MUTE");
                    if (mute)
                    {
                        Beep(50, 100);
                       // Beep(900, 50);
                        SkypeApp.Mute();
                    }
                    else
                    {
                        Beep(100, 100);
                       // Beep(1700, 50);
                        SkypeApp.Unmute();
                    }
                }

                if (avg == 0 && val == 0)
                {
                    // hardware mute
                    if (counter % 100 == 0)
                    {
                        Beep(500, 100);

                    }
                }
            }
        }


        public WaveFormat RecordingFormat
        {
            get
            {
                return recordingFormat;
            }
            set
            {
                recordingFormat = value;
                sampleAggregator.NotificationCount = value.SampleRate / 10;
            }
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
    */
}
