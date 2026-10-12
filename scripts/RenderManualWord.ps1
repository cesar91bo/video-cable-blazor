$manualRoot = Split-Path -Parent $PSScriptRoot
$manualInput = Join-Path $manualRoot 'docs\Manual de usuario Video Cable.docx'
$manualPreviewDirectory = Join-Path $manualRoot '.local-migration\manual-qa'
New-Item -ItemType Directory -Path $manualPreviewDirectory -Force | Out-Null
$manualPdf = Join-Path $manualPreviewDirectory 'manual-preview.pdf'
$manualWord = $null
$manualDocument = $null
try {
    $manualWord = New-Object -ComObject Word.Application
    $manualWord.Visible = $false
    $manualWord.DisplayAlerts = 0
    $manualDocument = $manualWord.Documents.Open($manualInput, $false, $true)
    $manualDocument.Repaginate()
    $manualDocument.ExportAsFixedFormat($manualPdf, 17)
    Write-Output $manualPdf
    Write-Output ('Pages: ' + $manualDocument.ComputeStatistics(2))
}
finally {
    if ($null -ne $manualDocument) { $manualDocument.Close(0); [void][Runtime.InteropServices.Marshal]::ReleaseComObject($manualDocument) }
    if ($null -ne $manualWord) { $manualWord.Quit(); [void][Runtime.InteropServices.Marshal]::ReleaseComObject($manualWord) }
}
