using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SpeechNote.Resources;
using PocketSphinxRntComp;
using System.Threading.Tasks;
using SpeechNote;
using System.Windows.Documents;
using System.Diagnostics;
using SpeechNote.Recorder;

namespace SpeechNote
{
    public partial class MainPage : PhoneApplicationPage
    {

        public MainPage()
        {
            InitializeComponent();

        }

        private async void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.outputtxtblock.Text = "Loading";

            // Khởi tạo bộ dịch phiên âm
            await initSpeechRecognizer();

            AudioManager.getInstance().InitAudioRecorder();
            this.outputtxtblock.Text = "Ready for use";
        }

        private async Task initSpeechRecognizer()
        {
            try
            {
                await Task.Run(() => {
                    // STEP 1: Initialize and Load searches
                    // Xem SpeechRecognizer.h
                    var speachrecognizer = AudioManager.getInstance().InitSpeechRecognizer();
                    speachrecognizer.resultFound += SpeechRecognizer_ResultFound;
                    speachrecognizer.resultFinalizedBySilence += SpeechRecognizer_FinalResultFound;
                });
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        private void SpeechRecognizer_ResultFound(string result)
        {
            // Sự kiện được kích hoạt khi nhận diện được từ nói
            Debug.WriteLine("ResultFound" + result);
            if (String.IsNullOrEmpty(result) == false)
                this.outputtxtblock.Text = result;
        }

        private void SpeechRecognizer_FinalResultFound(string finalresult)
        {
            // Sự kiện được kích hoạt khi phát hiện sự im lặng kết thúc câu nói.
            Debug.WriteLine("FinalResultFound " + finalresult);
            if (String.IsNullOrEmpty(finalresult) == false)
                this.outputtxtblock.Text = finalresult;
        }

        private void StartRecord_Click(object sender, RoutedEventArgs e)
        {
            AudioManager.getInstance().StartRecorder("time");
            this.outputtxtblock.Text = "Listenning";
        }

    }
}