using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emerita;

namespace loTechCS.Chess
{
    public sealed class SimpleStack<T> where T : struct
    {
        private readonly T[] stack;
        private int stackPointer;

        public SimpleStack(int capacity = 20)
        {
            stack = new T[capacity];
            stackPointer = 0;
        }

        public void Push(T item)
        {
            Util.Assert(stackPointer < stack.Length);
            stack[stackPointer++] = item;
        }

        public T Pop()
        {
            Util.Assert(stackPointer > 0);
            return stack[--stackPointer];
        }

        public bool TryPop(out T item)
        {
            item = default;
            if (stackPointer > 0)
            {
                item = stack[--stackPointer];
                return true;
            }

            return false;
        }

        public void Clear()
        {
            stackPointer = 0;
            Array.Clear(stack);
        }

        public T Peek()
        {
            Util.Assert(stackPointer > 0);
            return stack[stackPointer - 1];
        }
        public bool TryPeek(out T item)
        {
            item = default;
            if (stackPointer > 0)
            {
                item = stack[stackPointer - 1];
                return true;
            }

            return false;
        }

        public Span<T> ToSpan()
        {
            return stack[..stackPointer];
        }

        public bool IsEmpty => stackPointer == 0;

        public int Count => stackPointer;
    }
}
