using System.Windows.Forms;
using Eliason.Scrollbar;
using Eliason.TextEditor.TextDocument.ByLines;

namespace transcriber_winform
{
    public partial class ScrollTest : Form
    {
        public ScrollTest()
        {
            InitializeComponent();

            var textDocument = new TextDocumentByLines();
            textDocument.TextAppendLine("Hello, World!", 0);

            var container = new AdvScrollableControl();
            var textView = new ScrollableTextView(textDocument, new NoSettings(), container);
            container.Control = textView;
            container.Dock = DockStyle.Fill;

            this.Controls.Add(container);
        }
    }
}
