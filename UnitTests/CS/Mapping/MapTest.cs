using System;
using System.Collections;
using System.Data;
using System.Data.SqlTypes;

using NUnit.Framework;

using BLToolkit.Mapping;

namespace Mapping
{
	[TestFixture, Category("Mapping")]
	public class MapTest : TestFixtureBase
	{
		#region ToEnum, FromEnum

		[DefaultValue(Enum1.Value3)]
		public enum Enum1
		{
			[MapValue("1")] Value1,
			[NullValue]     Value2,
			[MapValue("3")] Value3
		}

		[Test]
		public void ToEnum()
		{
			Assert.AreEqual(Enum1.Value1, Map.ValueToEnum("1",         typeof(Enum1)));
			Assert.AreEqual(Enum1.Value2, Map.ValueToEnum(null,        typeof(Enum1)));
			Assert.AreEqual(Enum1.Value3, Map.ValueToEnum((Enum1)2727, typeof(Enum1)));
		}

		[Test]
		public void FromEnum()
		{
			Assert.AreEqual("1", Map.EnumToValue(Enum1.Value1));
			Assert.IsNull  (     Map.EnumToValue(Enum1.Value2));
			Assert.AreEqual("3", Map.EnumToValue(Enum1.Value3));
		}

		#endregion

		#region ObjectToObject

		public class SourceObject
		{
			public int    Field1 = 10;
			public string Field2 = "20";
			public double Field3 = 30.0;
		}

		public class Object1
		{
			public int Field1;
			public int Field2;
			public int Field3;
		}

		[Test]
		public void ObjectToObjectOO()
		{
			SourceObject so = new SourceObject();
			Object1      o  = new Object1();
 
			Map.ObjectToObject(so, o);

			Assert.AreEqual(10, o.Field1);
			Assert.AreEqual(20, o.Field2);
			Assert.AreEqual(30, o.Field3);
		}

		[Test]
		public void ObjectToObjectOT()
		{
			SourceObject so = new SourceObject();
			Object1      o  = (Object1)Map.ObjectToObject(so, typeof(Object1));

			Assert.AreEqual(10, o.Field1);
			Assert.AreEqual(20, o.Field2);
			Assert.AreEqual(30, o.Field3);
		}

#if FW2
		[Test]
		public void ObjectToObjectTO()
		{
			SourceObject so = new SourceObject();
			Object1      o  = Map.ObjectToObject<Object1>(so);

			Assert.AreEqual(10, o.Field1);
			Assert.AreEqual(20, o.Field2);
			Assert.AreEqual(30, o.Field3);
		}
#endif

		public class DefaultNullType
		{
			[NullValue(-1)]
			public int NullableInt;
		}

		#endregion

		#region DataRowToObject

		[Test]
		public void DataRowToObjectD()
		{
			DataTable table = new DataTable();

			table.Columns.Add("NullableInt", typeof(int));

			table.Rows.Add(new object[] { DBNull.Value });
			table.Rows.Add(new object[] { 1 });
			table.AcceptChanges();

			DefaultNullType dn = (DefaultNullType)Map.DataRowToObject(table.Rows[0], typeof(DefaultNullType));

			Assert.AreEqual(-1, dn.NullableInt);

			Map.DataRowToObject(table.Rows[1], DataRowVersion.Current, dn);

			Assert.AreEqual(1, dn.NullableInt);
		}

		#endregion

		#region ObjectToDictionary

		[Test]
		public void ObjectToDictionary()
		{
			SourceObject so = new SourceObject();
			Hashtable    ht = new Hashtable();
 
			Map.ObjectToDictionary(so, ht);

			Assert.AreEqual(10,   ht["Field1"]);
			Assert.AreEqual("20", ht["Field2"]);
			Assert.AreEqual(30,   ht["Field3"]);
		}

		#endregion

		#region SqlTypes

		public class SqlTypeTypes
		{
			public class SourceObject
			{
				public string    s1 = "123";
				public SqlString s2 = "1234";
				public int       i1 = 123;
				public SqlInt32  i2 = 1234;

				public DateTime    d1 = DateTime.Now;
				public SqlDateTime d2 = DateTime.Now;
			}

			public class Object1
			{
				public SqlString s1;
				public string    s2;
				public SqlInt32  i1;
				public int       i2;

				public SqlDateTime d1;
				public DateTime    d2;
			}
		}

		[Test]
		public void SqlTypes()
		{
			SqlTypeTypes.SourceObject so = new SqlTypeTypes.SourceObject();
			SqlTypeTypes.Object1      o  = (SqlTypeTypes.Object1)Map.ObjectToObject(so, typeof(SqlTypeTypes.Object1));

			Console.WriteLine(o.s1); Assert.AreEqual("123",  o.s1.Value);
			Console.WriteLine(o.s2); Assert.AreEqual("1234", o.s2);

			Console.WriteLine(o.i1); Assert.IsTrue(o.i1.Value == 123);
			Console.WriteLine(o.i2); Assert.IsTrue(o.i2 == 1234);

			Console.WriteLine("{0} - {1}", so.d2, o.d2); Assert.AreEqual(o.d2, so.d2.Value);
			//Console.WriteLine("{0} - {1}", s.d1, d.d1); Assert.IsTrue(d.d1.Value == s.d1);
		}

		#endregion

		#region SourceListToDestList

		[Test]
		public void ListToList()
		{
			DataTable table = GetDataTable();
			ArrayList list1 = Map.TableToList(table, typeof(TestObject));
			ArrayList list2 = new ArrayList();

			Map.ListToList(list1, list2, typeof(TestObject));

			CompareLists(table, list2);
		}

		[Test]
		public void TableToList()
		{
			DataTable table = GetDataTable();
			ArrayList list  = Map.TableToList(table, typeof(TestObject));

			CompareLists(table, list);
		}

		[Test]
		public void ListToTable1()
		{
			DataTable table1 = GetDataTable();
			ArrayList list   = Map.TableToList(table1, typeof(TestObject));
			DataTable table2 = Map.ListToTable(list);

			table2.AcceptChanges();

			CompareLists(table1, table2);
		}

		[Test]
		public void ListToTable2()
		{
			DataTable table1 = GetDataTable();
			ArrayList list   = Map.TableToList(table1, typeof(TestObject));
			DataTable table2 = table1.Clone();

			Map.ListToTable(list, table2);

			table2.AcceptChanges();

			CompareLists(table1, table2);
		}

		[Test]
		public void TableToTable1()
		{
			DataTable table1 = GetDataTable();
			DataTable table2 = Map.TableToTable(table1);

			table2.AcceptChanges();

			CompareLists(table1, table2);
		}

		[Test]
		public void TableToTable2()
		{
			DataTable table1 = GetDataTable();
			DataTable table2 = new DataTable();
				
			Map.TableToTable(table1, table2);

			table2.AcceptChanges();

			CompareLists(table1, table2);
		}

		#endregion
	}
}