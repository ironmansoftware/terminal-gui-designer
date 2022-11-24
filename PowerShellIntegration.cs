using System;
using System.IO;
using System.Linq;
using System.Text;
using NStack;
using System.Management.Automation;

namespace Terminal.Gui.Designer
{
    public class PowerShellIntegration
    {
        private DesignerState _state;

        public PowerShellIntegration(DesignerState state)
        {
            _state = state;
        }

        public void Save()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("# This file was generated at " + DateTime.Now);
            stringBuilder.AppendLine("# Manually editing this file may result in issues with the designer");

            var id = _state.Window.Id;

            stringBuilder.AppendLine($"${id} = [{_state.Window.GetType()}]::new()");
            stringBuilder.AppendLine($"${id}.Id = '{id}'");

            var window = (FrameView)_state.Window;

            stringBuilder.AppendLine($"${id}.Title = '{window.Title}'");
            stringBuilder.AppendLine($"${id}.X = 0");
            stringBuilder.AppendLine($"${id}.Y = 0");
            stringBuilder.AppendLine($"${id}.Width = [Terminal.Gui.Dim]::Fill()");
            stringBuilder.AppendLine($"${id}.Height = [Terminal.Gui.Dim]::Fill()");

            WriteSubViews(_state.Window, stringBuilder);

            stringBuilder.AppendLine($"${_state.Window.Id}");

            File.WriteAllText(_state.FileName, stringBuilder.ToString());
        }

        private void WriteView(View view, StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"${view.Id} = [{view.GetType()}]::new()");
            WriteProperties(view, stringBuilder);
            WriteSubViews(view, stringBuilder);
        }

        private void WriteSubViews(View view, StringBuilder stringBuilder)
        {
            if (view is FrameView)
            {
                // Skip nested content view
                WriteSubViews(view.Subviews.First(), stringBuilder);
            }
            else if (view is ComboBox)
            {
                return;
            }
            else
            {
                var id = view.Id;
                if (id.IsEmpty)
                {
                    id = view.SuperView.Id;
                }

                foreach (var subView in view.Subviews)
                {
                    WriteView(subView, stringBuilder);
                    stringBuilder.AppendLine($"${id}.Add(${subView.Id})");
                }
            }
        }

        private void WriteProperties(View view, StringBuilder stringBuilder)
        {
            var left = $"${view.Id}";
            foreach (var property in view.GetType().GetProperties().Where(m => m.CanWrite))
            {
                if (Constants.SkippedProperties.Contains(property.Name)) continue;
                var value = property.GetValue(view);
                if (value == null) continue;

                if (value is string || value is ustring)
                {
                    stringBuilder.AppendLine($"{left}.{property.Name} = '{value}'");
                }
                else if (value is ustring[] arr)
                {
                    stringBuilder.AppendLine($"{left}.{property.Name} = @({arr.Aggregate((x, y) => x + "," + y)})");
                }
                else if (value is bool)
                {
                    stringBuilder.AppendLine($"{left}.{property.Name} = ${value}");
                }
                else if (value is Pos pos)
                {
                    var posInfo = new PosInfo(pos);
                    stringBuilder.AppendLine($"{left}.{property.Name} = {posInfo}");
                }
                else if (value is Dim dim)
                {
                    var dimInfo = new DimInfo(dim);
                    stringBuilder.AppendLine($"{left}.{property.Name} = {dimInfo}");
                }
                else if (property.PropertyType.IsEnum)
                {
                    stringBuilder.AppendLine($"{left}.{property.Name} = '{value}'");
                }
                else
                {
                    stringBuilder.AppendLine($"{left}.{property.Name} = {value}");
                }
            }
        }

        public void Load()
        {
            if (string.IsNullOrEmpty(_state.FileName)) return;

            using (var powerShell = PowerShell.Create())
            {
                var contents = File.ReadAllText(_state.FileName);

                powerShell.AddScript(contents);
                var objects = powerShell.Invoke();

                var window = objects.Select(m => m.BaseObject).OfType<View>().First().SuperView;

                _state.LoadWindow(window);
            }
        }
    }
}