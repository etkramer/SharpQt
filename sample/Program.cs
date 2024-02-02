using System.Reflection;
using Qt;
using Qt.Widgets;
using HarmonyLib;

// It is unfortunate but we have to set it to Unknown first.
Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

// Needed for now, should fork CppSharp.Runtime instead
var harmony = new Harmony("com.sharpqt.sample.patch");
harmony.PatchAll(Assembly.GetExecutingAssembly());

var obj = new QObject(null);
obj.ObjectName = "Hello object\0 123!";

Console.WriteLine(obj.IsWidgetType);
Console.WriteLine(obj.ObjectName);

unsafe
{
    int argc = 0;
    var app = new QApplication(ref argc, null);

    var win = new QWidget(null);
    win.WindowTitle = "Some title";
    win.StyleSheet = "QWidget { background-color: black; }";
    win.Resize(1280, 720);
    win.Show();

    QApplication.Exec();
}
