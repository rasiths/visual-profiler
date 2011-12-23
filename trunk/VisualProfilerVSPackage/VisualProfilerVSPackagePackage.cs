using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using JanVratislav.VisualProfilerVSPackage.View;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace JanVratislav.VisualProfilerVSPackage
{
   
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(VisualProfilerToolWindow))]
    [Guid(GuidList.guidVisualProfilerVSPackagePkgString)]
    public sealed class VisualProfilerVSPackagePackage : Package
    {
        public VisualProfilerVSPackagePackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                CommandID menuCommandIDTracing = new CommandID(GuidList.guidVisualProfilerVSPackageCmdSet, (int)PkgCmdIDList.cmdidStartVisualProfilerTracingMode);
                MenuCommand menuItemTracing = new MenuCommand(MenuItemCallback, menuCommandIDTracing );
                mcs.AddCommand( menuItemTracing );

                CommandID menuCommandIDSampling = new CommandID(GuidList.guidVisualProfilerVSPackageCmdSet, (int)PkgCmdIDList.cmdidStartVisualProfilerSamplingMode);
                MenuCommand menuItemSampling = new MenuCommand(MenuItemCallback2, menuCommandIDSampling);
                mcs.AddCommand(menuItemSampling);
              
            }
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "VisualProfilerVSPackage",
                       string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.ToString()),
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result));
        } 
        
        private void MenuItemCallback2(object sender, EventArgs e)
        {

            ToolWindowPane window = this.FindToolWindow(typeof(VisualProfilerToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create a window.");
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

    }
}
