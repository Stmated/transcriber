using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Speech.Recognition;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Eliason.AudioVisualizer;
using Eliason.Scrollbar;
using ManagedBass;
using Transcriber.GSSF;

namespace Transcriber
{
    public partial class Form1 : Form, IMessageFilter
    {
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private readonly AudioVisualizer _audioVisualizer;

        private readonly Dictionary<string, Interval> _intervals = new Dictionary<string, Interval>();
        private readonly Regex _regex = new Regex(@"\[(\d\d:\d\d:\d\d,\d\d\d) -> (\d\d:\d\d:\d\d,\d\d\d)\](-?)");
        private readonly ScrollableTextView _textView;

        public Form1()
        {
            InitializeComponent();

            Size = new Size(400, 700);

            // TODO: Rita bilder ovanpå varandra, så det inte blir glapp och inte påbörjar ett work i mitten på en millisekund
            // TODO: Visa alla inställningar i GUI när ändrar, så vet vad värdet är just nu

            // TODO: Fixa buggar med scrollbar i texteditor -- den uppdaterar inte när suddar
            // TODO: Följ efter i texteditorn, så att viewport alltid är där man senast klickade med musen, och att viewport följer med när man går runt med tangentbordet

            // TODO: Don't pause on Modifier + Enter
            // TODO: Some kind of queue system of actions to be done, so that no matter what keys are hit, what you want to happen after that should happen
            //          Right now there might be some weird stuff when for example pausing when typing, then stepping backwards, or pause when type and manually starting (will glitch a bit)
            // TODO: Något form av streck som pekar på vart man bör bryta raden för att få en bra längd när man lägger det i en video

            _audioVisualizer = new AudioVisualizer();
            _audioVisualizer.Location = new Point(0, 0);
            _audioVisualizer.Size = new Size(ClientSize.Width, 200);
            _audioVisualizer.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;
            _audioVisualizer.GotFocus += AudioVisualizerGotFocus;
            _audioVisualizer.NoteRequest += AudioVisualizer_OnNoteRequest;
            _audioVisualizer.NoteMoved += OnAudioVisualizerOnNoteMoved;
            _audioVisualizer.NoteClicked += OnAudioVisualizerOnNoteClicked;
            _audioVisualizer.SetFrequencyRange(5000);
            _audioVisualizer.RepeatLength = 0;
            _audioVisualizer.RepeatBackwards = 0;

            var container = new AdvScrollableControl();
            _textView = new ScrollableTextView(null, new NoSettings(), container);
            _textView.Font = new Font(_textView.Font.FontFamily, 8);
            _textView.KeepAutoSavesOnDispose = true;
            _textView.KeyUp += TextView_OnKeyUp;
            _textView.AddStyle(new TextStyleSpeaker());

            TypingPauseLength = 0;

            container.Control = _textView;
            container.Location = new Point(0, _audioVisualizer.Bottom);
            container.Size = new Size(ClientSize.Width, ClientSize.Height - _audioVisualizer.Height);
            container.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;

            Controls.Add(_audioVisualizer);
            Controls.Add(container);

            _textView.TextDocument.TextAppendLine("", 0);

            var menuMenu = new MenuItem("&Menu");

            var menuOpenAudio = new MenuItem("Open &audio...");
            menuOpenAudio.Click += delegate
            {
                var file = new OpenFileDialog();
                if (file.ShowDialog() == DialogResult.OK) _audioVisualizer.Open(file.FileName);
            };

            var menuOpenText = new MenuItem("Open &text...");
            menuOpenText.Click += delegate
            {
                var file = new OpenFileDialog();
                if (file.ShowDialog() == DialogResult.OK) _textView.Open(file.FileName);
            };

            var menuSaveText = new MenuItem("&Save text");
            menuSaveText.Click += delegate { SaveText(); };

            var menuSaveTextAs = new MenuItem("Sa&ve text as...");
            menuSaveTextAs.Click += delegate
            {
                var file = new SaveFileDialog();
                if (file.ShowDialog() == DialogResult.OK) _textView.SaveAs(file.FileName);
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
                formSettings.TextView = _textView;
                formSettings.MainForm = this;
                formSettings.AudioVisualizer = _audioVisualizer;

                formSettings.ShowDialog();
            };

            var menuExit = new MenuItem("E&xit");
            menuExit.Click += delegate { Close(); };

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
            Menu = menuStrip;

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
                            _completed.Set();
                        else
                            Invoke(new EventHandler((a, b) =>
                            {
                                _textView.TextInsert(_textView.Caret.Index, e.Result.Text);
                                _textView.Select(_textView.Caret.Index + e.Result.Text.Length, 0);
                            }));
                    };

                    // TODO: recognizer.SetInputToWaveStream

                    var stream = _audioVisualizer.GetAudioStream();
                    var speechAudioFormat = _audioVisualizer.GetSpeechAudioFormat();
                    recognizer.SetInputToAudioStream(stream, speechAudioFormat);
                    recognizer
                        .SetInputToDefaultAudioDevice(); // set the input of the speech recognizer to the default audio device
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

        public long TypingPauseLength { get; set; }

        public bool PreFilterMessage(ref Message m)
        {
            // TODO: Listen for mouse4, mouse5
            if (m.Msg == WM_KEYDOWN)
            {
                var keyCode = (Keys) m.WParam;
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

                            var startingLineIndex = _textView.GetLineFromCharIndex(_textView.Caret.Index);
                            var closestPreviousTimestamp = GetClosestEarlierTimeStamp(startingLineIndex);

                            if (closestPreviousTimestamp != null)
                            {
                                var target = _audioVisualizer.GetCurrentMillisecondPosition();
                                UpdateTimeStamp(closestPreviousTimestamp.LineIndex, false, target);
                            }

                            _textView.TextInsert(_textView.Caret.Index, "\n\n");
                            _textView.Select(_textView.Caret.Index + 2, 0);
                            InsertNewTimestamp(exactPlacement);
                            _textView.TextInsert(_textView.Caret.Index, "\n");
                            _textView.Select(_textView.Caret.Index + 1, 0);

                            var newLineIndex = _textView.GetLineFromCharIndex(_textView.Caret.Index);

                            var latestTimestamp = GetClosestEarlierTimeStamp(newLineIndex);

                            if (closestPreviousTimestamp != null && latestTimestamp != null)
                            {
                                var latestStartMs = MatchToMilliseconds(latestTimestamp.LineMatch, true);
                                UpdateTimeStamp(closestPreviousTimestamp.LineIndex, false, latestStartMs);
                            }

                            return true;
                        }
                        else if (control)
                        {
                            // Add new at cursor
                            _textView.TextInsert(_textView.Caret.Index, "\n\n");
                            _textView.Select(_textView.Caret.Index + 2, 0);
                            InsertNewTimestamp(exactPlacement);
                            _textView.TextInsert(_textView.Caret.Index, "\n");
                            _textView.Select(_textView.Caret.Index + 1, 0);
                            return true;
                        }
                    }
                    else if (control)
                    {
                        if (keyCode == Keys.Add)
                        {
                            _audioVisualizer.VolumeIncrease();
                            return true;
                        }
                        else if (keyCode == Keys.Subtract)
                        {
                            _audioVisualizer.VolumeDecrease();
                            return true;
                        }
                        else if (keyCode == Keys.S)
                        {
                            SaveText();
                            return true;
                        }
                        else if (keyCode == Keys.G)
                        {
                            var ib = new InputBox();
                            var result = ib.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                var milliseconds = ib.MillisecondResult;
                                if (milliseconds > 0)
                                {
                                    _audioVisualizer.SetCaretOffset(0.5d);
                                    _audioVisualizer.SetLocationMs(milliseconds);
                                }
                            }

                            return true;
                        }

                        return false;
                    }
                    else if (keyCode == Keys.Escape)
                    {
                        _audioVisualizer.TogglePlayPause();
                        return true;
                    }
                    else if (keyCode == Keys.F1)
                    {
                        _audioVisualizer.SeekBackward(shift);
                        return true;
                    }
                    else if (keyCode == Keys.F2)
                    {
                        _audioVisualizer.SeekForward(shift);
                        return true;
                    }
                    else if (keyCode == Keys.F3)
                    {
                        _audioVisualizer.SetTempo(_audioVisualizer.GetTempo() - 5f);
                        return true;
                    }
                    else if (keyCode == Keys.F4)
                    {
                        _audioVisualizer.SetTempo(_audioVisualizer.GetTempo() + 5f);
                        return true;
                    }
                    else if (keyCode == Keys.F5)
                    {
                        // Insert timestamp from the current time and X seconds into the future.
                        var inserted = InsertTimeStamp(_textView.Caret.Index);
                        _textView.Select(_textView.Caret.Index + inserted.Length, 0);
                        _textView.TextInsert(_textView.Caret.Index, "\n");
                        _textView.Select(_textView.Caret.Index + 1, 0);

                        _textView.Invalidate();
                        return true;
                    }
                    else if (keyCode == Keys.F6)
                    {
                        // Insert (X seconds length) or dock current timestamp to previous timestamp.
                        var currentLineIndex = _textView.GetLineFromCharIndex(_textView.Caret.Index);
                        var earlierTimeStampLine = GetClosestEarlierTimeStamp(currentLineIndex);
                        if (earlierTimeStampLine == null)
                        {
                            // Has no previous. Just insert at the caret.
                            var inserted = InsertTimeStamp(_textView.Caret.Index);
                            _textView.Select(_textView.Caret.Index + inserted.Length, 0);
                            _textView.TextInsert(_textView.Caret.Index, "\n");
                            _textView.Select(_textView.Caret.Index + 1, 0);
                        }
                        else if (earlierTimeStampLine.WhiteSpaceLinesUntilMatch > 1)
                        {
                            // Found a previous. We should start this timestamp there.
                            var previousEnd = MatchToMilliseconds(earlierTimeStampLine.LineMatch, false);
                            var backtrackMs = _audioVisualizer.GetCurrentMillisecondPosition() - previousEnd;
                            var inserted = InsertTimeStamp(_textView.Caret.Index, previousEnd,
                                Math.Max(2000L, 2000L + backtrackMs));
                            _textView.Select(_textView.Caret.Index + inserted.Length, 0);
                            _textView.TextInsert(_textView.Caret.Index, "\n");
                            _textView.Select(_textView.Caret.Index + 1, 0);
                        }
                        else
                        {
                            var previousTimeStampLine = GetClosestEarlierTimeStamp(earlierTimeStampLine.LineIndex - 1);
                            if (previousTimeStampLine != null)
                            {
                                var previousEnd = MatchToMilliseconds(previousTimeStampLine.LineMatch, false);
                                UpdateTimeStamp(earlierTimeStampLine.LineIndex, true, previousEnd);
                            }
                        }

                        _textView.Invalidate();
                        return true;
                    }
                    else if (keyCode == Keys.F7 || keyCode == Keys.F8)
                    {
                        // Update start of current timestamp to the cursor location.
                        var currentLineIndex = _textView.GetLineFromCharIndex(_textView.Caret.Index);
                        var earlierTimeStampLine = GetClosestEarlierTimeStamp(currentLineIndex);
                        if (earlierTimeStampLine == null)
                        {
                            // Has no previous. Just insert at the caret.
                            var inserted = InsertTimeStamp(_textView.Caret.Index);
                            _textView.Select(_textView.Caret.Index + inserted.Length, 0);
                            _textView.TextInsert(_textView.Caret.Index, "\n");
                            _textView.Select(_textView.Caret.Index + 1, 0);
                        }
                        else
                        {
                            UpdateTimeStamp(earlierTimeStampLine.LineIndex, keyCode == Keys.F7,
                                _audioVisualizer.GetCurrentMillisecondPosition());
                        }

                        _textView.Invalidate();
                        return true;
                    }

                    // TODO: Figure out a way to see where there is silence and automatically dock to it when adding a timestamp
                }
                finally
                {
                    _audioVisualizer.Invalidate();
                }
            }

            return false;
        }

        private void OnMenuExportSrtOnClick(object sender, EventArgs e)
        {
            var srtString = _textView.Text;
            var i = 1;
            var searchStart = 0;
            Match match;
            while ((match = _regex.Match(srtString, searchStart)).Success)
            {
                srtString = srtString.Remove(match.Index + match.Length - 1, 1);
                srtString = srtString.Remove(match.Index, 1);
                srtString = srtString.Insert(match.Index + match.Groups[1].Length + 1, "-");
                srtString = srtString.Insert(match.Index, i + "\n");

                searchStart = match.Index + match.Length;
                i++;
            }

            var file = new SaveFileDialog();
            if (file.ShowDialog() == DialogResult.OK) File.WriteAllText(file.FileName, srtString);
        }

        private bool isMatchRemoval(Match match)
        {
            return match.Groups[3].Success && string.IsNullOrEmpty(match.Groups[3].Value) == false;
        }

        private string GetSSA(string targetDirectoryPath)
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

            var lines = _textView.Text.Split('\n');

            var script = new Script();

            script.People.Add(new Person(1) {ColorFore = "00FFFFFF"}); // White
            script.People.Add(new Person(2) {ColorFore = "0016FFF2"}); // Yellow
            script.People.Add(new Person(3) {ColorFore = "00FFBDBD"}); // Blue
            script.People.Add(new Person(4) {ColorFore = "009FFFB2"}); // Green
            script.People.Add(new Person(5) {ColorFore = "00BDBDFF"}); // Red
            script.People.Add(new Person(6) {ColorFore = "00C8C8C8"}); // Gray

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

            var sourceChannel = _audioVisualizer.CreateNewChannel(0, 0);

            try
            {
                var info = Bass.ChannelGetInfo(sourceChannel);
                var sourceLength = Bass.ChannelGetLength(sourceChannel);
                //double sourceLengthSeconds = Bass.BASS_ChannelBytes2Seconds(sourceChannel, );
                var bitsPerSample = info.Resolution == Resolution.Byte ? 8 :
                    info.Resolution == Resolution.Float ? 32 : 16;
                var combine = Path.Combine(targetDirectoryPath, DateTime.Now.ToString("yyyy-MM-dd hhmmss") + ".wav");
                //var waveWriter = new WaveFileWriter(combine, info.Channels, info.Frequency, bitsPerSample, true);

                var fileStream = new FileStream(combine, FileMode.CreateNew);
                var waveWriter = new WaveFileWriter(fileStream, WaveFormat.FromChannel(sourceChannel));

                var offsetTimestampWith = 0L;
                var lastOutputEnd = 0L;
                for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                {
                    var text = _textView.GetLineText(lineIndex);
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    var timestamp = GetTimeStampFromLineIndex(lineIndex);
                    if (timestamp != null)
                    {
                        // We've found a new timestamp.
                        // Let's save it and continue on to the next line.
                        // The previously known timestamp is what will be used for all lines of dialogue, until a new timestamp is detected.
                        var timestampStart = MatchToMilliseconds(timestamp.LineMatch, true);
                        var timestampEnd = MatchToMilliseconds(timestamp.LineMatch, false);
                        currentFrame = new Frame
                        {
                            TimestampStart = timestampStart - offsetTimestampWith,
                            TimestampEnd = timestampEnd - offsetTimestampWith,
                            Lines = new List<Line>()
                        };

                        if (timestamp.IsRemoval)
                        {
                            // The match is a removal. So we will output anything that was before this.
                            var startByteIndex =
                                lastOutputEnd; // Bass.BASS_ChannelSeconds2Bytes(sourceChannel, (timestampStart / 1000d));
                            var endByteIndex = Bass.ChannelSeconds2Bytes(sourceChannel, timestampStart / 1000d);
                            var byteCount = endByteIndex - startByteIndex;
                            lastOutputEnd = Bass.ChannelSeconds2Bytes(sourceChannel, timestampEnd / 1000d);

                            var buffer = new byte[byteCount];
                            Bass.ChannelSetPosition(sourceChannel, startByteIndex);
                            Bass.ChannelGetData(sourceChannel, buffer, buffer.Length);
                            waveWriter.Write(buffer, buffer.Length);
                            //waveWriter.WriteNoConvert(buffer, buffer.Length);

                            offsetTimestampWith += currentFrame.TimestampEnd - currentFrame.TimestampStart;
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
                        // We do not output this line! :)
                        continue;

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
                        text = personNumberRegex.Replace(text, string.Empty);
                    }
                    else if (newPersonMatch.Success)
                    {
                        // A line starting with "-" then followed by text means a new person is speaking.
                        // When this system is used, there's only 2 people speaking.
                        // Otherwise the writer would be using the "1: Hello" format
                        var currentPersonIndex = currentPerson == null ? 1 : currentPerson.Index;
                        currentPerson = currentPersonIndex >= 2 || currentPersonIndex == -1
                            ? script.People[0]
                            : script.People[1];
                        text = newPersonRegex.Replace(text, "$1");
                    }

                    if (currentPerson == null) currentPerson = script.People[0];

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
                    var startByteIndex =
                        lastOutputEnd; // Bass.BASS_ChannelSeconds2Bytes(sourceChannel, (timestampStart / 1000d));
                    var endByteIndex = sourceLength;
                    var byteCount = endByteIndex - startByteIndex;
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

            for (var i = script.Frames.Count - 1; i >= 0; i--)
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
            result +=
                "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding\n";

            foreach (var person in script.People)
                if (person.Lines.Count > 0)
                    result += "Style: Style" + person.Index + ",Arial,24,&H" + person.ColorFore + ",&H000000FF,&H" +
                              person.ColorOutline + ",&H" + person.ColorBack + ",-1,0,1,3,0,2,30,30,50,0,0\n";

            // Start writing the actual dialogue.
            result += "\n[Events]\n";
            //result += "Format: Marked, Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\n";
            result += "Format: Marked, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\n";

            foreach (var frame in script.Frames)
            foreach (var line in frame.Lines)
                line.Person.LargestLineIndex = Math.Max(line.Person.LargestLineIndex,
                    line.Text.Split(new[] {"\\N"}, StringSplitOptions.None).Length + 1);

            for (var i = 0; i < script.People.Count; i++)
                if (script.People[i].Lines.Count == 0)
                {
                    script.People.RemoveAt(i);
                    i--;
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
                        var linesToOutput = new List<string>();
                        for (var i = 0; i < frame.Lines.Count; i++)
                        {
                            var line = frame.Lines[i];
                            if (line.Person != person)
                                // This line does not belong to the current person.
                                continue;

                            // This line belongs to the current person.
                            // Let's start outputting those lines, and register how many lines they were.
                            //addedLines += line.Text.Split(new[] { "\\N" }, StringSplitOptions.None).Length + 1;
                            linesToOutput.Add(line.Text);
                        }

                        var dialogue = "Dialogue: Marked=0," + startString + "," + endString + ",Style" + person.Index +
                                       ",Person" + person.Index + ",0000,0000,0000,,";

                        /*int linesToAdd = person.LargestLineIndex - linesToOutput.Count;
                        while (linesToAdd > 0)
                        {
                            linesToAdd--;
                            dialogue += " \\N";
                        }*/

                        dialogue += string.Join("\\N", linesToOutput) + "\n";
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

                            var linesToOutput = new List<string>();
                            foreach (var personLine in frame.Lines)
                                if (personLine.Person == frameLine.Person)
                                    linesToOutput.Add(personLine.Text);

                            var dialogue = "Dialogue: Marked=0," + startString + "," + endString + ",Style" +
                                           frameLine.Person.Index + ",Person" + frameLine.Person.Index +
                                           ",0000,0000,0000,,";
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

                            dialogue += string.Join("\\N", linesToOutput) + "\n";
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
                var result = GetSSA(Path.GetDirectoryName(fileDialog.FileName));
                File.WriteAllText(fileDialog.FileName, result);
            }
        }

        private void OnMenuExportMp4OnClick(object sender, EventArgs e)
        {
            DxPlay m_play = null;
            var form = new Form();
            form.Closing += (o, args) => { m_play.Stop(); };
            var panel = new Panel();
            panel.Dock = DockStyle.Fill;

            form.Controls.Add(panel);

            m_play = new DxPlay("Hello", panel);

            form.Show();

            m_play.Start();
        }

        private void OnMenuGenerateDiarization(object sender, EventArgs e)
        {
            _audioVisualizer.GenerateSpeechDiarization();
        }

        private void TextView_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (TypingPauseLength <= 0) return;

            if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Alt ||
                e.KeyCode == Keys.Escape) return;

            if (e.KeyCode >= Keys.F1 && e.KeyCode <= Keys.F24) return;

            if ((e.Control || e.Shift) && e.KeyCode == Keys.Enter) return;

            var c = (char) e.KeyCode;
            char[] allowedChars = null;
            if (c == '\r' || c == '\n')
            {
                var currentLine = _textView.GetLineFromCharIndex(_textView.Caret.Index);
                if (currentLine != -1)
                {
                    var timestamp = GetTimeStampFromLineIndex(currentLine - 1);
                    if (timestamp != null) allowedChars = new char[] { };
                }
            }

            if (allowedChars == null) allowedChars = new[] {'\b', '\n', '\r'};

            if (Array.IndexOf(allowedChars, c) == -1 && char.IsControl(c)) return;

            _audioVisualizer.StartTemporaryPause(_audioVisualizer.GetCurrentBytePosition(), TypingPauseLength, true);
        }

        private void AudioVisualizer_OnNoteRequest(object sender, AudioPaintEventArgs e)
        {
            var focusedLineIndex = _textView.GetLineFromCharIndex(_textView.Caret.Index);
            var lineCount = _textView.LineCount;
            Note previousNote = null;
            Note focusedNote = null;
            for (var i = 0; i < lineCount; i++)
            {
                var lineText = _textView.GetLineText(i);
                Interval interval;
                if (lineText.StartsWith("[") == false)
                {
                    interval = null;
                }
                else if (_intervals.ContainsKey(lineText))
                {
                    interval = _intervals[lineText];
                }
                else
                {
                    var fromMs = LineIndexToMilliseconds(i, true);
                    if (fromMs != -1)
                    {
                        var toMs = LineIndexToMilliseconds(i, false);
                        var fromByteIndex = _audioVisualizer.SecondsToByteIndex(fromMs / 1000d);
                        var toByteIndex = _audioVisualizer.SecondsToByteIndex(toMs / 1000d);

                        interval = new Interval(fromByteIndex, toByteIndex);
                    }
                    else
                    {
                        interval = null;
                    }

                    _intervals.Add(lineText, interval);
                }

                if (interval == null) continue;

                if (e.ViewPort.IsOverlapping(interval))
                {
                    if (previousNote != null && i > focusedLineIndex)
                    {
                        previousNote.IsFocused = true;
                        focusedNote = previousNote;
                    }

                    var newNote = new Note
                    {
                        Id = "" + i,
                        Interval = interval,
                        Text = GetIntervalText(i)
                    };

                    previousNote = newNote;
                    e.Notes.Add(newNote);
                }
            }

            if (previousNote != null && focusedNote == null) previousNote.IsFocused = true;
        }

        private void OnAudioVisualizerOnNoteMoved(object sender, NoteMovedEventArgs args)
        {
            var lineIndex = int.Parse(args.Note.Id);
            var milliseconds = _audioVisualizer.ByteIndexToMilliseconds(args.ByteIndex);

            if (args.Area == HitTestArea.NoteCenter)
            {
                // Both the start and the end should update itself.
                var previousStartMs = LineIndexToMilliseconds(lineIndex, true);
                var previousEndMs = LineIndexToMilliseconds(lineIndex, false);
                var distance = milliseconds - previousStartMs;

                UpdateTimeStamp(lineIndex, true, milliseconds);
                UpdateTimeStamp(lineIndex, false, previousEndMs + distance);
            }
            else
            {
                UpdateTimeStamp(lineIndex, args.Area == HitTestArea.NoteLeft, milliseconds);
                _textView.Invalidate();
            }
        }

        private void OnAudioVisualizerOnNoteClicked(object sender, NoteClickedEventArgs args)
        {
            var lineIndex = int.Parse(args.Note.Id);
            //var milliseconds = this._audioVisualizer.ByteIndexToMilliseconds(args.ByteIndex);

            if (args.Area == HitTestArea.NoteCenter) _textView.Select(GetLastCharIndexOfSegment(lineIndex), 0);
        }

        private int GetLastCharIndexOfSegment(int lineIndex)
        {
            while (lineIndex < _textView.LineCount)
            {
                var lineText = _textView.GetLineText(lineIndex);
                if (string.IsNullOrWhiteSpace(lineText))
                    return _textView.GetFirstCharIndexFromLine(lineIndex - 1) + _textView.GetLineLength(lineIndex - 1);

                lineIndex++;
            }

            return _textView.TextLength;
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            _audioVisualizer.Invalidate();
        }

        private void AudioVisualizerGotFocus(object sender, EventArgs e)
        {
            _textView.Focus();
        }

        public void SaveText()
        {
            if (_textView.CurrentFilePath == null)
            {
                var file = new SaveFileDialog();
                if (file.ShowDialog() == DialogResult.OK) _textView.SaveAs(file.FileName);
            }
            else
            {
                _textView.Save();
            }
        }

        /// <summary>
        ///     Send in a millisecond which should be clipped, so it cannot overlap an already existing timestamp.
        /// </summary>
        public long GetClippedMillisecond(bool isStart, long target, int lineIndex = -1)
        {
            if (isStart)
            {
                var previous = GetClosestEarlierTimeStamp(lineIndex - 1);
                if (previous != null)
                {
                    var ms = MatchToMilliseconds(previous.LineMatch, false);
                    return Math.Max(ms, target);
                }
            }
            else
            {
                var next = GetClosestLaterTimeStamp(lineIndex + 1);
                if (next != null)
                {
                    var ms = MatchToMilliseconds(next.LineMatch, true);
                    return Math.Min(ms, target);
                }
            }

            return target;
        }

        public string GetIntervalText(int lineIndex)
        {
            var earlierTimeStamp = GetClosestEarlierTimeStamp(lineIndex + 1);
            var startIndex = earlierTimeStamp == null
                ? 0
                : _textView.GetFirstCharIndexFromLine(earlierTimeStamp.LineIndex) +
                  _textView.GetLineLength(earlierTimeStamp.LineIndex);

            var futureTimeStamp = GetClosestLaterTimeStamp(lineIndex + 1);
            var endIndex = futureTimeStamp == null
                ? _textView.TextLength
                : _textView.GetFirstCharIndexFromLine(futureTimeStamp.LineIndex);

            return _textView.TextGet(startIndex, endIndex - startIndex).Trim();
        }

        public long LineIndexToMilliseconds(int lineIndex, bool start)
        {
            var lineText = _textView.GetLineText(lineIndex);
            var matches = _regex.Matches(lineText);
            if (matches.Count > 0) return MatchToMilliseconds(matches[0], start);

            return -1;
        }

        public long MatchToMilliseconds(Match match, bool start)
        {
            if (start)
                return (long) TimeSpan.ParseExact(match.Groups[1].Value, "hh\\:mm\\:ss\\,fff", null).TotalMilliseconds;

            return (long) TimeSpan.ParseExact(match.Groups[2].Value, "hh\\:mm\\:ss\\,fff", null).TotalMilliseconds;
        }

        public string GetTimeStampString(long milliseconds)
        {
            return TimeSpan.FromMilliseconds(milliseconds).ToString("hh\\:mm\\:ss\\,fff");
        }

        public TimeStampLine GetClosestEarlierTimeStamp(int lineIndex)
        {
            var whitespaceLineCount = 0;
            var firstEncounterLineIndex = -1;
            while (lineIndex >= 0)
            {
                var lineText = _textView.GetLineText(lineIndex);
                if (string.IsNullOrWhiteSpace(lineText))
                    whitespaceLineCount++;
                else
                    firstEncounterLineIndex = lineIndex;

                var timeStamp = GetTimeStampFromLineIndex(lineIndex, lineText);
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
            while (lineIndex < _textView.LineCount)
            {
                var lineText = _textView.GetLineText(lineIndex);
                if (string.IsNullOrWhiteSpace(lineText)) whitespaceLineCount++;

                var timeStamp = GetTimeStampFromLineIndex(lineIndex, lineText);
                if (timeStamp != null)
                {
                    timeStamp.WhiteSpaceLinesUntilMatch = whitespaceLineCount;
                    return timeStamp;
                }

                lineIndex++;
            }

            return null;
        }

        public TimeStampLine GetTimeStampFromLineIndex(int lineIndex, string lineText = null)
        {
            lineText = lineText ?? _textView.GetLineText(lineIndex);
            if (string.IsNullOrEmpty(lineText) == false)
            {
                var matches = _regex.Matches(lineText);
                if (matches.Count > 0)
                    return new TimeStampLine
                    {
                        LineIndex = lineIndex,
                        LineMatch = matches[0],
                        IsRemoval = isMatchRemoval(matches[0])
                    };
            }

            return null;
        }

        private class Person
        {
            public Person(int index)
            {
                Index = index;
                Font = "Arial";
                ColorBack = "00000000";
                ColorOutline = "00000000";
                Lines = new List<Line>();
            }

            public int Index { get; }
            public string ColorFore { get; set; }
            public string ColorBack { get; }
            public string ColorOutline { get; }
            public string Font { get; }
            public List<Line> Lines { get; }
            public int LargestLineIndex { get; set; }
        }

        private class Line
        {
            public Person Person { get; set; }
            public Frame Frame { get; set; }
            public string Text { get; set; }
        }

        private class Frame
        {
            public long TimestampStart { get; set; }
            public long TimestampEnd { get; set; }
            public List<Line> Lines { get; set; }
        }

        private class Script
        {
            public Script()
            {
                People = new List<Person>();
                Frames = new List<Frame>();
            }

            public List<Person> People { get; }
            public List<Frame> Frames { get; }
        }

        public class TimeStampLine
        {
            private int _lineIndexFirstEncounter = -1;
            public int LineIndex { get; set; }

            public bool IsRemoval { get; set; }

            public int LineIndexFirstEncounter
            {
                get => _lineIndexFirstEncounter == -1 ? LineIndex : _lineIndexFirstEncounter;
                set => _lineIndexFirstEncounter = value;
            }

            public Match LineMatch { get; set; }
            public int WhiteSpaceLinesUntilMatch { get; set; }
        }
    }
}