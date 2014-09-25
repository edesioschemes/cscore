﻿using System;
using System.ComponentModel;
using System.Windows.Forms;
using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using CSCore.Streams;

namespace EqualizerTest
{
    public partial class MainWindow : Form
    {
        private const double MaxDB = 20;

        private Equalizer _equalizer;
        private ISoundOut _soundOut;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            var trackbar = sender as TrackBar;
            if (_equalizer != null && trackbar != null)
            {
                double perc = (trackbar.Value / (double) trackbar.Maximum);
                var value = (float) (perc * MaxDB);

                //the tag of the trackbar contains the index of the filter
                int filterIndex = Int32.Parse((string) trackbar.Tag);
                EqFilterEntry filter = _equalizer.SampleFilters[filterIndex];
                filter.SetGain(value);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var ofn = new OpenFileDialog();
            ofn.Filter = CodecFactory.SupportedFilesFilterEn;
            if (ofn.ShowDialog() == DialogResult.OK)
            {
                Stop();

                if (WasapiOut.IsSupportedOnCurrentPlatform)
                    _soundOut = new WasapiOut();
                else
                    _soundOut = new DirectSoundOut();

                var source = CodecFactory.Instance.GetCodec(ofn.FileName)
                    .AppendSource(x => new LoopStream(x) { EnableLoop = true })
                    .AppendSource(Equalizer.Create10BandEqualizer, out _equalizer)
                    .ToWaveSource(32);

                _soundOut.Initialize(source);
                _soundOut.Play();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Stop();
            base.OnClosing(e);
        }

        private void Stop()
        {
            if (_soundOut != null)
            {
                _soundOut.Stop();
                _soundOut.Dispose();
                _equalizer.Dispose();
                _soundOut = null;
            }
        }
    }
}