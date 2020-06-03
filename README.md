# Locutius.Capture

Client console tool to capture both microphone and speaker (loopback) on a local computer. 

- Convert audio streams to PCM format, 16000 samples per second, 16 bits per sample, 1 channel (mono). 
- Send streams via websockets to the targeted Uri (Piscato.Gateway).

## Build 

``` cmd
dotnet build
```
This application targets .Net Core 3.1.

## Run

Just start Locutius.Capture.exe with at least --uri option:

``` cmd
-d, --debug    Enable the debug mode (record audio files locally).
-u, --uri      Required. Uri of the audio gateway.
--help         Display this help screen.
--version      Display version information.
```
For instance: 
``` cmd
.\Locutius.Capture.exe --uri "ws://localhost:5000" --debug
```
The debug mode record audio file before and after the conversion. 

- guid-loopback-`raw`.wav : raw is the audio file directly captured (Ieee).
- guid-loopback-`out`.wav : out is the audio file converted (PCM).


# Locutius.Gateway

## Build

For local development, add `appsettings.Development.json` file to Locutius.Gateway root with the following format.

``` json
{
    "Logging": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    },
    "Azure.Storage.Container.Audio": "audios",
    "Azure.Storage.Container.Transcript": "transcripts",
    "Azure.Cognitive.Speech.Key": "<YourSpeechKey>",
    "Azure.Cognitive.Speech.Region": "westeurope",
    "Azure.Cognitive.Speech.Language": "en-US",
    "Azure.Cognitive.Speech.EndpointId":  "",
    "Azure.KeyVault.Name": "locutius-dev-keyvault",
}
```

Once deployed, the following environment variables MUST be provided:

- `Azure.Storage.Container.Audio`: Name of the Audio container,
- `Azure.Storage.Container.Transcript`: Name of the Transcript container,
- `Azure.Cognitive.Speech.Region`: Region of speech service (for instance 'westeurope').
- `Azure.Cognitive.Speech.Language`: Targeted language (for instance 'en-US').
- `Azure.Cognitive.Speech.EndpointId`:  Endpoint ID of a customized speech model.
- `Azure.KeyVault.Name`: KeyVault name.
- `Azure.Cognitive.Speech.Key`: Key of speech service. ** Should be placed in KeyVault **