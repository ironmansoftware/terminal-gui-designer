using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;

namespace Terminal.Gui.Designer
{
    [Cmdlet("Show", "TUIDesigner")]
    public class ShowDesignerCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var state = new DesignerState();
            Application.Init();
            var top = Application.Top;

            var fileNameStatus = new StatusItem(Key.Unknown, "Unsaved", () => { });
            var versionStatus = new StatusItem(Key.Unknown, base.MyInvocation.MyCommand.Module.Version.ToString(), () => { });

            var statusBar = new StatusBar(new StatusItem[] { fileNameStatus, versionStatus });

            Action<bool> save = (saveAs) =>
            {
                if (string.IsNullOrEmpty(state.FileName) || saveAs)
                {
                    var dialog = new SaveDialog("Save file", "Save designer file");
                    dialog.AllowedFileTypes = new string[] { ".ps1" };
                    Application.Run(dialog);

                    if (dialog.FilePath.IsEmpty || Directory.Exists(dialog.FilePath.ToString())) return;

                    state.FileName = dialog.FilePath.ToString();
                    fileNameStatus.Title = state.FileName;
                }

                fileNameStatus.Title = fileNameStatus.Title.TrimEnd("*");
                statusBar.SetNeedsDisplay();

                var powerShellIntegration = new PowerShellIntegration(state);

                try
                {
                    powerShellIntegration.Save();
                }
                catch (Exception ex)
                {
                    MessageBox.ErrorQuery("Failed", "Failed to save. " + ex.Message, "Ok");
                }
            };

            top.Add(new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_File", new MenuItem [] {
                        new MenuItem ("_Open", "", () => {
                            var dialog = new OpenDialog("Open file", "Open designer file");
                            dialog.CanChooseDirectories = false;
                            dialog.CanChooseFiles = true;
                            dialog.AllowsMultipleSelection = false;
                            dialog.AllowedFileTypes = new [] {".ps1"};

                            Application.Run(dialog);

                            if (dialog.FilePath.IsEmpty)
                            {
                                return;
                            }

                            try
                            {
                                state.FileName = dialog.FilePath.ToString();
                                var powerShellIntegration = new PowerShellIntegration(state);
                                powerShellIntegration.Load();
                                fileNameStatus.Title = state.FileName;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.ErrorQuery("Failed", "Failed to load Window: " + ex.Message, "Ok");
                            }
                        }),
                        new MenuItem ("_Save", "", () => {
                            save(false);
                        }),
                        new MenuItem ("Save As", "", () => {
                            save(true);
                        }),
                        new MenuItem ("_Quit", "", () => {
                            try
                            {
                                Application.Shutdown();
                            }
                            catch {}
                        })
                    }),
                    new MenuBarItem("_Help", new [] {
                        new MenuItem("_About", "", () => MessageBox.Query("About", $"PowerShell Pro Tools TUI Designer\nVersion: {base.MyInvocation.MyCommand.Module.Version.ToString()}\n", "Ok")),
                        new MenuItem("_Docs", "", () => {
                            var psi = new ProcessStartInfo
                            {
                                FileName = "https://docs.poshtools.com/powershell-pro-tools-documentation/tui-designer",
                                UseShellExecute = true
                            };
                            Process.Start (psi);
                        }),
                        new MenuItem("_Support", "", () => {
                            var psi = new ProcessStartInfo
                            {
                                FileName = "mailto:support@ironmansoftware.com",
                                UseShellExecute = true
                            };
                            Process.Start (psi);
                        })
                    })
                }));

            var controls = new ControlListView(state)
            {
                X = 0,
                Y = 1,
                Width = Dim.Percent(20),
                Height = Dim.Percent(30)
            };

            var properties = new PropertyListView(state)
            {
                X = 0,
                Y = Pos.Bottom(controls),
                Width = Dim.Percent(20),
                Height = Dim.Percent(50)
            };

            var definedControls = new DefinedControlsListView(state)
            {
                X = 0,
                Y = Pos.Bottom(properties),
                Width = Dim.Percent(20),
                Height = Dim.Fill() - 1
            };

            var designer = state.Window;

            designer.X = Pos.Right(controls);
            designer.Y = 1;
            designer.Height = Dim.Fill() - 1;
            designer.Width = Dim.Percent(80);
            designer.SetNeedsDisplay();

            state.ControlSelected += (sender, args) =>
            {
                properties.SetTargetView(args.SelectedControl);
            };

            state.ControlRemoved += (sender, args) =>
            {
                properties.SetTargetView(null);
            };

            state.Dirty += (sender, args) =>
            {
                if (!fileNameStatus.Title.EndsWith("*"))
                {
                    fileNameStatus.Title += "*";
                    statusBar.SetNeedsDisplay();
                }
            };

            top.Add(controls, properties, designer, definedControls, statusBar);

            try
            {
                Application.Run();
            }
            catch { }
        }
    }
}
