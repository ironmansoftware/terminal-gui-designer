using System;
using System.Collections.Generic;
using System.Linq;

namespace Terminal.Gui.Designer
{
    public class DesignerState
    {
        public View Window { get; private set; }
        public List<View> Views { get; } = new List<View>();
        public View SelectedView { get; private set; }

        private bool _isDirty;
        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            set
            {
                if (value)
                {
                    if (Dirty != null)
                    {
                        Dirty(this, null);
                    }
                }
                _isDirty = value;
            }
        }

        public string FileName { get; set; }

        private bool _dragging;

        private int _nextId;
        private Button _hiddenButton;

        public DesignerState()
        {
            Window = new FrameView("Window");
            Window.Id = "Window";

            _hiddenButton = new Button();
            _hiddenButton.Visible = false;
            Window.Add(_hiddenButton);

            Views.Add(Window);

            Application.RootMouseEvent += args =>
            {
                if (args.Flags.HasFlag(MouseFlags.Button1Pressed))
                {
                    var x = args.X - Window.Frame.X - 2;
                    var y = args.Y - Window.Frame.Y - 2;

                    if (SelectedView != null && x >= 0 && y >= 0)
                    {
                        _dragging = true;
                    }
                }
                if (args.Flags.HasFlag(MouseFlags.ReportMousePosition) && _dragging)
                {
                    IsDirty = true;
                    MoveSelected(args.X - Window.Frame.X - 2, args.Y - Window.Frame.Y - 2);
                }
                if (args.Flags.HasFlag(MouseFlags.Button1Released))
                {
                    _dragging = false;
                }
            };

            Window.KeyPress += args =>
            {
                if (args.KeyEvent.Key == Key.DeleteChar || args.KeyEvent.Key == Key.Delete)
                {
                    IsDirty = true;
                    DeleteSelected();
                }
            };
        }

        public void Add(View view)
        {
            if (Views.Contains(view)) return;

            view.KeyPress += args =>
            {
                if (view != SelectedView) return;

                args.Handled = true;
                Window.SetFocus();
                _hiddenButton.SetFocus();
                if (args.KeyEvent.Key == Key.CursorRight)
                {
                    SelectedView.X = SelectedView.X + 1;
                }

                if (args.KeyEvent.Key == Key.CursorLeft)
                {
                    SelectedView.X = SelectedView.X - 1;
                }

                if (args.KeyEvent.Key == Key.CursorDown)
                {
                    SelectedView.Y = SelectedView.Y + 1;
                }

                if (args.KeyEvent.Key == Key.CursorUp)
                {
                    SelectedView.Y = SelectedView.Y - 1;
                }
            };

            IsDirty = true;
            Window.Add(view);
            Views.Add(view);
            if (ControlAdded != null)
            {
                ControlAdded(this, new ControlEventArgs(view));
            }
        }

        public void Remove(View view)
        {
            if (!Views.Contains(view)) return;

            IsDirty = true;
            Window.Remove(view);
            Views.Remove(view);
            if (ControlRemoved != null)
            {
                ControlRemoved(this, new ControlEventArgs(view));
            }
        }

        public void Select(View view)
        {
            if (view == null) return;

            SelectedView = view;
            if (ControlSelected != null)
            {
                ControlSelected(this, new ControlSelectedEventArgs(view));
            }
        }

        public void MoveSelected(int x, int y)
        {
            if (SelectedView == Window) return;

            if (x < 0) x = 0;
            if (y < 0) y = 0;

            if (SelectedView != null)
            {
                SelectedView.X = x;
                SelectedView.Y = y;
            }
        }

        public void DeleteSelected()
        {
            if (SelectedView == Window) return;

            Remove(SelectedView);
            SelectedView = null;
        }

        public View CreateView(Type viewType)
        {
            IsDirty = true;
            var view = (View)Activator.CreateInstance(viewType);

            if (viewType == typeof(Button))
            {
                var button = (Button)view;
                button.Text = "Button";

                view = button;
            }

            if (viewType == typeof(ComboBox))
            {
                var button = (ComboBox)view;
                button.SetSource(new List<string>{
                    "Item1",
                    "Item2",
                    "Item3"
                });

                button.Height = 1;
                button.Width = 15;

                view = button;
            }


            if (viewType == typeof(Label))
            {
                var label = (Label)view;
                label.Text = "Label";
                label.Height = 1;
                label.Width = 5;

                view = label;
            }

            if (viewType == typeof(FrameView))
            {
                var fv = (FrameView)view;
                fv.Title = "FrameView";
                fv.Height = 5;
                fv.Width = 25;

                view = fv;
            }

            if (viewType == typeof(ProgressBar))
            {
                var fv = (ProgressBar)view;
                fv.Height = 1;
                fv.Width = 25;
                fv.Fraction = 10;

                view = fv;
            }

            if (viewType == typeof(RadioGroup))
            {
                var fv = (RadioGroup)view;
                fv.RadioLabels = new NStack.ustring[] {
                    "Item1",
                    "Item2",
                    "Item3"
                };

                view = fv;
            }

            view.Id = $"View{_nextId}";
            _nextId++;

            view.MouseClick += args =>
            {
                Select(view);
            };


            return view;
        }

        public void LoadWindow(View view)
        {
            var x = Window.X;
            var y = Window.Y;
            var width = Window.Width;
            var height = Window.Height;

            Application.Top.Remove(Window);
            Window = view;

            Window.X = x;
            Window.Y = y;
            Window.Width = width;
            Window.Height = height;

            Views.Clear();
            Views.Add(Window);

            foreach (var subView in view.Subviews.First().Subviews)
            {
                subView.MouseClick += args =>
                {
                    Select(subView);
                };
                Views.Add(subView);
            }

            Application.Top.Add(Window);
        }

        public event EventHandler<ControlEventArgs> ControlAdded;
        public event EventHandler<ControlEventArgs> ControlRemoved;
        public event EventHandler<ControlSelectedEventArgs> ControlSelected;
        public event EventHandler Dirty;
    }

    public class ControlEventArgs : EventArgs
    {
        public View Control { get; }

        public ControlEventArgs(View type)
        {
            Control = type;
        }
    }


    public class ControlSelectedEventArgs : EventArgs
    {
        public View SelectedControl { get; }

        public ControlSelectedEventArgs(View view)
        {
            SelectedControl = view;
        }
    }
}