using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using Eliason.TextEditor;
using Eliason.TextEditor.TextStyles;
using Eliason.TextEditor.TextTemplates;

namespace transcriber_winform
{
    public class NoSettings : ISettings
    {
        private INotifier _notifier = new NoNotifications();
        private Color _colorHighlightFore = Color.Green;
        private Color _colorHighlightBack = Color.Transparent;
        private Color _colorSpellcheckFore = Color.Transparent;
        private Color _colorSpellcheckBack = Color.Transparent;
        private Color _colorSpellcheckUnderline = Color.Red;
        private PenType _spellcheckUnderlineType = PenType.Dot;
        private bool _spellcheckUnderlineEnabled = true;
        private bool _inlineEnabled = true;
        private bool _ignoreAllCaps = true;
        private bool _ignoreWithNumbers = true;
        private bool _ignoreWithAsian = true;
        private string _ignoreLinesWithPrefix = null;
        private IEnumerable<string> _ignoredWords = new String[] { };
        private int _tabWidth = 4;
        private bool _autoSaveEnabled = true;
        private IEnumerable<string> _resourceDirectoryPaths = new String[] { };
        private Bitmap _bitmapInfo = new Bitmap(16, 16);
        private Bitmap _bitmapLineNumbers = new Bitmap(16, 16);
        private int _autoSaveInterval = 10000;

        public INotifier Notifier
        {
            get { return this._notifier; }
        }

        public TextStyleDisplayMode GetDisplayMode(TextStyleBase textStyle)
        {
            return TextStyleDisplayMode.Unspecified;
        }

        public TextStyleBase GetTextStyle(string key)
        {
            foreach (var textStyle in this.GetTextStyles())
            {
                if (textStyle.NameKey == key)
                {
                    return textStyle;
                }
            }

            return null;
        }

        public IEnumerable<TextStyleBase> GetTextStyles()
        {
            yield break;
        }

        public TokenTypeBase GetTemplateTokenType(string key)
        {
            foreach (var tokenType in this.GetTemplateTokenTypes())
            {
                if (tokenType.Key == key)
                {
                    return tokenType;
                }
            }

            return null;
        }

        public IEnumerable<TokenTypeBase> GetTemplateTokenTypes()
        {
            yield break;
        }

        public TokenAttributeTypeBase GetTemplateTokenAttributeType(string key)
        {
            foreach (var attributeType in this.GetTemplateTokenAttributeTypes())
            {
                if (attributeType.Key == key)
                {
                    return attributeType;
                }
            }

            return null;
        }

        public IEnumerable<TokenAttributeTypeBase> GetTemplateTokenAttributeTypes()
        {
            yield break;
        }

        public Color LineHighlightColor
        {
            get { return Color.LightYellow; }
        }

        public Color ColorHighlightFore
        {
            get { return this._colorHighlightFore; }
        }

        public Color ColorHighlightBack
        {
            get { return this._colorHighlightBack; }
        }

        public Color ColorSpellcheckFore
        {
            get { return this._colorSpellcheckFore; }
        }

        public Color ColorSpellcheckBack
        {
            get { return this._colorSpellcheckBack; }
        }

        public Color ColorSpellcheckUnderline
        {
            get { return this._colorSpellcheckUnderline; }
        }

        public PenType SpellcheckUnderlineType
        {
            get { return this._spellcheckUnderlineType; }
        }

        public bool SpellcheckUnderlineEnabled
        {
            get { return this._spellcheckUnderlineEnabled; }
        }

        /// <summary>
        /// true, depending on language, ofc
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public bool SpellcheckEnabled(CultureInfo cultureInfo)
        {
            return false;
        }

        /// <summary>
        /// True if spellchecking should be done inlined in the text control and updated while typing.
        /// </summary>
        public bool InlineEnabled
        {
            get { return this._inlineEnabled; }
        }

        /// <summary>
        /// True if spellchecking should not be done on a word written in all capital letters.
        /// </summary>
        public bool IgnoreAllCaps
        {
            get { return this._ignoreAllCaps; }
        }

        /// <summary>
        /// True if spellchecking should not be done on a word written containing numbers.
        /// </summary>
        public bool IgnoreWithNumbers
        {
            get { return this._ignoreWithNumbers; }
        }

        /// <summary>
        /// True if spellchecking should not be done on a word written containing asian characters.
        /// </summary>
        public bool IgnoreWithAsian
        {
            get { return this._ignoreWithAsian; }
        }

        public string IgnoreLinesWithPrefix
        {
            get { return this._ignoreLinesWithPrefix; }
        }

        public IEnumerable<string> IgnoredWords
        {
            get { return this._ignoredWords; }
        }

        public int TabWidth
        {
            get { return this._tabWidth; }
        }

        public int AutoSaveInterval
        {
            get { return this._autoSaveInterval; }
        }

        public bool AutoSaveEnabled
        {
            get { return this._autoSaveEnabled; }
        }

        public IEnumerable<string> ResourceDirectoryPaths
        {
            get { return this._resourceDirectoryPaths; }
        }

        /// <summary>
        /// This should give "info.png", wherever it might be
        /// </summary>
        public Bitmap BitmapInfo
        {
            get { return this._bitmapInfo; }
        }

        /// <summary>
        /// This should give "linenumbers.png" wherever it might be
        /// </summary>
        public Bitmap BitmapLineNumbers
        {
            get { return this._bitmapLineNumbers; }
        }

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