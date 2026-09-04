$files = @(
    git diff --name-only HEAD
    git ls-files --others --exclude-standard
) | Sort-Object -Unique

$output = "changed_files.txt"

Remove-Item $output -ErrorAction SilentlyContinue

foreach ($file in $files) {
    if (Test-Path $file) {
        Add-Content $output "`n==================== $file ====================`n"
        Get-Content $file | Add-Content $output
    }
}