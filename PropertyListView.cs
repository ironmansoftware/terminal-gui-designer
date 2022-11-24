using System;
using System.Collections.Generic;
using System.Linq;
using NStack;

namespace Terminal.Gui.Designer
{
    public class PropertyListView : FrameView
    {
        private View _targetView;
        private readonly DesignerState _state;

        public PropertyListView(DesignerState state) : base("Properties")
        {
            _state = state;
        }

        public void SetTargetView(View view)
        {
            if (view == null)
            {
                RemoveAll();
                return;
            }

            _targetView = view;

            RemoveAll();

            var scrollView = new ScrollView()
            {
                X = 0,
                Y = 0,
                Height = Dim.Fill(),
                Width = Dim.Fill(),
                ContentSize = new Size(50, 50),
                ShowVerticalScrollIndicator = true
            };

            int y = 0;
            foreach (var property in _targetView.GetType().GetProperties().Where(m => m.CanWrite).OrderBy(m => m.Name))
            {
                if (Constants.SkippedProperties.Contains(property.Name)) continue;

                var label = new Label(property.Name)
                {
                    Y = y,
                    X = 0,
                    Height = 1,
                    Width = Dim.Fill()
                };

                scrollView.Add(label);

                y++;

                var value = property.GetValue(_targetView);

                View valueView;

                if (property.PropertyType == typeof(Boolean))
                {
                    var chk = new CheckBox();
                    chk.Checked = (bool)property.GetValue(_targetView);
                    chk.Toggled += arg =>
                    {
                        _state.IsDirty = true;
                        property.SetValue(_targetView, chk.Checked);
                    };
                    valueView = chk;

                }
                else if (property.PropertyType.IsEnum)
                {
                    var text = new TextField
                    {
                        Text = value.ToString(),
                        ReadOnly = true
                    };

                    text.MouseClick += (args) =>
                    {
                        var currentValue = property.GetValue(_targetView);
                        var enumEditor = new EnumEditorView(currentValue, property.Name);
                        enumEditor.ValueChanged += (sender, args1) =>
                        {
                            _state.IsDirty = true;
                            property.SetValue(_targetView, args1);
                            text.Text = property.GetValue(_targetView).ToString();
                        };
                        Application.Run(enumEditor);
                    };

                    valueView = text;
                }
                else if (property.PropertyType == typeof(Dim))
                {
                    var dimEditorTxt = new TextField
                    {
                        Text = value.ToString(),
                        ReadOnly = true
                    };

                    dimEditorTxt.MouseClick += (args) =>
                    {
                        var currentValue = property.GetValue(_targetView);
                        var dimEditor = new DimEditorView(currentValue as Dim);
                        dimEditor.DimChanged += (sender, args1) =>
                        {
                            _state.IsDirty = true;
                            property.SetValue(_targetView, args1.Dim);
                            dimEditorTxt.Text = property.GetValue(_targetView).ToString();
                        };
                        Application.Run(dimEditor);
                    };

                    valueView = dimEditorTxt;
                }
                else if (property.PropertyType == typeof(Pos))
                {
                    var dimEditorTxt = new TextField
                    {
                        Text = value.ToString(),
                        ReadOnly = true
                    };

                    dimEditorTxt.MouseClick += (args) =>
                    {
                        var currentValue = property.GetValue(_targetView);
                        var dimEditor = new PosEditorView(currentValue as Pos, _state);
                        dimEditor.ValueChanged += (sender, args1) =>
                        {
                            _state.IsDirty = true;
                            property.SetValue(_targetView, args1.Pos);
                            dimEditorTxt.Text = property.GetValue(_targetView).ToString();
                        };
                        Application.Run(dimEditor);
                    };

                    valueView = dimEditorTxt;
                }
                else if (property.PropertyType == typeof(ustring[]))
                {
                    var dimEditorTxt = new TextField
                    {
                        Text = value.ToString(),
                        ReadOnly = true
                    };

                    dimEditorTxt.MouseClick += (args) =>
                    {
                        var currentValue = property.GetValue(_targetView);
                        var dimEditor = new StringListView(currentValue as ustring[], property.Name);
                        dimEditor.ValueChanged += (sender, args1) =>
                        {
                            _state.IsDirty = true;
                            property.SetValue(_targetView, args1);
                            dimEditorTxt.Text = property.GetValue(_targetView).ToString();
                        };
                        Application.Run(dimEditor);
                    };

                    valueView = dimEditorTxt;
                }
                else if (property.PropertyType == typeof(MenuBarItem[]))
                {
                    var dimEditorTxt = new TextField
                    {
                        Text = value.ToString(),
                        ReadOnly = true
                    };

                    dimEditorTxt.MouseClick += (args) =>
                    {
                        var currentValue = property.GetValue(_targetView);
                        var dimEditor = new StringListView(currentValue as ustring[], property.Name);
                        dimEditor.ValueChanged += (sender, args1) =>
                        {
                            _state.IsDirty = true;
                            property.SetValue(_targetView, args1);
                            dimEditorTxt.Text = property.GetValue(_targetView).ToString();
                        };
                        Application.Run(dimEditor);
                    };

                    valueView = dimEditorTxt;
                }
                else
                {
                    var textField = new TextField();

                    if (property.PropertyType == typeof(ustring))
                    {
                        textField.TextChanged += str =>
                        {
                            _state.IsDirty = true;
                            property.SetValue(_targetView, textField.Text);
                        };
                    }

                    if (value != null)
                    {
                        textField.Text = value.ToString();
                    }

                    valueView = textField;
                }

                valueView.Y = y;
                valueView.X = Pos.Left(label) + 1;
                valueView.Height = 1;
                valueView.Width = Dim.Fill();

                scrollView.Add(valueView);

                y++;
            }

            foreach (var property in _targetView.GetType().GetEvents().OrderBy(m => m.Name))
            {
                var label = new Label(property.Name)
                {
                    Y = y,
                    X = 0,
                    Height = 1,
                    Width = Dim.Fill()
                };

                scrollView.Add(label);

                y++;

                View valueView;
                var dimEditorTxt = new Button
                {
                    Text = "Edit",
                };

                dimEditorTxt.MouseClick += (args) =>
                {
                    var dimEditor = new EventEditorView(property.Name);
                    dimEditor.ValueChanged += (sender, args1) =>
                    {
                        _state.IsDirty = true;
                    };
                    Application.Run(dimEditor);
                };

                valueView = dimEditorTxt;

                valueView.Y = y;
                valueView.X = Pos.Left(label) + 1;
                valueView.Height = 1;
                valueView.Width = Dim.Fill();

                scrollView.Add(valueView);

                y++;
            }

            Add(scrollView);

            SetNeedsDisplay();
        }
    }
}