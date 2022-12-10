using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using LongFile = Pri.LongPath.File;
using LongDirectiory = Pri.LongPath.Directory;
using LongPath = Pri.LongPath.Path;

namespace Celarix.IO.FileAnalysis.PostProcessing
{
    public sealed class StorageBackedBitArrayList : IList<BitArray>
    {
        private struct BitWriter
        {
            private readonly BinaryWriter writer;
            private byte buffer;
            private int bufferPosition;

            public BitWriter(BinaryWriter writer)
            {
                this.writer = writer;
                buffer = 0;
                bufferPosition = 7;
            }

            public void WriteBit(bool bit)
            {
                if (bit)
                {
                    buffer |= (byte)(1 << bufferPosition);
                    bufferPosition -= 1;
                }

                if (bufferPosition == -1)
                {
                    writer.Write(buffer);
                    bufferPosition = 7;
                    buffer = 0;
                }
            }

            public void Flush() => writer.Write(buffer);
        }

        public sealed class StorageBackedBitArrayListEnumerator : IEnumerator<BitArray>
        {
            private readonly StorageBackedBitArrayList list;
            private int currentIndex;

            /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public BitArray Current => list[currentIndex];

            /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            object IEnumerator.Current => Current;

            public StorageBackedBitArrayListEnumerator(StorageBackedBitArrayList list) => this.list = list;

            /// <summary>Advances the enumerator to the next element of the collection.</summary>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
            /// <returns>
            /// <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                if (currentIndex >= list.Count)
                {
                    return false;
                }

                currentIndex += 1;
                return true;
            }

            /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
            public void Reset() => currentIndex = 0;
            
            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
            public void Dispose() { }
        }

        public sealed class StorageBackedBitArrayBatch
        {
            private readonly StorageBackedBitArrayList underlyingList;
            private readonly int batchOffset;

            public BitArray this[int index] => underlyingList[batchOffset + index];

            public StorageBackedBitArrayBatch(StorageBackedBitArrayList underlyingList, int batchOffset)
            {
                this.underlyingList = underlyingList;
                this.batchOffset = batchOffset;
            }
        }
        
        private const int BatchSize = 100_000;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private readonly string backingFolderPath;
        private int loadedBatchIndex;
        private readonly List<BitArray> loadedBatch;

        private int BatchCount =>
            Count % BatchSize == 0
                ? Count / BatchSize
                : (Count / BatchSize) + 1;

        /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public int Count { get; private set; }

        /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
        /// <returns>
        /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, <see langword="false" />.</returns>
        public bool IsReadOnly => false;

        /// <summary>Gets or sets the element at the specified index.</summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
        /// <returns>The element at the specified index.</returns>
        public BitArray this[int index]
        {
            get => GetBitArray(index);
            set => SetBitArray(index, value);
        }

        public StorageBackedBitArrayList(string backingFolderPath)
        {
            this.backingFolderPath = backingFolderPath;
            this.loadedBatch = new List<BitArray>(BatchSize);
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<BitArray> GetEnumerator() => new StorageBackedBitArrayListEnumerator(this);

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public void Add(BitArray item)
        {
            if (loadedBatch.Count == BatchSize)
            {
                SaveLoadedBatch();
                loadedBatch.Clear();
                loadedBatchIndex += 1;
            }
            
            loadedBatch.Add(item);
            Count += 1;
        }

        /// <summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public void Clear()
        {
            foreach (var batchFile in LongDirectiory.EnumerateFiles(backingFolderPath, "*", SearchOption.TopDirectoryOnly))
            {
                LongFile.Delete(batchFile);
            }

            loadedBatch.Clear();
            loadedBatchIndex = 0;
            Count = 0;
        }

        /// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />.</returns>
        public bool Contains(BitArray item)
        {
            if (loadedBatch.Contains(item))
            {
                return true;
            }
            
            // god what have you done
            var loadedBatchIndexBeforeSearch = loadedBatchIndex;
            var found = false;

            for (int i = 0; i < BatchCount; i++)
            {
                if (i == loadedBatchIndexBeforeSearch)
                {
                    // No use scanning the batch we already checked
                    continue;
                }
                
                SwitchBatch(i);
                if (loadedBatch.Contains(item))
                {
                    found = true;
                    break;
                }
            }
            
            SwitchBatch(loadedBatchIndexBeforeSearch);
            return found;
        }

        /// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
        public void CopyTo(BitArray[] array, int arrayIndex) { throw new NotImplementedException(); }

        /// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        /// <returns>
        /// <see langword="true" /> if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public bool Remove(BitArray item) => throw new NotImplementedException();

        /// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.</summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
        public int IndexOf(BitArray item) => throw new NotImplementedException();

        /// <summary>Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
        public void Insert(int index, BitArray item) { throw new NotImplementedException(); }

        /// <summary>Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.</summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
        public void RemoveAt(int index) { throw new NotImplementedException(); }

        public IEnumerable<(StorageBackedBitArrayBatch batch, int count)> BatchWithCount(int batchSize)
        {
            var batchOffset = 0;

            while (batchOffset < Count)
            {
                yield return (new StorageBackedBitArrayBatch(this, batchOffset), batchSize);

                batchOffset += batchSize;

                if (batchOffset + batchSize > Count)
                {
                    yield return (new StorageBackedBitArrayBatch(this, batchOffset), Count - batchOffset);

                    yield break;
                }
            }
        }

        private void SwitchBatch(int newBatchIndex)
        {
            SaveLoadedBatch();
            loadedBatch.Clear();
            loadedBatchIndex = newBatchIndex;
            
            var requestedBatchSavePath = LongPath.Combine(backingFolderPath, $"{newBatchIndex:X8}.bin");
            
            using var reader = new BinaryReader(LongFile.OpenRead(requestedBatchSavePath));

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var bitArrayLength = reader.ReadUInt16();
                var bitArrayLengthInBytes = bitArrayLength >> 3;
                if (bitArrayLength % 8 != 0)
                {
                    bitArrayLengthInBytes += 1;
                }
                
                var bitArray = new BitArray(bitArrayLength);
                var bytes = reader.ReadBytes(bitArrayLengthInBytes);

                for (int i = 0; i < bitArrayLength; i++)
                {
                    bitArray[i] = (bytes[i / 8] & (0x80 >> (i % 8))) != 0;
                }
                
                // WYLO: ugh, everything is wrong on the save and the load
                // we're saving Trues for bits that should be false (whitespace isn't being picked up correctly)
                // we're writing wrong lengths into the file
                // maybe not enough padding bits
                loadedBatch.Add(bitArray);
            }
            
            logger.Info($"Loaded bit array batch #{newBatchIndex}");
        }

        private void SaveLoadedBatch()
        {
            var batchSavePath = LongPath.Combine(backingFolderPath, $"{loadedBatchIndex:X8}.bin");

            using var writer = new BinaryWriter(LongFile.Open(batchSavePath, FileMode.Create, FileAccess.Write));
            foreach (var bitArray in loadedBatch)
            {
                var length = (ushort)bitArray.Length;
                writer.Write(length);
                
                var bitWriter = new BitWriter(writer);

                for (int i = 0; i < bitArray.Count; i++)
                {
                    bitWriter.WriteBit(bitArray[i]);
                }

                bitWriter.Flush();
            }
            
            logger.Info($"Saved bit array batch #{loadedBatchIndex} to disk");
        }

        private BitArray GetBitArray(int bitArrayIndex)
        {
            var batchIndex = bitArrayIndex / BatchSize;

            if (batchIndex != loadedBatchIndex)
            {
                SwitchBatch(batchIndex);
            }

            var offsetInBatch = bitArrayIndex % BatchSize;
            return loadedBatch[offsetInBatch];
        }

        private void SetBitArray(int bitArrayIndex, BitArray value)
        {
            var batchIndex = bitArrayIndex / BatchSize;

            if (batchIndex != loadedBatchIndex) { SwitchBatch(batchIndex); }

            var offsetInBatch = bitArrayIndex % BatchSize;
            loadedBatch[offsetInBatch] = value;
        }
    }
}
