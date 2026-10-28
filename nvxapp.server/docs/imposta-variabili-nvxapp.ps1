# Crea le variabili d'ambiente di SISTEMA con i segreti di nvxapp (punti 7 e 7b di ANALISI_DOCKER.md),
# lette da Visual Studio e da IIS sulla stessa macchina. Si esegue UNA VOLTA per macchina
# (e di nuovo solo per cambiare un valore). Lo script non contiene segreti e non li stampa.
#
# Per ogni variabile:
#   - se e' gia' impostata la lascia com'e' (con -Sovrascrivi la richiede di nuovo)
#   - altrimenti, per le chiavi Brevo/Groq/OpenRouter, la legge dal vecchio file *.key in nvxapp.server se c'e'
#   - altrimenti chiede il valore (digitato senza mostrarlo; Invio a vuoto = salta)
#
# Uso (PowerShell aperto come AMMINISTRATORE):
#   powershell -ExecutionPolicy Bypass -File imposta-variabili-nvxapp.ps1 -Prova         (mostra solo lo stato)
#   powershell -ExecutionPolicy Bypass -File imposta-variabili-nvxapp.ps1                (imposta + iisreset)
#   powershell -ExecutionPolicy Bypass -File imposta-variabili-nvxapp.ps1 -Sovrascrivi   (richiede anche quelle gia' impostate)
# Dopo: chiudere e riaprire Visual Studio.
#
# Per avere i valori gia' scritti dentro: copia locale *.segreti.ps1 (esclusa da git).

param([switch]$Prova, [switch]$Sovrascrivi)

$serverDir = Split-Path $PSScriptRoot -Parent   # cartella nvxapp.server

# nome variabile = descrizione, file *.key da cui leggerla (vecchio sistema, punto 7b)
$variabili = [ordered]@{
    'NVXAPP_ConnectionStrings__nvxappDbContext' = @('connessione al database PostgreSQL, es. Server=localhost;Port=5432;Database=nvxapp_V2;User Id=postgres;Password=...;Include Error Detail=true;', $null)
    'NVXAPP_JwtParameter__Key'                  = @('chiave che firma i token di login (almeno 32 caratteri; se cambia, tutti rifanno il login)', $null)
    'NVXAPP_RabbitQm__Connection__Password'     = @('password di RabbitMQ', $null)
    'NVXAPP_Email__Password'                    = @('chiave SMTP di Brevo (email di conferma e reset password)', 'brevo.key')
    'NVXAPP_AI__AI_groq__ApiKey'                = @('API key di Groq (chat AI con AI:UseLLM = AI_groq)', 'groq.key')
    'NVXAPP_AI__AI_openrouter__ApiKey'          = @('API key di OpenRouter (chat AI con AI:UseLLM = AI_openrouter)', 'openrouter.key')
    'NVXAPP_AI__AI_deepseek__ApiKey'            = @('API key di DeepSeek (chat AI con AI:UseLLM = AI_deepseek)', $null)
}

if (-not $Prova) {
    $admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    if (-not $admin) { Write-Host 'Serve un PowerShell aperto come amministratore.' -ForegroundColor Red; exit 1 }
}

$modificate = 0
foreach ($nome in $variabili.Keys) {
    $descrizione = $variabili[$nome][0]
    $fileKey = $variabili[$nome][1]
    $attuale = [Environment]::GetEnvironmentVariable($nome, 'Machine')

    if ($attuale -and -not $Sovrascrivi) {
        Write-Host ("{0,-45} gia' impostata ({1} caratteri)" -f $nome, $attuale.Length)
        continue
    }

    $valore = $null
    $origine = ''
    if ($fileKey) {
        $path = Join-Path $serverDir $fileKey
        if (Test-Path $path) { $valore = (Get-Content $path -Raw).Trim(); $origine = "dal file $fileKey" }
    }

    if ($Prova) {
        $stato = if ($valore) { "da impostare $origine" } else { 'da impostare (verra'' chiesta)' }
        Write-Host ("{0,-45} {1}" -f $nome, $stato) -ForegroundColor Yellow
        continue
    }

    if (-not $valore) {
        Write-Host ''
        Write-Host "$nome : $descrizione"
        $sicuro = Read-Host 'Valore (non viene mostrato; Invio a vuoto = salta)' -AsSecureString
        $valore = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($sicuro))
        $origine = 'digitata'
    }

    if ([string]::IsNullOrWhiteSpace($valore)) {
        Write-Host ("{0,-45} saltata" -f $nome) -ForegroundColor Yellow
        continue
    }

    [Environment]::SetEnvironmentVariable($nome, $valore.Trim(), 'Machine')
    Write-Host ("{0,-45} impostata ({1})" -f $nome, $origine) -ForegroundColor Green
    $modificate++
}

if (-not $Prova -and $modificate -gt 0) {
    Write-Host 'Riavvio di IIS (iisreset) perche'' veda le nuove variabili...'
    iisreset
    Write-Host 'Fatto. Chiudere e riaprire Visual Studio.' -ForegroundColor Green
}
