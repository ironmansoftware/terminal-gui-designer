using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NStack;

namespace Terminal.Gui.Designer
{
    public class PosEditorView : Dialog
    {
        private RadioGroup _radioGroup;
        private TextField _valueText;
        private ListView _cmoViews;
        private readonly Dictionary<ustring, PosType> _posTypes = new Dictionary<ustring, PosType>();
        private readonly DesignerState _state;

        public PosEditorView(Pos pos, DesignerState state) : base("Edit Position")
        {
            _state = state;

            Width = 50;
            Height = 15;

            var ok = new Button("Ok");
            var cancel = new Button("Cancel");

            ok.Clicked += () =>
            {
                var pos1 = MakePos();
                if (ValueChanged != null && pos1 != null)
                {
                    ValueChanged(this, new PosChangedEventArgs(pos1));
                }
                Application.RequestStop();
            };

            cancel.Clicked += () =>
            {
                Application.RequestStop();
            };

            foreach (PosType posType in Enum.GetValues(typeof(PosType)))
            {
                _posTypes.Add(posType.ToString(), posType);
            }

            var posInfo = new PosInfo(pos);

            var selectedType = _posTypes.Values.Select((v, i) => new { Value = v, index = i }).First(m => m.Value == posInfo.Type).index;

            var typeLabel = new Label("Type")
            {
                X = 0,
                Y = 0
            };

            _radioGroup = new RadioGroup(_posTypes.Keys.ToArray(), selectedType)
            {
                X = 0,
                Y = 1
            };

            Add(typeLabel);
            Add(_radioGroup);

            var valueLabel = new Label("Value")
            {
                X = Pos.Right(_radioGroup),
                Y = 0
            };

            _valueText = new TextField()
            {
                X = Pos.Right(_radioGroup),
                Y = 1,
                Width = Dim.Fill(),
                Visible = posInfo.Type == PosType.At,
                Text = posInfo.Value.ToString()
            };

            _cmoViews = new ListView()
            {
                X = Pos.Right(_radioGroup),
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 2,
                Visible = posInfo.Type != PosType.At && posInfo.Type != PosType.AnchorEnd
            };

            _cmoViews.SetSource(_state.Views);

            Add(valueLabel);
            Add(_cmoViews);
            Add(_valueText);


            _radioGroup.SelectedItemChanged += (args) =>
            {
                var pos1 = MakePos();
                var posInfo1 = new PosInfo(pos1);
                _valueText.Visible = posInfo1.Type == PosType.At;
                _cmoViews.Visible = posInfo1.Type != PosType.At && posInfo1.Type != PosType.AnchorEnd;
                SetNeedsDisplay();
            };

            AddButton(ok);
            AddButton(cancel);
        }

        private Pos MakePos()
        {
            if (!float.TryParse(_valueText.Text.ToString(), out float value))
            {
                return null;
            }

            var selectedType = _posTypes.Values.Select((v, i) => new { Value = v, index = i }).First(m => m.index == _radioGroup.SelectedItem).Value;
            var selectedView = _state.Views.Select((v, i) => new { Value = v, index = i }).First(m => m.index == _cmoViews.SelectedItem).Value;

            switch (selectedType)
            {
                case PosType.At:
                    return Pos.At((int)value);
                case PosType.AnchorEnd:
                    return Pos.AnchorEnd();
                case PosType.Bottom:
                    return Pos.Bottom(selectedView);
                case PosType.Top:
                    return Pos.Top(selectedView);
                case PosType.Right:
                    return Pos.Right(selectedView);
                case PosType.Left:
                    return Pos.Left(selectedView);
            }

            return null;
        }

        public event EventHandler<PosChangedEventArgs> ValueChanged;
    }

    public class PosChangedEventArgs : EventArgs
    {
        public Pos Pos { get; }

        public PosChangedEventArgs(Pos pos)
        {
            Pos = pos;
        }
    }

    public class PosInfo
    {

        private static readonly Type _posFactorType = typeof(Pos).Assembly.GetType("Terminal.Gui.Pos+PosFactor");
        private static readonly Type _posAnchorEndType = typeof(Pos).Assembly.GetType("Terminal.Gui.Pos+PosAnchorEnd");
        private static readonly Type _posAbsoluteType = typeof(Pos).Assembly.GetType("Terminal.Gui.Pos+PosAbsolute");
        private static readonly Type _posCombineType = typeof(Pos).Assembly.GetType("Terminal.Gui.Pos+PosCombine");
        private static readonly Type _posViewType = typeof(Pos).Assembly.GetType("Terminal.Gui.Pos+PosView");
        private static readonly FieldInfo _posFactorFactorField = _posFactorType.GetField("factor", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _posAbsoluteValueField = _posAbsoluteType.GetField("n", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _posCombineLeftField = _posCombineType.GetField("left", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _posCombineRightField = _posCombineType.GetField("right", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _posViewTargetField = _posViewType.GetField("Target", BindingFlags.Instance | BindingFlags.Public);
        private static readonly FieldInfo _posViewSideField = _posViewType.GetField("side", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly Regex argumentRegEx = new Regex("\\((.*)\\)");
        private readonly Regex viewArgumentsRegEx = new Regex("Pos\\.View\\(([^\\+]*)");

        public PosInfo(Pos pos)
        {
            if (pos == null) return;

            if (pos.GetType() == _posAbsoluteType)
            {
                Type = PosType.At;
                Value = (int)_posAbsoluteValueField.GetValue(pos);
            }

            if (pos.GetType() == _posAnchorEndType)
            {
                Type = PosType.AnchorEnd;
            }

            if (pos.GetType() == _posCombineType)
            {

                var posView = _posCombineLeftField.GetValue(pos);
                View = (View)_posViewTargetField.GetValue(posView);
                Value = (int)_posViewSideField.GetValue(posView);

                switch (Value)
                {
                    case 0:
                        Type = PosType.Left;
                        break;
                    case 1:
                        Type = PosType.Top;
                        break;
                    case 2:
                        Type = PosType.Right;
                        break;
                    case 3:
                        Type = PosType.Bottom;
                        break;
                }
            }

            if (pos.GetType() == _posFactorType)
            {

            }
        }
        public PosType Type { get; }
        public int Value { get; }
        public View View { get; set; }

        public override string ToString()
        {
            switch (Type)
            {
                case PosType.AnchorEnd:
                    return $"[Terminal.Gui.Pos]::AnchorEnd()";
                case PosType.At:
                    return $"[Terminal.Gui.Pos]::At({Value})";
                case PosType.Bottom:
                    return $"[Terminal.Gui.Pos]::Bottom(${View.Id})";
                case PosType.Top:
                    return $"[Terminal.Gui.Pos]::Top(${View.Id})";
                case PosType.Left:
                    return $"[Terminal.Gui.Pos]::Left(${View.Id})";
                case PosType.Right:
                    return $"[Terminal.Gui.Pos]::Right(${View.Id})";
            }

            return string.Empty;
        }
    }

    public enum PosType
    {
        AnchorEnd,
        At,
        Bottom,
        Left,
        Right,
        Top
    }
}