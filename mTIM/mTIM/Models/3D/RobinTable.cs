using System;
using System.Collections.Generic;
using System.Linq;
using mTIM.Models.D;

namespace mTIM.Models
{
	public class RobinObjectAdapter<Key>
	{
		public int CalcHash(Key key)
		{
			return key.GetHashCode();
		}

		public bool Equals(object key1, object key2)
        {
			return key1 == key2;
        }
    }

	public class RobinTableHelper
	{
		public static int CalcOptimalSize(int elementCount, int percentage = 120)
		{
			if (elementCount == 0)
			{
				return 0;
			}
			return Urho.MathHelper.NextPowerOfTwo(2 + elementCount * percentage / 100);
		}
	}

	public class RobinInfoNode
	{
		public int hash;
		public int elementCount;
		public bool IsEmpty()
		{
			return hash == 0;
		}
	}

	public class RobinDataNode<Key, T>
	{
		public Key key;
		public T element;
	}

	public class RobinTable<Key, T, P> : IDisposable where P : RobinObjectAdapter<Key>
	{
		public T GetValueByIndex(int index)
		{
			return dataTable[index].element;
		}

		/// Destructor.
		~RobinTable()
		{
		}

		public RobinTable(int _tabSize, RobinDataNode<Key, T>[] _table = null, int _size = 0, float _resizeThreshold = 0.85f)
		{
			tabSize = _tabSize;
			dataTable = _table;
			size = _size;
			resizeThreshold = 0.85f;
			Init();
		}

		public RobinTable(RobinTable<Key, T, P> otherTable)
		{
			tabSize = otherTable.tabSize;
			infoTable = null;
			dataTable = null;
			size = 0;
			Init();
			for (int i = 0; i < tabSize; i++)
			{
				RobinInfoNode infoNode = otherTable.infoTable[i];
				if (!infoNode.IsEmpty())
				{
					RobinDataNode<Key, T> dataNode = otherTable.dataTable[i];
					Add(dataNode.key, dataNode.element);
				}
			}
		}

		public RobinTable()
		{
			
		}


		public RobinTable(int tabSize = 64)
        {
			this.tabSize = tabSize;
        }


		// Clears the hash.
		public void Clear()
		{
			//Clear(IntToType<std::is_trivially_destructible<T>.value>());
		}

		public int GetSize()
		{
			return size;
		}

		public int GetCapacity()
		{
			return tabSize;
		}

		public float resizeThreshold;
		public int size;
		public int tabSize;
		public RobinInfoNode[] infoTable;
		public RobinDataNode<Key, T>[] dataTable;

		public int CalcHash(Key key)
		{
			P adapter = default(P);
			int value = adapter.CalcHash(key);
			return value == 0 ? 1 : value;
		}

		


		public int FindOrAdd(Key key, T obj)
		{
			int hash = CalcHash(key);
			int index = FindIndex(hash, key);

			if (index == -1)
			{
				Add(hash, key, obj);
			}
			return index;
		}

		public void Add(Key key, T obj)
		{
			//ASSERT(!Contains(key), "Duplicate key!");
			int hash = CalcHash(key);
			Add(hash, key, obj);
		}


		public void Add(int hash, Key key, T obj)
		{
			if ((float)(size + 1) / tabSize >= resizeThreshold)
			{
				Resize(Math.Max(tabSize * 2, 2));
				//Log("RobinTable resized\n");
			}

			size++;
			//ASSERT(size <= tabSize, String.Format("Size > tabSize! %d %d", size, tabSize).GetBuffer());
#if DEBUG
			if (size - 1 < tabSize * 8 / 10 && size >= tabSize * 8 / 10)
			{
				//Log("RobinTable with tabSize: %d 80 percent filled: %d\n", tabSize, size);
			}
#endif

			// get hash index     
			//ASSERT(hash != 0, "The calculated hash is equal to an empty node!");

			int sourceIndex = hash & (tabSize - 1);
			infoTable[sourceIndex].elementCount++;
			int index = sourceIndex;
			// search for an empty slot via linear probing
			while (infoTable[index].hash != 0)
			{
				index = (index + 1) & (tabSize - 1);
			}

			infoTable[index].hash = hash;

			//ASSERT(index >= 0, "Index out of range!");
			//ASSERT(index < tabSize, "Index out of range!");

			//__builtin_prefetch(&infoTable[sourceIndex]);       

			// if we have found an empty element, increase the element counter for the original index by one
			// and set the sourceIndex of the filled node to the original source index.
			// then set the key and the element values
			//new (&table[index]) Node();
			//Memory.Construct<T>(dataTable[index].element);
			//Memory.Construct<Key>(dataTable[index].key);
			dataTable[index].key = key;
			dataTable[index].element = obj;
		}

		public void Resize(int newTabSize)
		{
			//ASSERT(newTabSize >= size, "Size > newTabSize");
			//ASSERT((newTabSize & (newTabSize - 1)) == 0, "Size not power of 2!");

			List<KeyValuePair<Key, T>> list = GetPairArray();
			Dispose();

			tabSize = newTabSize;
			if (tabSize > 0)
			{
				Init();

				for (int i = 0; i < list.Count; i++)
				{
					Add(list[i].Key, list[i].Value);
				}
			}
		}


		public void Init()
		{
			// The size must be power of two to get rid of expensive modulos.
			//ASSERT((tabSize & (tabSize - 1)) == 0, "Size not power of 2!");
			size = 0;
			if (tabSize != 0)
			{
				// Allocate the table of hash nodes.
				infoTable = new RobinInfoNode[tabSize];
					//(InfoNode)GetMemoryAllocator().Alloc(sizeof(InfoNode) * tabSize, DEFAULT_ALLOC_ALIGNMENT, SOURCE_INFO, false);
				dataTable = new RobinDataNode<Key, T>[tabSize];
				//(DataNode)GetMemoryAllocator().Alloc(sizeof(DataNode) * tabSize, DEFAULT_ALLOC_ALIGNMENT, SOURCE_INFO, false);
			}
			
			//Platform.MemSet(infoTable, 0, sizeof(InfoNode) * tabSize);
		}



		public List<KeyValuePair<Key, T>> GetPairArray()
		{
			// Fill the ArrayList with the hash table elements.
			List<KeyValuePair<Key, T>> list = new List<KeyValuePair<Key, T>>();
			list.Resize(this.GetSize());
			int j = 0;
			for (int i = 0; i < tabSize; i++)
			{
				if (!infoTable[i].IsEmpty())
				{
					list[j++] = new KeyValuePair<Key, T>(dataTable[i].key, dataTable[i].element);
				}
			}
			//ASSERT(j == GetSize(), "Size error!");
			return new List<KeyValuePair<Key, T>>(list);
		}



		public int FindIndex(int hash, Key key)
		{
			P adapter = default(P);

			int sourceIndex = hash & (tabSize - 1);
			int index = sourceIndex;
			RobinInfoNode p = infoTable[index];
			int count = p.elementCount;
			while (count > 0)
			{
				while ((p.hash & (tabSize - 1)) != sourceIndex)
				{
					index = (index + 1) & (tabSize - 1);
					p = infoTable[index];
				}
				if (p.hash == hash && adapter.Equals(dataTable[index].key, key))
				{
					return index;
				}

				//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
				//ORIGINAL LINE: index = (index+1) & (tabSize-1);
				index = (index + 1) & (tabSize - 1);
				p = infoTable[index];
				count--;
			}


			// no suiting key found - return the EMPTYNODE constant 
			return -1;
		}


		public void DestructTables()
		{
			Clear();
			//DestructTables(IntToType<std::is_trivially_destructible<T>.value>());
		}

		//public void DestructTables(TrivialType UnnamedParameter)
		//{
		//}

		//public void DestructTables(NonTrivialType UnnamedParameter)
		//{
		//	Clear();
		//}

		public void Dispose()
		{
			if (infoTable != null)
			{
				DestructTables();

				//GetMemoryAllocator().Free(infoTable, false);
				//GetMemoryAllocator().Free(dataTable, false);
				//Platform::FreeMemory(table);
				infoTable = null;
				dataTable = null;
			}
			tabSize = 0;
			size = 0;

		}
	}

	public static class ListExtra
	{
		public static void Resize<T>(this List<T> list, int sz, T c)
		{
			int cur = list.Count;
			if (sz < cur)
				list.RemoveRange(sz, cur - sz);
			else if (sz > cur)
			{
				if (sz > list.Capacity)//this bit is purely an optimisation, to avoid multiple automatic capacity changes.
					list.Capacity = sz;
				list.AddRange(Enumerable.Repeat(c, sz - cur));
			}
		}
		public static void Resize<T>(this List<T> list, int sz) where T : new()
		{
			Resize(list, sz, new T());
		}
	}

	public class RobinTableIter<Key, T, P>
	{
		public RobinTableIter()
		{
			this.table = null;
			this.index = -1;
		}
		public RobinTableIter(RobinTableIter<Key, T, P> iter)
		{
			this.table = iter.table;
			this.index = iter.index;
		}
		public RobinTableIter(RobinTable<Key, T, RobinObjectAdapter<Key>> _table, int _index)
		{
			this.table = _table;
			this.index = _index;
		}

		public void CopyFrom(RobinTableIter<Key, T, P> iter)
		{
			this.index = iter.index;
			this.table = iter.table;
		}

		//public static bool operator != (RobinTableIter ImpliedObject, RobinTableIter<Key, T, P> other)
		//{
		//	return !(other == *ImpliedObject);
		//}

		public KeyValuePair<Key, T> Indirection()
		{
			return new KeyValuePair<Key, T>();
		}

		public int GetIndex()
		{
			return index;
		}
		private RobinTable<Key, T, RobinObjectAdapter<Key>> table;
		private int index;
	}

}
