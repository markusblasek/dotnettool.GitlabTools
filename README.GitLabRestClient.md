# GitLabTools.GitLabRestClient

RestClient to access GitLabRestAPI

## License

The MIT License. See the [license](https://github.com/markusblasek/dotnettool.gitlabtools/blob/main/LICENSE) file for details.

## Usage

```cs
private static void ConfigureServices(IServiceCollection services)
{
	// ...
	services.AddSingleton<IGitlabRestApiClient, GitLabRestApiClient>();
	services.AddSingleton(_ =>
	{
		var result = new FlurlClientCache()
			.WithDefaults(builder => builder.ConfigureInnerHandler(hch =>
			{
			    // Set Proxy if needed
				var proxyAddress = ProxyUtils.ReadProxyFromEnvironmentVariables();
				if (string.IsNullOrWhiteSpace(proxyAddress))
				{
					return;
				}
				var bypassList = ProxyUtils.ReadNoProxyValueAsArrayFromEnvironmentVariables();
				hch.Proxy = new WebProxy(proxyAddress, false, bypassList);
				hch.UseProxy = true;
			}));

		result.Add(FlurClientNameConstants.GitLabClient, baseUrl: null, builder =>
		{
		    // Add settings
			builder.WithSettings(x =>
			{
				x.Timeout = TimeSpan.FromSeconds(10);
				x.HttpVersion = HttpVersion.Version20.ToString();
			});
		});
		return result;
	});
}
```
