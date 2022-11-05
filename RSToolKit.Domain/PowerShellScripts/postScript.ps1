Param([string]$online, [string]$offline)
$username = "AJackson"
$password = ConvertTo-SecureString "Elise315#!%" -AsPlainText -Force
$cred = New-Object System.Management.Automation.PSCredential ("AJackson", $password)
$s = New-PSSession -ComputerName "regstep.com" -credential $cred
Invoke-Command -Session $s -ScriptBlock { Add-PSSnapin WebAdministration -ErrorAction SilentlyContinue }
Invoke-Command -Session $s -ScriptBlock { Import-Module WebAdministration -ErrorAction SilentlyContinue }
Invoke-Command -Session $s -ScriptBlock { Set-Location IIS:\ }
Invoke-Command -Session $s -ScriptBlock { param($offline) Stop-Website -Name $offline } -Arg $offline
Invoke-Command -Session $s -ScriptBlock { param($online) Start-Website -Name $online } -Arg $online
Remove-PSSession -Session $s