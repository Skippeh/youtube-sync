using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server
{
	public class SaveFile
	{
		private readonly string filePath;

		private Dictionary<string, object> savedKeyValues;

		public SaveFile(string filePath)
		{
			this.filePath = filePath;
			savedKeyValues = new Dictionary<string, object>();

			if (File.Exists(filePath))
			{
				LoadKeyValues();
			}
		}

		private void LoadKeyValues()
		{
			try
			{
				string contents;
				using (var reader = new StreamReader(filePath))
				{
					contents = reader.ReadToEnd();
				}

				savedKeyValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(contents);
			}
			catch (IOException)
			{
				Console.WriteLine("Failed to load settings!!");
				Environment.Exit(1);
			}
			catch (JsonException)
			{
				Console.WriteLine("Failed to deserialize settings!!");
				Environment.Exit(2);
			}
		}

		public void SaveKeyValues()
		{
			try
			{
				using (var writer = new StreamWriter(filePath))
				{
					var jsonString = JsonConvert.SerializeObject(savedKeyValues, Formatting.Indented);
					writer.Write(jsonString);
				}
			}
			catch (IOException)
			{
				Console.WriteLine("Failed to load settings!!");
				Environment.Exit(1);
			}
			catch (JsonException)
			{
				Console.WriteLine("Failed to deserialize settings!!");
				Environment.Exit(2);
			}
		}

		public void Set(string key, object value)
		{
			savedKeyValues[key] = value;
		}

		public object Get(string key, object defaultValue = null)
		{
			return Get<object>(key, defaultValue);
		}

		public T Get<T>(string key, object defaultValue = null)
		{
			if (savedKeyValues.ContainsKey(key))
				return (T) savedKeyValues[key];

			Set(key, defaultValue);
			return (T) defaultValue;
		}
	}
}