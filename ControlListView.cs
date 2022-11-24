using System;
using System.Collections.Generic;
using System.Linq;

namespace Terminal.Gui.Designer
{
    public class ControlListView : FrameView
    {
        private readonly DesignerState _state;
        private readonly List<Type> _controlList = new List<Type>() {
            typeof(Button),
            typeof(CheckBox),
            typeof(ComboBox),
            typeof(DateField),
            typeof(FrameView),
            typeof(GraphView),
            typeof(HexView),
            typeof(Label),
            typeof(ListView),
            typeof(MenuBar),
            typeof(ProgressBar),
            typeof(RadioGroup),
            typeof(TableView),
            typeof(TimeField),
            typeof(TextField),
            typeof(TextValidateField),
            typeof(TextView),
            typeof(TreeView),
            typeof(ScrollView),
            typeof(ScrollBarView),
            typeof(StatusBar),
            typeof(Window)
        };

        private DateTime _lastEnter = DateTime.MinValue;

        public ControlListView(DesignerState state) : base("Toolbox")
        {
            _state = state;

            var controlsListView = new ListView()
            {
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            controlsListView.SetSource(_controlList.Select(m => m.Name).ToArray());

            Add(controlsListView);

            controlsListView.MouseClick += (args) =>
            {
                if (args.MouseEvent.Flags == MouseFlags.Button1DoubleClicked)
                {
                    if (DateTime.Now - _lastEnter < TimeSpan.FromSeconds(1))
                    {
                        return;
                    }

                    var item = _controlList[controlsListView.SelectedItem];
                    var view = _state.CreateView(item);

                    _state.Add(view);
                    _lastEnter = DateTime.Now;
                }
            };
        }
    }
}