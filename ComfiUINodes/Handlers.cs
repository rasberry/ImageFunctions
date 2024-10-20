using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using System.Drawing;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImageFunctions.ComfiUINodes;

internal static class Handlers
{
	public static void HandleNotFound(HttpListenerContext ctx)
	{
		using HttpListenerResponse resp = ctx.Response;
		resp.ContentType = "text/plain";
		resp.StatusCode = (int)HttpStatusCode.NotFound;

		string err = $"404 - Not Found '{ctx.Request.Url?.LocalPath}'";
		resp.WriteText(err);
	}

	public static void ShowRegister(HttpListenerContext ctx)
	{
		if(!ctx.EnsureMethodIs(HttpMethod.Get)) { return; }

		var reg = Program.Register;
		var keyList = reg.All().OrderBy(n => $"{n.NameSpace}.{n.Name}");

		var resp = ctx.Response;
		resp.ContentEncoding = Encoding.UTF8;
		resp.ContentType = "application/json";
		resp.StatusCode = (int)HttpStatusCode.OK;

		Dictionary<string, List<object>> output = new();
		string currentSpace = "";
		foreach(var k in keyList) {
			if(k.NameSpace != currentSpace) {
				currentSpace = k.NameSpace;
			}
			var desc = reg.GetNameSpaceItemHelp(k);
			if(!output.TryGetValue(currentSpace, out var list)) {
				list = new List<object>();
				output.Add(currentSpace, list);
			}
			list.Add(new {
				Name = k.Name,
				Info = desc
			});
		}

		string json = JsonSerializer.Serialize(output);
		resp.WriteText(json);
	}

	public static void RunFunction(HttpListenerContext ctx)
	{
		var req = ctx.Request;
		var resp = ctx.Response;

		if(!ctx.EnsureMethodIs(HttpMethod.Post)) { return; }

		resp.StatusCode = (int)HttpStatusCode.OK;
		resp.WriteText("");
	}

	public static void FunctionInfo(HttpListenerContext ctx)
	{
		if(!ctx.EnsureMethodIs(HttpMethod.Get)) { return; }

		var reg = Program.Register;
		var fn = new FunctionRegister(reg);
		var list = fn.All().Order();

		var data = new Dictionary<string, List<UsageParam>>();

		foreach(string key in list) {
			var funcItem = fn.Get(key);
			var inst = funcItem.Item.Invoke(reg, null, null);
			var opts = inst.Options;
			if(opts is IUsageProvider uip) {
				//var namespaceList = GetFlagsFromUsageInfo(uip.GetUsageInfo());
				//var controls = GetControlParameters(uip);

				var usage = uip.GetUsageInfo();
				var paramsList = new List<UsageParam>();
				foreach(var p in usage.Parameters) {
					if(p is IUsageParameter iup) {
						var param = DetermineParamList(usage, iup);
						if(param != null) {
							paramsList.Add(param);
						}
					}
				}
				data.Add(key, paramsList);
			}
		}

		var json = JsonSerializer.Serialize(data, data.GetType(), options: JOptions);
		ctx.Response.WriteText(json);
	}

	static readonly JsonSerializerOptions JOptions = new() {
		IncludeFields = true,
	};

	static string GetParamDescription(IUsageProvider provider)
	{
		var usage = provider.GetUsageInfo();
		var ud = usage.Description;

		StringBuilder description = new();
		if((ud?.Descriptions?.Any()).GetValueOrDefault(false)) {
			foreach(var txt in usage.Description.Descriptions) {
				description.AppendLine(txt);
			}
		}
		return description.ToString();
	}

	static UsageParam DetermineParamList(Usage usage, IUsageParameter iup)
	{
		//bool isTwo = iup is IUsageParameterTwo; //TODO
		var it = iup.InputType.UnWrapNullable();

		if(iup is UsageRegistered ur) {
			return new UsageParamSync(iup, ur.NameSpace) { Type = "Register" };
		}
		else if(it.Is<bool>()) {
			return new UsageParam(iup) { Type = typeof(bool).Name };
		}
		else if(it.IsEnum) {
			IUsageEnum iue = null;
			foreach(var i in usage.EnumParameters) {
				if(i.EnumType.Equals(iup.InputType)) {
					iue = i; break;
				}
			}
			return new UsageSelection(iup, iue) { Type = "Enum" };
		}
		else if(it.Is<string>()) {
			return new UsageParamText(iup) { Type = typeof(string).Name };
		}
		else if(it.Is<ColorRGBA>() || it.Is<Color>()) {
			//TODO color picker ?
			return null;
		}
		else if(it.Is<Point>() || it.Is<PointF>() || it.Is<Core.PointD>()) {
			//TODO point picker .. ?
			return null;
		}
		else if(it.IsNumeric()) {
			return new UsageParamNumeric(iup);
		}

		throw Core.Logging.Squeal.NotSupported($"Type {it}");
	}

	[JsonDerivedType(typeof(UsageParamSync))]
	[JsonDerivedType(typeof(UsageSelection))]
	[JsonDerivedType(typeof(UsageParamText))]
	[JsonDerivedType(typeof(UsageParamNumeric))]
	class UsageParam
	{
		public UsageParam(IUsageParameter input)
		{
			Name = input.Name;
			Desc = input.Description;
		}

		public string Name;
		public string Desc;
		public string Type;
	}

	class UsageParamSync : UsageParam
	{
		public UsageParamSync(IUsageParameter input, string ns) : base(input)
		{
			this.NameSpace = ns;
		}

		public string NameSpace;
	}

	class UsageSelection : UsageParam
	{
		public UsageSelection(IUsageParameter input, IUsageEnum @enum) : base(input)
		{
			int index = 0;
			var valsList = Rasberry.Cli.PrintHelper.EnumAll(@enum.EnumType, @enum.ExcludeZero);
			foreach(var item in valsList) {
				var name = @enum.NameMap != null ? @enum.NameMap(item) : item.ToString();
				var desc = @enum.DescriptionMap != null ? @enum.DescriptionMap(item) : null;
				var sel = new OneSelection { Name = name, Desc = desc, Value = item };
				Choices.Add(sel);

				if(input.Default != null && input.Default.Equals(item)) {
					SelectedIndex = index;
				}
				index++;
			}
		}

		public List<OneSelection> Choices = new();
		public int SelectedIndex;
	}

	sealed class OneSelection
	{
		public string Name;
		public string Desc;
		public object Value;
	}

	class UsageParamText : UsageParam
	{
		public UsageParamText(IUsageParameter input) : base(input)
		{
			if(input.Default != null) {
				Text = input.Default.ToString();
			}
		}

		public string Text;
	}

	class UsageParamNumeric : UsageParam
	{
		public UsageParamNumeric(IUsageParameter input) : base(input)
		{
			NumberType = input.InputType.UnWrapNullable();
			IsNumberPct = input.IsNumberPct;
			SetDefaultsFromType(input);
			Type = NumberType?.Name;
		}

		public double Min;
		public double Max;
		public bool IsNumberPct;
		readonly Type NumberType;
		public double Value;

		void SetDefaultsFromType(IUsageParameter input)
		{
			// https://stackoverflow.com/questions/503263/how-to-determine-if-a-type-implements-a-specific-generic-interface-type
			bool isMinMax = NumberType.GetInterfaces().Any(x =>
				x.IsGenericType &&
				x.GetGenericTypeDefinition() == typeof(System.Numerics.IMinMaxValue<>)
			);

			if(isMinMax) {
				double defMin, defMax;
				if(IsNumberPct) {
					defMin = 0.0;
					defMax = 1.0;
				}
				else if(NumberType.Is<double>()) {
					//using the full double min max breaks the slider
					defMin = float.MinValue;
					defMax = float.MaxValue;
				}
				else {
					//Note int.min/max is only used for the name
					defMin = Convert.ToDouble(NumberType.GetField(nameof(int.MinValue)).GetValue(null));
					defMax = Convert.ToDouble(NumberType.GetField(nameof(int.MaxValue)).GetValue(null));
				}
				Min = input.Min ?? defMin;
				Max = input.Max ?? defMax;
				//Log.Debug($"{input.Name} {NumberType.Name} min={Min} max={Max}");
			}
			else {
				throw Core.Logging.Squeal.NotSupported($"Type {NumberType.Name}");
			}

			Value = input.Default == null ? 0.0 : Convert.ToDouble(input.Default);
		}
	}

	static List<string> GetFlagsFromUsageInfo(Usage info)
	{
		List<string> nameSpaceList = new();
		foreach(var p in info.Parameters) {
			if(p is UsageRegistered ur) {
				nameSpaceList.Add(ur.NameSpace);
			}
		}
		return nameSpaceList;
	}
}
