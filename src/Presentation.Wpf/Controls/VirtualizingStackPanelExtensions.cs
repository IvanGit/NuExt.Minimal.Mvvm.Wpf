using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;

namespace Presentation.Wpf.Controls
{
    public static class VirtualizingStackPanelExtensions
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo?> s_bringIndexIntoViewMethods = new();

        static VirtualizingStackPanelExtensions()
        {

        }

        /// <summary>
        /// Publically expose BringIndexIntoView.
        /// </summary>
        public static void BringIntoView(this VirtualizingStackPanel virtualizingPanel, int index)
        {
            ArgumentNullException.ThrowIfNull(virtualizingPanel);

            var mi = s_bringIndexIntoViewMethods.GetOrAdd(virtualizingPanel.GetType(), type => virtualizingPanel.GetType().GetMethod("BringIndexIntoView",
                BindingFlags.NonPublic | BindingFlags.Instance));
            Debug.Assert(mi != null);

            //virtualizingPanel.BringIndexIntoView(index);
            mi?.Invoke(virtualizingPanel, [index]);//TODO to delegate
        }
    }
}
