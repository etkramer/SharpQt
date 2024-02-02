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

    var win = new QWidget(null)
    {
        WindowTitle = "Some title",
        StyleSheet = "QWidget { background-color: black; }"
    };

    win.Resize(1280, 720);
    win.Show();

    QApplication.Exec();
}
