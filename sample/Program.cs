using Qt;

var obj = new QObject(null);
obj.ObjectName = "Hello object\0 123!";

Console.WriteLine(obj.IsWidgetType);
Console.WriteLine(obj.ObjectName);
