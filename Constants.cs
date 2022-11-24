using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using NStack;

namespace Terminal.Gui.Designer
{
    public static class Constants
    {
        public static string[] SkippedProperties = new string[] { "HotKeySpecifier", "Frame", "Bounds", "ColorScheme" };

        public static string Version
        {
            get
            {
                using (var ps = PowerShell.Create())
                {
                    ps.AddCommand("Get-Module").AddParameter("ListAvailable").AddParameter("Name", "PowerShellProTools");
                    var psobject = ps.Invoke().First();
                    return psobject.Properties["Version"].Value.ToString();
                }
            }
        }
    }
}