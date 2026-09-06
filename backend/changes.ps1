$repoRoot = git rev-parse --show-toplevel
$output = Join-Path $repoRoot "changed_files.txt"

Remove-Item $output -ErrorAction SilentlyContinue

Push-Location $repoRoot

$changes = git status --porcelain=v1 -uall

foreach ($line in $changes) {
    $status = $line.Substring(0, 2)
    $file = $line.Substring(3)

    # Для renamed: old.cs -> new.cs
    if ($file -match " -> ") {
        $file = ($file -split " -> ")[-1]
    }

    # Не добавляем в результат сам generated-файл
    if ($file -eq "changed_files.txt") {
        continue
    }

    Add-Content $output "`n============================================================"
    Add-Content $output "STATUS: $status"
    Add-Content $output "FILE: $file"
    Add-Content $output "============================================================`n"

    if (Test-Path $file -PathType Leaf) {
        Get-Content $file | Add-Content $output
    }
    else {
        Add-Content $output "[FILE DELETED]"
    }
}

Pop-Location

Write-Host "Saved to: $output"