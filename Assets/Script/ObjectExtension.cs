using System.Collections.Generic;
using UnityEngine;

public static class ObjectExtension
{
	public static List<T> FindObjectOfInterfaces<T>(this Object _) where T : class
	{
		var list = new List<T>();
		foreach (var component in Object.FindObjectsByType<Component>(
			FindObjectsInactive.Include, FindObjectsSortMode.None))
		{
			if(component is T t)
            {
				list.Add(t);
            }
		}
		return list;
	}
}
