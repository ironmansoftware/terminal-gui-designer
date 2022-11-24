using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NStack;

namespace Terminal.Gui.Designer
{
    public class DimEditorView : Dialog
    {
        private RadioGroup _radioGroup;
        private TextField _valueText;


        private Dictionary<ustring, DimType> _dimTypes = new Dictionary<ustring, DimType> {
            { "Absolute", DimType.Absolute },
            { "Fill", DimType.Fill },
            { "Percent", DimType.Percent }
        };

        public DimEditorView(Dim dim) : base("Edit Dimension")
        {
            Width = 50;
            Height = 7;

            var ok = new Button("Ok");
            var cancel = new Button("Cancel");

            var dimInfo = new DimInfo(dim);

            ok.Clicked += () =>
            {
                var dim1 = MakeDim();
                if (DimChanged != null && dim != null)
                {
                    DimChanged(this, new DimChangedEventArgs(dim1));
                }
                Application.RequestStop();
            };

            cancel.Clicked += () =>
            {
                Application.RequestStop();
            };

            var selectedType = _dimTypes.Values.Select((v, i) => new { Value = v, index = i }).First(m => m.Value == dimInfo.Type).index;

            _radioGroup = new RadioGroup(_dimTypes.Keys.ToArray(), selectedType);

            _radioGroup.DisplayMode = DisplayModeLayout.Horizontal;

            Add(_radioGroup);

            var valueLabel = new Label(dimInfo.ValueName)
            {
                X = 0,
                Y = Pos.Bottom(_radioGroup) + 1
            };

            _valueText = new TextField()
            {
                X = Pos.Right(valueLabel) + 1,
                Y = Pos.Bottom(_radioGroup) + 1,
                Width = Dim.Fill(),
                Text = dimInfo.Value.ToString()
            };

            Add(valueLabel);
            Add(_valueText);

            _radioGroup.SelectedItemChanged += (args) =>
            {
                var dim1 = MakeDim();
                var dimInfo1 = new DimInfo(dim1);
                _valueText.ReadOnly = dimInfo1.Type == DimType.Fill;
            };

            AddButton(ok);
            AddButton(cancel);
        }

        private Dim MakeDim()
        {
            if (!float.TryParse(_valueText.Text.ToString(), out float value))
            {
                return null;
            }

            var selectedType = _dimTypes.Values.Select((v, i) => new { Value = v, index = i }).First(m => m.index == _radioGroup.SelectedItem).Value;

            switch (selectedType)
            {
                case DimType.Absolute:
                    return Dim.Sized((int)value);
                case DimType.Fill:
                    return Dim.Fill();
                case DimType.Percent:
                    return Dim.Percent(value);
            }

            return null;
        }

        public event EventHandler<DimChangedEventArgs> DimChanged;
    }

    public class DimChangedEventArgs : EventArgs
    {
        public Dim Dim { get; }

        public DimChangedEventArgs(Dim dim)
        {
            Dim = dim;
        }
    }

    public class DimInfo
    {

        private readonly Regex argumentRegEx = new Regex("\\((.*)\\)");

        public DimInfo(Dim dim)
        {
            var str = dim.ToString();
            Match match = argumentRegEx.Match(str);
            var arguments = string.Empty;
            if (match.Success)
            {
                arguments = match.Groups[1].Value;
            }

            if (str.Contains("Absolute"))
            {
                Type = DimType.Absolute;
                if (int.TryParse(arguments, out int val))
                {
                    Value = val;
                }
            }

            if (str.Contains("Fill")) Type = DimType.Fill;
            if (str.Contains("DimView")) Type = DimType.Height;

            if (str.Contains("Factor"))
            {
                // factor=0.01, remaining=False
                var args = arguments.Split(',');
                Value = float.Parse(args[0].Split('=')[1]);
                Type = DimType.Percent;
            }
        }

        public DimType Type { get; set; }
        public float Value { get; set; }
        public string ValueName { get; } = "Value";

        public override string ToString()
        {
            switch (Type)
            {
                case DimType.Absolute:
                    return $"[Terminal.Gui.Dim]::Sized({Value})";
                case DimType.Fill:
                    return $"[Terminal.Gui.Dim]::Fill()";
                case DimType.Percent:
                    return $"[Terminal.Gui.Dim]::Percent({Value})";
            }

            return string.Empty;
        }
    }

    public enum DimType
    {
        Absolute,
        Fill,
        Height,
        Percent,
        Sized,
        Width
    }
}