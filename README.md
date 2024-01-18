# GitlabCiTools

URL encodes an string and decodes an url encoded string.

## License

The MIT License. See the [license](https://github.com/markusblasek/dotnettool.gitlabcitools/blob/main/LICENSE) file for details.

## Install
`$ dotnet tool install --global gitlabtools --version 1.0.0`

### Install from local nuget
`$ dotnet tool install --global --add-source "#path_to_folder#" gitlabtools --version 1.0.0`

## Usage
```bash
dotnet gitlabtools readProject --gitLabUrl https://gitlab.test.com --projectId 123456 --accessToken #PersonalAccessToken#
dotnet gitlabtools deletePipelines --gitLabUrl https://gitlab.test.com --pipelinesToKeep 80 --projectId 123456  --accessToken #PersonalAccessToken#
dotnet gitlabtools deletePipelines --gitLabUrl https://gitlab.test.com --pipelinesToKeep 80 --projectId 123456  --accessToken #PersonalAccessToken# --dryRun
```

## Possible exit codes

| Exit code | Description |
|-----------|-----------------------------------------|
| 0         | ok                                      |
| 1         | Illegal arguments - e.g. url is invalid |
| 128       | Unexpected error occured                |

## Proxy

Proxy configuration is read from the environment variables `http_proxy`, `https_proxy` and `no_proxy`.
