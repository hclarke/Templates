using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace Templates
{
	public class Template {
		
		private Template() {
		}
		static char[] buffer = new char[4096];
		static int bufferCount = 0;
		
		
		string prefix;
		
		string childName;
		string[] childPath;
		Template childNode;
		Template sepByNode;
		Template nextNode;
		
		Dictionary<Type, Func<object, object>> getters = new Dictionary<Type, Func<object, object>>();
		
		object GetData(object x) {
			Func<object,object> getter;
			var type = x.GetType();
			if(!getters.TryGetValue(type, out getter)) {
				var p = Expression.Parameter(typeof(object), "x");
				Expression e = Expression.Convert(p, type);
				for(int i = 1; i < childPath.Length; ++i) {
					e = Expression.PropertyOrField(e, childPath[i]);
				}
				e = Expression.Convert(e, typeof(object));
				var f = Expression.Lambda<Func<object,object>>(e, p);
				getter = getters[type] = f.Compile();
			}
			return getter(x);
		}
		
		static char  escape = '\\';
		static char  openChar = '@';
		static char  closeChar = '@';
		static char  endChar = '$';
		static char  separator = ':';
		static char  fieldAccess = '.';
		static char  sepStart = '?';
		
		public override string ToString () {
			var sb = new StringBuilder();
			ToString(sb);
			return sb.ToString();
		}
		
		class Binding {
			public string name;
			public object obj;
			public Binding next;
			public bool isLast = true;
		}
		
		public string Render(object obj) {
			var sb = new StringBuilder();
			Render (sb, obj);
			return sb.ToString();
		}
		public void Render(StringBuilder sb, object obj) {
			Binding b = null;
			var fields = obj.GetType().GetFields(System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
			foreach(var field in fields) {
				b = new Binding() {
					name = field.Name,
					obj = field.GetValue(obj),
					next = b,
				};
			}
			
			Render (sb, b);
		}
		void Render(StringBuilder sb, Binding bindings, bool last = true) {
			if(prefix != null) sb.Append(prefix);
			if(childPath != null) {
				//look up the path
				object obj = null;
				for(var n = bindings; n != null; n = n.next) {
					if(childPath[0] == n.name) {
						obj = GetData(n.obj);
					}
				}
				if(obj == null) throw new Exception("object missing: " + childPath[0]);
				if(childNode != null) {
					if(obj is IEnumerable) {
						var objs = (obj as IEnumerable).Cast<object>().ToArray();
						for(int i = 0; i < objs.Length; ++i) {
							var o = objs[i];
							var isLast = i == objs.Length-1;
							var newBindings = new Binding() {
								name = childName,
								obj = o,
								next = bindings,
							};
							childNode.Render(sb, newBindings, isLast);
						}
					}
					else {
						if(obj != null && !obj.Equals(false)) {
							var newBindings = new Binding() {
								name = childName,
								obj = obj,
								next = bindings,
							};
							childNode.Render(sb, newBindings);
						}
					}
				}
				else {
					sb.Append(obj.ToString());
				}
			}
			if(!last) {
				if(sepByNode != null) sepByNode.Render(sb, bindings);
			}
			if(nextNode != null) nextNode.Render(sb, bindings, last);
		}
		void ToString(StringBuilder sb) {
			
			if(prefix != null && prefix.Length > 0) {
				sb.Append(prefix);
			}
			if(childPath != null) {
				sb.Append(openChar);
				if(childName != null && childName.Length != 0) {
					sb.Append(childName);
					sb.Append(separator);
				}
				for(int i = 0; i < childPath.Length; ++i) {
					sb.Append(childPath[i]);
					if(i < childPath.Length-1) sb.Append(fieldAccess);
				}
				sb.Append(closeChar);
				
				if(childNode != null) {
					childNode.ToString(sb);
					sb.Append(endChar);
				}
			}
			if(sepByNode != null) {
				sb.Append(sepStart);
				sepByNode.ToString(sb);
				sb.Append(endChar);
			}
			if(nextNode != null) nextNode.ToString(sb);
		}
		
		public static Template Create(string text) {
			var i = 0;
			return Create (text, ref i);
		}
		public static Template Create(string text, ref int i) {
			var template = new Template();
			bufferCount = 0;
			for(; i < text.Length; ++i) {
				var c = text[i];
				if(c == escape) {
					++i;
				}
				else if(c == openChar) {
					template.prefix = new string(buffer, 0, bufferCount);
					var cpaths = new List<string>();
					bufferCount = 0;
					for(++i; i < text.Length; ++i) {
						if(text[i] == separator) {
							template.childName = new string(buffer, 0, bufferCount);
							bufferCount = 0;
							++i;
						}
						var nc = text[i];
						if(nc ==  closeChar|| nc == fieldAccess) {
							cpaths.Add(new string(buffer, 0, bufferCount));
							bufferCount = 0;
							++i;
							if(nc == closeChar) break;
						}
						buffer[bufferCount++] = text[i];
					}
					
					if(cpaths.Count > 0) {
						template.childPath = cpaths.ToArray();
					}
					if(template.childName != null) {
						template.childNode = Create (text, ref i);
					}
					template.nextNode = Create (text, ref i);
					return template;
				}
				else if(c == endChar) {
					++i;
					template.prefix = new string(buffer, 0, bufferCount);
					return template;
				}
				else if(c == sepStart) {
					++i;
					template.prefix = new string(buffer, 0, bufferCount);
					template.sepByNode = Create(text, ref i);
					template.nextNode = Create(text, ref i);
					return template;
				}
				buffer[bufferCount++] = text[i];
			}
			template.prefix = new string(buffer, 0, bufferCount);
			return template;
		}
	}
}
