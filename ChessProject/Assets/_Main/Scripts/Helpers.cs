using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class Extentions
{
	//Deep Clone from https://www.techiedelight.com/copy-object-csharp/
	public static T DeepClone<T>(this T obj)
	{
		using (MemoryStream stream = new MemoryStream())
		{
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, obj);
			stream.Position = 0;

			return (T)formatter.Deserialize(stream);
		}
	}
}
