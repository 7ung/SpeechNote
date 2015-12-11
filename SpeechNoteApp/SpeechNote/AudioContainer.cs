using PocketSphinxRntComp;
using SpeechNote.Recorder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechNote
{
    /// <summary>
    /// class demo, unuse.
    /// </summary>
    abstract public class AudioContainer
    {
        /// <summary>
        /// PocketSphinx speech recognizer in a Runtime Component
        /// </summary>
        public static PocketSphinxRntComp.SpeechRecognizer SphinxSpeechRecognizer
        { get; set; }

        /// <summary>
        /// Native Audio Recorder working with WASAPI
        /// </summary>
        public static SpeechNote.Recorder.WasapiAudioRecorder AudioRecorder
        { get; set; }

        /// <summary>
        /// Stop all
        /// </summary>
        public static void StopAllAudioProccesses()
        {
            if (AudioRecorder != null)
            {
                AudioRecorder.StopRecording();
            }

            if (SphinxSpeechRecognizer != null)
            {
                var result = string.Empty;
                result = SphinxSpeechRecognizer.StopProcessing();
                Debug.WriteLine(result);
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public static void Dispose()
        {
            if (SphinxSpeechRecognizer != null)
            {
                var cleanResult = string.Empty;
                cleanResult = SphinxSpeechRecognizer.CleanPocketSphinx();
                Debug.WriteLine(cleanResult);
            }
        }
    }

    public enum eSearchType
    {
        KEYPHRASE,
        GRAMMAR,
        NGRAM,
    }
    /// <summary>
    /// Từ class demo, implement bằng mẫu single.
    /// </summary>
    public class AudioManager
    {
        /// <summary>
        /// Thể hiện duy nhất đối tượng audiomanager
        /// </summary>
        private static AudioManager _instance = null;

        private List<string> _searchmodes = new List<string>();

        private string _currentsearchmode = String.Empty;


        /// <summary>
        /// PocketSphinx speech recognizer in a Runtime Component
        /// </summary>
        private PocketSphinxRntComp.SpeechRecognizer SphinxSpeechRecognizer
        { get; set; }

        /// <summary>
        /// Native Audio Recorder working with WASAPI
        /// </summary>
        private SpeechNote.Recorder.WasapiAudioRecorder AudioRecorder
        { get; set; }


        public static AudioManager getInstance()
        {
            if (_instance == null)
            {
                _instance = new AudioManager();
            }
            return _instance;
        }

        public bool AddSearchMode(eSearchType type, string name, string filepath)
        {
            if (_searchmodes.Contains(name) == true)
                return false;
            switch (type)
            {
                case eSearchType.KEYPHRASE:
                    this.SphinxSpeechRecognizer.AddKeyphraseSearch(name, filepath);
                    break;
                case eSearchType.GRAMMAR:
                    this.SphinxSpeechRecognizer.AddGrammarSearch(name, filepath);
                    break;
                case eSearchType.NGRAM:
                    var str = this.SphinxSpeechRecognizer.AddNgramSearch(name, filepath);
                    Debug.WriteLine("Add search mode result: " + str);
                    break;
                default:
                    break;
            }
            this._searchmodes.Add(name);
            return true;
        }

        private bool SetSearchMode(string name)
        {
            if (_searchmodes.Contains(name) == true)
            {
                if (this.SphinxSpeechRecognizer.IsProcessing() == true)
                {
                    this.SphinxSpeechRecognizer.StopProcessing();
                }
                this.SphinxSpeechRecognizer.SetSearch(name);
                this.SphinxSpeechRecognizer.StartProcessing();                    
                this._currentsearchmode = name;
#if DEBUG
                Debug.WriteLine("Set Search Mode: " + name);       
#endif
                return true;
            }
            return false;
        }

        public string getCurretnSearchMode()
        {
            return _currentsearchmode;
        }

        public SpeechRecognizer InitSpeechRecognizer()
        {
            this.SphinxSpeechRecognizer = new SpeechRecognizer();
            string rs = this.SphinxSpeechRecognizer.Initialize("\\Assets\\models\\hmm\\en-us-semi", "\\Assets\\models\\dict\\cmu07a.dic");

            this.AddSearchMode(eSearchType.GRAMMAR, "demo", "\\Assets\\models\\grammar\\menu.gram");
            this.AddSearchMode(eSearchType.GRAMMAR, "createnote", "\\Assets\\models\\grammar\\createnote.gram");
            this.AddSearchMode(eSearchType.GRAMMAR, "time", "\\Assets\\models\\grammar\\time.gram");
            this.AddSearchMode(eSearchType.GRAMMAR, "yesno", "\\Assets\\models\\grammar\\yesno.gram");
            this.AddSearchMode(eSearchType.NGRAM, "language", "\\Assets\\models\\lm\\100.arpa");
            // add some grammar here

#if DEBUG
            Debug.WriteLine(rs);
#endif
            return this.SphinxSpeechRecognizer;
        }

        public WasapiAudioRecorder InitAudioRecorder()
        {
            this.AudioRecorder = new WasapiAudioRecorder();
            this.AudioRecorder.BufferReady += WasAPIAudio_BufferReady;
            return this.AudioRecorder;
        }


        // Bắt đầu lắng nghe
        // @searchmode: tên của mode đã được add
        public void StartRecorder(string searchmode)
        {
            if (this.AudioRecorder == null)
            {
                throw new NullReferenceException();
            }
            else
            {
                if (this.SphinxSpeechRecognizer == null)
                {
                    this.SphinxSpeechRecognizer = new SpeechRecognizer();
                }
                this.SetSearchMode(searchmode);
                this.AudioRecorder.StartRecording();
                //string rs = this.SphinxSpeechRecognizer.StartProcessing();
#if DEBUG
                    //Debug.WriteLine(rs);
#endif

            }
        }

        public void StopRecorder()
        {
            if (this.AudioRecorder == null)
            {
                throw new NullReferenceException();
            }
            else
            {
                this.AudioRecorder.StopRecording();
                if (this.SphinxSpeechRecognizer == null)
                    return;
                if (this.SphinxSpeechRecognizer.IsProcessing() == true)
                {
                    this.SphinxSpeechRecognizer.StopProcessing();
                }
            }
        }

        public void release()
        {
            if (this.SphinxSpeechRecognizer == null)
                return;
            string rs = SphinxSpeechRecognizer.CleanPocketSphinx();
#if DEBUG
            Debug.WriteLine(rs);
#endif
        }

        private void WasAPIAudio_BufferReady(object sender, AudioDataEventArgs args)
        {
            // Sự kiện được kích hoạt khi nhận được âm thanh.
            // @sender: WasapiAudioRecorder
            // @e: chứa mảng bytes trình ghi âm đọc được.
            try
            {
                // Từ dữ liệu ghi âm, tiến hành phân tích và tìm kiếm trong từ điển.
                // Nếu tìm thấy từ, phát động sự kiện resultfound.
                // Nếu dữ liệu ghi âm là 'silent', phát động sự kiện finalresultfound.
                this.SphinxSpeechRecognizer.RegisterAudioBytes(args.Data);
            }
            catch  (Exception e)
            {
                //StopNativeRecorder();
                //StopSpeechRecognizerProcessing();
#if DEBUG
                Debug.WriteLine(e.Message);
#endif
            }
        }
    }
}
