using System;
using System.Collections.Generic;
using System.Linq;
using NStack;

namespace Terminal.Gui.Designer
{
    public class StringListView : Dialog
    {
        public StringListView(ustring[] values, string name) : base(name)
        {
            Width = 50;
            Height = 15;

            var ok = new Button("Ok");
            var cancel = new Button("Cancel");

            var textField = new TextView();
            textField.Height = 12;
            textField.Width = Dim.Fill();

            var str = string.Join("\n", values.Select(m => m.ToString()));
            textField.Text = str;

            ok.Clicked += () =>
            {
                if (ValueChanged != null)
                {
                    var text = textField.Text.ToString();
                    var vals = text.Split('\n').Select(m => m.TrimEnd()).Where(m => !string.IsNullOrEmpty(m)).Select(m => ustring.Make(m)).ToArray();
                    ValueChanged(this, vals);
                }
                Application.RequestStop();
            };

            cancel.Clicked += () =>
            {
                Application.RequestStop();
            };

            ok.X = Pos.Bottom(textField);
            cancel.X = Pos.Bottom(textField);

            Add(textField);
            AddButton(ok);
            AddButton(cancel);
        }

        public event EventHandler<object> ValueChanged;
    }
}