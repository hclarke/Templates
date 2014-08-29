using System;
using Templates;
using System.Reflection;
using System.IO;
using System.Text;
using System.Linq;

namespace Example
{
	class MainClass
	{
		class VectorDef {
			public int dim;
			public string[] components;
			public string[] operators;
			public string baseType;
		}
		
		public static void Main (string[] args)
		{
			var asm = Assembly.GetExecutingAssembly();
			var stream = asm.GetManifestResourceStream("Example.vector.txt");
			var text = new StreamReader(stream).ReadToEnd();
			
			var template = Template.Create(text);
			
			var comps = new[] { "x", "y", "z", "w" };
			var operators = new[] { "+", "-", "*", "/" };
			
			for(int i = 0; i < 4; ++i) {
				var dim = i+1;
				var def = new VectorDef() {
					dim = dim,
					components = comps.Take(dim).ToArray(),
					operators = operators,
					baseType = "float",
				};
				
				Console.WriteLine(template.Render(def));
			}
		}
	}
}
