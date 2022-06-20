using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Clocker.Mod
{
	public class CXml
	{
		public static CXml Instance;
		
		public static void Init() {
			Instance = new CXml();
		}
		
		public Dictionary<Type, XmlSerializer> Serializers;
		
		public CXml() {
			Serializers = new Dictionary<Type, XmlSerializer>();
		}
		
		public void Add<T>() {
			Add(typeof(T));
		}
		
		public void Add(Type type) {
			if (Has(type)) return;
			var ser = new XmlSerializer(type);
			Serializers.Add(type, ser);
		}
		
		public bool Has<T>() {
			return Has(typeof(T));
		}
		
		public bool Has(Type type) {
			return Serializers.ContainsKey(type);
		}
		
		public XmlSerializer Get<T>() {
			return Get(typeof(T));
		}
		
		public XmlSerializer Get(Type type) {
			return Serializers[type];
		}
		
		public string ToText<T>(T inst) {
			return ToText(inst, typeof(T));
		}
		
		public string ToText(object inst, Type type) {
			Add(type);
			var text = new StringBuilder();
			var writer = XmlWriter.Create(text);
			Get(type).Serialize(writer, inst);
			return text.ToString();
		}
		
		public string ToTextEx(object inst) {
			return ToText(inst, inst.GetType());
		}
		
		public void ToStream<T>(T inst, Stream stream) {
			ToStream(inst, typeof(T), stream);
		}
		
		public void ToStream(object inst, Type type, Stream stream) {
			Add(type);
			Get(type).Serialize(stream, inst);
		}
		
		public void ToStreamEx(object inst, Stream stream) {
			ToStream(inst, inst.GetType(), stream);
		}
	}
}
