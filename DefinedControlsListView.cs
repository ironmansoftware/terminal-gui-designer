using System;
using System.Collections.Generic;
using System.Linq;

namespace Terminal.Gui.Designer
{
    public class DefinedControlsListView : FrameView
    {
        private readonly DesignerState _state;

        public DefinedControlsListView(DesignerState state) : base("Controls")
        {
            _state = state;

            var controlsListView = new ListView() {
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            controlsListView.SetSource(state.Views);

            Add(controlsListView);

            controlsListView.MouseClick += (args) => {
                var item = state.Views[controlsListView.SelectedItem];
                _state.Select(item);
            };
        }
    }
}