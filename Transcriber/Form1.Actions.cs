using System;

namespace Transcriber
{
    partial class Form1
    {
        private void InsertNewTimestamp(bool exactPlacement)
        {
            // Ctrl + Enter
            var caretIndex = _textView.Caret.Index;
            var lineIndex = _textView.GetLineFromCharIndex(caretIndex);

            var startCharIndex = _textView.GetFirstCharIndexFromLine(lineIndex);

            long target;
            if (exactPlacement)
            {
                target = _audioVisualizer.GetCurrentMillisecondPosition();
            }
            else
            {
                var byteIndex = _audioVisualizer.GetPositionOfClosestSilence(_audioVisualizer.GetCurrentBytePosition());
                target = _audioVisualizer.ByteIndexToMilliseconds(byteIndex);
            }

            var newFromString = InsertTimeStamp(startCharIndex, target);
            _textView.Select(_textView.Caret.Index + newFromString.Length, 0);

            _audioVisualizer.Invalidate();
        }

        private void ProlongClosestEarlierTimestamp(bool exactPlacement)
        {
            // Shift + Enter
            var caretIndex = _textView.Caret.Index;
            var lineIndex = _textView.GetLineFromCharIndex(caretIndex);
            var startLineIndex = lineIndex;

            while (lineIndex >= 0)
            {
                var lineText = _textView.GetLineText(lineIndex);
                if (lineIndex != startLineIndex && string.IsNullOrWhiteSpace(lineText)) break;

                var matches = _regex.Matches(lineText);
                if (matches.Count > 0)
                {
                    long target;
                    if (exactPlacement)
                    {
                        target = _audioVisualizer.GetCurrentMillisecondPosition();
                    }
                    else
                    {
                        var byteIndex =
                            _audioVisualizer.GetPositionOfClosestSilence(_audioVisualizer.GetCurrentBytePosition());
                        target = _audioVisualizer.ByteIndexToMilliseconds(byteIndex);
                    }

                    UpdateTimeStamp(lineIndex, false, target);
                    _textView.Invalidate();
                    break;
                }

                lineIndex--;
            }
        }

        public void UpdateTimeStamp(int lineIndex, bool start, long ms)
        {
            var lineText = _textView.GetLineText(lineIndex);
            var firstLineIndex = _textView.GetFirstCharIndexFromLine(lineIndex);
            var matches = _regex.Matches(lineText);
            if (matches.Count == 0) return;

            var currentStart = MatchToMilliseconds(matches[0], true);
            var currentEnd = MatchToMilliseconds(matches[0], false);

            if (start)
            {
                ms = GetClippedMillisecond(true, ms, lineIndex);
                ms = Math.Min(currentEnd - 100, ms);
                var g = matches[0].Groups[1];
                _textView.TextRemove(firstLineIndex + g.Index, g.Length);
                _textView.TextInsert(firstLineIndex + g.Index, GetTimeStampString(ms));
            }
            else
            {
                ms = GetClippedMillisecond(false, ms, lineIndex);
                ms = Math.Max(currentStart + 100, ms);
                var g = matches[0].Groups[2];
                _textView.TextRemove(firstLineIndex + g.Index, g.Length);
                _textView.TextInsert(firstLineIndex + g.Index, GetTimeStampString(ms));
            }
        }

        public string InsertTimeStamp(int charIndex, long newCurrentMs = -1, long length = 2000)
        {
            if (newCurrentMs == -1) newCurrentMs = _audioVisualizer.GetCurrentMillisecondPosition();

            length = Math.Max(100, length);

            var lineIndex = _textView.GetLineFromCharIndex(charIndex);
            var from = newCurrentMs;
            var to = newCurrentMs + length;

            from = GetClippedMillisecond(true, from, lineIndex);
            to = GetClippedMillisecond(false, to, lineIndex);

            var newFromString = "[" + GetTimeStampString(from) + " -> " + GetTimeStampString(to) + "]";
            _textView.TextInsert(charIndex, newFromString);
            return newFromString;
        }
    }
}