#!/usr/bin/env pwsh
# Publishes self-contained packages for the supported platforms.
# Usage: pwsh scripts/publicar-plataformas.ps1 [-Configuration Release] [-OutputRoot dist]
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$OutputRoot = 'dist'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Empaqueta el contenido en un archivo tar.gz. Al publicar desde Windows el archivo del
# ejecutable sale sin el bit de ejecución, por lo que la instalación en Linux y macOS
# incluye un paso explícito para otorgarlo.
function ComprimirTarGz {
    param(
        [Parameter(Mandatory)][string]$RutaContenido,
        [Parameter(Mandatory)][string]$RutaArchivo
    )

    $archivoTemporal = "$RutaArchivo.tar"
    $flujo = [System.IO.File]::Create($archivoTemporal)
    try {
        $escritor = [System.Formats.Tar.TarWriter]::new($flujo, [System.Formats.Tar.TarEntryFormat]::Pax, $true)
        try {
            foreach ($ruta in Get-ChildItem -LiteralPath $RutaContenido -Recurse -File | Sort-Object FullName) {
                $relativa = $ruta.FullName.Substring($RutaContenido.Length).TrimStart('\', '/') -replace '\\', '/'
                $escritor.WriteEntry($ruta.FullName, $relativa)
            }
        }
        finally {
            $escritor.Dispose()
        }
    }
    finally {
        $flujo.Dispose()
    }

    # Comprime el tar con gzip mediante la API de .NET.
    $origenTar = [System.IO.File]::OpenRead($archivoTemporal)
    $destinoGz = [System.IO.File]::Create($RutaArchivo)
    try {
        $compresor = [System.IO.Compression.GZipStream]::new($destinoGz, [System.IO.Compression.CompressionLevel]::Optimal)
        try {
            $origenTar.CopyTo($compresor)
        }
        finally {
            $compresor.Dispose()
        }
    }
    finally {
        $destinoGz.Dispose()
        $origenTar.Dispose()
        Remove-Item -LiteralPath $archivoTemporal -Force -ErrorAction SilentlyContinue
    }
}

$raiz = Split-Path -Parent $PSScriptRoot
$proyectoEscritorio = Join-Path $raiz 'src/MCP.MicrosoftPlanner.Desktop/MCP.MicrosoftPlanner.Desktop.csproj'

# Each entry lists the runtime identifier and the package format that users download.
$plataformas = @(
    @{ Rid = 'win-x64';    Carpeta = 'windows-x64';    Formato = 'zip' }
    @{ Rid = 'win-arm64';  Carpeta = 'windows-arm64';  Formato = 'zip' }
    @{ Rid = 'linux-x64';  Carpeta = 'linux-x64';      Formato = 'tar.gz' }
    @{ Rid = 'linux-arm64'; Carpeta = 'linux-arm64';   Formato = 'tar.gz' }
    @{ Rid = 'osx-x64';    Carpeta = 'macos-x64';      Formato = 'tar.gz' }
    @{ Rid = 'osx-arm64';  Carpeta = 'macos-arm64';    Formato = 'tar.gz' }
)

$destinoRaiz = Join-Path $raiz $OutputRoot
New-Item -ItemType Directory -Force -Path $destinoRaiz | Out-Null

# Los avisos de licencia acompañan a cada paquete: las dependencias de terceros
# se redistribuyen dentro de los binarios y deben conservar sus avisos.
$avisos = @('LICENSE', 'NOTICE', 'THIRD-PARTY-NOTICES.md')

foreach ($plataforma in $plataformas) {
    $destino = Join-Path $destinoRaiz $plataforma.Carpeta
    if (Test-Path $destino) {
        Remove-Item -LiteralPath $destino -Recurse -Force
    }

    Write-Host "Publicando aplicación y servidor MCP $($plataforma.Rid)..."
    dotnet publish $proyectoEscritorio `
        --configuration $Configuration `
        --runtime $plataforma.Rid `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:EnableCompressionInSingleFile=true `
        --output $destino

    if ($LASTEXITCODE -ne 0) {
        throw "Falló la publicación para $($plataforma.Rid)."
    }

    $extension = if ($plataforma.Formato -eq 'zip') { '.exe' } else { '' }
    $origenEjecutable = Join-Path $destino "MCP.MicrosoftPlanner.Desktop$extension"
    if (-not (Test-Path $origenEjecutable)) {
        throw "No se encontró el ejecutable publicado para $($plataforma.Rid)."
    }
    Move-Item -LiteralPath $origenEjecutable -Destination (Join-Path $destino "MCP.MicrosoftPlanner$extension")

    # Symbols, runtime metadata for the referenced project, and the sample configuration
    # are useful for development but not required by the standalone user executable.
    Get-ChildItem -LiteralPath $destino -File | Where-Object {
        $_.Extension -eq '.pdb' -or $_.Name -eq 'MCP.MicrosoftPlanner.runtimeconfig.json' -or $_.Name -eq 'appsettings.json'
    } | Remove-Item -Force

    foreach ($aviso in $avisos) {
        $origenAviso = Join-Path $raiz $aviso
        if (Test-Path $origenAviso) {
            Copy-Item $origenAviso (Join-Path $destino $aviso) -Force
        }
    }

    $archivo = "MCP.MicrosoftPlanner-$($plataforma.Rid).$($plataforma.Formato)"
    $rutaArchivo = Join-Path $destinoRaiz $archivo
    if (Test-Path $rutaArchivo) {
        Remove-Item -LiteralPath $rutaArchivo -Force
    }

    Write-Host "Empaquetando $archivo..."
    if ($plataforma.Formato -eq 'zip') {
        Compress-Archive -Path (Join-Path $destino '*') -DestinationPath $rutaArchivo -Force
    }
    else {
        ComprimirTarGz -RutaContenido $destino -RutaArchivo $rutaArchivo
    }
}

Write-Host "Paquetes generados en $destinoRaiz"
