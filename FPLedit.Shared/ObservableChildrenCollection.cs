using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace FPLedit.Shared
{
    /// <summary>
    /// A collection of XML children of a given type that can be monitored and modified while modifying the underlying
    /// XML structure in the correct way.
    /// </summary>
    /// <typeparam name="T">Entity type of the children.</typeparam>
    [Templating.TemplateSafe]
    public class ObservableChildrenCollection<T> : ObservableCollection<T>, IChildrenCollection<T> where T : Entity
    {
        private readonly Entity parentEntity;
        private readonly string childXName;
        private readonly bool initialized;

        /// <summary>
        /// Creates a new ObservableChildrenCollection (OSC).
        /// </summary>
        /// <param name="parent">Parent entity, of which the childfren will be listed.</param>
        /// <param name="childXName">XML node name of the child type.</param>
        /// <param name="tt">Parent timetable.</param>
        public ObservableChildrenCollection(Entity parent, string childXName, Timetable tt)
        {
            initialized = false;
            parentEntity = parent;
            this.childXName = childXName;

            foreach (var c in parentEntity.Children.Where(x => x.XName == childXName)) // Filtert andere Elemente
            {
                var entity = Activator.CreateInstance(typeof(T), c, tt);
                if (entity == null)
                    throw new Exception("Entity initialization failed!");
                Add((T)entity);
            }

            initialized = true;
        }

        protected sealed override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!initialized)
                return;
            
            /*
             * CheatSheet for ObservableCollection.CollectionChanged
             * | Action        | NewItems       | OldItems      | NewStartIndex        | OldStartIndex          |
             * |---------------|----------------|---------------|----------------------|------------------------|
             * | Add == Insert | inserted items | null          | index of added items | -1                     |
             * | Remove        | null           | deleted items | -1                   | index of deleted items |
             * | Move          | moved items  ==  moved items   | new index            | old index              |
             * | Replace       | new item       | old item      | item index     ==      item index             |
             * | Reset         | null           | null          | -1                   | -1                     |
             */
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
            
            base.OnCollectionChanged(e);
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
            parentEntity.Children.InsertRange(idx, e.NewItems.Cast<T>().Select(t => t.XMLEntity));
        }

        private void RemoveItem(NotifyCollectionChangedEventArgs e)
        {
            var toRemove = e.OldItems.Cast<T>().Select(t => t.XMLEntity).ToList();
            parentEntity.Children.RemoveAll(t => toRemove.Contains(t));
        }

        /// <summary>
        /// Sort the collection, reflecting the changes also in the XML tree.
        /// </summary>
        /// <param name="comparer">Function to retreive the value being compared, for each child.</param>
        /// <typeparam name="TCompare">Type that will be compared for each child.</typeparam>
        public void Sort<TCompare>(Func<T, TCompare> comparer) where TCompare : IComparable
        {
            for (int n = Count; n > 1; n--)
            {
                // Bubblesort
                for (int i = 0; i < n - 1; i++)
                {
                    if (i + 1 == Count)
                        break; // Wir sind durch

                    var cur = this[i];
                    var next = this[i + 1];

                    if (comparer(cur).CompareTo(comparer(next)) > 0)
                        Move(i + 1, i);
                }
            }
        }
    }

    /// <summary>
    /// Base interface for a collection with movable and sortable elements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IChildrenCollection<T> : IList<T>
    {
        /// <summary>
        /// Move one element from <paramref name="oldIndex"/> to <paramref name="newIndex"/>.
        /// </summary>
        void Move(int oldIndex, int newIndex);
        
        /// <summary>
        /// Sort the collection.
        /// </summary>
        /// <param name="comparer">Function to retreive the value being compared, for each child.</param>
        /// <typeparam name="TCompare">Type that will be compared for each child.</typeparam>
        void Sort<TCompare>(Func<T, TCompare> comparer) where TCompare : IComparable;
    }
    
    public static class ObservableCollectionExtensions
    {
        public static int RemoveAll<T>(this IList<T> coll, Func<T, bool> condition)
        {
            var itemsToRemove = coll.Where(condition).ToArray();

            foreach (var itemToRemove in itemsToRemove)
            {
                coll.Remove(itemToRemove);
            }

            return itemsToRemove.Length;
        }

        public static void InsertRange<T>(this IList<T> coll, int index, IEnumerable<T> range)
        {
            foreach (var r in range)
                coll.Insert(index++, r);
        }
    }
}
