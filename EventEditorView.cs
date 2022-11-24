using System;
using System.Collections.Generic;
using System.Linq;

namespace Terminal.Gui.Designer
{
    public class EventEditorView : Dialog
    {
        public EventEditorView(string name) : base(name)
        {
            Width = 50;
            Height = 15;

            var ok = new Button("Ok");
            var cancel = new Button("Cancel");

            ok.Clicked += () =>
            {
                if (ValueChanged != null)
                {
                    ValueChanged(this, "Hello");
                }
                Application.RequestStop();
            };

            cancel.Clicked += () =>
            {
                Application.RequestStop();
            };

            AddButton(ok);
            AddButton(cancel);
        }

        public event EventHandler<object> ValueChanged;
    }
}