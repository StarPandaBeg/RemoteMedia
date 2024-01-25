#Requires -RunAsAdministrator
param (
  [switch]$uninstall = $false
)

function Read-Required {
  param (
    [string]$prompt
  )
  do {
    $value = Read-Host -Prompt $prompt
    $value = $value.Trim();
  } while ($value -eq "");
  return $value;
}

function Read-Optional {
  param (
    [string]$prompt,
    [object]$optional
  )
  $value = Read-Host -Prompt $prompt
  $value = $value.Trim();
  if ($value -eq "") {
    return $optional;
  }
  return $value;
}

function Test-RemoteMedia-Service {
  $service = Get-Service -Name "RemoteMedia" -ErrorAction SilentlyContinue
  return $service.Length -gt 0
}

function Install-RemoteMedia-Service {
  if (Test-RemoteMedia-Service) {
    Write-Host "Service was already installed, removing..."
    Remove-RemoteMedia-Service
  }

  $path = Join-Path $PSScriptRoot "RemoteMedia.exe"
  $params = @{
    Name           = "RemoteMedia"
    BinaryPathName = $path
    DisplayName    = "RemoteMedia Service"
    StartupType    = "Automatic"
    Description    = "This service provides remote control of the playing media using MQTT commands."
    ErrorAction    = "Stop"
  }
  New-Service @params | Out-Null

  Write-Host "Service was successfully installed!`n"
}

function Remove-RemoteMedia-Service {
  C:\Windows\System32\sc.exe stop "RemoteMedia" | Out-Null
  C:\Windows\System32\sc.exe delete "RemoteMedia" | Out-Null
}

Write-Host "Welcome to the " -NoNewline;
Write-Host "Remote" -ForegroundColor Blue -NoNewline;
Write-Host "Media" -ForegroundColor Magenta -NoNewline;
Write-Host " installer script!";
Write-Host "Developed by " -NoNewline;
Write-Host "StarPanda" -ForegroundColor DarkRed -NoNewline;
Write-Host " (https://github.com/StarPandaBeg)`n";

if ($uninstall -eq $true) {
  if (Test-RemoteMedia-Service) {
    Write-Host "Removing..."
    Remove-RemoteMedia-Service -ErrorAction Stop
    Write-Host "Service removed successfully"
  }
  else {
    Write-Host "Service isn't installed"
  }
  
  Write-Host "`nThat's all. Good bye!"
  Pause
  Exit
}

Install-RemoteMedia-Service -ErrorAction Stop

Write-Host "We'll ask you for some configuration";
Write-Host "Read more about at https://github.com/StarPandaBeg/RemoteMediaService`n";

$mqttHost = Read-Required "Enter MQTT server host";
do {
  $mqttPort = Read-Optional "Enter MQTT server port (0-65353) [1883]" 1883;
} while (-not [int]::TryParse($mqttPort, [ref]$null) -and $mqttHost -ge 0 -and - $mqttHost -le 65353);

$hasCredentials = Read-Optional "Should authorize by login/password? (0-1) [0]" "0"
$mqttUser = "";
$mqttPassword = "";
if ($hasCredentials -eq "1") {
  $mqttUser = Read-Required "Enter username";
  $mqttPassword = Read-Required "Enter password";
}

$mqttListenTopic = Read-Optional "Which topic service should listen? [/remotemedia/control]" "/remotemedia/control";
$mqttResponseTopic = Read-Optional "Which topic service should response? [/remotemedia/out]" "/remotemedia/out";
$mqttPingTopic = Read-Optional "Which topic service should ping? [/remotemedia/ping]" "/remotemedia/ping";

Write-Host "`nVerify the entered configuration data`n"
Write-Host "MQTT Host:" $mqttHost
Write-Host "MQTT Port:" $mqttPort
if ($hasCredentials -eq "1") {
  Write-Host "MQTT Username:" $mqttUser
  Write-Host "MQTT Password:" $mqttPassword
}
Write-Host "Listen topic:" $mqttListenTopic
Write-Host "Response topic:" $mqttResponseTopic
Write-Host "Ping topic:" $mqttPingTopic

$verified = Read-Optional "`nContinue? (0-1) [0]" "0"
if ($verified -eq "0") {
  Write-Host "Abort! Removing application service`n"
  Remove-RemoteMedia-Service
  Exit
}

$encryptionKey = -join (((48..57) + (65..90) + (97..122)) * 80 | Get-Random -Count 32 | ForEach-Object { [char]$_ })
$hasCredentialsValue = If ($hasCredentials) { "true" } Else { "false" }

$params = @{
  Path         = "HKLM:\SYSTEM\CurrentControlSet\Services\RemoteMedia"
  Name         = "Environment"
  PropertyType = "MultiString"
  ErrorAction  = "Stop"
  Value        = "MQTT_HOST=$mqttHost`0MQTT_PORT=$mqttPort`0MQTT_USE_CREDENTIALS=$hasCredentialsValue`0MQTT_USERNAME=$mqttUser`0MQTT_PASSWORD=$mqttPassword`0MQTT_LISTEN_TOPIC=$mqttListenTopic`0MQTT_RESPONSE_TOPIC=$mqttResponseTopic`0MQTT_PING_TOPIC=$mqttPingTopic`0ENCRYPTION_KEY=$encryptionKey`0"
}
New-ItemProperty @params -Force | Out-Null
Write-Host "Configuration set!"

try {
  Start-Service -name "RemoteMedia" -ErrorAction Stop | Out-Null
  Write-Host "Service 'RemoteMedia Service' started successfully"
}
catch {
  Write-Host "Service start failed. Start 'RemoteMedia Service' manually from services.msc"
}

Write-Host "`nEncryption key: " -NoNewline
Write-Host $encryptionKey -ForegroundColor Yellow
Write-Host "Please ensure to keep this key confidential. You'll need it to install RemoteMediaWebService."

Write-Host "`nThat's all. Good bye!"
Pause