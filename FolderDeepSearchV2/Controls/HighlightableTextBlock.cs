using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace FolderDeepSearchV2.Controls {
    public class HighlightableTextBlock : TextBlock {
        public static readonly DependencyProperty SelectionForegroundProperty =
            DependencyProperty.Register(
                "SelectionForeground",
                typeof(Brush),
                typeof(HighlightableTextBlock),
                new PropertyMetadata(new SolidColorBrush(Colors.WhiteSmoke)));

        public static readonly DependencyProperty SelectionBrushProperty =
            DependencyProperty.Register(
                "SelectionBrush",
                typeof(Brush),
                typeof(HighlightableTextBlock),
                new PropertyMetadata(new SolidColorBrush(Colors.DodgerBlue)));

        public static readonly DependencyProperty SelectionProperty =
            DependencyProperty.RegisterAttached(
                "Selection",
                typeof(string),
                typeof(HighlightableTextBlock),
                new PropertyMetadata((d, e) => ((HighlightableTextBlock) d).SelectAllOccourences((string) e.NewValue)));

        public static readonly DependencyProperty SelectionStartIndexProperty =
            DependencyProperty.Register(
                "SelectionStartIndex",
                typeof(int),
                typeof(HighlightableTextBlock),
                new PropertyMetadata(-1, (d, e) => ((HighlightableTextBlock) d).OnIndexableSelectionChanged()));

        public static readonly DependencyProperty SelectionEndIndexProperty =
            DependencyProperty.Register(
                "SelectionEndIndex",
                typeof(int),
                typeof(HighlightableTextBlock),
                new PropertyMetadata(-1, (d, e) => ((HighlightableTextBlock) d).OnIndexableSelectionChanged()));

        public Brush SelectionForeground {
            get => (Brush) GetValue(SelectionForegroundProperty);
            set => SetValue(SelectionForegroundProperty, value);
        }

        public Brush SelectionBrush {
            get => (Brush) GetValue(SelectionBrushProperty);
            set => SetValue(SelectionBrushProperty, value);
        }

        public string Selection {
            get => (string) GetValue(SelectionProperty);
            set => SetValue(SelectionProperty, value);
        }

        public int SelectionStartIndex {
            get => (int) GetValue(SelectionStartIndexProperty);
            set => SetValue(SelectionStartIndexProperty, value);
        }

        public int SelectionEndIndex {
            get => (int) GetValue(SelectionEndIndexProperty);
            set => SetValue(SelectionEndIndexProperty, value);
        }

        public void SelectAllOccourences(string selection) {
            string text = this.Text;
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(selection)) {
                return;
            }

            int index = text.IndexOf(selection, StringComparison.CurrentCultureIgnoreCase);
            if (index == -1) {
                return;
            }

            Brush selectionColor = this.SelectionBrush;
            Brush forecolor = this.SelectionForeground;

            this.Inlines.Clear();
            // idk stops it from freezing the whole app in the event of a bug
            for (int i = 0; i < 500; i++) {
                this.Inlines.AddRange(new Inline[] {
                    new Run(text.Substring(0, index)),
                    new Run(text.Substring(index, selection.Length)) {Background = selectionColor, Foreground = forecolor}
                });

                text = text.Substring(index + selection.Length);

                // search for next occurrence
                if ((index = text.IndexOf(selection, StringComparison.CurrentCultureIgnoreCase)) == -1) {
                    this.Inlines.Add(new Run(text));
                    break;
                }
            }
        }

        private void OnIndexableSelectionChanged() {
            int start = this.SelectionStartIndex;
            int end = this.SelectionEndIndex;
            if (start < 0 || end < 1) {
                return;
            }

            this.ClearSelections();
            this.AddSelection(start, end);
        }

        public void ClearSelections() {
            this.Inlines.Clear();
        }

        public bool AddSelection(int startIndex, int endIndex) {
            if (startIndex == -1 || endIndex == -1 || (endIndex - startIndex) == 0) {
                return false;
            }

            if (startIndex < endIndex) {
                throw new IndexOutOfRangeException($"Start index cannot be below end index: {startIndex} < {endIndex}");
            }

            string text = this.Text;
            if (endIndex > text.Length) {
                throw new IndexOutOfRangeException($"endIndex cannot be above length of the text: {endIndex} > {text.Length}");
            }

            if (string.IsNullOrEmpty(text)) {
                return false;
            }

            Brush selectionColor = this.SelectionBrush;
            Brush forecolor = this.SelectionForeground;

            this.Inlines.AddRange(new Inline[] {
                new Run(text.Substring(0, startIndex)),
                new Run(text.Substring(startIndex, endIndex - startIndex)) {
                    Background = selectionColor,
                    Foreground = forecolor
                }
            });

            return true;
        }
    }
}