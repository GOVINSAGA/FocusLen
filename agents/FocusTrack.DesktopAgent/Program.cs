using FocusTrack.DesktopAgent;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "FocusTrack Agent";
});

builder.Services.AddHttpClient("FocusTrackApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5028/");
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
