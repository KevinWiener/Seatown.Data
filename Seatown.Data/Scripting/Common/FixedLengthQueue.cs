using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seatown.Data.Scripting.Common
{
    public class FixedLengthQueue<T>
    {
        private Queue<T> _content = new Queue<T>();
        private readonly object _lockObject = new object();

        public T[] Content
        {
            get
            {
                return this._content.ToArray();
            }
        }
        public long Length { get; private set; }

        public FixedLengthQueue(long length)
        {
            this.Length = length;
        }

        public FixedLengthQueue(long length, params T[] content)
        {
            this.Length = length;
            if (content != null)
            {
                foreach (T value in content)
                {
                    this.Add(value);
                }
            }
        }

        public void Add(T value)
        {
            lock (_lockObject)
            {
                this._content.Enqueue(value);
                while (this._content.Count > this.Length)
                {
                    this._content.Dequeue();
                }
            }
        }

        public void Clear()
        {
            lock (_lockObject)
            {
                this._content.Clear();
            }
        }

    }
}
