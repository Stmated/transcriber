using System;
using System.Windows.Forms;
using Eliason.TextEditor;

namespace transcriber_winform
{
    public class NoNotifications : INotifier
    {
        public void Success(string title, string message)
        {
        }

        public void Info(string title, string message)
        {
        }

        public void Warning(string title, string message)
        {
        }

        public void Error(string title, string message, Exception exception = null)
        {
        }

        public AskResult AskYesNoCancel(string title, string message)
        {
            var result = MessageBox.Show(message, title, MessageBoxButtons.YesNoCancel);
            switch (result)
            {
                case DialogResult.Yes:
                    return AskResult.Yes;
                case DialogResult.No:
                    return AskResult.No;
                default:
                    return AskResult.Cancel;
            }
        }

        public NotifierInputResponse<T> AskInput<T>(NotifierInputRequest<T> request)
        {
            return new NotifierInputResponse<T>()
            {

            };
        }

        public NotifierSelectResponse<T> AskSelect<T>(NotifierSelectRequest<T> notifierSelectRequest)
        {
            return new NotifierSelectResponse<T>()
            {

            };
        }

        public bool AskYesNo(string title, string message)
        {
            var result = MessageBox.Show(message, title, MessageBoxButtons.YesNo);
            switch (result)
            {
                case DialogResult.Yes:
                    return true;
                default:
                    return false;
            }
        }
    }
}