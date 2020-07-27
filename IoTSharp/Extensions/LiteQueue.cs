/* Copyright 2018 by Nomadeon Software LLC. Licensed uinder MIT: https://opensource.org/licenses/MIT */
using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;
using System.Linq;
using System.Threading.Tasks;
#pragma warning disable CS1584,CS0419,CS1570
namespace LiteQueue
{
    public class QueueEntry<T>
    {
        public long Id { get; set; }
        public T Payload { get; set; }
        public bool IsCheckedOut { get; set; }

        public QueueEntry()
        {

        }

        public QueueEntry(T payload)
        {
            Payload = payload;
        }
    }
    /// <summary>
    /// Uses LiteDB to provide a persisted, thread safe, (optionally) transactional, FIFO queue.
    /// 
    /// Suitable for use on clients as a lightweight, portable alternative to MSMQ. Not recommended for use 
    /// on large server side applications due to performance limitations of LiteDB.
    /// </summary>
    public class LiteQueue<T>
    {
        ILiteCollection<QueueEntry<T>> _collection;
        bool _transactional = true;
        readonly object _dequeueLock = new object();

        /// <summary>
        /// Impacts operation of <see cref="Dequeue"/> method. Can only be set once in constructor.
        /// </summary>
        public bool IsTransactional
        {
            get
            {
                return _transactional;
            }
        }

        /// <summary>
        /// Creates a collection for you in the database
        /// </summary>
        /// <param name="db">The LiteDB database. You are responsible for its lifecycle (using/dispose)</param>
        /// <param name="collectionName">Name of the collection to create</param>
        /// <param name="transactional">Whether the queue should use transaction logic, default true</param>
        public LiteQueue(LiteDatabase db, string collectionName, bool transactional = true)
        {
            _collection = db.GetCollection<QueueEntry<T>>(collectionName);
            _transactional = transactional;
        }

        /// <summary>
        /// Uses the provided database collection
        /// </summary>
        /// <param name="collection">A LiteDB collection.</param>
        /// <param name="transactional">Whether the queue should use transaction logic, default true</param>
        public LiteQueue(ILiteCollection<QueueEntry<T>> collection, bool transactional = true)
        {
            _collection = collection;
            _transactional = transactional;
            _collection.EnsureIndex(x => x.Id);
            _collection.EnsureIndex(x => x.IsCheckedOut);
        }
        /// <summary>
        /// Creates a collection for you in the database, collection's name is  <typeparamref name="T"/>
        /// </summary>
        /// <param name="db">The LiteDB database. You are responsible for its lifecycle (using/dispose)</param>
        /// <param name="transactional">Whether the queue should use transaction logic, default true</param>
        public LiteQueue(LiteDatabase db, bool transactional = true)
        {
            _collection = db.GetCollection<QueueEntry<T>>(typeof(T).Name);
            _transactional = transactional;
        }

        /// <summary>
        /// Adds a single item to queue. See <see cref="Enqueue(IEnumerable{T})"/> for adding a batch.
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            QueueEntry<T> insert = new QueueEntry<T>(item);

            _collection.Insert(insert);
        }

        /// <summary>
        /// Adds a batch of items to the queue. See <see cref="Enqueue(T)"/> for adding a single item.
        /// </summary>
        /// <param name="items"></param>
        public void Enqueue(IEnumerable<T> items)
        {
            List<QueueEntry<T>> inserts = new List<QueueEntry<T>>();
            foreach (var item in items)
            {
                inserts.Add(new QueueEntry<T>(item));
            }

            _collection.InsertBulk(inserts);
        }

        /// <summary>
        /// Transactional queues:
        ///     Marks item as checked out but does not remove from queue. You are expected to later call <see cref="Commit(QueueEntry{T})"/> or <see cref="Abort(QueueEntry{T})"/>
        /// Non-transactional queues:
        ///     Removes item from queue with no need to call <see cref="Commit(QueueEntry{T})"/> or <see cref="Abort(QueueEntry{T})"/>
        /// </summary>
        /// <returns>An item if found or null</returns>
        public QueueEntry<T> Dequeue()
        {
            var result = Dequeue(1);
            if (result.Count == 0)
            {
                return null;
            }
            else
            {
                return result[0];
            }
        }

        /// <summary>
        /// Batch equivalent of <see cref="Dequeue"/>
        /// </summary>
        /// <param name="batchSize">The maximum number of items to dequeue</param>
        /// <returns>The items found or an empty collection (never null)</returns>
        public List<QueueEntry<T>> Dequeue(int batchSize)
        {
            if (_transactional)
            {
                lock (_dequeueLock)
                {
                    var items = _collection.Find(x => !x.IsCheckedOut, 0, batchSize);

                    // Capture the result before changing IsCheckedOut, otherwise items is being changed
                    var result = new List<QueueEntry<T>>(items);

                    foreach (var item in result)
                    {
                        item.IsCheckedOut = true;
                        _collection.Update(item);
                    }

                    return result;
                }
            }
            else
            {
                var items = _collection.Find(x => true, 0, batchSize);
                var result = new List<QueueEntry<T>>(items);

                foreach (var item in items)
                {
                    _collection.Delete(new BsonValue(item.Id));
                }

                return result;
            }
        }

        /// <summary>
        /// Obtains list of items currently checked out (but not yet commited or aborted) as a result of Dequeue calls on a transactional queue
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
        /// <returns>Items found or empty collection (never null)</returns>
        public List<QueueEntry<T>> CurrentCheckouts()
        {
            if (!_transactional)
            {
                throw new InvalidOperationException("Cannot call " + nameof(CurrentCheckouts) + " unless queue is transactional");
            }

            var records = _collection.Find(item => item.IsCheckedOut);
            return new List<QueueEntry<T>>(records);
        }

        /// <summary>
        /// Aborts all currently checked out items. Equivalent of calling <see cref="CurrentCheckouts"/> followed by <see cref="Abort(IEnumerable{QueueEntry{T}})"/>
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
        public void ResetOrphans()
        {
            if (!_transactional)
            {
                throw new InvalidOperationException("Cannot call " + nameof(ResetOrphans) + " unless queue is transactional");
            }

            var checkouts = CurrentCheckouts();
            Abort(checkouts);
        }

        /// <summary>
        /// Aborts a transaction on a single item. See <see cref="Abort(IEnumerable{QueueEntry{T}})"/> for batches.
        /// </summary>
        /// <param name="item">An item that was obtained from a <see cref="Dequeue"/> call</param>
        /// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
        public void Abort(QueueEntry<T> item)
        {
            if (!_transactional)
            {
                throw new InvalidOperationException("Cannot call " + nameof(Abort) + " unless queue is transactional");
            }
            else if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            item.IsCheckedOut = false;
            _collection.Update(item);
        }

        /// <summary>
        /// Aborts a transation on a group of items. See <see cref="Abort(QueueEntry{T})"/> for a single item.
        /// </summary>
        /// <param name="items">Items that were obtained from a <see cref="Dequeue"/> call</param>
        /// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
        public void Abort(IEnumerable<QueueEntry<T>> items)
        {
            foreach (var item in items)
            {
                Abort(item);
            }
        }

        /// <summary>
        /// Commits a transaction on a single item. See <see cref="Commit(IEnumerable{QueueEntry{T}})"/> for batches.
        /// </summary>
        /// <param name="item">An item that was obtained from a <see cref="Dequeue"/> call</param>
        /// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
        public void Commit(QueueEntry<T> item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (!_transactional)
            {
                throw new InvalidOperationException("Cannot call " + nameof(Commit) + " unless queue is transactional");
            }

            BsonValue id = new BsonValue(item.Id);
            _collection.Delete(id);
        }


        /// <summary>
        /// Commits a transation on a group of items. See <see cref="Commit(QueueEntry{T})/> for a single item.
        /// </summary>
        /// <param name="items">Items that were obtained from a <see cref="Dequeue"/> call</param>
        /// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
        public void Commit(IEnumerable<QueueEntry<T>> items)
        {
            foreach (var item in items)
            {
                Commit(item);
            }
        }

        /// <summary>
        /// Number of items in queue, including those that have been checked out.
        /// </summary>
        public int Count()
        {
            return _collection.Count();
        }

        /// <summary>
        /// Removes all items from queue, including any that have been checked out.
        /// </summary>
        public void Clear()
        {
            _collection.DeleteAll();
        }
    }
}
#pragma warning restore CS1584, CS0419, CS1570