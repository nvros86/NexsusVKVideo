# Полноэкранное видео VK Video

Цель: дать элементу video на официальном сайте VK Video переходить в полноэкранный режим внутри NexsusVKVideo.

Базовый commit: `c9b6824`.

Затронутые компоненты: WebView2 lifecycle главного WPF-окна.

Результат: обработчик `ContainsFullScreenElementChanged` WebView2 переводит WPF-окно в безрамочный maximized-режим, когда сайт VK Video запрашивает HTML fullscreen. При выходе из fullscreen исходные WindowState, WindowStyle и ResizeMode восстанавливаются.

Проверки: `dotnet build NexsusVKVideo.sln -c Release --no-restore -m:1` — успешно, 0 warnings/0 errors; `dotnet test NexsusVKVideo.sln -c Release --no-build -m:1` — успешно, 32/32. Ручная проверка: открыть видео, нажать fullscreen в плеере, затем выйти клавишей Escape или кнопкой плеера.

Миграция/совместимость: данных и профиля WebView2 не изменяет.

Откат: `git revert <commit>` для commit этого изменения, затем повторить build и test.
