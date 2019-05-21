using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

#nullable enable
namespace HideWasteBar
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad("{ADFC4E64-0397-11D1-9F4E-00A0C911004F}", PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad("{F1536EF8-92EC-443C-9ED7-FDADF150DA82}", PackageAutoLoadFlags.BackgroundLoad)]
    [Guid("efbf9aff-f52e-4e34-9bd0-13f0f01a50b7")]
    public sealed class HideWasteBarPackage : AsyncPackage
    {
        Window? _mainWindow;
        UIElement? _wasteBar;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            _mainWindow = Application.Current.MainWindow;
            if (_mainWindow != null) _mainWindow.LayoutUpdated += LayoutUpdated;
        }

        void LayoutUpdated(object sender, EventArgs e)
        {
            if (_wasteBar is null)
            {
                // Name="PART_ToolBarHost" Type="VsToolBarHostControl" Uid="VsToolBarHostControl"
                if (FindByUid(_mainWindow!, "VsToolBarHostControl") is {} toolbarHost)
                {
                    if (FindByUid(toolbarHost, "DraggableDockPanel") is {} wasteBarByUid)
                    {
                        _wasteBar = wasteBarByUid;
                    }
                    else if (
                        // Type="Grid"
                        GetFirstChild(toolbarHost) is Grid grid
                        // Name="DraggableDockPanel" Type="DraggableDockPanel" 
                        && GetFirstChild(grid) is {} wasteBar
                    )
                    {
                        _wasteBar = wasteBar;
                    }
                }
            }

            if (_wasteBar != null && _wasteBar.Visibility != Visibility.Collapsed)
                _wasteBar.Visibility = Visibility.Collapsed;
        }

        static UIElement? GetFirstChild(UIElement parent) =>
            VisualTreeHelper.GetChildrenCount(parent) > 0
                ? VisualTreeHelper.GetChild(parent, 0) as UIElement 
                : null;

        static UIElement? FindByUid(UIElement parent, string uid)
        {
            var queue = new Queue<UIElement>();
            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                UIElement element = queue.Dequeue();
                if (element.Uid == uid
                    || AutomationProperties.GetAutomationId(element) == uid
                    || AutomationProperties.GetName(element) == uid)
                {
                    return element;
                }

                int count = VisualTreeHelper.GetChildrenCount(element);
                for (var i = 0; i < count; i++)
                {
                    if (VisualTreeHelper.GetChild(element, i) is UIElement child)
                        queue.Enqueue(child);
                }
            }

            return null;
        }
    }
}
