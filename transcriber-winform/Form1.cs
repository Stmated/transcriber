using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Speech.AudioFormat;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Eliason.AudioVisualizer;
using Eliason.Scrollbar;
using System.Speech.Recognition;
using System.Threading;
using DirectShowLib;
using Newtonsoft.Json;
using transcriber_winform.Giphy;
using transcriber_winform.GSSF;
using ManagedBass;

namespace transcriber_winform
{
    public partial class Form1 : Form, IMessageFilter
    {
        private readonly AudioVisualizer _audioVisualizer;
        private readonly ScrollableTextView _textView;

        private readonly Dictionary<String, Interval> _intervals = new Dictionary<string, Interval>();
        private readonly Regex _regex = new Regex(@"\[(\d\d:\d\d:\d\d,\d\d\d) -> (\d\d:\d\d:\d\d,\d\d\d)\](-?)");

        public long TypingPauseLength { get; set; }

        public Form1()
        {
            InitializeComponent();

            this.Size = new Size(400, 700);

            // TODO: Rita bilder ovanpå varandra, så det inte blir glapp och inte påbörjar ett work i mitten på en millisekund
            // TODO: Visa alla inställningar i GUI när ändrar, så vet vad värdet är just nu

            // TODO: Fixa buggar med scrollbar i texteditor -- den uppdaterar inte när suddar
            // TODO: Följ efter i texteditorn, så att viewport alltid är där man senast klickade med musen, och att viewport följer med när man går runt med tangentbordet

            // TODO: Don't pause on Modifier + Enter
            // TODO: Some kind of queue system of actions to be done, so that no matter what keys are hit, what you want to happen after that should happen
            //          Right now there might be some weird stuff when for example pausing when typing, then stepping backwards, or pause when type and manually starting (will glitch a bit)
            // TODO: Något form av streck som pekar på vart man bör bryta raden för att få en bra längd när man lägger det i en video

            this._audioVisualizer = new AudioVisualizer();
            this._audioVisualizer.Location = new Point(0, 0);
            this._audioVisualizer.Size = new Size(this.ClientSize.Width, 200);
            this._audioVisualizer.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;
            this._audioVisualizer.GotFocus += this.AudioVisualizerGotFocus;
            this._audioVisualizer.NoteRequest += this.AudioVisualizer_OnNoteRequest;
            this._audioVisualizer.NoteMoved += this.OnAudioVisualizerOnNoteMoved;
            this._audioVisualizer.NoteClicked += this.OnAudioVisualizerOnNoteClicked;
            this._audioVisualizer.SetFrequencyRange(5000);
            this._audioVisualizer.RepeatLength = 0;
            this._audioVisualizer.RepeatBackwards = 0;

            var container = new AdvScrollableControl();
            this._textView = new ScrollableTextView(null, new NoSettings(), container);
            this._textView.Font = new Font(this._textView.Font.FontFamily, 8);
            this._textView.KeepAutoSavesOnDispose = true;
            this._textView.KeyUp += this.TextView_OnKeyUp;
            this._textView.AddStyle(new TextStyleSpeaker());

            this.TypingPauseLength = 0;

            container.Control = this._textView;
            container.Location = new Point(0, this._audioVisualizer.Bottom);
            container.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - this._audioVisualizer.Height);
            container.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;

            this.Controls.Add(this._audioVisualizer);
            this.Controls.Add(container);

            this._textView.TextDocument.TextAppendLine("", 0);

            var menuMenu = new MenuItem("&Menu");

            var menuOpenAudio = new MenuItem("Open &audio...");
            menuOpenAudio.Click += delegate
            {
                OpenFileDialog file = new OpenFileDialog();
                if (file.ShowDialog() == DialogResult.OK)
                {
                    this._audioVisualizer.Open(file.FileName);
                }
            };

            var menuOpenText = new MenuItem("Open &text...");
            menuOpenText.Click += delegate
            {
                OpenFileDialog file = new OpenFileDialog();
                if (file.ShowDialog() == DialogResult.OK)
                {
                    this._textView.Open(file.FileName);
                }
            };

            var menuSaveText = new MenuItem("&Save text");
            menuSaveText.Click += delegate
            {
                this.SaveText();
            };

            var menuSaveTextAs = new MenuItem("Sa&ve text as...");
            menuSaveTextAs.Click += delegate
            {
                var file = new SaveFileDialog();
                if (file.ShowDialog() == DialogResult.OK)
                {
                    this._textView.SaveAs(file.FileName);
                }
            };

            var menuExportSRT = new MenuItem("E&xport to SRT...");
            menuExportSRT.Click += OnMenuExportSrtOnClick;

            var menuExportASS = new MenuItem("Export to &SSA...");
            menuExportASS.Click += OnMenuExportAssOnClick;

            var menuExportMp4 = new MenuItem("Export to &MP4...");
            menuExportMp4.Click += OnMenuExportMp4OnClick;

            var menuGenerateDiarization = new MenuItem("Generate &Diarization...");
            menuExportMp4.Click += OnMenuGenerateDiarization;

            var menuSettings = new MenuItem("&Settings...");
            menuSettings.Click += delegate
            {
                var formSettings = new FormSettings();
                formSettings.TextView = this._textView;
                formSettings.MainForm = this;
                formSettings.AudioVisualizer = this._audioVisualizer;

                formSettings.ShowDialog();
            };

            var menuExit = new MenuItem("E&xit");
            menuExit.Click += delegate
            {
                this.Close();
            };

            var menuStrip = new MainMenu();
            menuMenu.MenuItems.Add(menuOpenAudio);
            menuMenu.MenuItems.Add("-");
            menuMenu.MenuItems.Add(menuOpenText);
            menuMenu.MenuItems.Add(menuSaveTextAs);
            menuMenu.MenuItems.Add(menuSaveText);
            menuMenu.MenuItems.Add(menuExportSRT);
            menuMenu.MenuItems.Add(menuExportASS);
            menuMenu.MenuItems.Add(menuExportMp4);
            menuMenu.MenuItems.Add("-");
            menuMenu.MenuItems.Add(menuSettings);
            menuMenu.MenuItems.Add("-");
            menuMenu.MenuItems.Add(menuExit);
            menuStrip.MenuItems.Add(menuMenu);
            this.Menu = menuStrip;

            Application.AddMessageFilter(this);

            if (false)
            {
                #region speech recognition (swedish? read from audio stream? place into seaprate column; and just use as reference for quick glance of sentence length?)
#pragma warning disable CS0162 // Unreachable code detected
                var recognitionThread = new Thread(() =>
                {
                    var recognizer = new SpeechRecognitionEngine();

                    var grammar = new DictationGrammar();
                    recognizer.LoadGrammar(grammar);

                    var _completed = new ManualResetEvent(false);
                    recognizer.SpeechRecognized += delegate(object sender, SpeechRecognizedEventArgs e)
                    {
                        if (e.Result.Text.ToLower() == "exit")
                        {
                            _completed.Set();
                        }
                        else
                        {
                            this.Invoke(new EventHandler((a, b) =>
                            {
                                this._textView.TextInsert(this._textView.Caret.Index, e.Result.Text);
                                this._textView.Select(this._textView.Caret.Index + e.Result.Text.Length, 0);
                            }));
                        }
                    };

                    // TODO: recognizer.SetInputToWaveStream

                    var stream = this._audioVisualizer.GetAudioStream();
                    var speechAudioFormat = this._audioVisualizer.GetSpeechAudioFormat();
                    recognizer.SetInputToAudioStream(stream, speechAudioFormat);
                    recognizer.SetInputToDefaultAudioDevice(); // set the input of the speech recognizer to the default audio device
                    recognizer.RecognizeAsync(RecognizeMode.Multiple); // recognize speech asynchronous

                    _completed.WaitOne(); // wait until speech recognition is completed
                    recognizer.Dispose();
                });
                recognitionThread.IsBackground = true;
                recognitionThread.Start();
                #endregion
            }
#pragma warning restore CS0162 // Unreachable code detected

            // TODO: There's a small docking glitch when moving the note with the mouse
        }

        private void OnMenuExportSrtOnClick(object sender, EventArgs e)
        {
            var srtString = this._textView.Text;
            var i = 1;
            var searchStart = 0;
            Match match;
            while ((match = this._regex.Match(srtString, searchStart)).Success)
            {
                srtString = srtString.Remove(match.Index + match.Length - 1, 1);
                srtString = srtString.Remove(match.Index, 1);
                srtString = srtString.Insert(match.Index + match.Groups[1].Length + 1, "-");
                srtString = srtString.Insert(match.Index, i + "\n");

                searchStart = match.Index + match.Length;
                i++;
            }

            var file = new SaveFileDialog();
            if (file.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(file.FileName, srtString);
            }
        }

        private bool isMatchRemoval(Match match)
        {
            return match.Groups[3].Success && String.IsNullOrEmpty(match.Groups[3].Value) == false;
        }

        private class Person
        {
            public int Index { get; private set; }
            public String ColorFore { get; set; }
            public String ColorBack { get; set; }
            public String ColorOutline { get; set; }
            public String Font { get; set; }
            public List<Line> Lines { get; set; }
            public int LargestLineIndex { get; set; }

            public Person(int index)
            {
                this.Index = index;
                this.Font = "Arial";
                this.ColorBack = "00000000";
                this.ColorOutline = "00000000";
                this.Lines = new List<Line>();
            }
        }

        private class Line
        {
            public Person Person { get; set; }
            public Frame Frame { get; set; }
            public String Text { get; set; }
        }

        private class Frame
        {
            public long TimestampStart { get; set; }
            public long TimestampEnd { get; set; }
            public List<Line> Lines { get; set; }
        }

        private class Script
        {
            public List<Person> People { get; set; }
            public List<Frame> Frames { get; set; }

            public Script()
            {
                this.People = new List<Person>();
                this.Frames = new List<Frame>();
            }
        }

        private String GetSSA(String targetDirectoryPath)
        {

            // TODO:
            // * Output in ASS format
            // * Allow basic scripting
            //  * A note about something, which will appear at the top of the screen for X seconds
            //  * A counter of some sort, where the number can be increments/decremented
            // * Being able to easily write who is saying what.
            //  * Color the lines in the text editor after who's speaking, to make it easier to see the conversations.
            // * Make each person on a separate layer
            // * Make each person on a separate height
            // * Calculate each person's most lines, and calculate render positions based on that.
            //  * The same person's text should ALWAYS be in the SAME POSITION

            var lines = this._textView.Text.Split('\n');

            var script = new Script();

            script.People.Add(new Person(1) { ColorFore = "00FFFFFF" }); // White
            script.People.Add(new Person(2) { ColorFore = "0016FFF2" }); // Yellow
            script.People.Add(new Person(3) { ColorFore = "00FFBDBD" }); // Blue
            script.People.Add(new Person(4) { ColorFore = "009FFFB2" }); // Green
            script.People.Add(new Person(5) { ColorFore = "00BDBDFF" }); // Red
            script.People.Add(new Person(6) { ColorFore = "00C8C8C8" }); // Gray

            // Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding
            // Colour format: BBGGRR <-- long integer version, ie. the byte order 
            // TertiaryColour, aka OutlineColor <-- Use this new name
            // Booleans are true (-1) or false (0)

            var personNumberRegex = new Regex("^(\\d+):\\s*");
            var newPersonRegex = new Regex("^-([^\\s\\-])");

            var italicRegex = new Regex("\\*([^*]+?)\\*");
            var boldRegex = new Regex("\\*\\*([^*]+?)\\*\\*(?=[^*])");

            Person currentPerson = null;
            Frame currentFrame = null;

            int sourceChannel = this._audioVisualizer.CreateNewChannel(0, 0);

            try
            {
                var info = Bass.ChannelGetInfo(sourceChannel);
                var sourceLength = Bass.ChannelGetLength(sourceChannel);
                //double sourceLengthSeconds = Bass.BASS_ChannelBytes2Seconds(sourceChannel, );
                var bitsPerSample = info.Resolution == Resolution.Byte ? 8 : info.Resolution == Resolution.Float ? 32 : 16;
                var combine = Path.Combine(targetDirectoryPath, DateTime.Now.ToString("yyyy-MM-dd hhmmss") + ".wav");
                //var waveWriter = new WaveFileWriter(combine, info.Channels, info.Frequency, bitsPerSample, true);
                
                var fileStream = new FileStream(combine, FileMode.CreateNew);
                var waveWriter = new WaveFileWriter(fileStream, WaveFormat.FromChannel(sourceChannel));

                var offsetTimestampWith = 0L;
                var lastOutputEnd = 0L;
                for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                {
                    var text = this._textView.GetLineText(lineIndex);
                    if (String.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    var timestamp = this.GetTimeStampFromLineIndex(lineIndex);
                    if (timestamp != null)
                    {
                        // We've found a new timestamp.
                        // Let's save it and continue on to the next line.
                        // The previously known timestamp is what will be used for all lines of dialogue, until a new timestamp is detected.
                        var timestampStart = this.MatchToMilliseconds(timestamp.LineMatch, true);
                        var timestampEnd = this.MatchToMilliseconds(timestamp.LineMatch, false);
                        currentFrame = new Frame
                        {
                            TimestampStart = timestampStart - offsetTimestampWith,
                            TimestampEnd = timestampEnd - offsetTimestampWith,
                            Lines = new List<Line>()
                        };

                        if (timestamp.IsRemoval)
                        {
                            // The match is a removal. So we will output anything that was before this.
                            long startByteIndex = lastOutputEnd; // Bass.BASS_ChannelSeconds2Bytes(sourceChannel, (timestampStart / 1000d));
                            long endByteIndex = Bass.ChannelSeconds2Bytes(sourceChannel, (timestampStart / 1000d));
                            long byteCount = endByteIndex - startByteIndex;
                            lastOutputEnd = Bass.ChannelSeconds2Bytes(sourceChannel, (timestampEnd / 1000d));

                            var buffer = new byte[byteCount];
                            Bass.ChannelSetPosition(sourceChannel, startByteIndex);
                            Bass.ChannelGetData(sourceChannel, buffer, buffer.Length);
                            waveWriter.Write(buffer, buffer.Length);
                            //waveWriter.WriteNoConvert(buffer, buffer.Length);
                            
                            offsetTimestampWith += (currentFrame.TimestampEnd - currentFrame.TimestampStart);
                            continue;
                        }

                        script.Frames.Add(currentFrame);
                        continue;
                    }

                    if (currentFrame == null)
                    {
                        Console.WriteLine("Could not add line '{0}' because there was no known frame", lineIndex);
                        continue;
                    }

                    if (text.StartsWith("#"))
                    {
                        // We do not output this line! :)
                        continue;
                    }

                    // \u = underline, \s = strikeout, 
                    text = boldRegex.Replace(text, "{\\b2}$1{\\b1}");
                    text = italicRegex.Replace(text, "{\\i1}$1{\\i0}");

                    var personIndexMatch = personNumberRegex.Match(text);
                    var newPersonMatch = newPersonRegex.Match(text);

                    if (personIndexMatch.Success)
                    {
                        // The line starts with a number and color, which means it's a person counter.
                        var newPersonIndex = int.Parse(personIndexMatch.Groups[1].Value);
                        currentPerson = script.People[newPersonIndex - 1];
                        text = personNumberRegex.Replace(text, String.Empty);
                    }
                    else if (newPersonMatch.Success)
                    {
                        // A line starting with "-" then followed by text means a new person is speaking.
                        // When this system is used, there's only 2 people speaking.
                        // Otherwise the writer would be using the "1: Hello" format
                        var currentPersonIndex = (currentPerson == null) ? 1 : currentPerson.Index;
                        currentPerson = currentPersonIndex >= 2 || currentPersonIndex == -1 ? script.People[0] : script.People[1];
                        text = newPersonRegex.Replace(text, "$1");
                    }

                    if (currentPerson == null)
                    {
                        currentPerson = script.People[0];
                    }

                    var line = new Line
                    {
                        Frame = currentFrame,
                        Person = currentPerson,
                        Text = text
                    };

                    currentPerson.Lines.Add(line);
                    currentFrame.Lines.Add(line);
                }

                {
                    // Write whatever is left of the channel stream to the output file
                    long startByteIndex = lastOutputEnd; // Bass.BASS_ChannelSeconds2Bytes(sourceChannel, (timestampStart / 1000d));
                    long endByteIndex = sourceLength;
                    long byteCount = endByteIndex - startByteIndex;
                    //lastOutputEnd = Bass.BASS_ChannelSeconds2Bytes(sourceChannel, (timestampEnd / 1000d));

                    var buffer = new byte[byteCount];
                    Bass.ChannelSetPosition(sourceChannel, startByteIndex);
                    Bass.ChannelGetData(sourceChannel, buffer, buffer.Length);
                    //waveWriter.WriteNoConvert(buffer, buffer.Length);
                    waveWriter.Write(buffer, buffer.Length);
                }

                waveWriter.Dispose();
                //waveWriter.Close();
            }
            catch (Exception)
            {
            }
            finally
            {
                Bass.ChannelStop(sourceChannel);
                Bass.StreamFree(sourceChannel);
            }

            for (int i = script.Frames.Count - 1; i >= 0; i--)
            {
                // We move backwards to make it easier to deal with 
            }

            var result = "";
            
            // TODO:
            // 3 should be put on top of video!
            // Fix colors of 4 and 5!
            
            // Start writing the script info
            result += "[Script Info]\n";
            result += "; Generated by Transcriber\n";
            result += "Title: Translation file\n";
            result += "ScriptType: v4.00+\n";
            result += "WrapStyle: 0\n"; // 0: broken evenly, 2: no wrapping, \n does, 3: same as 0 but lower line gets wider
            result += "Collisions: Reverse\n";
            result += "ScaledBorderAndShadow: yes\n";
            result += "YCbCr Matrix: None\n";
            result += "PlayResX: 640\n";
            result += "PlayResY: 480\n";

            //PlayResY:			This is the height of the screen used by the script's author(s) when playing the script. SSA v4 will automatically select the nearest enabled setting, if you are using Directdraw playback.
            //PlayResX:			This is the width of the screen used by the script's author(s) when playing the script. SSA will automatically select the nearest enabled, setting if you are using Directdraw playback.
            //PlayDepth:		This is the colour depth used by the script's author(s) when playing the script. SSA will automatically select the nearest enabled setting if you are using Directdraw playback.

            // Start writing the stylings. Especially useful for multiple speakers.
            result += "\n[v4 Styles]\n";
            result += "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding\n";

            foreach (var person in script.People)
            {
                if (person.Lines.Count > 0)
                {
                    result += "Style: Style" + person.Index + ",Arial,24,&H" + person.ColorFore + ",&H000000FF,&H" + person.ColorOutline + ",&H" + person.ColorBack + ",-1,0,1,3,0,2,30,30,50,0,0\n";
                }
            }

            // Start writing the actual dialogue.
            result += "\n[Events]\n";
            //result += "Format: Marked, Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\n";
            result += "Format: Marked, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\n";

            foreach (var frame in script.Frames)
            {
                foreach (var line in frame.Lines)
                {
                    line.Person.LargestLineIndex = Math.Max(line.Person.LargestLineIndex, line.Text.Split(new[] { "\\N" }, StringSplitOptions.None).Length + 1);
                }
            }

            for (var i = 0; i < script.People.Count; i++)
            {
                if (script.People[i].Lines.Count == 0)
                {
                    script.People.RemoveAt(i);
                    i--;
                }
            }

            var personCentric = false;

            if (personCentric)
            {
                #region Person-Centric -- turned out to be really confusing
                foreach (var frame in script.Frames)
                {
                    var startString = TimeSpan.FromMilliseconds(frame.TimestampStart).ToString("h\\:mm\\:ss\\.ff");
                    var endString = TimeSpan.FromMilliseconds(frame.TimestampEnd).ToString("h\\:mm\\:ss\\.ff");

                    foreach (var person in script.People)
                    {
                        var linesToOutput = new List<String>();
                        for (var i = 0; i < frame.Lines.Count; i++)
                        {
                            var line = frame.Lines[i];
                            if (line.Person != person)
                            {
                                // This line does not belong to the current person.
                                continue;
                            }

                            // This line belongs to the current person.
                            // Let's start outputting those lines, and register how many lines they were.
                            //addedLines += line.Text.Split(new[] { "\\N" }, StringSplitOptions.None).Length + 1;
                            linesToOutput.Add(line.Text);
                        }

                        var dialogue = "Dialogue: Marked=0," + startString + "," + endString + ",Style" + person.Index + ",Person" + person.Index + ",0000,0000,0000,,";

                        /*int linesToAdd = person.LargestLineIndex - linesToOutput.Count;
                        while (linesToAdd > 0)
                        {
                            linesToAdd--;
                            dialogue += " \\N";
                        }*/

                        dialogue += String.Join("\\N", linesToOutput) + "\n";
                        result += dialogue;
                    }
                }
                #endregion
            }
            else
            {
                foreach (var frame in script.Frames)
                {
                    var startString = TimeSpan.FromMilliseconds(frame.TimestampStart).ToString("h\\:mm\\:ss\\.ff");
                    var endString = TimeSpan.FromMilliseconds(frame.TimestampEnd).ToString("h\\:mm\\:ss\\.ff");

                    var outputtedPeople = new List<Person>();
                    for (var i = frame.Lines.Count - 1; i >= 0; i--)
                    {
                        var frameLine = frame.Lines[i];
                        if (outputtedPeople.Contains(frameLine.Person) == false)
                        {
                            outputtedPeople.Add(frameLine.Person);

                            var linesToOutput = new List<String>();
                            foreach (var personLine in frame.Lines)
                            {
                                if (personLine.Person == frameLine.Person)
                                {
                                    linesToOutput.Add(personLine.Text);
                                }
                            }

                            var dialogue = "Dialogue: Marked=0," + startString + "," + endString + ",Style" + frameLine.Person.Index + ",Person" + frameLine.Person.Index + ",0000,0000,0000,,";
                            /*int linesToAdd = frameLine.Person.LargestLineIndex - linesToOutput.Count;
                            while (linesToAdd > 0)
                            {
                                linesToAdd--;
                                dialogue += " \\N";
                            }*/

                            if (frameLine.Person.Index == 3)
                            {
                                //dialogue += "";
                            }

                            dialogue += String.Join("\\N", linesToOutput) + "\n";
                            result += dialogue;
                        }
                    }
                }
            }

            // Start writing out any images or stuff.
            // Not really used, and probably will never be?
            // Maybe for something *really* silly, like adding heads to the left of the lines?
            result += "\n[Graphics]\n";

            return result;
        }

        private int CompareLinePersonOrder(Line a, Line b)
        {
            return a.Person.Index.CompareTo(b.Person.Index);
        }

        private void OnMenuExportAssOnClick(object sender, EventArgs e)
        {
            var fileDialog = new SaveFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                var result = this.GetSSA(Path.GetDirectoryName(fileDialog.FileName));
                File.WriteAllText(fileDialog.FileName, result);
            }
        }

        private void OnMenuExportMp4OnClick(object sender, EventArgs e)
        {
            DxPlay m_play = null;
            var form = new Form();
            form.Closing += (o, args) =>
            {
                m_play.Stop();
            };
            var panel = new Panel();
            panel.Dock = DockStyle.Fill;

            form.Controls.Add(panel);

            m_play = new DxPlay("Hello", panel);

            form.Show();

            m_play.Start();
        }

        private void OnMenuGenerateDiarization(object sender, EventArgs e)
        {
            this._audioVisualizer.GenerateSpeechDiarization();
        }

        void TextView_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (this.TypingPauseLength <= 0)
            {
                return;
            }

            if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Alt || e.KeyCode == Keys.Escape)
            {
                return;
            }

            if (e.KeyCode >= Keys.F1 && e.KeyCode <= Keys.F24)
            {
                return;
            }

            if ((e.Control || e.Shift) && e.KeyCode == Keys.Enter)
            {
                return;
            }

            var c = (char)e.KeyCode;
            char[] allowedChars = null;
            if (c == '\r' || c == '\n')
            {
                var currentLine = this._textView.GetLineFromCharIndex(this._textView.Caret.Index);
                if (currentLine != -1)
                {
                    var timestamp = this.GetTimeStampFromLineIndex(currentLine - 1);
                    if (timestamp != null)
                    {
                        allowedChars = new char[] { };
                    }
                }
            }

            if (allowedChars == null)
            {
                allowedChars = new char[] { '\b', '\n', '\r' };
            }

            if (Array.IndexOf(allowedChars, c) == -1 && char.IsControl(c))
            {
                return;
            }

            this._audioVisualizer.StartTemporaryPause(this._audioVisualizer.GetCurrentBytePosition(), this.TypingPauseLength, restartable: true);
        }

        private void AudioVisualizer_OnNoteRequest(object sender, AudioPaintEventArgs e)
        {
            var focusedLineIndex = this._textView.GetLineFromCharIndex(this._textView.Caret.Index);
            var lineCount = this._textView.LineCount;
            Note previousNote = null;
            Note focusedNote = null;
            for (var i = 0; i < lineCount; i++)
            {
                var lineText = this._textView.GetLineText(i);
                Interval interval;
                if (lineText.StartsWith("[") == false)
                {
                    interval = null;
                }
                else if (this._intervals.ContainsKey(lineText))
                {
                    interval = this._intervals[lineText];
                }
                else
                {
                    var fromMs = this.LineIndexToMilliseconds(i, true);
                    if (fromMs != -1)
                    {
                        var toMs = this.LineIndexToMilliseconds(i, false);
                        var fromByteIndex = this._audioVisualizer.SecondsToByteIndex(fromMs / 1000d);
                        var toByteIndex = this._audioVisualizer.SecondsToByteIndex(toMs / 1000d);

                        interval = new Interval(fromByteIndex, toByteIndex);
                    }
                    else
                    {
                        interval = null;
                    }

                    this._intervals.Add(lineText, interval);
                }

                if (interval == null)
                {
                    continue;
                }

                if (e.ViewPort.IsOverlapping(interval))
                {
                    if (previousNote != null && i > focusedLineIndex)
                    {
                        previousNote.IsFocused = true;
                        focusedNote = previousNote;
                    }

                    var newNote = new Note()
                    {
                        Id = "" + i,
                        Interval = interval,
                        Text = this.GetIntervalText(i)
                    };

                    previousNote = newNote;
                    e.Notes.Add(newNote);
                }
            }

            if (previousNote != null && focusedNote == null)
            {
                previousNote.IsFocused = true;
            }
        }

        private void OnAudioVisualizerOnNoteMoved(object sender, NoteMovedEventArgs args)
        {
            var lineIndex = int.Parse(args.Note.Id);
            var milliseconds = this._audioVisualizer.ByteIndexToMilliseconds(args.ByteIndex);

            if (args.Area == HitTestArea.NoteCenter)
            {
                // Both the start and the end should update itself.
                var previousStartMs = this.LineIndexToMilliseconds(lineIndex, true);
                var previousEndMs = this.LineIndexToMilliseconds(lineIndex, false);
                var distance = milliseconds - previousStartMs;

                this.UpdateTimeStamp(lineIndex, true, milliseconds);
                this.UpdateTimeStamp(lineIndex, false, previousEndMs + distance);
            }
            else
            {
                this.UpdateTimeStamp(lineIndex, args.Area == HitTestArea.NoteLeft, milliseconds);
                this._textView.Invalidate();
            }
        }

        private void OnAudioVisualizerOnNoteClicked(object sender, NoteClickedEventArgs args)
        {
            var lineIndex = int.Parse(args.Note.Id);
            //var milliseconds = this._audioVisualizer.ByteIndexToMilliseconds(args.ByteIndex);

            if (args.Area == HitTestArea.NoteCenter)
            {
                this._textView.Select(this.GetLastCharIndexOfSegment(lineIndex), 0);
            }
        }

        private int GetLastCharIndexOfSegment(int lineIndex)
        {
            while (lineIndex < this._textView.LineCount)
            {
                var lineText = this._textView.GetLineText(lineIndex);
                if (String.IsNullOrWhiteSpace(lineText))
                {
                    return this._textView.GetFirstCharIndexFromLine(lineIndex - 1) + this._textView.GetLineLength(lineIndex - 1);
                }

                lineIndex++;
            }

            return this._textView.TextLength;
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            this._audioVisualizer.Invalidate();
        }

        private void AudioVisualizerGotFocus(object sender, EventArgs e)
        {
            this._textView.Focus();
        }

        public void SaveText()
        {
            if (this._textView.CurrentFilePath == null)
            {
                var file = new SaveFileDialog();
                if (file.ShowDialog() == DialogResult.OK)
                {
                    this._textView.SaveAs(file.FileName);
                }
            }
            else
            {
                this._textView.Save();
            }
        }

        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_SYSKEYDOWN = 0x104;

        public bool PreFilterMessage(ref Message m)
        {
            // TODO: Listen for mouse4, mouse5
            if (m.Msg == WM_KEYDOWN)
            {
                var keyCode = (Keys)m.WParam;
                var control = (ModifierKeys & Keys.Control) == Keys.Control;
                var shift = (ModifierKeys & Keys.Shift) == Keys.Shift;
                var alt = (ModifierKeys & Keys.Alt) == Keys.Alt;

                try
                {
                    if (keyCode == Keys.Enter)
                    {
                        var exactPlacement = alt;
                        //var exactPlacement = true;

                        /*if (shift && control)
                        {
                            // Add new at end of previous until the cursor
                            this._textView.TextInsert(this._textView.Caret.Index, "\n\n");
                            this._textView.Select(this._textView.Caret.Index + 2, 0);
                            var earlierTimeStamp = this.GetClosestEarlierTimeStamp(this._textView.GetLineFromCharIndex(this._textView.Caret.Index));

                            var previousMilliseconds = earlierTimeStamp != null ? this.MatchToMilliseconds(earlierTimeStamp.LineMatch, false) : 0;
                            var currentMilliseconds = this._audioVisualizer.GetCurrentMillisecondPosition();
                            var distanceMilliseconds = currentMilliseconds - previousMilliseconds;

                            var newString = this.InsertTimeStamp(this._textView.Caret.Index, previousMilliseconds, length: distanceMilliseconds);
                            this._textView.Select(this._textView.Caret.Index + newString.Length, 0);
                            this._textView.TextInsert(this._textView.Caret.Index, "\n");
                            this._textView.Select(this._textView.Caret.Index + 1, 0);
                            return true;
                        }
                        else */
                        if (shift)
                        {
                            // Prolong previous and add new at cursor
                            // this.ProlongClosestEarlierTimestamp(exactPlacement);
                            
                            var startingLineIndex = this._textView.GetLineFromCharIndex(this._textView.Caret.Index);
                            var closestPreviousTimestamp = this.GetClosestEarlierTimeStamp(startingLineIndex);

                            if (closestPreviousTimestamp != null)
                            {
                                var target = this._audioVisualizer.GetCurrentMillisecondPosition();
                                this.UpdateTimeStamp(closestPreviousTimestamp.LineIndex, false, target);
                            }

                            this._textView.TextInsert(this._textView.Caret.Index, "\n\n");
                            this._textView.Select(this._textView.Caret.Index + 2, 0);
                            this.InsertNewTimestamp(exactPlacement);
                            this._textView.TextInsert(this._textView.Caret.Index, "\n");
                            this._textView.Select(this._textView.Caret.Index + 1, 0);

                            var newLineIndex = this._textView.GetLineFromCharIndex(this._textView.Caret.Index);

                            var latestTimestamp = this.GetClosestEarlierTimeStamp(newLineIndex);
                            
                            if (closestPreviousTimestamp != null && latestTimestamp != null)
                            {
                                var latestStartMs = this.MatchToMilliseconds(latestTimestamp.LineMatch, true);
                                this.UpdateTimeStamp(closestPreviousTimestamp.LineIndex, false, latestStartMs);
                            }

                            return true;
                        }
                        else if (control)
                        {
                            // Add new at cursor
                            this._textView.TextInsert(this._textView.Caret.Index, "\n\n");
                            this._textView.Select(this._textView.Caret.Index + 2, 0);
                            this.InsertNewTimestamp(exactPlacement);
                            this._textView.TextInsert(this._textView.Caret.Index, "\n");
                            this._textView.Select(this._textView.Caret.Index + 1, 0);
                            return true;
                        }
                    }
                    else if (control)
                    {
                        if (keyCode == Keys.Add)
                        {
                            this._audioVisualizer.VolumeIncrease();
                            return true;
                        }
                        else if (keyCode == Keys.Subtract)
                        {
                            this._audioVisualizer.VolumeDecrease();
                            return true;
                        }
                        else if (keyCode == Keys.S)
                        {
                            this.SaveText();
                            return true;
                        }
                        else if (keyCode == Keys.G)
                        {
                            InputBox ib = new InputBox();
                            var result = ib.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                var milliseconds = ib.MillisecondResult;
                                if (milliseconds > 0)
                                {
                                    this._audioVisualizer.SetCaretOffset(0.5d);
                                    this._audioVisualizer.SetLocationMs(milliseconds);
                                }
                            }

                            return true;
                        }

                        return false;
                    }
                    else if (keyCode == Keys.Escape)
                    {
                        this._audioVisualizer.TogglePlayPause();
                        return true;
                    }
                    else if (keyCode == Keys.F1)
                    {
                        this._audioVisualizer.SeekBackward(shift);
                        return true;
                    }
                    else if (keyCode == Keys.F2)
                    {
                        this._audioVisualizer.SeekForward(shift);
                        return true;
                    }
                    else if (keyCode == Keys.F3)
                    {
                        this._audioVisualizer.SetTempo(this._audioVisualizer.GetTempo() - 5f);
                        return true;
                    }
                    else if (keyCode == Keys.F4)
                    {
                        this._audioVisualizer.SetTempo(this._audioVisualizer.GetTempo() + 5f);
                        return true;
                    }
                    else if (keyCode == Keys.F5)
                    {
                        // Insert timestamp from the current time and X seconds into the future.
                        var inserted = this.InsertTimeStamp(this._textView.Caret.Index);
                        this._textView.Select(this._textView.Caret.Index + inserted.Length, 0);
                        this._textView.TextInsert(this._textView.Caret.Index, "\n");
                        this._textView.Select(this._textView.Caret.Index + 1, 0);

                        this._textView.Invalidate();
                        return true;
                    }
                    else if (keyCode == Keys.F6)
                    {
                        // Insert (X seconds length) or dock current timestamp to previous timestamp.
                        var currentLineIndex = this._textView.GetLineFromCharIndex(this._textView.Caret.Index);
                        var earlierTimeStampLine = this.GetClosestEarlierTimeStamp(currentLineIndex);
                        if (earlierTimeStampLine == null)
                        {
                            // Has no previous. Just insert at the caret.
                            var inserted = this.InsertTimeStamp(this._textView.Caret.Index);
                            this._textView.Select(this._textView.Caret.Index + inserted.Length, 0);
                            this._textView.TextInsert(this._textView.Caret.Index, "\n");
                            this._textView.Select(this._textView.Caret.Index + 1, 0);
                        }
                        else if (earlierTimeStampLine.WhiteSpaceLinesUntilMatch > 1)
                        {
                            // Found a previous. We should start this timestamp there.
                            var previousEnd = this.MatchToMilliseconds(earlierTimeStampLine.LineMatch, false);
                            var backtrackMs = this._audioVisualizer.GetCurrentMillisecondPosition() - previousEnd;
                            var inserted = this.InsertTimeStamp(this._textView.Caret.Index, previousEnd, Math.Max(2000L, 2000L + backtrackMs));
                            this._textView.Select(this._textView.Caret.Index + inserted.Length, 0);
                            this._textView.TextInsert(this._textView.Caret.Index, "\n");
                            this._textView.Select(this._textView.Caret.Index + 1, 0);
                        }
                        else
                        {
                            var previousTimeStampLine = this.GetClosestEarlierTimeStamp(earlierTimeStampLine.LineIndex - 1);
                            if (previousTimeStampLine != null)
                            {
                                var previousEnd = this.MatchToMilliseconds(previousTimeStampLine.LineMatch, false);
                                this.UpdateTimeStamp(earlierTimeStampLine.LineIndex, true, previousEnd);
                            }
                        }

                        this._textView.Invalidate();
                        return true;
                    }
                    else if (keyCode == Keys.F7 || keyCode == Keys.F8)
                    {
                        // Update start of current timestamp to the cursor location.
                        var currentLineIndex = this._textView.GetLineFromCharIndex(this._textView.Caret.Index);
                        var earlierTimeStampLine = this.GetClosestEarlierTimeStamp(currentLineIndex);
                        if (earlierTimeStampLine == null)
                        {
                            // Has no previous. Just insert at the caret.
                            var inserted = this.InsertTimeStamp(this._textView.Caret.Index);
                            this._textView.Select(this._textView.Caret.Index + inserted.Length, 0);
                            this._textView.TextInsert(this._textView.Caret.Index, "\n");
                            this._textView.Select(this._textView.Caret.Index + 1, 0);
                        }
                        else
                        {
                            this.UpdateTimeStamp(earlierTimeStampLine.LineIndex, keyCode == Keys.F7, this._audioVisualizer.GetCurrentMillisecondPosition());
                        }

                        this._textView.Invalidate();
                        return true;
                    }

                    // TODO: Figure out a way to see where there is silence and automatically dock to it when adding a timestamp
                }
                finally
                {
                    this._audioVisualizer.Invalidate();
                }
            }

            return false;
        }

        /// <summary>
        /// Send in a millisecond which should be clipped, so it cannot overlap an already existing timestamp.
        /// </summary>
        public long GetClippedMillisecond(bool isStart, long target, int lineIndex = -1)
        {
            if (isStart)
            {
                var previous = this.GetClosestEarlierTimeStamp(lineIndex - 1);
                if (previous != null)
                {
                    var ms = this.MatchToMilliseconds(previous.LineMatch, false);
                    return Math.Max(ms, target);
                }
            }
            else
            {
                var next = this.GetClosestLaterTimeStamp(lineIndex + 1);
                if (next != null)
                {
                    var ms = this.MatchToMilliseconds(next.LineMatch, true);
                    return Math.Min(ms, target);
                }
            }

            return target;
        }

        public String GetIntervalText(int lineIndex)
        {
            var earlierTimeStamp = this.GetClosestEarlierTimeStamp(lineIndex + 1);
            var startIndex = earlierTimeStamp == null
                ? 0
                : this._textView.GetFirstCharIndexFromLine(earlierTimeStamp.LineIndex) + this._textView.GetLineLength(earlierTimeStamp.LineIndex);

            var futureTimeStamp = this.GetClosestLaterTimeStamp(lineIndex + 1);
            var endIndex = futureTimeStamp == null
                ? this._textView.TextLength
                : this._textView.GetFirstCharIndexFromLine(futureTimeStamp.LineIndex);

            return this._textView.TextGet(startIndex, endIndex - startIndex).Trim();
        }

        public long LineIndexToMilliseconds(int lineIndex, bool start)
        {
            var lineText = this._textView.GetLineText(lineIndex);
            var matches = this._regex.Matches(lineText);
            if (matches.Count > 0)
            {
                return this.MatchToMilliseconds(matches[0], start);
            }

            return -1;
        }

        public long MatchToMilliseconds(Match match, bool start)
        {
            if (start)
            {
                return (long)TimeSpan.ParseExact(match.Groups[1].Value, "hh\\:mm\\:ss\\,fff", null).TotalMilliseconds;
            }

            return (long)TimeSpan.ParseExact(match.Groups[2].Value, "hh\\:mm\\:ss\\,fff", null).TotalMilliseconds;
        }

        public String GetTimeStampString(long milliseconds)
        {
            return TimeSpan.FromMilliseconds(milliseconds).ToString("hh\\:mm\\:ss\\,fff");
        }

        public class TimeStampLine
        {
            private int _lineIndexFirstEncounter = -1;
            public int LineIndex { get; set; }

            public bool IsRemoval { get; set; }

            public int LineIndexFirstEncounter
            {
                get { return (this._lineIndexFirstEncounter == -1) ? this.LineIndex : this._lineIndexFirstEncounter; }
                set { this._lineIndexFirstEncounter = value; }
            }

            public Match LineMatch { get; set; }
            public int WhiteSpaceLinesUntilMatch { get; set; }
        }

        public TimeStampLine GetClosestEarlierTimeStamp(int lineIndex)
        {
            var whitespaceLineCount = 0;
            var firstEncounterLineIndex = -1;
            while (lineIndex >= 0)
            {
                var lineText = this._textView.GetLineText(lineIndex);
                if (String.IsNullOrWhiteSpace(lineText))
                {
                    whitespaceLineCount++;
                }
                else
                {
                    firstEncounterLineIndex = lineIndex;
                }

                var timeStamp = this.GetTimeStampFromLineIndex(lineIndex, lineText);
                if (timeStamp != null)
                {
                    timeStamp.WhiteSpaceLinesUntilMatch = whitespaceLineCount;
                    timeStamp.LineIndexFirstEncounter = firstEncounterLineIndex;
                    return timeStamp;
                }

                lineIndex--;
            }

            return null;
        }

        public TimeStampLine GetClosestLaterTimeStamp(int lineIndex)
        {
            var whitespaceLineCount = 0;
            while (lineIndex < this._textView.LineCount)
            {
                var lineText = this._textView.GetLineText(lineIndex);
                if (String.IsNullOrWhiteSpace(lineText))
                {
                    whitespaceLineCount++;
                }

                var timeStamp = this.GetTimeStampFromLineIndex(lineIndex, lineText);
                if (timeStamp != null)
                {
                    timeStamp.WhiteSpaceLinesUntilMatch = whitespaceLineCount;
                    return timeStamp;
                }

                lineIndex++;
            }

            return null;
        }

        public TimeStampLine GetTimeStampFromLineIndex(int lineIndex, String lineText = null)
        {
            lineText = lineText ?? this._textView.GetLineText(lineIndex);
            if (String.IsNullOrEmpty(lineText) == false)
            {
                var matches = this._regex.Matches(lineText);
                if (matches.Count > 0)
                {
                    return new TimeStampLine
                    {
                        LineIndex = lineIndex,
                        LineMatch = matches[0],
                        IsRemoval = this.isMatchRemoval(matches[0])
                    };
                }
            }

            return null;
        }
    }
}
