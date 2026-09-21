# v0.1.2-beta.1 — per-user installer

## Что изменено
- Добавлены Inno Setup сценарий и PowerShell build script для упаковки self-contained single-file приложения в Setup EXE.
- Setup создаёт ярлык в меню «Пуск», опциональный ярлык на рабочем столе и uninstaller без запроса прав администратора.
- Перед запуском приложения setup проверяет Evergreen WebView2 Runtime и предлагает открыть только официальную страницу Microsoft, если Runtime не найден.

## Проверка
- Выполнены: `dotnet build NexsusVKVideo.sln -c Release --no-restore -m:1`, `dotnet test NexsusVKVideo.sln -c Release --no-build -m:1` (32/32), Inno Setup 7 compile и скрипт `Build-Installer.ps1`.
- Собран `artifacts/installer/NexsusVKVideo-Setup-0.1.2-beta.1.exe`; его SHA-256: `F34A8F8866511FDE5B84D545F281E6282169FAF1C0AEFFD7B53D6F74160F6133`.
- Выполнен локальный тихий install/uninstall smoke: приложение и `unins000.exe` созданы, затем тестовый каталог удалён штатным uninstaller.
- Перед публикацией обязательны: чистая Windows VM, ярлыки, отсутствие Runtime, наличие Runtime и запуск после установки.

## Ограничения и откат
- Установщик не подписан, Runtime не встраивается и не скачивается автоматически.
- Откат: удалить приложение через Windows «Установленные приложения» или запустить uninstaller; пользовательский профиль WebView2 не удаляется автоматически.
