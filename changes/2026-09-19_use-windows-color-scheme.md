# Системная цветовая схема окна

Цель: использовать цветовую схему Windows в нативном окне минимальной оболочки VK Video.

Базовый commit: `6ac850f`.

Затронутые компоненты: XAML главного окна.

Результат: фон, текст и аварийное сообщение окна используют динамические `SystemColors.WindowBrushKey` и `SystemColors.WindowTextBrushKey`. WPF обновляет эти системные кисти при изменении цветовой схемы Windows. Содержимое сайта VK Video управляет своей темой самостоятельно.

Проверки: `dotnet build NexsusVKVideo.sln -c Release --no-restore -m:1` — успешно, 0 warnings/0 errors; `dotnet test NexsusVKVideo.sln -c Release --no-build -m:1` — успешно, 32/32. Ручная проверка: переключить системную цветовую схему Windows и вызвать аварийное состояние WebView2.

Миграция/совместимость: данных и профиля WebView2 не изменяет.

Откат: `git revert <commit>` для commit этого изменения, затем повторить build и test.
