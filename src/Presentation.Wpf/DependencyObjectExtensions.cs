using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Presentation.Wpf
{
    public static class DependencyObjectExtensions
    {
        extension(DependencyObject dependencyObject)
        {
            /// <summary>
            /// Recursively searches for and returns the first child element of a certain type in the visual tree.
            /// </summary>
            /// <typeparam name="T">The type of element to find.</typeparam>
            /// <returns>The first child element of a certain type, or null if no such element exists.</returns>
            /// <exception cref="ArgumentNullException">Thrown when the dependencyObject or name parameter is null.</exception>
            public T? FindChild<T>() where T : DependencyObject
            {
                ArgumentNullException.ThrowIfNull(dependencyObject);

                int childCount = VisualTreeHelper.GetChildrenCount(dependencyObject);
                for (int i = 0; i < childCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(dependencyObject, i);

                    if (child is T correctlyTyped)
                    {
                        return correctlyTyped;
                    }

                    T? descendent = FindChild<T>(child);
                    if (descendent != null)
                    {
                        return descendent;
                    }
                }

                return null;
            }

            /// <summary>
            /// Recursively searches for and returns the first child element with the specified name in the visual tree.
            /// </summary>
            /// <param name="name">The name of the child element to find.</param>
            /// <returns>The first child element with the specified name, or null if no such element exists.</returns>
            /// <exception cref="ArgumentNullException">Thrown when the dependencyObject or name parameter is null.</exception>
            public FrameworkElement? FindChildByName(string name)
            {
                ArgumentNullException.ThrowIfNull(dependencyObject);
                ArgumentException.ThrowIfNullOrEmpty(name);

                int childCount = VisualTreeHelper.GetChildrenCount(dependencyObject);

                for (int i = 0; i < childCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(dependencyObject, i);

                    if (child is FrameworkElement frameworkElement && frameworkElement.Name == name)
                        return frameworkElement;

                    var result = FindChildByName(child, name);
                    if (result is not null)
                        return result;
                }

                return null;
            }

            /// <summary>
            /// Recursively searches for and returns all child elements of a specified type within the visual tree of a given parent element.
            /// </summary>
            /// <typeparam name="T">The type of the child elements to find. Must derive from DependencyObject.</typeparam>
            /// <returns>An IEnumerable containing all found child elements of the specified type.</returns>
            /// <exception cref="ArgumentNullException">Thrown when the dependencyObject parameter is null.</exception>
            public IEnumerable<T> FindChildren<T>() where T : DependencyObject
            {
                ArgumentNullException.ThrowIfNull(dependencyObject);

                int childCount = VisualTreeHelper.GetChildrenCount(dependencyObject);
                for (int i = 0; i < childCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);

                    if (child is T typedChild)
                        yield return typedChild;

                    foreach (T childOfChild in FindChildren<T>(child))
                        yield return childOfChild;
                }
            }

            /// <summary>
            /// Finds the first parent of a specified type in the visual tree.
            /// </summary>
            /// <typeparam name="T">The type of the parent to find.</typeparam>
            /// <returns>The first parent of the specified type, or null if no such parent exists.</returns>
            /// <exception cref="ArgumentNullException">Thrown when the dependencyObject parameter is null.</exception>
            public T? FindParent<T>() where T : DependencyObject
            {
                ArgumentNullException.ThrowIfNull(dependencyObject);

                var obj = dependencyObject;
                while (obj != null)
                {
                    obj = VisualTreeHelper.GetParent(obj);
                    if (obj is T parent) return parent;
                }
                return null;
            }

            public T? FindAncestor<T>(bool includeSelf = false) where T : DependencyObject
            {
                ArgumentNullException.ThrowIfNull(dependencyObject);

                for (var current = includeSelf ? dependencyObject : VisualTreeHelper.GetParent(dependencyObject);
                     current is not null;
                     current = VisualTreeHelper.GetParent(current))
                {
                    if (current is T hit) return hit;
                }

                var logical = dependencyObject;
                while (logical is not null)
                {
                    logical = LogicalTreeHelper.GetParent(logical);
                    if (logical is T match) return match;
                }

                return null;
            }

            /// <summary>
            /// Gets the value of a dependency property in a thread-safe manner.
            /// </summary>
            public T GetValueSafe<T>(DependencyProperty property)
            {
                if (dependencyObject.CheckAccess())
                    return (T)dependencyObject.GetValue(property);

                return dependencyObject.Dispatcher.Invoke(() => (T)dependencyObject.GetValue(property), DispatcherPriority.Send);
            }

            /// <summary>
            /// Sets the value of a dependency property in a thread-safe manner.
            /// </summary>
            public void SetValueSafe<T>(DependencyProperty property, T value)
            {
                if (dependencyObject.CheckAccess())
                    dependencyObject.SetValue(property, value);
                else
                    dependencyObject.Dispatcher.Invoke(() => dependencyObject.SetValue(property, value), DispatcherPriority.Send);
            }

            /// <summary>
            /// Safely executes a getter function that accesses the DependencyObject on the UI thread.
            /// Use for complex property expressions or chained calls.
            /// </summary>
            /// <param name="getter">Function that returns the property value.</param>
            public T GetPropertySafe<T>(Func<T> getter)
            {
                if (dependencyObject.CheckAccess()) 
                    return getter();

                return dependencyObject.Dispatcher.Invoke(getter, DispatcherPriority.Send);
            }

            /// <summary>
            /// Safely executes a setter action that modifies the DependencyObject on the UI thread.
            /// Use for complex property assignments or multiple operations.
            /// </summary>
            /// <param name="setter">Action that sets the property value.</param>
            public void SetPropertySafe(Action setter)
            {
                if (dependencyObject.CheckAccess()) 
                    setter();
                else 
                    dependencyObject.Dispatcher.Invoke(setter, DispatcherPriority.Send);
            }
        }
    }
}
