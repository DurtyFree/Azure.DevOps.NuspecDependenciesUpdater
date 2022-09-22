[CmdletBinding()]
param(
    [string]$RootDirectory
)

begin
{
    $PackagesConfigMergerExe = ".\exe\NuspecDependenciesUpdater.exe"
}

process
{
    Write-Verbose "Executing command: $RootDirectory"
    & $PackagesConfigMergerExe $RootDirectory
}

end
{

}