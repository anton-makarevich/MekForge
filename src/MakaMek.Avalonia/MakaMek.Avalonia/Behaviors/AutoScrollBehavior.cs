using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Xaml.Interactivity;
using System.Collections.Specialized;
using System.Linq;

namespace Sanet.MakaMek.Avalonia.Behaviors;

public class AutoScrollBehavior : Behavior<ScrollViewer>
{
    private INotifyCollectionChanged? _currentCollection;

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject != null)
        {
            AssociatedObject.PropertyChanged += OnScrollViewerPropertyChanged;
            SubscribeToItemsControl();
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject == null) return;
        AssociatedObject.PropertyChanged -= OnScrollViewerPropertyChanged;
        UnsubscribeFromCurrentCollection();
    }

    private void OnScrollViewerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ContentControl.ContentProperty)
        {
            SubscribeToItemsControl();
        }
    }

    private void SubscribeToItemsControl()
    {
        UnsubscribeFromCurrentCollection();

        if (AssociatedObject == null) return;

        // Try to find ItemsControl either as direct content or as a child
        var itemsControl = AssociatedObject.Content as ItemsControl 
            ?? AssociatedObject.GetLogicalDescendants().OfType<ItemsControl>().FirstOrDefault();

        if (itemsControl?.Items is INotifyCollectionChanged collection)
        {
            _currentCollection = collection;
            _currentCollection.CollectionChanged += OnCollectionChanged;
        }
    }

    private void UnsubscribeFromCurrentCollection()
    {
        if (_currentCollection == null) return;
        _currentCollection.CollectionChanged -= OnCollectionChanged;
        _currentCollection = null;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            AssociatedObject?.ScrollToEnd();
        }
    }
}
