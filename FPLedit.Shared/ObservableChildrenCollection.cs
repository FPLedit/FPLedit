using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared
{
    [Serializable]
    public class ObservableChildrenCollection<T> : ObservableCollection<T> where T : Entity
    {
        private Entity parentEntity;

        public ObservableChildrenCollection(Entity parent, string childXName, Timetable tt)
        {
            parentEntity = parent;

            foreach (var c in parentEntity.Children.Where(x => x.XName == childXName)) // Filtert andere Elemente
                Add((T)Activator.CreateInstance(typeof(T), c, tt));

            CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        InsertItem(e);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        RemoveItem(e);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                    case NotifyCollectionChangedAction.Move:
                        RemoveItem(e);
                        InsertItem(e);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        parentEntity.Children.RemoveAll(x => x.XName == childXName);
                        break;
                    default:
                        throw new InvalidOperationException("Unerwartete Listenaktion");
                }
            };
        }

        private void InsertItem(NotifyCollectionChangedEventArgs e)
        {
            var idx = e.NewStartingIndex; // Index vorläufig ermitteln

            // Es können ja noch andere Nodes in den Children sein.
            if (idx != 0)
            {
                var elmBefore = this[idx - 1];
                idx = parentEntity.Children.IndexOf(elmBefore.XMLEntity) + 1;
            }
            else if (this.Count > idx + 1)
            {
                var elmAfter = this[idx + 1];
                idx = parentEntity.Children.IndexOf(elmAfter.XMLEntity); // Davor einfügen
            }
            parentEntity.Children.InsertRange(idx, e.NewItems.Cast<Track>().Select(t => t.XMLEntity));
        }

        private void RemoveItem(NotifyCollectionChangedEventArgs e)
        {
            var toRemove = e.OldItems.Cast<T>().Select(t => t.XMLEntity).ToList();
            parentEntity.Children.RemoveAll(t => toRemove.Contains(t));
        }
    }
}
