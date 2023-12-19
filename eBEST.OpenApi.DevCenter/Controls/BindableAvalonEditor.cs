using ICSharpCode.AvalonEdit.Editing;
using System.ComponentModel;
using System.Windows;

namespace eBEST.OpenApi.DevCenter.Controls
{
    public class BindableAvalonEditor : ICSharpCode.AvalonEdit.TextEditor, INotifyPropertyChanged
    {
        public BindableAvalonEditor()
        {
            ShowLineNumbers = true;
            var leftMargins = TextArea.LeftMargins;
            foreach (var margin in leftMargins)
            {
                if (margin is LineNumberMargin lmargin)
                {
                    lmargin.Width = 30;
                    break;
                }
            }
        }
        /// <summary>
        /// A bindable Text property
        /// </summary>
        public new string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
                RaisePropertyChanged("Text");
            }
        }

        /// <summary>
        /// The bindable text property dependency property
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(BindableAvalonEditor),
                new FrameworkPropertyMetadata
                {
                    DefaultValue = default(string),
                    BindsTwoWayByDefault = true,
                    PropertyChangedCallback = OnDependencyPropertyChanged,
                }
            );

        protected static void OnDependencyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var target = (BindableAvalonEditor)obj;

            if (target.Document is not null)
            {
                var caretOffset = target.CaretOffset;
                var newValue = args.NewValue ?? string.Empty;
                string text = (string)newValue;
                target.Document.Text = text;
                target.CaretOffset = Math.Min(caretOffset, text.Length);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (Document is not null)
            {
                Text = Document.Text;
            }

            base.OnTextChanged(e);
        }



        public int UndoStackSizeLimite
        {
            get { return (int)GetValue(UndoStackSizeLimiteProperty); }
            set { SetValue(UndoStackSizeLimiteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UndoStackSizeLimite.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UndoStackSizeLimiteProperty =
            DependencyProperty.Register("UndoStackSizeLimite", typeof(int), typeof(BindableAvalonEditor), new PropertyMetadata(10, OnUndoStackSizeLimitePropertyChanged));

        private static void OnUndoStackSizeLimitePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (BindableAvalonEditor)d;

            if (target.Document is not null)
            {
                target.Document.UndoStack.SizeLimit = (int)e.NewValue;
            }
        }



        /// <summary>
        /// Raises a property changed event
        /// </summary>
        /// <param name="property">The name of the property that updates</param>
        public void RaisePropertyChanged(string property)
        {
            if (PropertyChanged is not null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
