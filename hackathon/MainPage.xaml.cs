using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace hackathon
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    /// 

    class Recorder
    {
        MediaCapture _capture;
        InMemoryRandomAccessStream _buffer;
        bool _record;
        private string _filename;
        private readonly string _audioFile;

        public Recorder(string filename)
        {
            _audioFile = filename;
        }

        private async Task<bool> RecordProcess()
        {
            _buffer?.Dispose();
            _buffer = new InMemoryRandomAccessStream();
            _capture?.Dispose();
            try
            {
                MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Audio
                };
                _capture = new MediaCapture();
                await _capture.InitializeAsync(settings);
                _capture.RecordLimitationExceeded += (MediaCapture sender) =>
                {
                    //Stop
                    //   await capture.StopRecordAsync();
                    _record = false;
                    throw new Exception("Record Limitation Exceeded ");
                };
                _capture.Failed += (MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs) =>
                {
                    _record = false;
                    throw new Exception(string.Format("Code: {0}. {1}", errorEventArgs.Code, errorEventArgs.Message));
                };
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType() == typeof(UnauthorizedAccessException))
                {
                    throw ex.InnerException;
                }
                throw;
            }
            return true;
        }
        private async Task PlayRecordedAudio(CoreDispatcher UiDispatcher)
        {
            MediaElement playback = new MediaElement();
            IRandomAccessStream audio = _buffer.CloneStream();

            if (audio == null)
                throw new ArgumentNullException("buffer boom!");
            StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            if (!string.IsNullOrEmpty(_filename))
            {
                StorageFile original = await storageFolder.GetFileAsync(_filename);
                await original.DeleteAsync();
            }
            await UiDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                StorageFile storageFile = await storageFolder.CreateFileAsync(_audioFile, CreationCollisionOption.GenerateUniqueName);
                _filename = storageFile.Name;
                using (IRandomAccessStream fileStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await RandomAccessStream.CopyAndCloseAsync(audio.GetInputStreamAt(0), fileStream.GetOutputStreamAt(0));
                    await audio.FlushAsync();
                    audio.Dispose();
                }
                IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.Read);
                playback.SetSource(stream, storageFile.FileType);
                playback.Play();
            });
        }

        public async void recordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_record)
            {
                //already recored process
            }
            else
            {
                await RecordProcess();
                var outProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Low);
                outProfile.Audio = AudioEncodingProperties.CreatePcm(16000, 1, 16);
                await _capture.StartRecordToStreamAsync(outProfile, _buffer);
                if (_record)
                {
                    throw new InvalidOperationException("cannot excute two records at the same time");
                }
                _record = true;
            }

        }

        public async void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            await _capture.StopRecordAsync();
            _record = false;
        }

        public async void playBtn_Click(object sender, RoutedEventArgs e, CoreDispatcher dispatcher)
        {
            await PlayRecordedAudio(dispatcher);
        }
    }

    public sealed partial class MainPage : Page
    {
        private readonly Recorder r1, r2, r3;
        public MainPage()
        {
            this.InitializeComponent();
            r1 = new Recorder("audio1.wav");
            r2 = new Recorder("audio2.wav");
            r3 = new Recorder("text.wav");
        }

        private void Record1_Click(object sender, RoutedEventArgs e)
        {
            r1.recordBtn_Click(sender, e);
        }
        
        private void Stop1_Click(object sender, RoutedEventArgs e)
        {
            r1.stopBtn_Click(sender, e);
        }

        private void Play1_Click(object sender, RoutedEventArgs e)
        {
            r1.playBtn_Click(sender, e, Dispatcher);
        }

        private void Record2_Click(object sender, RoutedEventArgs e)
        {
            r2.recordBtn_Click(sender, e);
        }
        
        private void Stop2_Click(object sender, RoutedEventArgs e)
        {
            r2.stopBtn_Click(sender, e);
        }

        private void Play2_Click(object sender, RoutedEventArgs e)
        {
            r2.playBtn_Click(sender, e, Dispatcher);
        }
        
        private void Verify1_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Verify2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Record3_Click(object sender, RoutedEventArgs e)
        {
            r3.recordBtn_Click(sender, e);
        }

        private void Stop3_Click(object sender, RoutedEventArgs e)
        {
            r3.stopBtn_Click(sender, e);
        }

        private void Play3_Click(object sender, RoutedEventArgs e)
        {
            r3.playBtn_Click(sender, e, Dispatcher);
        }



        private async void Select_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add(".wav");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                this.textBox.Text = "Picked photo: " + file.Name;
            }
            else
            {
                this.textBox.Text = "Nothing selected";
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
                
        }

        private void Refresh1_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Refresh2_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
