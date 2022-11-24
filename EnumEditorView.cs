using System;
using System.Collections.Generic;
using System.Linq;

namespace Terminal.Gui.Designer
{
    public class EnumEditorView : Dialog
    {
        private readonly object _value;

        public EnumEditorView(object value, string name) : base(name)
        {
            _value = value;

            Width = 50;
            Height = 15;

            var ok = new Button("Ok");
            var cancel = new Button("Cancel");

            var names = Enum.GetNames(value.GetType());
            var list = new ListView();

            ok.Clicked += () =>
            {
                if (ValueChanged != null)
                {
                    var name1 = names[list.SelectedItem];
                    var val = Enum.Parse(value.GetType(), name1);
                    ValueChanged(this, val);
                }
                Application.RequestStop();
            };

            cancel.Clicked += () =>
            {
                Application.RequestStop();
            };




            list.Height = Dim.Fill() - 1;
            list.Width = Dim.Fill();
            list.SetSource(names);

            ok.X = Pos.Bottom(list);
            cancel.X = Pos.Bottom(list);

            Add(list);
            AddButton(ok);
            AddButton(cancel);
        }

        public event EventHandler<object> ValueChanged;
    }
}