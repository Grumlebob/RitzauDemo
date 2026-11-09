#Requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('BlazorAuto')]
    [string]$FrontendProfile,
    [Parameter(Mandatory = $true)]
    [ValidateSet('BlazorAutoV1')]
    [string]$AcceptanceProfile,
    [string]$Node,
    [string]$Address,
    [string]$BaseUrl,
    [string]$ForwardedProto,
    [ValidateRange(1, 65535)]
    [int]$Port = 80
)

$ErrorActionPreference = 'Stop'
$script:failed = $false
$script:checkName = 'parameters'
$script:forwardedProto = $ForwardedProto

function Write-Check([string]$Name, [bool]$Success, [string]$Reason) {
    if ($Success) { Write-Output ('PASS ' + $Name + ': ' + $Reason) }
    else { $script:failed = $true; Write-Output ('FAIL ' + $Name + ': ' + $Reason) }
}

function Get-ResponseUri($Response) {
    return $Response.BaseResponse.ResponseUri
}

function Request {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        $Session,
        [hashtable]$Body,
        [hashtable]$Headers,
        [bool]$FollowRedirects = $true,
        [string]$RequestMethod = 'GET'
    )
    $currentUri = [Uri]($script:site + $Path)
    $method = if ($Body) { 'POST' } else { $RequestMethod }
    $formBody = $null
    if ($Body) {
        $pairs = @()
        foreach ($key in $Body.Keys) {
            $pairs += [System.Net.WebUtility]::UrlEncode([string]$key) + '=' + [System.Net.WebUtility]::UrlEncode([string]$Body[$key])
        }
        $formBody = [string]::Join('&', $pairs)
    }

    for ($redirect = 0; $redirect -le 10; $redirect++) {
        $request = [System.Net.HttpWebRequest]::Create($currentUri)
        $request.AllowAutoRedirect = $false
        $request.CookieContainer = $Session.Cookies
        $request.Method = $method
        $request.Timeout = 30000
        $request.ReadWriteTimeout = 30000
        if ($Headers) {
            foreach ($name in $Headers.Keys) {
                if ($name -ieq 'Accept') { $request.Accept = [string]$Headers[$name] }
                else { $request.Headers[$name] = [string]$Headers[$name] }
            }
        }
        if ($script:forwardedProto) {
            $request.Headers['X-Forwarded-Proto'] = $script:forwardedProto
        }
        if ($method -eq 'POST') {
            $request.ContentType = 'application/x-www-form-urlencoded'
            $bytes = [System.Text.Encoding]::UTF8.GetBytes($formBody)
            $request.ContentLength = $bytes.Length
            $stream = $request.GetRequestStream()
            try { $stream.Write($bytes, 0, $bytes.Length) } finally { $stream.Dispose() }
        }

        $response = $null
        try {
            try { $response = $request.GetResponse() }
            catch [System.Net.WebException] {
                if ($_.Exception.Response) { $response = $_.Exception.Response } else { throw }
            }
            $status = [int]$response.StatusCode
            $location = $response.Headers['Location']
            if ($FollowRedirects -and $status -in @(301, 302, 303, 307, 308) -and $location) {
                if ($redirect -eq 10) { throw 'Same-origin redirect limit exceeded' }
                $target = [Uri]::new($currentUri, $location)
                if ($target.GetLeftPart([System.UriPartial]::Authority) -ne $script:site) {
                    throw 'Response redirected outside the requested origin'
                }
                if ($status -in @(301, 302, 303) -and $method -eq 'POST') {
                    $method = 'GET'
                    $formBody = $null
                }
                $currentUri = $target
                continue
            }

            $content = ''
            if ($method -ne 'HEAD') {
                $reader = New-Object System.IO.StreamReader($response.GetResponseStream())
                try { $content = $reader.ReadToEnd() } finally { $reader.Dispose() }
            }
            $responseHeaders = @{}
            foreach ($name in $response.Headers.AllKeys) { $responseHeaders[$name] = $response.Headers[$name] }
            return [pscustomobject]@{
                StatusCode = $status
                Content = $content
                Headers = $responseHeaders
                BaseResponse = [pscustomobject]@{ ResponseUri = $response.ResponseUri }
            }
        } finally {
            if ($response) { $response.Dispose() }
        }
    }
    throw 'Same-origin redirect limit exceeded'
}

function Form([string]$Path, [string]$Handler, $Session, [hashtable]$Fields) {
    $page = Request -Path $Path -Session $Session -Headers @{ Accept = 'text/html' }
    $token = $null
    foreach ($match in [regex]::Matches($page.Content, '<input\b[^>]*>', 'IgnoreCase')) {
        if ($match.Value -match 'name=["'']__RequestVerificationToken["'']' -and
            $match.Value -match 'value=["'']([^"'']+)["'']') {
            $token = [System.Net.WebUtility]::HtmlDecode($Matches[1])
            break
        }
    }
    if (-not $token) { throw 'Antiforgery input missing from account form' }
    $body = @{ __RequestVerificationToken = $token; _handler = $Handler }
    foreach ($key in $Fields.Keys) { $body[$key] = $Fields[$key] }
    return Request -Path $Path -Session $Session -Body $body
}

function Login($Session, [string]$Email, [string]$Password) {
    return Form '/Account/Login' 'login' $Session @{
        'Input.Email' = $Email
        'Input.Password' = $Password
        'Input.RememberMe' = 'false'
    }
}

function Is-Authenticated($Session) {
    $response = Request -Path '/api/books' -Session $Session -FollowRedirects $false
    return ([int]$response.StatusCode -eq 200 -and -not $response.Headers['Location'])
}

function Invoke-CommonAcceptance($Anonymous) {
    $script:checkName = 'health'
    $health = Request -Path '/health/ready' -Session $Anonymous
    Write-Check 'health' ([int]$health.StatusCode -eq 200 -and $health.Content.Trim() -eq 'Healthy') 'Application readiness returns Healthy'

    $script:checkName = 'anonymous-API'
    $api = Request -Path '/api/books' -Session $Anonymous -FollowRedirects $false
    $apiDenied = [int]$api.StatusCode -eq 401 -and -not $api.Headers['Location']
    Write-Check 'anonymous-API' $apiDenied 'Anonymous API returns 401 without redirect'
}

function Invoke-BlazorAccountAcceptance($Uri) {
    # Accounts are always on. Deployments may keep registration off, and registration needs
    # email confirmation, so this check never creates an account (OD-6, Q12).
    $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $script:checkName = 'Blazor'
    $homeResponse = Request -Path '/' -Session $session -Headers @{ Accept = 'text/html' }
    Write-Check 'Blazor' ([int]$homeResponse.StatusCode -eq 200 -and $homeResponse.Content.Contains('_framework/blazor.web')) 'Home contains Blazor script'

    $script:checkName = 'account-management-removed'
    $anonymous = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $manage = Request -Path '/Account/Manage' -Session $anonymous -Headers @{ Accept = 'text/html' } -FollowRedirects $false
    Write-Check 'account-management-removed' ([int]$manage.StatusCode -eq 404 -and -not $manage.Headers['Location']) 'Removed account-management route returns 404'

    # Account forms need HTTPS: their antiforgery cookie is Secure. Plain LAN HTTP checks stop here.
    if ($Uri.Scheme -eq 'https' -or $script:forwardedProto -eq 'https') {
        $script:checkName = 'account-login-page'
        $loginPage = Request -Path '/Account/Login' -Session $session -Headers @{ Accept = 'text/html' }
        Write-Check 'account-login-page' ([int]$loginPage.StatusCode -eq 200 -and $loginPage.Content -match 'name=["'']__RequestVerificationToken["'']' -and $loginPage.Content -match 'name=["'']Input\.Email["'']') 'Sign-in page renders its protected form'
    }
    if ($Uri.Scheme -eq 'https') {
        # The client sends Secure cookies, so form posts and cookie checks need a real HTTPS origin.
        $antiforgeryCookies = @($session.Cookies.GetCookies($Uri) | Where-Object { $_.Name -like '__Host-CSRF-*' })
        Write-Check 'HTTPS-cookie' ($antiforgeryCookies.Count -gt 0 -and @($antiforgeryCookies | Where-Object { -not ($_.Secure -and $_.HttpOnly) }).Count -eq 0) 'Account cookies are Secure and HttpOnly'
        $script:checkName = 'default-admin-disabled'
        $negative = New-Object Microsoft.PowerShell.Commands.WebRequestSession
        $denied = Login $negative 'admin@admin.com' 'Admin123'
        Write-Check 'default-admin-disabled' ($denied.Content.Contains('Invalid login attempt') -and -not (Is-Authenticated $negative)) 'Published default credentials rejected'
    }
}

try {
    if ($ForwardedProto -and $ForwardedProto -notin @('http', 'https')) {
        throw 'ForwardedProto must be http or https'
    }
    $expectedAcceptance = 'BlazorAutoV1'
    if ($AcceptanceProfile -ne $expectedAcceptance) {
        throw 'FrontendProfile and AcceptanceProfile do not match'
    }
    if (-not $BaseUrl) {
        $hostName = if ($Address) { $Address } elseif ($Node) { $Node + '.local' } else { throw 'Provide -Node, -Address or -BaseUrl' }
        $BaseUrl = 'http://' + $hostName + ':' + $Port
    }
    $uri = [Uri]$BaseUrl
    if ($uri.Scheme -notin @('http', 'https') -or $uri.UserInfo -or $uri.AbsolutePath -ne '/' -or $uri.Query -or $uri.Fragment) {
        throw 'BaseUrl must be an HTTP or HTTPS origin without credentials, a path, query or fragment'
    }
    $script:site = $uri.GetLeftPart([System.UriPartial]::Authority)

    try {
        $script:checkName = 'DNS'
        $addresses = [System.Net.Dns]::GetHostAddresses($uri.DnsSafeHost)
        if (-not $addresses) { throw 'No addresses' }
        Write-Check 'DNS' $true 'Host resolves'
    } catch {
        Write-Check 'DNS' $false 'Host did not resolve'
        if ($Node -and -not $Address) { Write-Output ('Re-run: powershell -NoProfile -File Scripts/Test-DeployedSite.ps1 -FrontendProfile ' + $FrontendProfile + ' -AcceptanceProfile ' + $AcceptanceProfile + ' -Address <node IPv4> -Port ' + $Port) }
        throw 'Name resolution failed'
    }

    $script:checkName = 'TCP'
    $tcp = New-Object System.Net.Sockets.TcpClient
    try {
        $connect = $tcp.ConnectAsync($uri.DnsSafeHost, $uri.Port)
        if (-not $connect.Wait(5000)) { throw 'TCP timeout' }
        $null = $connect.GetAwaiter().GetResult()
        Write-Check 'TCP' $tcp.Connected 'HTTP port accepts connections'
    } finally { $tcp.Dispose() }

    $anonymous = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    Invoke-CommonAcceptance $anonymous
    Invoke-BlazorAccountAcceptance $uri
} catch {
    # Never emit request bodies, credentials or exception dumps.
    $reason = 'HTTP acceptance stopped (' + $_.Exception.GetType().Name + '); inspect server diagnostics'
    if ($_.Exception.Message -in @(
        'FrontendProfile and AcceptanceProfile do not match',
        'Provide -Node, -Address or -BaseUrl',
        'BaseUrl must be an HTTP or HTTPS origin without credentials, a path, query or fragment',
        'Antiforgery input missing from account form'
    )) {
        $reason = $_.Exception.Message
    }
    if ($script:checkName -in @('parameters', 'DNS', 'TCP')) {
        $reason = $_.Exception.Message
    }
    Write-Check $script:checkName $false $reason
}

if ($script:failed) { Write-Output 'RESULT: FAIL'; exit 1 }
Write-Output 'RESULT: PASS'
exit 0
