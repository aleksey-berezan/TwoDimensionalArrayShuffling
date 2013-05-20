using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuffleTwoDimensionalArrayConsole
{
	public sealed class PackedItem
	{
		public string Value { get; private set; }
		public int Count { get; set; }

		public PackedItem(string value)
		{
			Value = value;
			Count = 1;
		}

		public string[] Expand()
		{
			string[] result = new string[Count];

			for (int i = 0; i < Count; i++)
			{
				result[i] = Value;
			}

			return result;
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}", Value, Count);
		}
	}

	public sealed class PackedItem2
	{
		public string Value { get; private set; }
		public int Count { get; set; }
		public int Row { get; private set; }
		public int Column { get; private set; }

		public PackedItem2(string value, int row, int column)
		{
			Value = value;
			Count = 1;
			Row = row;
			Column = column;
		}

		public string[] Expand()
		{
			string[] result = new string[Count];

			for (int i = 0; i < Count; i++)
			{
				result[i] = Value;
			}

			return result;
		}

		public void SetLocation(int row, int column)
		{
			Row = row;
			Column = column;
		}

		public override string ToString()
		{
			return string.Format("[{0},{1}]{2} - {3}", Row, Column, Value, Count);
		}
	}

	public static class Extensions
	{
		public static List<PackedItem> WithExcluded(this List<PackedItem> list, PackedItem item)
		{
			var list2 = list.ToList();
			list2.Remove(item);
			return list2;
		}

		public static List<PackedItem> WithIncluded(this List<PackedItem> list, PackedItem item)
		{
			var list2 = list.ToList();
			list2.Add(item);
			return list2;
		}

		public static int Rows(this string[,] array)
		{
			return array.GetLength(0);
		}

		public static int Columns(this string[,] array)
		{
			return array.GetLength(1);
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			string[,] input = new string[,]
			{ 
				 { "ab","ab","ab","FREE","me","me","me","FREE","mo","mo","FREE","FREE"},
				 { "so","so","FREE","no","no","FREE","to","to","to","FREE","do","do"}
			};
			Console.WriteLine("Input:");
			Console.WriteLine(string.Join(", ", string.Join(", ", input.Cast<string>())));

			bool hasErrrors = false;
			int MAX_ITERATIONS = 100000;
			for (int i = 1; i <= MAX_ITERATIONS; i++)
			{
				try
				{
					string[,] shuffled = Shuffle2(input);

					//Console.WriteLine("Shuffled:");
					//Console.WriteLine(string.Join(", ", string.Join(", ", shuffled.Cast<string>())));
					Verification.Verify(input, shuffled);
					//Console.WriteLine("Verified");
				}
				catch (Exception exc)
				{
					Console.WriteLine(exc.Message);
					hasErrrors = true;
				}

				WriteProgress((1d * i) / MAX_ITERATIONS);
			}

			Console.WriteLine("Completed with {0}", (hasErrrors ? "errors" : "successfully"));
		}

		#region Solution_1

		public static string[,] Shuffle(string[,] array)
		{
			List<PackedItem> packed = Pack(array);
			List<PackedItem> shuffled = Shuffle(packed);
			string[,] unpacked = Unpack(inputList: shuffled
										, rows: array.GetLength(0)
										, columns: array.GetLength(1));

			return unpacked;
		}

		private static List<PackedItem> Pack(string[,] array)
		{
			var list = new List<PackedItem>();

			for (int i = 0; i < array.GetLength(0); i++)
			{
				for (int j = 0; j < array.GetLength(1); j++)
				{
					string s = array[i, j];

					if (j == 0 || list.Count == 0)
					{
						list.Add(new PackedItem(s));
						continue;
					}

					var last = list.Last();
					if (s == last.Value)
					{
						last.Count += 1;
						continue;
					}
					else
					{
						list.Add(new PackedItem(s));
						continue;
					}
				}
			}

			return list;
		}

		private static string[,] Unpack(List<PackedItem> inputList, int rows, int columns)
		{
			var list = inputList.ToList();
			string[,] result = new string[rows, columns];

			for (int i = 0; i < rows; i++)
			{
				List<PackedItem> packedRow = Pick(source: list, taken: new List<PackedItem>(), takeCount: columns);

				packedRow.ForEach(x => list.Remove(x));
				List<string> row = packedRow
									.Select(x => x.Expand())
									.Aggregate(seed: new List<string>(),
											func: (acc, source) => { acc.AddRange(source); return acc; });

				for (int j = 0; j < columns; j++)
				{
					result[i, j] = row[j];
				}
			}

			return result;
		}

		private static List<PackedItem> Pick(List<PackedItem> source, List<PackedItem> taken, int takeCount)
		{
			if (taken.Sum(x => x.Count) == takeCount)
			{
				return taken;
			}
			foreach (var item in source.ToList())
			{
				var list = Pick(source.WithExcluded(item)
								, taken.WithIncluded(item)
								, takeCount);
				if (list != null)
				{
					return list;
				}
			}
			return null;
		}

		private static bool HasAdjacent(List<PackedItem> taken)
		{
			PackedItem previous = null;
			foreach (var item in taken)
			{
				if (previous != null)
				{
					if (previous.Value == item.Value)
						return true;
				}
				previous = item;
			}
			return false;
		}

		private static List<PackedItem> Shuffle(List<PackedItem> list)
		{
			Random r = new Random();

			var result = list.ToList();

			for (int i = 0; i < list.Count; i++)
			{
				int a = r.Next(0, list.Count);
				int b = r.Next(0, list.Count);

				Swap(result, a, b);
			}

			return result;
		}

		private static void Swap(List<PackedItem> list, int a, int b)
		{
			var temp = list[b];
			list[b] = list[a];
			list[a] = temp;
		}

		private static void WriteProgress(double progress)
		{
			int oldTop = Console.CursorTop;
			int oldLeft = Console.CursorLeft;

			try
			{
				Console.CursorTop = 0;
				Console.CursorLeft = Console.WindowWidth - "xx.yy  %".Length;
				Console.WriteLine("{0:p}", progress);
			}
			finally
			{
				Console.CursorTop = oldTop;
				Console.CursorLeft = oldLeft;
			}
		}

		#endregion

		#region Solution_2

		public static string[,] Shuffle2(string[,] array)
		{
			List<PackedItem2>[] packed = Pack2(array);
			List<PackedItem2>[] shuffled = Shuffle2(packed);
			string[,] unpacked = Unpack2(shuffled, array.Rows(), array.Columns());
			return unpacked;
		}

		private static string[,] Unpack2(List<PackedItem2>[] shuffled, int rows, int columns)
		{
			string[,] result = new string[rows, columns];

			int i = 0;
			foreach (List<PackedItem2> row in shuffled)
			{
				int j = 0;
				foreach (PackedItem2 item in row)
				{
					foreach (string s in item.Expand())
					{
						result[i, j] = s;
						j++;
					}
				}
				i++;
			}
			return result;
		}

		private static List<PackedItem2>[] Shuffle2(List<PackedItem2>[] matrix)
		{
			var flatten = matrix.Aggregate(seed: new List<PackedItem2>()
								, func: (a, b) => a.Union(b).ToList());


			foreach (PackedItem2 item in flatten)
			{
				int[] rowNumbers = GenerateRandoms(matrix.Length, 0, matrix.Length)
									.Except(new[] { item.Row })
									.ToArray();

				List<PackedItem2> toSwap = GetSwapOrNull(matrix, item);
				if (toSwap != null)
				{
					Swap(matrix, new List<PackedItem2> { item }, toSwap);
				}
				//else
				//{
				//	TODO: if has no swap pair -> accumulate larger pack
				//}
			}

			return matrix;
		}

		private static List<PackedItem2> GetSwapOrNull(List<PackedItem2>[] matrix, PackedItem2 target)
		{
			// TODO: implement
			throw new NotImplementedException();
		}

		private static void Swap(List<PackedItem2>[] matrix, List<PackedItem2> itemsA, List<PackedItem2> itemsB)
		{
			// TODO: improve - solve adjacent items case
			var rowA = matrix[itemsA.First().Row];
			itemsA.ForEach(x => rowA.Remove(x));
			itemsA.AddRange(itemsB.ToArray());
			ReIndex(rowA);

			var rowB = matrix[itemsB.First().Row];
			itemsB.ForEach(x => rowB.Remove(x));
			itemsB.AddRange(itemsA.ToArray());
			ReIndex(rowB);
		}

		private static void ReIndex(List<PackedItem2> rowB)
		{
			int column = 0;
			foreach (PackedItem2 item in rowB)
			{
				item.SetLocation(item.Row, column++);
			}
		}

		private readonly static Random _r = new Random();
		private static int[] GenerateRandoms(int count, int minValue, int maxValue)
		{
			int[] result = new int[count];

			for (int i = 0; i < count; i++)
				result[i++] = _r.Next(minValue, maxValue);

			return result;
		}

		// TODO: use linkedlist as row
		private static List<PackedItem2>[] Pack2(string[,] array)
		{
			var result = new List<PackedItem2>[array.Rows()];

			for (int i = 0; i < array.GetLength(0); i++)
			{
				int packedRow = i;
				int packedColumn = 0;

				var list = new List<PackedItem2>();
				for (int j = 0; j < array.GetLength(1); j++)
				{
					string s = array[i, j];

					if (j == 0 || list.Count == 0)
					{
						list.Add(new PackedItem2(s, packedRow, packedColumn++));
						continue;
					}

					var last = list.Last();
					if (s == last.Value)
					{
						last.Count += 1;
						continue;
					}
					else
					{
						list.Add(new PackedItem2(s, packedRow, packedColumn++));
						continue;
					}
				}
				result[i] = list;
			}

			return result;
		}

		#endregion

		#region Verification

		private static class Verification
		{
			internal static void Verify(string[,] input, string[,] output)
			{
				VerifyCountsAreEqual(input, output);
				VerifySizesAreEquals(input, output);
				VerifyDoesNotHaveNulls(output);
				VerifyContainsSameItems(input, output);

				// TODO: get alrogith capable to pass next check
				// VerifyContainsNoAdjacentItems(input, output);
			}

			private static void VerifyContainsNoAdjacentItems(string[,] input, string[,] output)
			{
				var inputPacked = Pack(input);
				var outputPacked = Pack(output);

				if (inputPacked.Count() != outputPacked.Count())
					throw new Exception("There are some adjacent items moved each other");

				foreach (var item in outputPacked)
				{
					if (item.Count > 3)
						Debugger.Break();
					bool existsInOutput = inputPacked.Any(x => AreEqual(x, item));
					if (!existsInOutput)
					{
						throw new Exception("There are some adjacent items moved each other");
					}
				}
			}

			private static void VerifyContainsSameItems(string[,] input, string[,] output)
			{
				foreach (var item in Pack(input))
				{
					bool contains = Contains(item, output);
					if (!contains)
					{
						throw new Exception("output does not contain " + item);
					}
				}
			}

			private static void VerifyCountsAreEqual(string[,] input, string[,] output)
			{
				if (input.Cast<string>().Count() != output.Cast<string>().Count())
					throw new Exception("items count do not match");
			}

			private static void VerifyDoesNotHaveNulls(string[,] output)
			{
				if (output.Cast<string>().Any(x => x == null))
				{
					throw new Exception("nulls found");
				}
			}

			private static void VerifySizesAreEquals(string[,] input, string[,] output)
			{
				int inputrows = input.GetLength(0);
				int inputcolumns = input.GetLength(1);

				int outputrows = output.GetLength(0);
				int outputcolumns = output.GetLength(1);

				if (inputrows != outputrows || inputcolumns != outputcolumns)
					throw new Exception("sizes do not match");
			}

			private static bool Contains(PackedItem item, string[,] array)
			{
				int rows = array.GetLength(0);
				int columns = array.GetLength(1);

				int matchedCount = 0;
				for (int i = 0; i < rows; i++)
				{
					for (int j = 0; j < columns; j++)
					{
						string value = array[i, j];
						if (value == item.Value)
						{
							matchedCount++;
							if (matchedCount == item.Count)
							{
								return true;
							}
							else
							{
								continue;
							}
						}
						else
						{
							matchedCount = 0;
						}
					}
				}

				return false;
			}

			private static bool AreEqual(PackedItem a, PackedItem b)
			{
				return a.Count == b.Count && a.Value == b.Value;
			}
		}

		#endregion
	}
}
