using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace transcriber_winform
{
    partial class Form1
    {
        private void InsertNewTimestamp(bool exactPlacement)
        {
            // Ctrl + Enter
            var caretIndex = this._textView.Caret.Index;
            var lineIndex = this._textView.GetLineFromCharIndex(caretIndex);

            var startCharIndex = this._textView.GetFirstCharIndexFromLine(lineIndex);

            long target;
            if (exactPlacement)
            {
                target = this._audioVisualizer.GetCurrentMillisecondPosition();
            }
            else
            {
                var byteIndex = this._audioVisualizer.GetPositionOfClosestSilence(this._audioVisualizer.GetCurrentBytePosition());
                target = this._audioVisualizer.ByteIndexToMilliseconds(byteIndex);
            }

            var newFromString = this.InsertTimeStamp(startCharIndex, target);
            this._textView.Select(this._textView.Caret.Index + newFromString.Length, 0);

            this._audioVisualizer.Invalidate();
        }

        private void ProlongClosestEarlierTimestamp(bool exactPlacement)
        {
            // Shift + Enter
            var caretIndex = this._textView.Caret.Index;
            var lineIndex = this._textView.GetLineFromCharIndex(caretIndex);
            var startLineIndex = lineIndex;

            while (lineIndex >= 0)
            {
                var lineText = this._textView.GetLineText(lineIndex);
                if (lineIndex != startLineIndex && String.IsNullOrWhiteSpace(lineText))
                {
                    break;
                }

                var matches = this._regex.Matches(lineText);
                if (matches.Count > 0)
                {
                    long target;
                    if (exactPlacement)
                    {
                        target = this._audioVisualizer.GetCurrentMillisecondPosition();
                    }
                    else
                    {
                        var byteIndex = this._audioVisualizer.GetPositionOfClosestSilence(this._audioVisualizer.GetCurrentBytePosition());
                        target = this._audioVisualizer.ByteIndexToMilliseconds(byteIndex);
                    }

                    this.UpdateTimeStamp(lineIndex, false, target);
                    this._textView.Invalidate();
                    break;
                }

                lineIndex--;
            }
        }

        public void UpdateTimeStamp(int lineIndex, bool start, long ms)
        {
            var lineText = this._textView.GetLineText(lineIndex);
            var firstLineIndex = this._textView.GetFirstCharIndexFromLine(lineIndex);
            var matches = this._regex.Matches(lineText);
            if (matches.Count == 0)
            {
                return;
            }

            var currentStart = this.MatchToMilliseconds(matches[0], true);
            var currentEnd = this.MatchToMilliseconds(matches[0], false);

            if (start)
            {
                ms = this.GetClippedMillisecond(true, ms, lineIndex);
                ms = Math.Min(currentEnd - 100, ms);
                var g = matches[0].Groups[1];
                this._textView.TextRemove(firstLineIndex + g.Index, g.Length);
                this._textView.TextInsert(firstLineIndex + g.Index, this.GetTimeStampString(ms));
            }
            else
            {
                ms = this.GetClippedMillisecond(false, ms, lineIndex);
                ms = Math.Max(currentStart + 100, ms);
                var g = matches[0].Groups[2];
                this._textView.TextRemove(firstLineIndex + g.Index, g.Length);
                this._textView.TextInsert(firstLineIndex + g.Index, this.GetTimeStampString(ms));
            }
        }

        public String InsertTimeStamp(int charIndex, long newCurrentMs = -1, long length = 2000)
        {
            if (newCurrentMs == -1)
            {
                newCurrentMs = this._audioVisualizer.GetCurrentMillisecondPosition();
            }

            length = Math.Max(100, length);

            var lineIndex = this._textView.GetLineFromCharIndex(charIndex);
            var from = newCurrentMs;
            var to = newCurrentMs + length;

            from = this.GetClippedMillisecond(true, from, lineIndex);
            to = this.GetClippedMillisecond(false, to, lineIndex);

            var newFromString = "[" + this.GetTimeStampString(from) + " -> " + this.GetTimeStampString(to) + "]";
            this._textView.TextInsert(charIndex, newFromString);
            return newFromString;
        }
    }
}
