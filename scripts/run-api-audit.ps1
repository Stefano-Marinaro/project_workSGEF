# Run synthetic API workflows on a fresh isolated database, then remove test resources.
# Requires running project database, a built backend image, Docker and Python.
$ErrorActionPreference = 'Stop'
$auditRoot = Split-Path -Parent $PSScriptRoot
$auditDatabase = 'gocare_audit_' + (Get-Date -Format 'yyyyMMddHHmmss')
$auditContainer = 'gocare-audit-api'
$auditDatabaseCreated = $false
$auditContainerCreated = $false
$auditPreviousDatabase = $env:AUDIT_DATABASE
$auditPreviousFrontend = $env:AUDIT_FRONTEND_URL
$auditExitCode = 1

try {
    docker inspect $auditContainer *> $null
    if ($LASTEXITCODE -eq 0) { throw 'An audit container already exists. Finish that audit before starting another.' }
    if ($auditDatabase -notmatch '^gocare_audit_[0-9]+$') { throw 'Invalid audit database name.' }
    docker exec project_worksgef-db-1 psql -U postgres -v ON_ERROR_STOP=1 -c "CREATE DATABASE $auditDatabase;" | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Could not create the isolated database.' }
    $auditDatabaseCreated = $true

    $auditSettings = @{}
    Get-Content -LiteralPath (Join-Path $auditRoot '.env') | ForEach-Object {
        if ($_ -match '^([A-Z_]+)=(.*)$') { $auditSettings[$Matches[1]] = $Matches[2].Trim() }
    }
    $auditFrontend = $auditSettings['FRONTEND_PUBLIC_URL']
    if (-not $auditFrontend) { $auditFrontend = 'http://localhost:8081' }
    $auditFrontend = $auditFrontend.TrimEnd('/')
    $auditArguments = @('run', '--detach', '--name', $auditContainer, '--network', 'project_worksgef_default',
        '--publish', '127.0.0.1:15161:8080', '--env', 'ASPNETCORE_ENVIRONMENT=Development',
        '--env', "ConnectionStrings__GoCareDb=Host=db;Port=5432;Database=$auditDatabase;Username=postgres;Password=postgres",
        '--env', "GraphHopper__ApiKey=$($auditSettings['GRAPHHOPPER_API_KEY'])",
        '--env', 'Jwt__SigningKey=audit-only-signing-key-at-least-32-characters',
        '--env', 'Smtp__Username=', '--env', 'Stripe__SecretKey=',
        '--env', "Frontend__CaregiverInviteUrl=$auditFrontend/invito-caregiver",
        '--env', 'Logging__LogLevel__Microsoft.EntityFrameworkCore=Warning', 'project_worksgef-backend')
    & docker @auditArguments | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Could not start the isolated API.' }
    $auditContainerCreated = $true
    $auditReady = $false
    for ($auditAttempt = 0; $auditAttempt -lt 40; $auditAttempt++) {
        try {
            $null = Invoke-RestMethod 'http://127.0.0.1:15161/swagger/v1/swagger.json' -TimeoutSec 2
            $auditReady = $true
            break
        } catch { Start-Sleep -Milliseconds 250 }
    }
    if (-not $auditReady) { throw 'The isolated API did not start.' }
    $env:AUDIT_DATABASE = $auditDatabase
    $env:AUDIT_FRONTEND_URL = $auditFrontend
    python (Join-Path $PSScriptRoot 'audit-api.py')
    $auditExitCode = $LASTEXITCODE
} finally {
    if ($auditContainerCreated) { docker rm -f $auditContainer | Out-Null }
    if ($auditDatabaseCreated -and $auditDatabase -match '^gocare_audit_[0-9]+$') {
        docker exec project_worksgef-db-1 psql -U postgres -v ON_ERROR_STOP=1 -c "DROP DATABASE $auditDatabase WITH (FORCE);" | Out-Null
    }
    $env:AUDIT_DATABASE = $auditPreviousDatabase
    $env:AUDIT_FRONTEND_URL = $auditPreviousFrontend
}
exit $auditExitCode
