using System;

using BLToolkit.Mapping;

namespace BLToolkit.Reflection.MetadataProvider
{
	using Extension;

	public delegate void                 OnCreateProvider(MetadataProviderBase parentProvider);
	public delegate MetadataProviderBase CreateProvider();
	public delegate MemberMapper         EnsureMapperHandler(string mapName, string origName);

	public abstract class MetadataProviderBase
	{
		#region Provider Support

		public virtual void AddProvider(MetadataProviderBase provider)
		{
		}

		public virtual void InsertProvider(int index, MetadataProviderBase provider)
		{
		}

		public virtual MetadataProviderBase[] GetProviders()
		{
			return new MetadataProviderBase[0];
		}

		#endregion

		#region GetFieldName

		public virtual string GetFieldName(TypeExtension typeExtension, MemberAccessor member, out bool isSet)
		{
			isSet = false;
			return member.Name;
		}

		#endregion

		#region EnsureMapper

		public virtual void EnsureMapper(TypeAccessor typeAccessor, MappingSchema mappingSchema, EnsureMapperHandler handler)
		{
		}

		#endregion

		#region GetMapIgnore

		public virtual bool GetMapIgnore(TypeExtension typeExtension, MemberAccessor member, out bool isSet)
		{
			isSet = false;

			return
				TypeHelper.IsScalar(member.Type) == false;// ||
				//(member.MemberInfo is FieldInfo && ((FieldInfo)member.MemberInfo).IsLiteral);
		}

		#endregion

		#region GetTrimmable

		public virtual bool GetTrimmable(TypeExtension typeExtension, MemberAccessor member, out bool isSet)
		{
			isSet = member.Type != typeof(string);
			return isSet? false: TrimmableAttribute.Default.IsTrimmable;
		}

		#endregion

		#region GetMapValues

		public virtual MapValue[] GetMapValues(TypeExtension typeExtension, MemberAccessor member, out bool isSet)
		{
			isSet = false;
			return null;
		}

		public virtual MapValue[] GetMapValues(TypeExtension typeExtension, Type type, out bool isSet)
		{
			isSet = false;
			return null;
		}

		#endregion

		#region GetDefaultValue

		public virtual object GetDefaultValue(MappingSchema mappingSchema, TypeExtension typeExtension, MemberAccessor member, out bool isSet)
		{
			isSet = false;
			return null;
		}

		public virtual object GetDefaultValue(MappingSchema mappingSchema, TypeExtension typeExtension, Type type, out bool isSet)
		{
			isSet = false;
			return null;
		}

		#endregion

		#region GetNullable

		public virtual bool GetNullable(MappingSchema mappingSchema, TypeExtension typeExtension, MemberAccessor member, out bool isSet)
		{
			isSet = false;
			return false;
		}

		#endregion

		#region GetNullValue

		public virtual object GetNullValue(MappingSchema mappingSchema, TypeExtension typeExtension, MemberAccessor member, out bool isSet)
		{
			isSet = false;

			if (member.Type.IsEnum)
				return null;

			object value = mappingSchema.GetNullValue(member.Type);

			if (value is Type && value == typeof(DBNull))
			{
				value = DBNull.Value;

				if (member.Type == typeof(string))
					value = null;
			}

			return value;
		}

		#endregion

		#region GetTableName

		public virtual string GetTableName(Type type, ExtensionList extensions, out bool isSet)
		{
			isSet = false;
			return type.Name;
		}

		#endregion

		#region GetPrimaryKeyOrder

		public virtual int GetPrimaryKeyOrder(Type type, TypeExtension typeExt, MemberAccessor member, out bool isSet)
		{
			isSet = false;
			return 0;
		}

		#endregion

		#region GetNonUpdatableFlag

		public virtual bool GetNonUpdatableFlag(Type type, TypeExtension typeExt, MemberAccessor member, out bool isSet)
		{
			isSet = false;
			return false;
		}

		#endregion

		#region GetSqlIgnore

		public virtual bool GetSqlIgnore(TypeExtension typeExtension, MemberAccessor member, out bool isSet)
		{
			isSet = false;
			return false;
		}

		#endregion

		#region Static Members

		public static event OnCreateProvider OnCreateProvider;

		private static CreateProvider _createProvider = CreateInternal;
		public  static CreateProvider  CreateProvider
		{
			get { return _createProvider; }
			set { _createProvider = value ?? new CreateProvider(CreateInternal); }
		}

		private static MetadataProviderBase CreateInternal()
		{
			MetadataProviderList list = new MetadataProviderList();

			if (OnCreateProvider != null)
				OnCreateProvider(list);

			return list;
		}

		#endregion
	}
}
