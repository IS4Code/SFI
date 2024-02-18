using System;
using System.Collections.Generic;
using System.Threading;

namespace IS4.SFI.RDF
{
    /// <summary>
    /// Provides a synchronized unbounded sequence of bits.
    /// </summary>
    internal unsafe struct SparseBitTable
    {
        const int arraySizeInts = 8;
        const int arraySizeBytes = arraySizeInts * sizeof(int);
        const int arraySizeBits = arraySizeBytes * 8;

        fixed int array[arraySizeInts];
        Dictionary<int, long>? dict;

        public bool TryAdd(int index)
        {
            if(index < 0)
            {
                throw new ArgumentNullException(nameof(index));
            }
            int bitIndex;
            if(index < arraySizeBits)
            {
                var arrIndex = Math.DivRem(index, sizeof(int) * 8, out bitIndex);
                var bit = 1 << bitIndex;
                var oldBits = array[arrIndex];
                int previousBits, newBits;
                do{
                    if((oldBits & bit) != 0)
                    {
                        return false;
                    }
                    previousBits = oldBits;
                    newBits = oldBits | bit;
                }while(previousBits != (oldBits = Interlocked.CompareExchange(ref array[arrIndex], newBits, oldBits)));
                return true;
            }
            var dict = this.dict;
            if(dict == null && Interlocked.Exchange(ref this.dict, dict = new()) != null)
            {
                // A new dictionary was created in another thread
                dict = this.dict!;
            }
            var dictIndex = Math.DivRem(index, sizeof(long) * 8, out bitIndex);
            lock(dict)
            {
                var bit = 1L << bitIndex;
                if(dict.TryGetValue(dictIndex, out var dictBits))
                {
                    if((dictBits & bit) != 0)
                    {
                        return false;
                    }
                }else{
                    dictBits = 0;
                }
                dict[dictIndex] = dictBits | bit;
                return true;
            }
        }
    }
}
