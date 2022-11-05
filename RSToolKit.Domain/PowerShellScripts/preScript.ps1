Param([string]$online, [string]$offline)
$username = "AJackson"
$password = ConvertTo-SecureString "Elise315#!%" -AsPlainText -Force
$cred = New-Object System.Management.Automation.PSCredential ("AJackson", $password)
$s = New-PSSession -ComputerName "regstep.com" -credential $cred
Invoke-Command -Session $s -ScriptBlock { Add-PSSnapin WebAdministration -ErrorAction SilentlyContinue }
Invoke-Command -Session $s -ScriptBlock { Import-Module WebAdministration -ErrorAction SilentlyContinue }
Invoke-Command -Session $s -ScriptBlock { Set-Location IIS:\ }
Invoke-Command -Session $s -ScriptBlock { param($online) Stop-Website -Name $online } -Arg $online
Invoke-Command -Session $s -ScriptBlock { param($offline) Start-Website -Name $offline } -Arg $offline
Remove-PSSession -Session $s