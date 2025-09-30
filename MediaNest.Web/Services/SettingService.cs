namespace MediaNest.Web.Services;

public class SettingService {


    private readonly string _configPath;

    public SettingService(IWebHostEnvironment env) {
        _configPath = Path.Combine(env.ContentRootPath, "appsettings.json");
    }

}
