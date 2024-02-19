using Qt.Widgets;
using Qt.Gui;

// It is unfortunate but we have to set it to Unknown first.
Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

unsafe
{
    int argc = 0;
    _ = new QApplication(ref argc, null);

    var win = new CustomButton() { Text = "Click me" };

    win.Clicked += o => Console.WriteLine($"Button clicked (\"checked\" was {o})");
    win.WindowTitleChanged += o => Console.WriteLine($"Title changed to \"{o}\"");

    win.WindowTitle = "Some title";
    win.Resize(1280, 720);
    win.Show();

    QApplication.Exec();
}

class CustomButton : QPushButton
{
    protected override void OnPaint(QPaintEvent args)
    {
        Console.WriteLine("Painted");
        base.OnPaint(args);
    }
}
