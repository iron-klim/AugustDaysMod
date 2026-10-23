# Управление игрой при проверке мода. Гарантирует, что запущен ровно один экземпляр
# Использование: powershell -File scripts/game.ps1 restart|stop|focus|count
param([Parameter(Mandatory = $true)][ValidateSet('restart', 'stop', 'focus', 'count')] [string]$Command)

$ProcessName = 'CrisisInTheKremlin2'

Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class GameWindow {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}
"@

function Get-Game { @(Get-Process $ProcessName -ErrorAction SilentlyContinue) }

function Stop-Game {
    Get-Game | Stop-Process -Force
    for ($i = 0; $i -lt 30 -and (Get-Game).Count -gt 0; $i++) { Start-Sleep -Milliseconds 500 }
    if ((Get-Game).Count -gt 0) { throw "игра не закрылась" }
}

switch ($Command) {
    'stop' { Stop-Game; 'остановлено' }
    'count' { "экземпляров: $((Get-Game).Count)" }
    'focus' {
        $games = Get-Game | Where-Object { $_.MainWindowHandle -ne 0 }
        if ($games.Count -ne 1) { throw "ожидался один экземпляр игры с окном, найдено: $($games.Count)" }
        [GameWindow]::ShowWindow($games[0].MainWindowHandle, 9) | Out-Null
        [GameWindow]::SetForegroundWindow($games[0].MainWindowHandle) | Out-Null
        'окно игры на переднем плане'
    }
    'restart' {
        Stop-Game
        Start-Process 'steam://rungameid/1922740'
        # Steam может перезапустить исполняемый файл: ждем, пока останется один процесс с окном
        for ($i = 0; $i -lt 120; $i++) {
            Start-Sleep -Milliseconds 500
            $games = Get-Game
            if ($games.Count -eq 1 -and $games[0].MainWindowHandle -ne 0) { break }
        }
        $games = Get-Game
        if ($games.Count -gt 1) {
            # Оставляем самый ранний экземпляр
            $games | Sort-Object StartTime | Select-Object -Skip 1 | Stop-Process -Force
        }
        "запущено, экземпляров: $((Get-Game).Count)"
    }
}
