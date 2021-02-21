using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Eliason.TextEditor;
using Eliason.TextEditor.TextStyles;
using Eliason.TextEditor.TextTemplates;

namespace Transcriber
{
    public class NoSettings : ISettings
    {
        public INotifier Notifier { get; } = new NoNotifications();

        public TextStyleDisplayMode GetDisplayMode(TextStyleBase textStyle)
        {
            return TextStyleDisplayMode.Unspecified;
        }

        public TextStyleBase GetTextStyle(string key)
        {
            foreach (var textStyle in GetTextStyles())
                if (textStyle.NameKey == key)
                    return textStyle;

            return null;
        }

        public IEnumerable<TextStyleBase> GetTextStyles()
        {
            yield break;
        }

        public TokenTypeBase GetTemplateTokenType(string key)
        {
            foreach (var tokenType in GetTemplateTokenTypes())
                if (tokenType.Key == key)
                    return tokenType;

            return null;
        }

        public IEnumerable<TokenTypeBase> GetTemplateTokenTypes()
        {
            yield break;
        }

        public TokenAttributeTypeBase GetTemplateTokenAttributeType(string key)
        {
            foreach (var attributeType in GetTemplateTokenAttributeTypes())
                if (attributeType.Key == key)
                    return attributeType;

            return null;
        }

        public IEnumerable<TokenAttributeTypeBase> GetTemplateTokenAttributeTypes()
        {
            yield break;
        }

        public Color LineHighlightColor => Color.LightYellow;

        public Color ColorHighlightFore { get; } = Color.Green;

        public Color ColorHighlightBack { get; } = Color.Transparent;

        public Color ColorSpellcheckFore { get; } = Color.Transparent;

        public Color ColorSpellcheckBack { get; } = Color.Transparent;

        public Color ColorSpellcheckUnderline { get; } = Color.Red;

        public PenType SpellcheckUnderlineType { get; } = PenType.Dot;

        public bool SpellcheckUnderlineEnabled { get; } = true;

        /// <summary>
        ///     true, depending on language, ofc
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public bool SpellcheckEnabled(CultureInfo cultureInfo)
        {
            return false;
        }

        /// <summary>
        ///     True if spellchecking should be done inlined in the text control and updated while typing.
        /// </summary>
        public bool InlineEnabled { get; } = true;

        /// <summary>
        ///     True if spellchecking should not be done on a word written in all capital letters.
        /// </summary>
        public bool IgnoreAllCaps { get; } = true;

        /// <summary>
        ///     True if spellchecking should not be done on a word written containing numbers.
        /// </summary>
        public bool IgnoreWithNumbers { get; } = true;

        /// <summary>
        ///     True if spellchecking should not be done on a word written containing asian characters.
        /// </summary>
        public bool IgnoreWithAsian { get; } = true;

        public string IgnoreLinesWithPrefix { get; } = null;

        public IEnumerable<string> IgnoredWords { get; } = new string[] { };

        public int TabWidth { get; } = 4;

        public int AutoSaveInterval { get; } = 10000;

        public bool AutoSaveEnabled { get; } = true;

        public IEnumerable<string> ResourceDirectoryPaths { get; } = new string[] { };

        /// <summary>
        ///     This should give "info.png", wherever it might be
        /// </summary>
        public Bitmap BitmapInfo { get; } = new Bitmap(16, 16);

        /// <summary>
        ///     This should give "linenumbers.png" wherever it might be
        /// </summary>
        public Bitmap BitmapLineNumbers { get; } = new Bitmap(16, 16);

        public void AddIgnoreWord(string word)
        {
        }

        public bool CheckIfWordIsValid(string word, string line)
        {
            return true;
        }

        public bool IsSpelledCorrectly(string word)
        {
            return true;
        }

        public bool IsTextColumnEnabled(string key)
        {
            return false;
        }

        public void SetTextColumnEnabled(string key, bool value)
        {
        }
    }
}