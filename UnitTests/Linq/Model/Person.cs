﻿using System;
using BLToolkit.DataAccess;

namespace Data.Linq.Model
{
	using BLToolkit.Mapping;

	[TableName(Database = "BLTD")]
	public class Person
	{
		public Person()
		{
		}

		public Person(int id)
		{
			ID = id;
		}

		public Person(int id, string firstName)
		{
			ID        = id;
			FirstName = firstName;
		}

		[MapField("PersonID")] public int    ID;
		                       public string FirstName { get; set; }
		                       public string LastName;
		[Nullable]             public string MiddleName;
		                       public char   Gender;

		public override bool Equals(object obj)
		{
			return Equals(obj as Person);
		}

		public bool Equals(Person other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return
				other.ID == ID &&
				Equals(other.LastName,   LastName) &&
				Equals(other.MiddleName, MiddleName) &&
				other.Gender == Gender &&
				Equals(other.FirstName,  FirstName);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var result = ID;
				result = (result * 397) ^ (LastName   != null ? LastName.GetHashCode()   : 0);
				result = (result * 397) ^ (MiddleName != null ? MiddleName.GetHashCode() : 0);
				result = (result * 397) ^ Gender.GetHashCode();
				result = (result * 397) ^ (FirstName  != null ? FirstName.GetHashCode()  : 0);
				return result;
			}
		}
	}
}
