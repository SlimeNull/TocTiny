using System;
using System.Collections.Generic;

namespace Null.Library.Linker
{
    public class Linker<TA, TB>
    {
        private readonly Dictionary<TA, TB> a2b = new Dictionary<TA, TB>();
        private readonly Dictionary<TB, TA> b2a = new Dictionary<TB, TA>();

        public void AppendLink(TA a, TB b)
        {
            if ((!a2b.ContainsKey(a)) && (!b2a.ContainsKey(b)))
            {
                a2b[a] = b;
                b2a[b] = a;
            }
            else
            {
                throw new ArgumentOutOfRangeException("链接器中已经含有该元素");
            }
        }
        public void AppendLink(TB b, TA a)
        {
            AppendLink(a, b);
        }
        public TB GetTarget(TA a)
        {
            return a2b[a];
        }
        public TA GetTarget(TB b)
        {
            return b2a[b];
        }
        public bool Remove(TA a)
        {
            if (a2b.ContainsKey(a))
            {
                TB b = a2b[a];
                return a2b.Remove(a) & b2a.Remove(b);
            }
            else
            {
                return false;
            }
        }
        public bool Remove(TB b)
        {
            if (b2a.ContainsKey(b))
            {
                TA a = b2a[b];
                return a2b.Remove(a) & b2a.Remove(b);
            }
            else
            {
                return false;
            }
        }
        public void SetTarget(TA a, TB b)
        {
            if (a2b.ContainsKey(a))
            {
                TB oldB = a2b[a];
                a2b[a] = b;
                b2a.Remove(oldB);
                b2a[b] = a;
            }
            else
            {
                AppendLink(a, b);
            }
        }
        public void SetTarget(TB b, TA a)
        {
            SetTarget(a, b);
        }
        public TB this[TA a]
        {
            get => GetTarget(a);
            set => SetTarget(a, value);
        }
        public TA this[TB b]
        {
            get => GetTarget(b);
            set => SetTarget(value, b);
        }
        public bool ContainsNode(TA a)
        {
            return a2b.ContainsKey(a);
        }
        public bool ContainsNode(TB b)
        {
            return b2a.ContainsKey(b);
        }
        public void Clear()
        {
            a2b.Clear();
            b2a.Clear();
        }
    }
}
