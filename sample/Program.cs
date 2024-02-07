using System.Reflection;
using Qt.Widgets;
using HarmonyLib;

// It is unfortunate but we have to set it to Unknown first.
Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

// Needed for now, should fork CppSharp.Runtime instead
var harmony = new Harmony("com.sharpqt.sample.patch");
harmony.PatchAll(Assembly.GetExecutingAssembly());

unsafe
{
    int argc = 0;
    _ = new QApplication(ref argc, null);

    var win = new QPushButton(null) { Text = "Click me" };

    win.Clicked += o => Console.WriteLine($"Button clicked (\"checked\" was {o})");
    win.WindowTitleChanged += o => Console.WriteLine($"Title changed to \"{o}\"");

    win.WindowTitle = "Some title";
    win.Resize(1280, 720);
    win.Show();

    QApplication.Exec();
}
