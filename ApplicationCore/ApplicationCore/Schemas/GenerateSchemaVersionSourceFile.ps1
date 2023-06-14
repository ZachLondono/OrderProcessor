if ($args.Count -ne 2) {
	Throw "Invalid number of arguments"
}

$ClassName = $args[0] 
$Version = $args[1]

$contents = @"
namespace ApplicationCore.Schemas;

internal static class $($ClassName) {
	internal const int SCHEMA_VERSION = $($Version);
}
"@

Out-File -FilePath "$($PSScriptRoot)\$($ClassName).cs" -InputObject $contents 
