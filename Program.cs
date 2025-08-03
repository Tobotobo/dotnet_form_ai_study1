DotNetEnv.Env.Load();

NXUI.Desktop.NXUI.Run(
    () => new MainWindow(), 
    "dotnet_form_ai_study1", 
    args, 
    ThemeVariant.Light,
    ShutdownMode.OnLastWindowClose);
