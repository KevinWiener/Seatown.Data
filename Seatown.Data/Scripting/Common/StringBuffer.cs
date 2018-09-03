using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seatown.Data.Scripting.Common
{
    internal class StringBuffer
    {
        private StringBuilder _buffer = new StringBuilder();
        public string Content { get { return this._buffer.ToString(); } }
        public int Length { get; private set; }

        public StringBuffer(int length)
        {
            this.Length = length;
        }

        public StringBuffer(int length, params string[] contents)
        {
            this.Length = length;
            if (contents != null)
            {
                foreach (string s in contents)
                {
                    this.Append(s);
                }
            }
        }

        public StringBuffer(int length, params char[] contents)
        {
            this.Length = length;
            if (contents != null)
            {
                foreach (char c in contents)
                {
                    this.Append(c);
                }
            }
        }

        public void Append(char c)
        {
            this._buffer.Append(c);
            this.RemoveExtraContent();
        }

        public void Append(string s)
        {
            this._buffer.Append(s);
            this.RemoveExtraContent();
        }

        public void Clear()
        {
            this._buffer.Clear();
        }

        private void RemoveExtraContent()
        {
            if (this._buffer.Length > this.Length)
            {
                this._buffer.Remove(0, this._buffer.Length - this.Length);
            }
        }

    }
}
