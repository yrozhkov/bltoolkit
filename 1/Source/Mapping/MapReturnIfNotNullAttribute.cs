/*
 * File:    MapReturnIfNotNullAttribute.cs
 * Created: 08/15/2005
 * Author:  Igor Tkachev
 *          mailto:it@rsdn.ru
 */

using System;

namespace Rsdn.Framework.Data.Mapping
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(AttributeTargets.ReturnValue)]
	public class MapReturnIfNotNullAttribute : MapReturnIfNonZeroAttribute
	{
	}
}
